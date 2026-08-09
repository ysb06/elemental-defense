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
        private const int SchemaVersion = 4;
        private const int MigratableSchemaVersion = 3;
        private const long PlayerId = 1;
        private const int FinalStageDisplayOrder = 10;
        private const string StageRunsTableName = "stage_runs";
        private const string PlayerProgressTableName = "player_progress";
        private const string SelectRunColumns =
            "SELECT completion_sequence, run_id, payload_hash, stage_id, stage_display_order, " +
            "play_duration_ms, headquarters_remaining_hp, headquarters_max_hp, " +
            "defeated_enemy_count, " +
            "attack_count, earned_credits, earned_experience, outcome, " +
            "completed_at_utc_ms FROM stage_runs";
        private const string SelectPlayerProgressColumns =
            "SELECT player_id, total_credits, total_experience, max_stage_progress, " +
            "loop, total_defeat_count, updated_at_utc_ms FROM player_progress";

        private static readonly HashSet<string> ExpectedStageRunColumns =
            new(StringComparer.Ordinal)
            {
                "completion_sequence",
                "run_id",
                "payload_hash",
                "stage_id",
                "stage_display_order",
                "play_duration_ms",
                "headquarters_remaining_hp",
                "headquarters_max_hp",
                "defeated_enemy_count",
                "attack_count",
                "earned_credits",
                "earned_experience",
                "outcome",
                "completed_at_utc_ms"
            };

        private static readonly HashSet<string> ExpectedPlayerProgressColumns =
            new(StringComparer.Ordinal)
            {
                "player_id",
                "total_credits",
                "total_experience",
                "max_stage_progress",
                "loop",
                "total_defeat_count",
                "updated_at_utc_ms"
            };

        private static readonly HashSet<string> VersionThreeStageRunColumns =
            new(StringComparer.Ordinal)
            {
                "completion_sequence",
                "run_id",
                "payload_hash",
                "stage_id",
                "stage_display_order",
                "play_duration_ms",
                "headquarters_remaining_hp",
                "defeated_enemy_count",
                "attack_count",
                "earned_credits",
                "earned_experience",
                "outcome",
                "completed_at_utc_ms"
            };

        private static readonly HashSet<string> VersionThreePlayerProgressColumns =
            new(StringComparer.Ordinal)
            {
                "player_id",
                "total_credits",
                "total_experience",
                "max_stage_progress",
                "loop",
                "updated_at_utc_ms"
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

                int currentVersion = connection.ExecuteScalar<int>("PRAGMA user_version");
                if (currentVersion == MigratableSchemaVersion)
                {
                    MigrateVersionThreeToFour(connection);
                }

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
            long progressUpdatedAtUtcMilliseconds =
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
                    "(run_id, payload_hash, stage_id, stage_display_order, play_duration_ms, " +
                    "headquarters_remaining_hp, headquarters_max_hp, defeated_enemy_count, attack_count, " +
                    "earned_credits, earned_experience, outcome, completed_at_utc_ms) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    snapshot.RunId,
                    payloadHash,
                    snapshot.StageId,
                    snapshot.StageDisplayOrder,
                    snapshot.PlayDurationMilliseconds,
                    snapshot.HeadquartersRemainingHealth,
                    snapshot.HeadquartersMaxHealth,
                    snapshot.DefeatedEnemyCount,
                    snapshot.AttackCount,
                    snapshot.EarnedCredits,
                    snapshot.EarnedExperience,
                    (int)snapshot.Outcome,
                    snapshot.CompletedAtUtc.ToUnixTimeMilliseconds());

                long completionSequence = connection.ExecuteScalar<long>("SELECT last_insert_rowid()");
                UpdatePlayerProgress(snapshot, progressUpdatedAtUtcMilliseconds);

                result = new CompletedStageRunCommitResult(
                    CompletedStageRunCommitStatus.Committed,
                    new CompletedStageRunRecord(
                        completionSequence,
                        snapshot.RunId,
                        snapshot.StageId,
                        snapshot.StageDisplayOrder,
                        snapshot.PlayDurationMilliseconds,
                        snapshot.HeadquartersRemainingHealth,
                        snapshot.HeadquartersMaxHealth,
                        snapshot.DefeatedEnemyCount,
                        snapshot.AttackCount,
                        snapshot.EarnedCredits,
                        snapshot.EarnedExperience,
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

        public PlayerProgressSnapshot GetPlayerProgress()
        {
            EnsureReady();
            PlayerProgressRow row = FindPlayerProgressRow();
            if (row == null)
            {
                throw new InvalidOperationException(
                    "The singleton player-progress row is missing.");
            }

            return ToProgressSnapshot(row);
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
                int version = probe.ExecuteScalar<int>("PRAGMA user_version");
                if (version == 1 || version == 2)
                {
                    return true;
                }

                if (version > SchemaVersion)
                {
                    throw new InvalidOperationException(
                        $"The database schema version {version} is newer than the supported " +
                        $"version {SchemaVersion}.");
                }

                if (version == MigratableSchemaVersion && !HasExpectedVersionThreeSchema(probe))
                {
                    throw new InvalidOperationException(
                        "The version 3 database does not match the expected schema.");
                }

                if (version == SchemaVersion && !HasExpectedSchema(probe))
                {
                    throw new InvalidOperationException(
                        $"The version {SchemaVersion} database does not match the expected schema.");
                }

                return false;
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
                        // Preserve the original probe or initialization exception.
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
            target.BusyTimeout = TimeSpan.FromSeconds(5);
        }

        private static bool HasExpectedSchema(SQLiteConnection target)
        {
            return HasExpectedSchema(
                target,
                SchemaVersion,
                ExpectedStageRunColumns,
                ExpectedPlayerProgressColumns);
        }

        private static bool HasExpectedVersionThreeSchema(SQLiteConnection target)
        {
            return HasExpectedSchema(
                target,
                MigratableSchemaVersion,
                VersionThreeStageRunColumns,
                VersionThreePlayerProgressColumns);
        }

        private static bool HasExpectedSchema(
            SQLiteConnection target,
            int expectedVersion,
            HashSet<string> expectedStageRunColumns,
            HashSet<string> expectedPlayerProgressColumns)
        {
            int version = target.ExecuteScalar<int>("PRAGMA user_version");
            if (version != expectedVersion)
            {
                return false;
            }

            List<SqliteNameRow> tables = target.Query<SqliteNameRow>(
                "SELECT name FROM sqlite_master " +
                "WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name");
            var actualTables = new HashSet<string>(StringComparer.Ordinal);
            foreach (SqliteNameRow table in tables)
            {
                actualTables.Add(table.Name);
            }

            if (!actualTables.SetEquals(new[] { StageRunsTableName, PlayerProgressTableName }))
            {
                return false;
            }

            List<SqliteNameRow> stageRunColumns = target.Query<SqliteNameRow>(
                "PRAGMA table_info(stage_runs)");
            var actualStageRunColumns = new HashSet<string>(StringComparer.Ordinal);
            foreach (SqliteNameRow column in stageRunColumns)
            {
                actualStageRunColumns.Add(column.Name);
            }

            if (!actualStageRunColumns.SetEquals(expectedStageRunColumns))
            {
                return false;
            }

            List<SqliteNameRow> playerProgressColumns = target.Query<SqliteNameRow>(
                "PRAGMA table_info(player_progress)");
            var actualPlayerProgressColumns = new HashSet<string>(StringComparer.Ordinal);
            foreach (SqliteNameRow column in playerProgressColumns)
            {
                actualPlayerProgressColumns.Add(column.Name);
            }

            return actualPlayerProgressColumns.SetEquals(expectedPlayerProgressColumns);
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
                    "stage_display_order INTEGER NOT NULL " +
                    "CHECK (stage_display_order BETWEEN 1 AND 10), " +
                    "play_duration_ms INTEGER NOT NULL CHECK (play_duration_ms >= 0), " +
                    "headquarters_remaining_hp REAL NOT NULL CHECK (headquarters_remaining_hp >= 0), " +
                    "headquarters_max_hp REAL NOT NULL CHECK (headquarters_max_hp > 0), " +
                    "defeated_enemy_count INTEGER NOT NULL CHECK (defeated_enemy_count >= 0), " +
                    "attack_count INTEGER NOT NULL CHECK (attack_count >= 0), " +
                    "earned_credits INTEGER NOT NULL CHECK (earned_credits >= 0), " +
                    "earned_experience INTEGER NOT NULL CHECK (earned_experience >= 0), " +
                    "outcome INTEGER NOT NULL CHECK (outcome IN (1, 2)), " +
                    "completed_at_utc_ms INTEGER NOT NULL, " +
                    "CHECK (headquarters_remaining_hp <= headquarters_max_hp), " +
                    "CHECK (outcome = 1 OR " +
                    "(earned_credits = 0 AND earned_experience = 0)))");
                target.Execute(
                    "CREATE TABLE player_progress (" +
                    "player_id INTEGER PRIMARY KEY CHECK (player_id = 1), " +
                    "total_credits INTEGER NOT NULL CHECK (total_credits >= 0), " +
                    "total_experience INTEGER NOT NULL CHECK (total_experience >= 0), " +
                    "max_stage_progress INTEGER NOT NULL " +
                    "CHECK (max_stage_progress BETWEEN 0 AND 9), " +
                    "loop INTEGER NOT NULL CHECK (loop >= 0), " +
                    "total_defeat_count INTEGER NOT NULL CHECK (total_defeat_count >= 0), " +
                    "updated_at_utc_ms INTEGER NOT NULL)");
                target.Execute(
                    "INSERT INTO player_progress " +
                    "(player_id, total_credits, total_experience, max_stage_progress, " +
                    "loop, total_defeat_count, updated_at_utc_ms) " +
                    "VALUES (?, 0, 0, 0, 0, 0, 0)",
                    PlayerId);
                EnsureIndexes(target);
                target.Execute($"PRAGMA user_version = {SchemaVersion}");
            });
        }

        private static void MigrateVersionThreeToFour(SQLiteConnection target)
        {
            if (!HasExpectedVersionThreeSchema(target))
            {
                throw new InvalidOperationException(
                    "The version 3 database does not match the expected schema.");
            }

            target.RunInTransaction(() =>
            {
                target.Execute(
                    "ALTER TABLE stage_runs ADD COLUMN headquarters_max_hp REAL " +
                    "CHECK (headquarters_max_hp IS NULL OR headquarters_max_hp > 0)");
                target.Execute(
                    "ALTER TABLE player_progress ADD COLUMN total_defeat_count INTEGER " +
                    "NOT NULL DEFAULT 0 CHECK (total_defeat_count >= 0)");

                int updatedRows = target.Execute(
                    "UPDATE player_progress SET total_defeat_count = " +
                    "(SELECT COUNT(*) FROM stage_runs WHERE outcome = ?), " +
                    "updated_at_utc_ms = updated_at_utc_ms WHERE player_id = ?",
                    (int)StageRunOutcome.Defeat,
                    PlayerId);
                if (updatedRows != 1)
                {
                    throw new InvalidOperationException(
                        $"Expected to migrate one player-progress row, but updated {updatedRows}.");
                }

                EnsureIndexes(target);
                target.Execute($"PRAGMA user_version = {SchemaVersion}");
            });

            if (!HasExpectedSchema(target))
            {
                throw new InvalidOperationException(
                    $"The migrated version {SchemaVersion} database does not match the expected schema.");
            }
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

        private PlayerProgressRow FindPlayerProgressRow()
        {
            return connection.FindWithQuery<PlayerProgressRow>(
                SelectPlayerProgressColumns + " WHERE player_id = ? LIMIT 1",
                PlayerId);
        }

        private void UpdatePlayerProgress(
            CompletedStageRunSnapshot snapshot,
            long updatedAtUtcMilliseconds)
        {
            PlayerProgressRow progress = FindPlayerProgressRow();
            if (progress == null)
            {
                throw new InvalidOperationException(
                    "The singleton player-progress row is missing.");
            }

            long totalCredits;
            long totalExperience;
            int maxStageProgress = progress.MaxStageProgress;
            long loop = progress.Loop;
            long totalDefeatCount = progress.TotalDefeatCount;

            checked
            {
                totalCredits = progress.TotalCredits + snapshot.EarnedCredits;
                totalExperience = progress.TotalExperience + snapshot.EarnedExperience;
                if (snapshot.Outcome == StageRunOutcome.Defeat)
                {
                    totalDefeatCount += 1;
                }

                if (snapshot.Outcome == StageRunOutcome.Victory &&
                    snapshot.StageDisplayOrder == maxStageProgress + 1)
                {
                    if (snapshot.StageDisplayOrder == FinalStageDisplayOrder)
                    {
                        maxStageProgress = 0;
                        loop += 1;
                    }
                    else
                    {
                        maxStageProgress = snapshot.StageDisplayOrder;
                    }
                }
            }

            int updatedRows = connection.Execute(
                "UPDATE player_progress SET total_credits = ?, total_experience = ?, " +
                "max_stage_progress = ?, loop = ?, total_defeat_count = ?, " +
                "updated_at_utc_ms = ? " +
                "WHERE player_id = ?",
                totalCredits,
                totalExperience,
                maxStageProgress,
                loop,
                totalDefeatCount,
                Math.Max(progress.UpdatedAtUtcMilliseconds, updatedAtUtcMilliseconds),
                PlayerId);
            if (updatedRows != 1)
            {
                throw new InvalidOperationException(
                    $"Expected to update one player-progress row, but updated {updatedRows}.");
            }
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
                row.StageDisplayOrder,
                row.PlayDurationMilliseconds,
                row.HeadquartersRemainingHealth,
                row.HeadquartersMaxHealth,
                row.DefeatedEnemyCount,
                row.AttackCount,
                row.EarnedCredits,
                row.EarnedExperience,
                (StageRunOutcome)row.Outcome,
                DateTimeOffset.FromUnixTimeMilliseconds(row.CompletedAtUtcMilliseconds));
        }

        private static PlayerProgressSnapshot ToProgressSnapshot(PlayerProgressRow row)
        {
            return new PlayerProgressSnapshot(
                row.TotalCredits,
                row.TotalExperience,
                row.MaxStageProgress,
                row.Loop,
                row.TotalDefeatCount,
                DateTimeOffset.FromUnixTimeMilliseconds(row.UpdatedAtUtcMilliseconds));
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
                    writer.Write(snapshot.StageDisplayOrder);
                    writer.Write(snapshot.PlayDurationMilliseconds);
                    writer.Write(snapshot.HeadquartersRemainingHealth);
                    writer.Write(snapshot.HeadquartersMaxHealth);
                    writer.Write(snapshot.DefeatedEnemyCount);
                    writer.Write(snapshot.AttackCount);
                    writer.Write(snapshot.EarnedCredits);
                    writer.Write(snapshot.EarnedExperience);
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

        [Column("stage_display_order")]
        public int StageDisplayOrder { get; set; }

        [Column("play_duration_ms")]
        public long PlayDurationMilliseconds { get; set; }

        [Column("headquarters_remaining_hp")]
        public double HeadquartersRemainingHealth { get; set; }

        [Column("headquarters_max_hp")]
        public double? HeadquartersMaxHealth { get; set; }

        [Column("defeated_enemy_count")]
        public long DefeatedEnemyCount { get; set; }

        [Column("attack_count")]
        public long AttackCount { get; set; }

        [Column("earned_credits")]
        public long EarnedCredits { get; set; }

        [Column("earned_experience")]
        public long EarnedExperience { get; set; }

        [Column("outcome")]
        public int Outcome { get; set; }

        [Column("completed_at_utc_ms")]
        public long CompletedAtUtcMilliseconds { get; set; }
    }

    [Table("player_progress")]
    [Preserve(AllMembers = true)]
    internal sealed class PlayerProgressRow
    {
        [Column("player_id")]
        public long PlayerId { get; set; }

        [Column("total_credits")]
        public long TotalCredits { get; set; }

        [Column("total_experience")]
        public long TotalExperience { get; set; }

        [Column("max_stage_progress")]
        public int MaxStageProgress { get; set; }

        [Column("loop")]
        public long Loop { get; set; }

        [Column("total_defeat_count")]
        public long TotalDefeatCount { get; set; }

        [Column("updated_at_utc_ms")]
        public long UpdatedAtUtcMilliseconds { get; set; }
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
