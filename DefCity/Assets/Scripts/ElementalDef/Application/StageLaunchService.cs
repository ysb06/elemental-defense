using System;
using System.Collections.Generic;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Runtime
{
    public sealed class StageLaunchService
    {
        private StageDifficultyService difficultyService;

        public StageRunContext Current { get; private set; }
        public bool HasCurrent => Current != null;

        internal void ConfigureDifficultyService(
            StageDifficultyService configuredDifficultyService)
        {
            difficultyService = configuredDifficultyService ??
                throw new ArgumentNullException(nameof(configuredDifficultyService));
        }

        public StageRunContext Prepare(WaveBundle stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            float difficultyMultiplier = difficultyService?
                .GetPerformanceDifficulty()
                .DifficultyMultiplier ?? 1f;
            StageRunContext context = StageRunContext.Create(
                stage,
                difficultyMultiplier);
            Current = context;
            return context;
        }
    }

    public sealed class StageDifficultySnapshot
    {
        public int ConsideredRunCount { get; }
        public int VictoryCount { get; }
        public float VictoryRate { get; }
        public float DifficultyMultiplier { get; }

        internal StageDifficultySnapshot(
            int consideredRunCount,
            int victoryCount)
        {
            if (consideredRunCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(consideredRunCount));
            }

            if (victoryCount < 0 || victoryCount > consideredRunCount)
            {
                throw new ArgumentOutOfRangeException(nameof(victoryCount));
            }

            ConsideredRunCount = consideredRunCount;
            VictoryCount = victoryCount;
            VictoryRate = consideredRunCount == 0
                ? 0f
                : victoryCount / (float)consideredRunCount;
            DifficultyMultiplier = 1f + VictoryRate;
        }
    }

    public sealed class StageDifficultyService
    {
        public const int RecentRunLimit = 10;
        public const double AverageClearTimeCoefficient = -0.0005d;
        public const double AverageRemainingHealthCoefficient = 0.002d;
        public const double AverageDefeatedEnemyCoefficient = 0.001d;
        public const double VictoryRateCoefficient = 0.5d;
        public const double MinimumDifficultyMultiplier = 1d;
        public const double MaximumDifficultyMultiplier = 2d;

        private readonly IElementalDefRunStore runStore;

        public StageDifficultyService(IElementalDefRunStore runStore)
        {
            this.runStore = runStore ?? throw new ArgumentNullException(nameof(runStore));
        }

        public StageDifficultySnapshot GetCurrentDifficulty()
        {
            IReadOnlyList<CompletedStageRunRecord> recentRuns =
                runStore.GetRecentRuns(RecentRunLimit);
            int victoryCount = 0;
            foreach (CompletedStageRunRecord run in recentRuns)
            {
                if (run.Outcome == StageRunOutcome.Victory)
                {
                    victoryCount++;
                }
            }

            return new StageDifficultySnapshot(recentRuns.Count, victoryCount);
        }

        public PerformanceStageDifficultySnapshot GetPerformanceDifficulty()
        {
            IReadOnlyList<CompletedStageRunRecord> recentRuns =
                runStore.GetRecentRuns(RecentRunLimit);
            int victoryCount = 0;
            double totalClearTimeSeconds = 0d;
            double totalRemainingHealth = 0d;
            double totalDefeatedEnemyCount = 0d;

            foreach (CompletedStageRunRecord run in recentRuns)
            {
                totalRemainingHealth += run.HeadquartersRemainingHealth;
                totalDefeatedEnemyCount += run.DefeatedEnemyCount;

                if (run.Outcome == StageRunOutcome.Victory)
                {
                    victoryCount++;
                    totalClearTimeSeconds +=
                        run.PlayDurationMilliseconds / 1000d;
                }
            }

            double averageClearTimeSeconds = victoryCount == 0
                ? 0d
                : totalClearTimeSeconds / victoryCount;
            double averageRemainingHealth = recentRuns.Count == 0
                ? 0d
                : totalRemainingHealth / recentRuns.Count;
            double averageDefeatedEnemyCount = recentRuns.Count == 0
                ? 0d
                : totalDefeatedEnemyCount / recentRuns.Count;
            double victoryRate = recentRuns.Count == 0
                ? 0d
                : victoryCount / (double)recentRuns.Count;

            double rawDifficultyMultiplier =
                1d +
                (AverageClearTimeCoefficient * averageClearTimeSeconds) +
                (AverageRemainingHealthCoefficient * averageRemainingHealth) +
                (AverageDefeatedEnemyCoefficient * averageDefeatedEnemyCount) +
                (VictoryRateCoefficient * victoryRate);
            double difficultyMultiplier = Math.Max(
                MinimumDifficultyMultiplier,
                Math.Min(MaximumDifficultyMultiplier, rawDifficultyMultiplier));

            return new PerformanceStageDifficultySnapshot(
                recentRuns.Count,
                victoryCount,
                averageClearTimeSeconds,
                averageRemainingHealth,
                averageDefeatedEnemyCount,
                victoryRate,
                rawDifficultyMultiplier,
                (float)difficultyMultiplier);
        }
    }

    public sealed class PerformanceStageDifficultySnapshot
    {
        public int ConsideredRunCount { get; }
        public int VictoryCount { get; }
        public double AverageClearTimeSeconds { get; }
        public double AverageRemainingHeadquartersHealth { get; }
        public double AverageDefeatedEnemyCount { get; }
        public double VictoryRate { get; }
        public double RawDifficultyMultiplier { get; }
        public float DifficultyMultiplier { get; }

        internal PerformanceStageDifficultySnapshot(
            int consideredRunCount,
            int victoryCount,
            double averageClearTimeSeconds,
            double averageRemainingHeadquartersHealth,
            double averageDefeatedEnemyCount,
            double victoryRate,
            double rawDifficultyMultiplier,
            float difficultyMultiplier)
        {
            if (consideredRunCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(consideredRunCount));
            }

            if (victoryCount < 0 || victoryCount > consideredRunCount)
            {
                throw new ArgumentOutOfRangeException(nameof(victoryCount));
            }

            EnsureFiniteNonNegative(averageClearTimeSeconds, nameof(averageClearTimeSeconds));
            EnsureFiniteNonNegative(
                averageRemainingHeadquartersHealth,
                nameof(averageRemainingHeadquartersHealth));
            EnsureFiniteNonNegative(
                averageDefeatedEnemyCount,
                nameof(averageDefeatedEnemyCount));
            EnsureFiniteNonNegative(victoryRate, nameof(victoryRate));
            if (double.IsNaN(rawDifficultyMultiplier) ||
                double.IsInfinity(rawDifficultyMultiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(rawDifficultyMultiplier));
            }

            if (float.IsNaN(difficultyMultiplier) ||
                float.IsInfinity(difficultyMultiplier) ||
                difficultyMultiplier < StageDifficultyService.MinimumDifficultyMultiplier ||
                difficultyMultiplier > StageDifficultyService.MaximumDifficultyMultiplier)
            {
                throw new ArgumentOutOfRangeException(nameof(difficultyMultiplier));
            }

            ConsideredRunCount = consideredRunCount;
            VictoryCount = victoryCount;
            AverageClearTimeSeconds = averageClearTimeSeconds;
            AverageRemainingHeadquartersHealth = averageRemainingHeadquartersHealth;
            AverageDefeatedEnemyCount = averageDefeatedEnemyCount;
            VictoryRate = victoryRate;
            RawDifficultyMultiplier = rawDifficultyMultiplier;
            DifficultyMultiplier = difficultyMultiplier;
        }

        private static void EnsureFiniteNonNegative(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
