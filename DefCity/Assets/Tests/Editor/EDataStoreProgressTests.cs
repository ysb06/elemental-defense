using System;
using System.IO;
using ElementalDef.Data;
using NUnit.Framework;
using SQLite;
using UnityEngine;

namespace ElementalDef.Tests.Editor
{
    public sealed class EDataStoreProgressTests
    {
        private static readonly DateTimeOffset TestEpoch =
            new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private string testDirectory;
        private string databasePath;
        private EDataStore store;
        private int snapshotSequence;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(
                Application.temporaryCachePath,
                "ElementalDefTests",
                Guid.NewGuid().ToString("N"));
            databasePath = Path.Combine(testDirectory, "elementaldef-test.sqlite3");
            Directory.CreateDirectory(testDirectory);
            snapshotSequence = 0;
        }

        [TearDown]
        public void TearDown()
        {
            store?.Dispose();
            store = null;

            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [Test]
        public void Initialize_CreatesVersionFourSchemaAndZeroProgress()
        {
            InitializeStore();

            Assert.That(store.State, Is.EqualTo(DataStoreState.Ready));
            AssertProgress(credits: 0, experience: 0, maxStageProgress: 0, loop: 0);

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<int>("PRAGMA user_version"),
                    Is.EqualTo(4));
                Assert.That(TableExists(connection, "stage_runs"), Is.True);
                Assert.That(TableExists(connection, "player_progress"), Is.True);
                Assert.That(
                    connection.ExecuteScalar<long>(
                        "SELECT COUNT(*) FROM player_progress WHERE player_id = 1"),
                    Is.EqualTo(1));
                Assert.That(ColumnExists(connection, "stage_runs", "headquarters_max_hp"), Is.True);
                Assert.That(
                    ColumnExists(connection, "player_progress", "total_defeat_count"),
                    Is.True);
            }
        }

        [Test]
        public void Commit_AdvancesOnlyForSequentialVictories()
        {
            InitializeStore();

            DateTimeOffset beforeFirstCommit = DateTimeOffset.UtcNow;
            Commit(stageDisplayOrder: 1, StageRunOutcome.Victory);
            AssertProgress(0, 0, 1, 0);
            Assert.That(
                store.GetPlayerProgress().UpdatedAtUtc,
                Is.GreaterThanOrEqualTo(beforeFirstCommit.AddSeconds(-1)));

            Commit(stageDisplayOrder: 2, StageRunOutcome.Victory);
            AssertProgress(0, 0, 2, 0);

            Commit(stageDisplayOrder: 4, StageRunOutcome.Victory);
            AssertProgress(0, 0, 2, 0);

            Commit(stageDisplayOrder: 2, StageRunOutcome.Victory);
            AssertProgress(0, 0, 2, 0);

            Commit(stageDisplayOrder: 3, StageRunOutcome.Defeat);
            AssertProgress(0, 0, 2, 0, totalDefeatCount: 1);

            Commit(stageDisplayOrder: 3, StageRunOutcome.Victory);
            AssertProgress(0, 0, 3, 0, totalDefeatCount: 1);
        }

        [Test]
        public void Commit_StageTenRollsOverAndCannotBeRepeatedOutOfSequence()
        {
            InitializeStore();

            CommitFullLoop();
            AssertProgress(0, 0, 0, 1);

            Commit(stageDisplayOrder: 10, StageRunOutcome.Victory);
            AssertProgress(0, 0, 0, 1);

            CommitFullLoop();
            AssertProgress(0, 0, 0, 2);
        }

        [Test]
        public void Commit_AccumulatesVictoryRewardsWithoutAdvancingSkippedOrRepeatedStages()
        {
            InitializeStore();

            Commit(1, StageRunOutcome.Victory, earnedCredits: 10, earnedExperience: 20);
            Commit(1, StageRunOutcome.Victory, earnedCredits: 3, earnedExperience: 4);
            Commit(3, StageRunOutcome.Victory, earnedCredits: 5, earnedExperience: 6);
            Commit(2, StageRunOutcome.Defeat);
            AssertProgress(
                credits: 18,
                experience: 30,
                maxStageProgress: 1,
                loop: 0,
                totalDefeatCount: 1);

            Commit(2, StageRunOutcome.Victory, earnedCredits: 7, earnedExperience: 8);
            AssertProgress(
                credits: 25,
                experience: 38,
                maxStageProgress: 2,
                loop: 0,
                totalDefeatCount: 1);

            Assert.Throws<ArgumentException>(() =>
                CreateSnapshot(
                    stageDisplayOrder: 3,
                    outcome: StageRunOutcome.Defeat,
                    earnedCredits: 1));
        }

        [Test]
        public void Commit_DuplicateAndConflictDoNotChangeProgressOrRaiseAnotherEvent()
        {
            InitializeStore();
            int committedEventCount = 0;
            store.RunCommitted += _ => committedEventCount++;

            CompletedStageRunSnapshot original = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Victory,
                earnedCredits: 7,
                earnedExperience: 11);

            CompletedStageRunCommitResult committed = store.Commit(original);
            CompletedStageRunCommitResult duplicate = store.Commit(original);
            CompletedStageRunSnapshot conflicting = CopyWithHeadquartersMaxHealth(original, 2000d);
            CompletedStageRunCommitResult conflict = store.Commit(conflicting);

            Assert.That(committed.Status, Is.EqualTo(CompletedStageRunCommitStatus.Committed));
            Assert.That(duplicate.Status, Is.EqualTo(CompletedStageRunCommitStatus.AlreadyCommitted));
            Assert.That(conflict.Status, Is.EqualTo(CompletedStageRunCommitStatus.RunIdConflict));
            Assert.That(conflict.Record.HeadquartersMaxHealth, Is.EqualTo(1000d));
            Assert.That(committedEventCount, Is.EqualTo(1));
            AssertProgress(credits: 7, experience: 11, maxStageProgress: 1, loop: 0);
            Assert.That(store.GetRecentRuns(10), Has.Count.EqualTo(1));

            Assert.That(store.TryGetRun(original.RunId, out CompletedStageRunRecord record), Is.True);
            Assert.That(record.StageDisplayOrder, Is.EqualTo(1));
        }

        [Test]
        public void Commit_PersistsHeadquartersMaxHealth()
        {
            InitializeStore();
            CompletedStageRunSnapshot snapshot = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Victory,
                headquartersRemainingHealth: 750.5d,
                headquartersMaxHealth: 1250d);

            CompletedStageRunCommitResult result = store.Commit(snapshot);

            Assert.That(result.Record.HeadquartersRemainingHealth, Is.EqualTo(750.5d));
            Assert.That(result.Record.HeadquartersMaxHealth, Is.EqualTo(1250d));
            Assert.That(store.TryGetRun(snapshot.RunId, out CompletedStageRunRecord record), Is.True);
            Assert.That(record.HeadquartersMaxHealth, Is.EqualTo(1250d));
        }

        [Test]
        public void Snapshot_RejectsInvalidHeadquartersHealthValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateSnapshot(
                    stageDisplayOrder: 1,
                    outcome: StageRunOutcome.Victory,
                    headquartersMaxHealth: 0d));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateSnapshot(
                    stageDisplayOrder: 1,
                    outcome: StageRunOutcome.Victory,
                    headquartersRemainingHealth: 1001d,
                    headquartersMaxHealth: 1000d));
        }

        [Test]
        public void Commit_DuplicateAndConflictDoNotIncrementDefeatCountAgain()
        {
            InitializeStore();
            CompletedStageRunSnapshot original = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Defeat);

            CompletedStageRunCommitResult committed = store.Commit(original);
            CompletedStageRunCommitResult duplicate = store.Commit(original);
            CompletedStageRunCommitResult conflict = store.Commit(
                CopyWithHeadquartersMaxHealth(original, 2000d));

            Assert.That(committed.Status, Is.EqualTo(CompletedStageRunCommitStatus.Committed));
            Assert.That(duplicate.Status, Is.EqualTo(CompletedStageRunCommitStatus.AlreadyCommitted));
            Assert.That(conflict.Status, Is.EqualTo(CompletedStageRunCommitStatus.RunIdConflict));
            AssertProgress(0, 0, 0, 0, totalDefeatCount: 1);
            Assert.That(store.GetRecentRuns(10), Has.Count.EqualTo(1));
        }

        [Test]
        public void Commit_MissingProgressRowRollsBackRunInsertion()
        {
            InitializeStore();
            int committedEventCount = 0;
            store.RunCommitted += _ => committedEventCount++;

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.Execute("DELETE FROM player_progress WHERE player_id = 1"),
                    Is.EqualTo(1));
            }

            CompletedStageRunSnapshot snapshot = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Victory,
                earnedCredits: 10,
                earnedExperience: 20);

            Assert.Throws<InvalidOperationException>(() => store.Commit(snapshot));
            Assert.That(store.TryGetRun(snapshot.RunId, out _), Is.False);
            Assert.That(committedEventCount, Is.EqualTo(0));

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<long>("SELECT COUNT(*) FROM stage_runs"),
                    Is.EqualTo(0));
            }
        }

        [Test]
        public void Commit_RewardOverflowRollsBackRunAndProgressChanges()
        {
            InitializeStore();
            int committedEventCount = 0;
            store.RunCommitted += _ => committedEventCount++;

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.Execute(
                        "UPDATE player_progress SET total_credits = ? WHERE player_id = 1",
                        long.MaxValue),
                    Is.EqualTo(1));
            }

            CompletedStageRunSnapshot snapshot = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Victory,
                earnedCredits: 1);

            Assert.Throws<OverflowException>(() => store.Commit(snapshot));
            Assert.That(store.TryGetRun(snapshot.RunId, out _), Is.False);
            Assert.That(committedEventCount, Is.EqualTo(0));
            AssertProgress(
                credits: long.MaxValue,
                experience: 0,
                maxStageProgress: 0,
                loop: 0);
        }

        [Test]
        public void Commit_DefeatCountOverflowRollsBackRunAndProgressChanges()
        {
            InitializeStore();
            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.Execute(
                        "UPDATE player_progress SET total_defeat_count = ? WHERE player_id = 1",
                        long.MaxValue),
                    Is.EqualTo(1));
            }

            CompletedStageRunSnapshot snapshot = CreateSnapshot(
                stageDisplayOrder: 1,
                outcome: StageRunOutcome.Defeat);

            Assert.Throws<OverflowException>(() => store.Commit(snapshot));
            Assert.That(store.TryGetRun(snapshot.RunId, out _), Is.False);
            AssertProgress(0, 0, 0, 0, totalDefeatCount: long.MaxValue);

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<long>("SELECT COUNT(*) FROM stage_runs"),
                    Is.EqualTo(0));
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public void Initialize_VersionOneOrTwoDevelopmentDatabaseIsResetToVersionFour(
            int legacyVersion)
        {
            string legacyRunId = Guid.NewGuid().ToString("N");
            CreateLegacyDevelopmentDatabase(legacyRunId, legacyVersion);

            InitializeStore();

            AssertProgress(credits: 0, experience: 0, maxStageProgress: 0, loop: 0);
            Assert.That(store.TryGetRun(legacyRunId, out _), Is.False);

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<int>("PRAGMA user_version"),
                    Is.EqualTo(4));
                Assert.That(TableExists(connection, "player_progress"), Is.True);
                Assert.That(
                    connection.ExecuteScalar<long>(
                        "SELECT COUNT(*) FROM pragma_table_info('stage_runs') " +
                        "WHERE name = 'stage_display_order'"),
                    Is.EqualTo(1));
            }
        }

        [Test]
        public void Initialize_VersionThreeDatabaseMigratesToVersionFourWithoutDataLoss()
        {
            string victoryRunId = Guid.NewGuid().ToString("N");
            string firstDefeatRunId = Guid.NewGuid().ToString("N");
            string secondDefeatRunId = Guid.NewGuid().ToString("N");
            CreateVersionThreeDatabase(victoryRunId, firstDefeatRunId, secondDefeatRunId);

            InitializeStore();

            AssertProgress(
                credits: 123,
                experience: 456,
                maxStageProgress: 2,
                loop: 3,
                totalDefeatCount: 2);
            Assert.That(store.GetRecentRuns(10), Has.Count.EqualTo(3));
            Assert.That(store.TryGetRun(victoryRunId, out CompletedStageRunRecord victory), Is.True);
            Assert.That(victory.StageId, Is.EqualTo("stage_01"));
            Assert.That(victory.HeadquartersRemainingHealth, Is.EqualTo(80d));
            Assert.That(victory.HeadquartersMaxHealth, Is.Null);
            Assert.That(store.TryGetRun(firstDefeatRunId, out CompletedStageRunRecord defeat), Is.True);
            Assert.That(defeat.Outcome, Is.EqualTo(StageRunOutcome.Defeat));
            Assert.That(defeat.HeadquartersMaxHealth, Is.Null);

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<int>("PRAGMA user_version"),
                    Is.EqualTo(4));
                Assert.That(ColumnExists(connection, "stage_runs", "headquarters_max_hp"), Is.True);
                Assert.That(
                    ColumnExists(connection, "player_progress", "total_defeat_count"),
                    Is.True);
                Assert.That(
                    connection.ExecuteScalar<long>(
                        "SELECT COUNT(*) FROM stage_runs WHERE headquarters_max_hp IS NULL"),
                    Is.EqualTo(3));
            }
        }

        [Test]
        public void Initialize_NewerDatabaseFailsWithoutDeletingIt()
        {
            using (SQLiteConnection connection = OpenDirectConnection())
            {
                connection.Execute(
                    "CREATE TABLE newer_schema_marker (marker_value INTEGER NOT NULL)");
                connection.Execute(
                    "INSERT INTO newer_schema_marker (marker_value) VALUES (42)");
                connection.Execute("PRAGMA user_version = 5");
            }

            store = new EDataStore(databasePath);
            Assert.Throws<InvalidOperationException>(() => store.Initialize());
            Assert.That(store.State, Is.EqualTo(DataStoreState.Faulted));

            using (SQLiteConnection connection = OpenDirectConnection())
            {
                Assert.That(
                    connection.ExecuteScalar<int>("PRAGMA user_version"),
                    Is.EqualTo(5));
                Assert.That(TableExists(connection, "newer_schema_marker"), Is.True);
                Assert.That(
                    connection.ExecuteScalar<long>(
                        "SELECT marker_value FROM newer_schema_marker LIMIT 1"),
                    Is.EqualTo(42));
            }
        }

        private void InitializeStore()
        {
            store = new EDataStore(databasePath);
            store.Initialize();
        }

        private CompletedStageRunCommitResult Commit(
            int stageDisplayOrder,
            StageRunOutcome outcome,
            long earnedCredits = 0,
            long earnedExperience = 0)
        {
            return store.Commit(CreateSnapshot(
                stageDisplayOrder,
                outcome,
                earnedCredits,
                earnedExperience));
        }

        private CompletedStageRunSnapshot CreateSnapshot(
            int stageDisplayOrder,
            StageRunOutcome outcome,
            long earnedCredits = 0,
            long earnedExperience = 0,
            string runId = null,
            double headquartersRemainingHealth = 100d,
            double headquartersMaxHealth = 1000d)
        {
            snapshotSequence++;
            return new CompletedStageRunSnapshot(
                runId ?? Guid.NewGuid().ToString("N"),
                $"stage_{stageDisplayOrder:00}",
                stageDisplayOrder,
                playDurationMilliseconds: 1000 + snapshotSequence,
                headquartersRemainingHealth: headquartersRemainingHealth,
                headquartersMaxHealth: headquartersMaxHealth,
                defeatedEnemyCount: stageDisplayOrder,
                attackCount: stageDisplayOrder * 2L,
                earnedCredits: earnedCredits,
                earnedExperience: earnedExperience,
                outcome: outcome,
                completedAtUtc: TestEpoch.AddSeconds(snapshotSequence));
        }

        private static CompletedStageRunSnapshot CopyWithHeadquartersMaxHealth(
            CompletedStageRunSnapshot source,
            double headquartersMaxHealth)
        {
            return new CompletedStageRunSnapshot(
                source.RunId,
                source.StageId,
                source.StageDisplayOrder,
                source.PlayDurationMilliseconds,
                source.HeadquartersRemainingHealth,
                headquartersMaxHealth,
                source.DefeatedEnemyCount,
                source.AttackCount,
                source.EarnedCredits,
                source.EarnedExperience,
                source.Outcome,
                source.CompletedAtUtc);
        }

        private void CommitFullLoop()
        {
            for (int stageDisplayOrder = 1; stageDisplayOrder <= 10; stageDisplayOrder++)
            {
                Commit(stageDisplayOrder, StageRunOutcome.Victory);
            }
        }

        private void AssertProgress(
            long credits,
            long experience,
            int maxStageProgress,
            long loop,
            long totalDefeatCount = 0)
        {
            PlayerProgressSnapshot progress = store.GetPlayerProgress();
            Assert.That(progress.TotalCredits, Is.EqualTo(credits));
            Assert.That(progress.TotalExperience, Is.EqualTo(experience));
            Assert.That(progress.MaxStageProgress, Is.EqualTo(maxStageProgress));
            Assert.That(progress.Loop, Is.EqualTo(loop));
            Assert.That(progress.TotalDefeatCount, Is.EqualTo(totalDefeatCount));
        }

        private SQLiteConnection OpenDirectConnection()
        {
            return new SQLiteConnection(
                databasePath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        }

        private static bool TableExists(SQLiteConnection connection, string tableName)
        {
            return connection.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = ?",
                tableName) == 1;
        }

        private static bool ColumnExists(
            SQLiteConnection connection,
            string tableName,
            string columnName)
        {
            return connection.ExecuteScalar<long>(
                $"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name = ?",
                columnName) == 1;
        }

        private void CreateLegacyDevelopmentDatabase(string legacyRunId, int legacyVersion)
        {
            using (SQLiteConnection connection = OpenDirectConnection())
            {
                connection.Execute(
                    "CREATE TABLE stage_runs (" +
                    "completion_sequence INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "run_id TEXT NOT NULL UNIQUE, " +
                    "payload_hash TEXT NOT NULL, " +
                    "stage_id TEXT NOT NULL, " +
                    "play_duration_ms INTEGER NOT NULL, " +
                    "headquarters_remaining_hp REAL NOT NULL, " +
                    "defeated_enemy_count INTEGER NOT NULL, " +
                    "attack_count INTEGER NOT NULL, " +
                    "earned_credits INTEGER NOT NULL, " +
                    "earned_experience INTEGER NOT NULL, " +
                    "outcome INTEGER NOT NULL, " +
                    "completed_at_utc_ms INTEGER NOT NULL)");
                connection.Execute(
                    "INSERT INTO stage_runs " +
                    "(run_id, payload_hash, stage_id, play_duration_ms, " +
                    "headquarters_remaining_hp, defeated_enemy_count, attack_count, " +
                    "earned_credits, earned_experience, outcome, completed_at_utc_ms) " +
                    "VALUES (?, 'legacy-hash', 'stage_01', 1000, 100, 1, 1, 1, 1, 1, 0)",
                    legacyRunId);
                connection.Execute($"PRAGMA user_version = {legacyVersion}");
            }
        }

        private void CreateVersionThreeDatabase(
            string victoryRunId,
            string firstDefeatRunId,
            string secondDefeatRunId)
        {
            using (SQLiteConnection connection = OpenDirectConnection())
            {
                connection.Execute(
                    "CREATE TABLE stage_runs (" +
                    "completion_sequence INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "run_id TEXT NOT NULL UNIQUE, " +
                    "payload_hash TEXT NOT NULL, " +
                    "stage_id TEXT NOT NULL, " +
                    "stage_display_order INTEGER NOT NULL, " +
                    "play_duration_ms INTEGER NOT NULL, " +
                    "headquarters_remaining_hp REAL NOT NULL, " +
                    "defeated_enemy_count INTEGER NOT NULL, " +
                    "attack_count INTEGER NOT NULL, " +
                    "earned_credits INTEGER NOT NULL, " +
                    "earned_experience INTEGER NOT NULL, " +
                    "outcome INTEGER NOT NULL, " +
                    "completed_at_utc_ms INTEGER NOT NULL)");
                connection.Execute(
                    "CREATE TABLE player_progress (" +
                    "player_id INTEGER PRIMARY KEY, " +
                    "total_credits INTEGER NOT NULL, " +
                    "total_experience INTEGER NOT NULL, " +
                    "max_stage_progress INTEGER NOT NULL, " +
                    "loop INTEGER NOT NULL, " +
                    "updated_at_utc_ms INTEGER NOT NULL)");

                InsertVersionThreeRun(
                    connection,
                    victoryRunId,
                    "victory-hash",
                    "stage_01",
                    stageDisplayOrder: 1,
                    headquartersRemainingHealth: 80d,
                    outcome: StageRunOutcome.Victory);
                InsertVersionThreeRun(
                    connection,
                    firstDefeatRunId,
                    "first-defeat-hash",
                    "stage_02",
                    stageDisplayOrder: 2,
                    headquartersRemainingHealth: 0d,
                    outcome: StageRunOutcome.Defeat);
                InsertVersionThreeRun(
                    connection,
                    secondDefeatRunId,
                    "second-defeat-hash",
                    "stage_03",
                    stageDisplayOrder: 3,
                    headquartersRemainingHealth: 0d,
                    outcome: StageRunOutcome.Defeat);

                connection.Execute(
                    "INSERT INTO player_progress " +
                    "(player_id, total_credits, total_experience, max_stage_progress, " +
                    "loop, updated_at_utc_ms) VALUES (1, 123, 456, 2, 3, ?)",
                    TestEpoch.ToUnixTimeMilliseconds());
                connection.Execute("PRAGMA user_version = 3");
            }
        }

        private static void InsertVersionThreeRun(
            SQLiteConnection connection,
            string runId,
            string payloadHash,
            string stageId,
            int stageDisplayOrder,
            double headquartersRemainingHealth,
            StageRunOutcome outcome)
        {
            connection.Execute(
                "INSERT INTO stage_runs " +
                "(run_id, payload_hash, stage_id, stage_display_order, play_duration_ms, " +
                "headquarters_remaining_hp, defeated_enemy_count, attack_count, " +
                "earned_credits, earned_experience, outcome, completed_at_utc_ms) " +
                "VALUES (?, ?, ?, ?, 65432, ?, 3, 7, 0, 0, ?, ?)",
                runId,
                payloadHash,
                stageId,
                stageDisplayOrder,
                headquartersRemainingHealth,
                (int)outcome,
                TestEpoch.AddMinutes(stageDisplayOrder).ToUnixTimeMilliseconds());
        }
    }
}
