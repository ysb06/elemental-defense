using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SQLite;

namespace ElementalDef.Data
{
    public sealed class EDataStore : IElementalDefRunStore
    {
        private const int SchemaVersion = 1;
        private const string StageRunsTableName = "stage_runs";
        private const string SelectRunColumns =
            "SELECT completion_sequence, run_id, payload_hash, stage_id, " +
            "play_duration_ms, headquarters_remaining_hp, defeated_enemy_count, " +
            "attack_count, outcome, completed_at_utc_ms FROM stage_runs";

        private static readonly HashSet<string> ExpectedStageRunColumns =
            new(StringComparer.Ordinal)
            {
                "completion_sequence",
                "run_id",
                "payload_hash",
                "stage_id",
                "play_duration_ms",
                "headquarters_remaining_hp",
                "defeated_enemy_count",
                "attack_count",
                "outcome",
                "completed_at_utc_ms"
            };

        private readonly string databasePath;
        private SQLiteConnection connection;

        public DataStoreState State { get; private set; }
        public string DatabasePath => databasePath;

        public event Action<CompletedStageRunRecord> RunCommitted;

        public EDataStore(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A database path is required.", nameof(databasePath));
            }

            this.databasePath = databasePath;
            State = DataStoreState.Uninitialized;
        }

        public void Initialize()
        {
            if (State == DataStoreState.Ready)
            {
                return;
            }

            if (State != DataStoreState.Uninitialized)
            {
                throw new InvalidOperationException($"The data store cannot be initialized from state {State}.");
            }

            try
            {
                string directoryPath = Path.GetDirectoryName(databasePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("The database path must include a directory.");
                }

                Directory.CreateDirectory(directoryPath);
                if (RequiresDevelopmentReset())
                {
                    DeleteDatabaseFiles();
                }

                connection = OpenConnection();
                ConfigureConnection(connection);

                if (!HasExpectedSchema(connection))
                {
                    CreateSchema(connection);
                }
                else
                {
                    EnsureIndexes(connection);
                }

                State = DataStoreState.Ready;
            }
            catch (Exception exception)
            {
                CloseConnectionWithoutThrowing();
                State = DataStoreState.Faulted;
                throw new InvalidOperationException(
                    "The ElementalDef stage-run database could not be initialized.",
                    exception);
            }
        }

        public CompletedStageRunCommitResult Commit(CompletedStageRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            EnsureReady();
            string payloadHash = ComputePayloadHash(snapshot);
            CompletedStageRunCommitResult result = null;

            connection.RunInTransaction(() =>
            {
                StageRunRow existing = FindRunRow(snapshot.RunId);
                if (existing != null)
                {
                    CompletedStageRunCommitStatus status = string.Equals(
                        existing.PayloadHash,
                        payloadHash,
                        StringComparison.Ordinal)
                            ? CompletedStageRunCommitStatus.AlreadyCommitted
                            : CompletedStageRunCommitStatus.RunIdConflict;
                    result = new CompletedStageRunCommitResult(status, ToRecord(existing));
                    return;
                }

                connection.Execute(
                    "INSERT INTO stage_runs " +
                    "(run_id, payload_hash, stage_id, play_duration_ms, " +
                    "headquarters_remaining_hp, defeated_enemy_count, attack_count, " +
                    "outcome, completed_at_utc_ms) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    snapshot.RunId,
                    payloadHash,
                    snapshot.StageId,
                    snapshot.PlayDurationMilliseconds,
                    snapshot.HeadquartersRemainingHealth,
                    snapshot.DefeatedEnemyCount,
                    snapshot.AttackCount,
                    (int)snapshot.Outcome,
                    snapshot.CompletedAtUtc.ToUnixTimeMilliseconds());

                long completionSequence = connection.ExecuteScalar<long>("SELECT last_insert_rowid()");
                result = new CompletedStageRunCommitResult(
                    CompletedStageRunCommitStatus.Committed,
                    new CompletedStageRunRecord(
                        completionSequence,
                        snapshot.RunId,
                        snapshot.StageId,
                        snapshot.PlayDurationMilliseconds,
                        snapshot.HeadquartersRemainingHealth,
                        snapshot.DefeatedEnemyCount,
                        snapshot.AttackCount,
                        snapshot.Outcome,
                        snapshot.CompletedAtUtc));
            });

            CompletedStageRunCommitResult commitResult = result ??
                throw new InvalidOperationException(
                    "The completed stage run transaction produced no result.");
            if (commitResult.IsNewlyCommitted)
            {
                NotifyRunCommitted(commitResult.Record);
            }

            return commitResult;
        }

        public IReadOnlyList<CompletedStageRunRecord> GetRecentRuns(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            EnsureReady();
            List<StageRunRow> rows = connection.Query<StageRunRow>(
                SelectRunColumns + " ORDER BY completion_sequence DESC LIMIT ?",
                limit);
            var records = new List<CompletedStageRunRecord>(rows.Count);
            foreach (StageRunRow row in rows)
            {
                records.Add(ToRecord(row));
            }

            return records.AsReadOnly();
        }

        public IReadOnlyList<CompletedStageRunRecord> GetRecentRunsForStage(
            string stageId,
            int limit)
        {
            string validatedStageId = PersistenceValidation.RequireId(stageId, nameof(stageId));
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            EnsureReady();
            List<StageRunRow> rows = connection.Query<StageRunRow>(
                SelectRunColumns +
                " WHERE stage_id = ? ORDER BY completion_sequence DESC LIMIT ?",
                validatedStageId,
                limit);
            var records = new List<CompletedStageRunRecord>(rows.Count);
            foreach (StageRunRow row in rows)
            {
                records.Add(ToRecord(row));
            }

            return records.AsReadOnly();
        }

        public bool TryGetRun(string runId, out CompletedStageRunRecord record)
        {
            string normalizedRunId = PersistenceValidation.NormalizeGuidN(runId, nameof(runId));
            EnsureReady();

            StageRunRow row = FindRunRow(normalizedRunId);
            if (row == null)
            {
                record = null;
                return false;
            }

            record = ToRecord(row);
            return true;
        }

        public void Dispose()
        {
            if (State == DataStoreState.Disposed)
            {
                return;
            }

            try
            {
                connection?.Close();
            }
            finally
            {
                connection = null;
                State = DataStoreState.Disposed;
            }
        }

        private bool RequiresDevelopmentReset()
        {
            if (!File.Exists(databasePath))
            {
                return false;
            }

            SQLiteConnection probe = null;
            try
            {
                probe = OpenConnection();
                ConfigureConnection(probe);
                return !HasExpectedSchema(probe);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (probe != null)
                {
                    try
                    {
                        probe.Close();
                    }
                    catch
                    {
                        // The incompatible development database will be removed below.
                    }
                }
            }
        }

        private SQLiteConnection OpenConnection()
        {
            return new SQLiteConnection(
                databasePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        }

        private static void ConfigureConnection(SQLiteConnection target)
        {
            target.Execute("PRAGMA foreign_keys = ON");
            target.Execute("PRAGMA busy_timeout = 5000");
        }

        private static bool HasExpectedSchema(SQLiteConnection target)
        {
            int version = target.ExecuteScalar<int>("PRAGMA user_version");
            if (version != SchemaVersion)
            {
                return false;
            }

            List<SqliteNameRow> tables = target.Query<SqliteNameRow>(
                "SELECT name FROM sqlite_master " +
                "WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name");
            if (tables.Count != 1 ||
                !string.Equals(tables[0].Name, StageRunsTableName, StringComparison.Ordinal))
            {
                return false;
            }

            List<SqliteNameRow> columns = target.Query<SqliteNameRow>(
                "PRAGMA table_info(stage_runs)");
            var actualColumns = new HashSet<string>(StringComparer.Ordinal);
            foreach (SqliteNameRow column in columns)
            {
                actualColumns.Add(column.Name);
            }

            return actualColumns.SetEquals(ExpectedStageRunColumns);
        }

        private static void CreateSchema(SQLiteConnection target)
        {
            target.RunInTransaction(() =>
            {
                target.Execute(
                    "CREATE TABLE stage_runs (" +
                    "completion_sequence INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "run_id TEXT NOT NULL UNIQUE, " +
                    "payload_hash TEXT NOT NULL, " +
                    "stage_id TEXT NOT NULL, " +
                    "play_duration_ms INTEGER NOT NULL CHECK (play_duration_ms >= 0), " +
                    "headquarters_remaining_hp REAL NOT NULL CHECK (headquarters_remaining_hp >= 0), " +
                    "defeated_enemy_count INTEGER NOT NULL CHECK (defeated_enemy_count >= 0), " +
                    "attack_count INTEGER NOT NULL CHECK (attack_count >= 0), " +
                    "outcome INTEGER NOT NULL CHECK (outcome IN (1, 2)), " +
                    "completed_at_utc_ms INTEGER NOT NULL)");
                EnsureIndexes(target);
                target.Execute($"PRAGMA user_version = {SchemaVersion}");
            });
        }

        private static void EnsureIndexes(SQLiteConnection target)
        {
            target.Execute(
                "CREATE INDEX IF NOT EXISTS idx_stage_runs_completion " +
                "ON stage_runs(completion_sequence DESC)");
            target.Execute(
                "CREATE INDEX IF NOT EXISTS idx_stage_runs_stage_completion " +
                "ON stage_runs(stage_id, completion_sequence DESC)");
        }

        private StageRunRow FindRunRow(string runId)
        {
            return connection.FindWithQuery<StageRunRow>(
                SelectRunColumns + " WHERE run_id = ? LIMIT 1",
                runId);
        }

        private void NotifyRunCommitted(CompletedStageRunRecord record)
        {
            Action<CompletedStageRunRecord> handlers = RunCommitted;
            if (handlers == null)
            {
                return;
            }

            foreach (Action<CompletedStageRunRecord> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(record);
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(
                        $"A RunCommitted subscriber failed after run {record.RunId} was committed: " +
                        exception);
                }
            }
        }

        private static CompletedStageRunRecord ToRecord(StageRunRow row)
        {
            return new CompletedStageRunRecord(
                row.CompletionSequence,
                row.RunId,
                row.StageId,
                row.PlayDurationMilliseconds,
                row.HeadquartersRemainingHealth,
                row.DefeatedEnemyCount,
                row.AttackCount,
                (StageRunOutcome)row.Outcome,
                DateTimeOffset.FromUnixTimeMilliseconds(row.CompletedAtUtcMilliseconds));
        }

        private static string ComputePayloadHash(CompletedStageRunSnapshot snapshot)
        {
            byte[] payload;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(snapshot.RunId);
                    writer.Write(snapshot.StageId);
                    writer.Write(snapshot.PlayDurationMilliseconds);
                    writer.Write(snapshot.HeadquartersRemainingHealth);
                    writer.Write(snapshot.DefeatedEnemyCount);
                    writer.Write(snapshot.AttackCount);
                    writer.Write((int)snapshot.Outcome);
                    writer.Write(snapshot.CompletedAtUtc.ToUnixTimeMilliseconds());
                }

                payload = stream.ToArray();
            }

            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(payload);
            }

            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }

        private void DeleteDatabaseFiles()
        {
            File.Delete(databasePath);
            File.Delete(databasePath + "-wal");
            File.Delete(databasePath + "-shm");
            File.Delete(databasePath + "-journal");
        }

        private void EnsureReady()
        {
            if (State != DataStoreState.Ready || connection == null)
            {
                throw new InvalidOperationException(
                    $"The data store is not ready. Current state: {State}.");
            }
        }

        private void CloseConnectionWithoutThrowing()
        {
            if (connection == null)
            {
                return;
            }

            try
            {
                connection.Close();
            }
            catch
            {
                // Preserve the original initialization exception.
            }
            finally
            {
                connection = null;
            }
        }
    }

    [Table("stage_runs")]
    [Preserve(AllMembers = true)]
    internal sealed class StageRunRow
    {
        [Column("completion_sequence")]
        public long CompletionSequence { get; set; }

        [Column("run_id")]
        public string RunId { get; set; }

        [Column("payload_hash")]
        public string PayloadHash { get; set; }

        [Column("stage_id")]
        public string StageId { get; set; }

        [Column("play_duration_ms")]
        public long PlayDurationMilliseconds { get; set; }

        [Column("headquarters_remaining_hp")]
        public double HeadquartersRemainingHealth { get; set; }

        [Column("defeated_enemy_count")]
        public long DefeatedEnemyCount { get; set; }

        [Column("attack_count")]
        public long AttackCount { get; set; }

        [Column("outcome")]
        public int Outcome { get; set; }

        [Column("completed_at_utc_ms")]
        public long CompletedAtUtcMilliseconds { get; set; }
    }

    [Preserve(AllMembers = true)]
    internal sealed class SqliteNameRow
    {
        [Column("name")]
        public string Name { get; set; }

        public SqliteNameRow()
        {
        }
    }
}
