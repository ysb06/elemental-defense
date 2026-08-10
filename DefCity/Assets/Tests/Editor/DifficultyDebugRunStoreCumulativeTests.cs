using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Flow.Settings;
using ElementalDef.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ElementalDef.Tests.Editor
{
    public sealed class DifficultyDebugRunStoreCumulativeTests
    {
        private const string StageThreeAssetPath =
            "Assets/Settings/ElementalDef/Wave/ElementalDef Stage 03 Wave Bundle.asset";
        private const float FloatTolerance = 0.000001f;

        private string testDirectory;
        private EDataStore persistentStore;
        private DifficultyDebugRunStore debugStore;

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(
                Application.temporaryCachePath,
                "DifficultyDebugRunStoreCumulativeTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDirectory);

            persistentStore = new EDataStore(
                Path.Combine(testDirectory, "elementaldef-test.sqlite3"));
            persistentStore.Initialize();
            debugStore = new DifficultyDebugRunStore(persistentStore);
        }

        [TearDown]
        public void TearDown()
        {
            debugStore?.Dispose();
            debugStore = null;
            persistentStore?.Dispose();
            persistentStore = null;

            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [Test]
        public void Inject_AccumulatesNewestFirstAndEvictsOnlyTheOldestAtEleven()
        {
            for (int marker = 0; marker < DifficultyDebugRunStore.MaxInjectedRunCount;
                 marker++)
            {
                Inject(marker);
                Assert.That(debugStore.InjectedRunCount, Is.EqualTo(marker + 1));
                Assert.That(
                    debugStore.InjectedRun.PlayDurationMilliseconds,
                    Is.EqualTo(marker * 1000L));
            }

            Inject(DifficultyDebugRunStore.MaxInjectedRunCount);

            Assert.That(
                debugStore.InjectedRunCount,
                Is.EqualTo(DifficultyDebugRunStore.MaxInjectedRunCount));
            Assert.That(debugStore.HasInjectedRun, Is.True);
            CollectionAssert.AreEqual(
                Enumerable.Range(1, DifficultyDebugRunStore.MaxInjectedRunCount)
                    .Reverse()
                    .Select(marker => marker * 1000L),
                debugStore.InjectedRuns.Select(run => run.PlayDurationMilliseconds));
            Assert.That(
                debugStore.InjectedRun,
                Is.SameAs(debugStore.InjectedRuns[0]));
        }

        [Test]
        public void GetRecentRuns_MergesByCompletionTimeAndAppliesExactTieRules()
        {
            Inject(marker: 7);
            CompletedStageRunRecord injected = debugStore.InjectedRun;
            CompletedStageRunRecord firstStored = CommitStoredRun(
                injected.CompletedAtUtc,
                marker: 1);
            CompletedStageRunRecord secondStored = CommitStoredRun(
                injected.CompletedAtUtc,
                marker: 2);

            IReadOnlyList<CompletedStageRunRecord> recent = debugStore.GetRecentRuns(3);

            Assert.That(recent, Has.Count.EqualTo(3));
            Assert.That(recent[0], Is.SameAs(injected),
                "An injected run must win an exact completion-time tie.");
            Assert.That(recent[1].RunId, Is.EqualTo(secondStored.RunId),
                "Stored-run ties must use descending completion sequence.");
            Assert.That(recent[2].RunId, Is.EqualTo(firstStored.RunId));
            Assert.That(debugStore.GetRecentRuns(1)[0], Is.SameAs(injected));
            Assert.Throws<ArgumentOutOfRangeException>(() => debugStore.GetRecentRuns(0));
        }

        [Test]
        public void GetRecentRuns_NewerStoredRunDisplacesOnlyOldestInjectedCandidate()
        {
            for (int marker = 0; marker < DifficultyDebugRunStore.MaxInjectedRunCount;
                 marker++)
            {
                Inject(marker);
            }

            string oldestInjectedRunId = debugStore.InjectedRuns[^1].RunId;
            DateTimeOffset newerCompletionTime =
                debugStore.InjectedRun.CompletedAtUtc.AddMinutes(1d);
            CompletedStageRunRecord stored = CommitStoredRun(
                newerCompletionTime,
                marker: 99);

            IReadOnlyList<CompletedStageRunRecord> recent =
                debugStore.GetRecentRuns(StageDifficultyService.RecentRunLimit);

            Assert.That(recent, Has.Count.EqualTo(StageDifficultyService.RecentRunLimit));
            Assert.That(recent[0].RunId, Is.EqualTo(stored.RunId));
            Assert.That(
                recent.Count(run => run.StageId == "debug_difficulty"),
                Is.EqualTo(9));
            Assert.That(recent.Any(run => run.RunId == oldestInjectedRunId), Is.False);
            Assert.That(debugStore.InjectedRunCount, Is.EqualTo(10),
                "Merging must not delete the in-memory injection history.");
        }

        [Test]
        public void Inject_InvalidInputLeavesTheAccumulatedListUntouched()
        {
            Inject(marker: 1);
            Inject(marker: 2);
            string[] beforeRunIds = debugStore.InjectedRuns
                .Select(run => run.RunId)
                .ToArray();

            Assert.Throws<ArgumentOutOfRangeException>(() => debugStore.Inject(
                new DifficultyDebugRunInput(
                    StageRunOutcome.Victory,
                    playDurationSeconds: -1d,
                    headquartersRemainingHealth: 0d,
                    headquartersMaxHealth: 100d,
                    defeatedEnemyCount: 0L)));

            CollectionAssert.AreEqual(
                beforeRunIds,
                debugStore.InjectedRuns.Select(run => run.RunId));
        }

        [Test]
        public void InjectAndClear_DoNotChangePersistentRunsOrPlayerProgress()
        {
            CommitStoredRun(
                DateTimeOffset.UtcNow.AddMinutes(-1d),
                marker: 1,
                outcome: StageRunOutcome.Defeat);
            PlayerProgressSnapshot beforeProgress = persistentStore.GetPlayerProgress();
            string[] beforeRunIds = persistentStore.GetRecentRuns(10)
                .Select(run => run.RunId)
                .ToArray();
            int committedEventCount = 0;
            debugStore.RunCommitted += _ => committedEventCount++;

            for (int marker = 0; marker < 3; marker++)
            {
                Inject(marker);
            }

            debugStore.ClearInjectedRuns();
            Inject(marker: 4);
            debugStore.ClearInjectedRun();

            PlayerProgressSnapshot afterProgress = persistentStore.GetPlayerProgress();
            CollectionAssert.AreEqual(
                beforeRunIds,
                persistentStore.GetRecentRuns(10).Select(run => run.RunId));
            AssertProgressEqual(beforeProgress, afterProgress);
            Assert.That(debugStore.HasInjectedRun, Is.False);
            Assert.That(debugStore.InjectedRunCount, Is.Zero);
            Assert.That(committedEventCount, Is.Zero);
        }

        [Test]
        public void RepeatedIdenticalResults_AccumulateButCanKeepTheSameDifficulty()
        {
            var difficultyService = new StageDifficultyService(debugStore);
            var input = new DifficultyDebugRunInput(
                StageRunOutcome.Victory,
                playDurationSeconds: 120d,
                headquartersRemainingHealth: 50d,
                headquartersMaxHealth: 100d,
                defeatedEnemyCount: 20L);

            debugStore.Inject(input);
            PerformanceStageDifficultySnapshot first =
                difficultyService.GetPerformanceDifficulty();
            for (int index = 1; index < DifficultyDebugRunStore.MaxInjectedRunCount;
                 index++)
            {
                debugStore.Inject(input);
            }

            PerformanceStageDifficultySnapshot tenth =
                difficultyService.GetPerformanceDifficulty();

            Assert.That(first.ConsideredRunCount, Is.EqualTo(1));
            Assert.That(tenth.ConsideredRunCount, Is.EqualTo(10));
            Assert.That(tenth.VictoryCount, Is.EqualTo(10));
            Assert.That(
                tenth.RawDifficultyMultiplier,
                Is.EqualTo(first.RawDifficultyMultiplier).Within(FloatTolerance));
            Assert.That(
                tenth.DifficultyMultiplier,
                Is.EqualTo(first.DifficultyMultiplier).Within(FloatTolerance));
        }

        [Test]
        public void NewInjection_LeavesPreparedContextFrozenAndChangesOnlyLaterPreview()
        {
            WaveBundle stage = AssetDatabase.LoadAssetAtPath<WaveBundle>(
                StageThreeAssetPath);
            Assert.That(stage, Is.Not.Null);

            var launchService = new StageLaunchService();
            ConfigureService(
                launchService,
                "ConfigurePlayerProgressService",
                new PlayerProgressService(persistentStore));
            ConfigureService(
                launchService,
                "ConfigureDifficultyService",
                new StageDifficultyService(debugStore));

            debugStore.Inject(new DifficultyDebugRunInput(
                StageRunOutcome.Defeat,
                playDurationSeconds: 0d,
                headquartersRemainingHealth: 0d,
                headquartersMaxHealth: 100d,
                defeatedEnemyCount: 0L));
            StageRunContext prepared = launchService.Prepare(stage);
            float preparedPerformanceDifficulty =
                prepared.PerformanceDifficultyMultiplier;

            debugStore.Inject(new DifficultyDebugRunInput(
                StageRunOutcome.Victory,
                playDurationSeconds: 0d,
                headquartersRemainingHealth: 100d,
                headquartersMaxHealth: 100d,
                defeatedEnemyCount: 100L));
            StageLaunchPreview nextPreview = launchService.CreatePreview(stage);

            Assert.That(launchService.Current, Is.SameAs(prepared));
            Assert.That(
                launchService.Current.PerformanceDifficultyMultiplier,
                Is.EqualTo(preparedPerformanceDifficulty).Within(FloatTolerance));
            Assert.That(
                nextPreview.PerformanceDifficultyMultiplier,
                Is.GreaterThan(preparedPerformanceDifficulty + FloatTolerance));
        }

        private void Inject(int marker)
        {
            debugStore.Inject(new DifficultyDebugRunInput(
                StageRunOutcome.Victory,
                playDurationSeconds: marker,
                headquartersRemainingHealth: 50d,
                headquartersMaxHealth: 100d,
                defeatedEnemyCount: marker));
        }

        private CompletedStageRunRecord CommitStoredRun(
            DateTimeOffset completedAtUtc,
            int marker,
            StageRunOutcome outcome = StageRunOutcome.Victory)
        {
            var snapshot = new CompletedStageRunSnapshot(
                Guid.NewGuid().ToString("N"),
                "stage_01",
                1,
                marker * 1000L,
                50d,
                100d,
                marker,
                marker,
                0L,
                0L,
                outcome,
                completedAtUtc);
            CompletedStageRunCommitResult result = persistentStore.Commit(snapshot);
            Assert.That(
                result.Status,
                Is.EqualTo(CompletedStageRunCommitStatus.Committed));
            return result.Record;
        }

        private static void ConfigureService(
            StageLaunchService target,
            string methodName,
            object service)
        {
            MethodInfo method = typeof(StageLaunchService).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(target, new[] { service });
        }

        private static void AssertProgressEqual(
            PlayerProgressSnapshot expected,
            PlayerProgressSnapshot actual)
        {
            Assert.That(actual.TotalCredits, Is.EqualTo(expected.TotalCredits));
            Assert.That(actual.TotalExperience, Is.EqualTo(expected.TotalExperience));
            Assert.That(actual.MaxStageProgress, Is.EqualTo(expected.MaxStageProgress));
            Assert.That(actual.Loop, Is.EqualTo(expected.Loop));
            Assert.That(actual.TotalDefeatCount, Is.EqualTo(expected.TotalDefeatCount));
            Assert.That(actual.UpdatedAtUtc, Is.EqualTo(expected.UpdatedAtUtc));
        }
    }
}
