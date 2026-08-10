using System;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Gameplay.Flow
{
    public sealed class StageRunContext
    {
        public string RunId { get; }
        public WaveBundle SelectedStage { get; }
        public string StageId { get; }
        public int DisplayOrder { get; }
        public int BaseMapSeed { get; }
        public long LoopAtLaunch { get; }
        public int MapSeed { get; }
        public int BaseCreditReward { get; }
        public int BaseExperienceReward { get; }
        public int StartingTowerEnergy { get; }
        public long StartedAtUtcMilliseconds { get; }
        public float StageEnemyDifficultyMultiplier { get; }
        public float PerformanceDifficultyMultiplier { get; }
        public float DifficultyMultiplier { get; }
        public float CreditRewardMultiplier { get; }
        public float ExperienceRewardMultiplier { get; }
        public float CharacterStatMultiplier { get; }
        public int CharacterLevel { get; }

        private StageRunContext(
            string runId,
            WaveBundle selectedStage,
            string stageId,
            int displayOrder,
            int baseMapSeed,
            long loopAtLaunch,
            int mapSeed,
            int baseCreditReward,
            int baseExperienceReward,
            int startingTowerEnergy,
            long startedAtUtcMilliseconds,
            float stageEnemyDifficultyMultiplier,
            float performanceDifficultyMultiplier,
            float effectiveDifficultyMultiplier,
            float creditRewardMultiplier,
            float experienceRewardMultiplier,
            float characterStatMultiplier,
            int characterLevel)
        {
            RunId = runId;
            SelectedStage = selectedStage;
            StageId = stageId;
            DisplayOrder = displayOrder;
            BaseMapSeed = baseMapSeed;
            LoopAtLaunch = loopAtLaunch;
            MapSeed = mapSeed;
            BaseCreditReward = baseCreditReward;
            BaseExperienceReward = baseExperienceReward;
            StartingTowerEnergy = startingTowerEnergy;
            StartedAtUtcMilliseconds = startedAtUtcMilliseconds;
            StageEnemyDifficultyMultiplier = stageEnemyDifficultyMultiplier;
            PerformanceDifficultyMultiplier = performanceDifficultyMultiplier;
            DifficultyMultiplier = effectiveDifficultyMultiplier;
            CreditRewardMultiplier = creditRewardMultiplier;
            ExperienceRewardMultiplier = experienceRewardMultiplier;
            CharacterStatMultiplier = characterStatMultiplier;
            CharacterLevel = characterLevel;
        }

        public static StageRunContext Create(WaveBundle waveBundle)
        {
            return Create(waveBundle, 1f);
        }

        public static StageRunContext Create(
            WaveBundle waveBundle,
            float performanceDifficultyMultiplier)
        {
            int baseMapSeed = waveBundle == null ? 0 : waveBundle.MapSeed;
            return Create(
                waveBundle,
                performanceDifficultyMultiplier,
                0L,
                baseMapSeed);
        }

        internal static StageRunContext Create(
            WaveBundle waveBundle,
            float performanceDifficultyMultiplier,
            long loopAtLaunch,
            int effectiveMapSeed)
        {
            if (waveBundle == null)
            {
                throw new ArgumentNullException(nameof(waveBundle));
            }

            waveBundle.ValidateOrThrow();
            if (float.IsNaN(performanceDifficultyMultiplier) ||
                float.IsInfinity(performanceDifficultyMultiplier) ||
                performanceDifficultyMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(performanceDifficultyMultiplier),
                    performanceDifficultyMultiplier,
                    "A performance difficulty multiplier must be finite and at least 1.");
            }

            if (loopAtLaunch < 0L)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(loopAtLaunch),
                    loopAtLaunch,
                    "Loop at launch cannot be negative.");
            }

            float stageEnemyDifficultyMultiplier = waveBundle.EnemyDifficultyMultiplier;
            float effectiveDifficultyMultiplier = StageDifficultyMultiplierCalculator.Calculate(
                stageEnemyDifficultyMultiplier,
                performanceDifficultyMultiplier,
                waveBundle.StageId);

            return new StageRunContext(
                Guid.NewGuid().ToString("N"),
                waveBundle,
                waveBundle.StageId,
                waveBundle.DisplayOrder,
                waveBundle.MapSeed,
                loopAtLaunch,
                effectiveMapSeed,
                waveBundle.BaseCreditReward,
                waveBundle.BaseExperienceReward,
                waveBundle.StartingTowerEnergy,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                stageEnemyDifficultyMultiplier,
                performanceDifficultyMultiplier,
                effectiveDifficultyMultiplier,
                effectiveDifficultyMultiplier,
                effectiveDifficultyMultiplier,
                1f,
                1);
        }
    }

    internal static class StageDifficultyMultiplierCalculator
    {
        public static float Calculate(
            float stageDifficultyMultiplier,
            float performanceDifficultyMultiplier,
            string stageId)
        {
            if (float.IsNaN(stageDifficultyMultiplier) ||
                float.IsInfinity(stageDifficultyMultiplier) ||
                stageDifficultyMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stageDifficultyMultiplier),
                    stageDifficultyMultiplier,
                    "A stage difficulty multiplier must be finite and at least 1.");
            }

            if (float.IsNaN(performanceDifficultyMultiplier) ||
                float.IsInfinity(performanceDifficultyMultiplier) ||
                performanceDifficultyMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(performanceDifficultyMultiplier),
                    performanceDifficultyMultiplier,
                    "A performance difficulty multiplier must be finite and at least 1.");
            }

            float effectiveDifficultyMultiplier =
                stageDifficultyMultiplier * performanceDifficultyMultiplier;
            if (float.IsNaN(effectiveDifficultyMultiplier) ||
                float.IsInfinity(effectiveDifficultyMultiplier) ||
                effectiveDifficultyMultiplier < 1f)
            {
                throw new InvalidOperationException(
                    $"Stage '{stageId}' produced an invalid effective enemy difficulty " +
                    $"multiplier from stage {stageDifficultyMultiplier} and performance " +
                    $"{performanceDifficultyMultiplier}.");
            }

            return effectiveDifficultyMultiplier;
        }
    }

    public static class StageRewardCalculator
    {
        private const double ExclusiveLongUpperBound = 9223372036854775808d;

        public static long Calculate(int baseReward, float rewardMultiplier)
        {
            if (baseReward < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseReward),
                    baseReward,
                    "A base reward cannot be negative.");
            }

            if (float.IsNaN(rewardMultiplier) ||
                float.IsInfinity(rewardMultiplier) ||
                rewardMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rewardMultiplier),
                    rewardMultiplier,
                    "A reward multiplier must be finite and non-negative.");
            }

            double scaledReward = baseReward * (double)rewardMultiplier;
            double roundedReward = Math.Round(
                scaledReward,
                MidpointRounding.AwayFromZero);
            if (roundedReward >= ExclusiveLongUpperBound)
            {
                throw new OverflowException(
                    $"The calculated reward {scaledReward} exceeds {long.MaxValue}.");
            }

            return checked((long)roundedReward);
        }
    }
}
