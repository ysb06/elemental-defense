using System;
using System.Collections.Generic;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Runtime
{
    public sealed class StageLaunchPreview
    {
        public WaveBundle Stage { get; }
        public long Loop { get; }
        public long AbsoluteStageNumber { get; }
        public int EffectiveMapSeed { get; }
        public float StageEnemyDifficultyMultiplier { get; }
        public float PerformanceDifficultyMultiplier { get; }
        public float DifficultyMultiplier { get; }
        public long VictoryCreditReward { get; }
        public long VictoryExperienceReward { get; }

        internal StageLaunchPreview(
            WaveBundle stage,
            long loop,
            long absoluteStageNumber,
            int effectiveMapSeed,
            float stageEnemyDifficultyMultiplier,
            float performanceDifficultyMultiplier,
            float difficultyMultiplier,
            long victoryCreditReward,
            long victoryExperienceReward)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Loop = loop;
            AbsoluteStageNumber = absoluteStageNumber;
            EffectiveMapSeed = effectiveMapSeed;
            StageEnemyDifficultyMultiplier = stageEnemyDifficultyMultiplier;
            PerformanceDifficultyMultiplier = performanceDifficultyMultiplier;
            DifficultyMultiplier = difficultyMultiplier;
            VictoryCreditReward = victoryCreditReward;
            VictoryExperienceReward = victoryExperienceReward;
        }
    }

    public sealed class StageLaunchService
    {
        private StageDifficultyService difficultyService;
        private PlayerProgressService playerProgressService;

        public StageRunContext Current { get; private set; }
        public bool HasCurrent => Current != null;

        internal void ConfigureDifficultyService(
            StageDifficultyService configuredDifficultyService)
        {
            difficultyService = configuredDifficultyService ??
                throw new ArgumentNullException(nameof(configuredDifficultyService));
        }

        internal void ConfigurePlayerProgressService(
            PlayerProgressService configuredPlayerProgressService)
        {
            playerProgressService = configuredPlayerProgressService ??
                throw new ArgumentNullException(nameof(configuredPlayerProgressService));
        }

        public StageRunContext Prepare(WaveBundle stage)
        {
            StageLaunchPreview preview = CreatePreview(stage);
            StageRunContext context = StageRunContext.Create(
                stage,
                preview.PerformanceDifficultyMultiplier,
                preview.Loop,
                preview.EffectiveMapSeed);
            Current = context;
            return context;
        }

        public StageLaunchPreview CreatePreview(WaveBundle stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            stage.ValidateOrThrow();
            if (stage.DisplayOrder > StageCatalog.RequiredStageCount)
            {
                throw new InvalidOperationException(
                    $"Cannot preview stage '{stage.StageId}' with display order " +
                    $"{stage.DisplayOrder}. Display order must be between 1 and " +
                    $"{StageCatalog.RequiredStageCount}.");
            }

            if (playerProgressService == null)
            {
                throw new InvalidOperationException(
                    "Player progress must be configured before previewing or preparing a stage.");
            }

            float performanceDifficultyMultiplier = difficultyService?
                .GetPerformanceDifficulty()
                .DifficultyMultiplier ?? 1f;
            long loop = playerProgressService.GetProgress().Loop;
            long absoluteStageNumber = StageLaunchValueCalculator
                .CalculateAbsoluteStageNumber(stage, loop);
            int effectiveMapSeed = StageLaunchValueCalculator
                .CalculateEffectiveMapSeed(stage, loop);
            float difficultyMultiplier = StageDifficultyMultiplierCalculator.Calculate(
                stage.EnemyDifficultyMultiplier,
                performanceDifficultyMultiplier,
                stage.StageId);
            long victoryCreditReward = StageRewardCalculator.Calculate(
                stage.BaseCreditReward,
                difficultyMultiplier);
            long victoryExperienceReward = StageRewardCalculator.Calculate(
                stage.BaseExperienceReward,
                difficultyMultiplier);

            return new StageLaunchPreview(
                stage,
                loop,
                absoluteStageNumber,
                effectiveMapSeed,
                stage.EnemyDifficultyMultiplier,
                performanceDifficultyMultiplier,
                difficultyMultiplier,
                victoryCreditReward,
                victoryExperienceReward);
        }
    }

    internal static class StageLaunchValueCalculator
    {
        public static long CalculateAbsoluteStageNumber(
            WaveBundle stage,
            long loop)
        {
            RequireStageAndLoop(stage, loop);

            try
            {
                return checked(
                    (loop * (long)StageCatalog.RequiredStageCount) +
                    stage.DisplayOrder);
            }
            catch (OverflowException exception)
            {
                throw new InvalidOperationException(
                    $"Cannot preview stage '{stage.StageId}': Loop {loop} and display " +
                    $"order {stage.DisplayOrder} produce an overall stage number outside " +
                    "the Int64 range.",
                    exception);
            }
        }

        public static int CalculateEffectiveMapSeed(
            WaveBundle stage,
            long loop)
        {
            RequireStageAndLoop(stage, loop);

            long effectiveMapSeed;
            try
            {
                long loopOffset = checked(
                    loop * (long)StageCatalog.RequiredStageCount);
                effectiveMapSeed = checked(stage.MapSeed + loopOffset);
            }
            catch (OverflowException exception)
            {
                throw CreateMapSeedOutOfRangeException(stage, loop, exception);
            }

            if (effectiveMapSeed < int.MinValue || effectiveMapSeed > int.MaxValue)
            {
                throw CreateMapSeedOutOfRangeException(stage, loop);
            }

            return (int)effectiveMapSeed;
        }

        private static void RequireStageAndLoop(WaveBundle stage, long loop)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            if (loop < 0L)
            {
                throw new InvalidOperationException(
                    $"Cannot preview or prepare stage '{stage.StageId}' with a negative Loop " +
                    $"value ({loop}).");
            }
        }

        private static InvalidOperationException CreateMapSeedOutOfRangeException(
            WaveBundle stage,
            long loop,
            Exception innerException = null)
        {
            string message =
                $"Cannot preview or prepare stage '{stage.StageId}': base map seed " +
                $"{stage.MapSeed} + Loop {loop} * " +
                $"{StageCatalog.RequiredStageCount} is outside the Int32 map-seed range.";
            return innerException == null
                ? new InvalidOperationException(message)
                : new InvalidOperationException(message, innerException);
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
        public const double AverageClearTimeCoefficient = -0.00036d;
        public const double AverageRemainingHealthCoefficient = 0.0124d;
        public const double AverageDefeatedEnemyCoefficient = 0.0184d;
        public const double VictoryRateCoefficient = 0.62d;
        public const double MinimumDifficultyMultiplier = 1d;
        public const double MaximumDifficultyMultiplier = 2d;

        private readonly IElementalDefRunStore runStore;

        public StageDifficultyService(IElementalDefRunStore runStore)
        {
            this.runStore = runStore ?? throw new ArgumentNullException(nameof(runStore));
        }

        public StageDifficultySnapshot GetCurrentDifficulty()
        {
            IReadOnlyList<CompletedStageRunRecord> recentRuns = runStore.GetRecentRuns(RecentRunLimit);
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
