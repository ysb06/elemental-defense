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
        public int MapSeed { get; }
        public int BaseCreditReward { get; }
        public int BaseExperienceReward { get; }
        public long StartedAtUtcMilliseconds { get; }
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
            int mapSeed,
            int baseCreditReward,
            int baseExperienceReward,
            long startedAtUtcMilliseconds,
            float difficultyMultiplier,
            float creditRewardMultiplier,
            float experienceRewardMultiplier,
            float characterStatMultiplier,
            int characterLevel)
        {
            RunId = runId;
            SelectedStage = selectedStage;
            StageId = stageId;
            DisplayOrder = displayOrder;
            MapSeed = mapSeed;
            BaseCreditReward = baseCreditReward;
            BaseExperienceReward = baseExperienceReward;
            StartedAtUtcMilliseconds = startedAtUtcMilliseconds;
            DifficultyMultiplier = difficultyMultiplier;
            CreditRewardMultiplier = creditRewardMultiplier;
            ExperienceRewardMultiplier = experienceRewardMultiplier;
            CharacterStatMultiplier = characterStatMultiplier;
            CharacterLevel = characterLevel;
        }

        public static StageRunContext Create(WaveBundle waveBundle)
        {
            if (waveBundle == null)
            {
                throw new ArgumentNullException(nameof(waveBundle));
            }

            waveBundle.ValidateOrThrow();

            return new StageRunContext(
                Guid.NewGuid().ToString("N"),
                waveBundle,
                waveBundle.StageId,
                waveBundle.DisplayOrder,
                waveBundle.MapSeed,
                waveBundle.BaseCreditReward,
                waveBundle.BaseExperienceReward,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                1f,
                1f,
                1f,
                1f,
                1);
        }
    }
}
