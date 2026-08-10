using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.Flow.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Wave Bundle")]
    public class WaveBundle : ScriptableObject
    {
        [Header("Stage")]
        [SerializeField] private string stageId;
        [SerializeField, Min(1)] private int displayOrder = 1;
        [SerializeField] private int mapSeed;
        [SerializeField, Min(0.01f)] private float timeLimitSeconds = 600f;
        [SerializeField, Min(1f)] private float enemyDifficultyMultiplier = 1f;
        [SerializeField, Min(1)] private int startingTowerEnergy = 1;

        [Header("Base Rewards")]
        [SerializeField, Min(0)] private int baseCreditReward;
        [SerializeField, Min(0)] private int baseExperienceReward;
        [SerializeField] private bool usesPlaceholderTuning = true;

        [Header("Waves")]
        [SerializeField] private List<WaveSchedule> waves = new();

        public string StageId => stageId;
        public int DisplayOrder => displayOrder;
        public int MapSeed => mapSeed;
        public float TimeLimitSeconds => timeLimitSeconds;
        public float EnemyDifficultyMultiplier => enemyDifficultyMultiplier;
        public int StartingTowerEnergy => startingTowerEnergy;
        public int BaseCreditReward => baseCreditReward;
        public int BaseExperienceReward => baseExperienceReward;
        public bool UsesPlaceholderTuning => usesPlaceholderTuning;
        public IReadOnlyList<WaveSchedule> Waves => waves;

        public void AddWave(WaveSchedule wave)
        {
            if (wave == null)
            {
                throw new ArgumentNullException(nameof(wave));
            }

            waves.Add(wave);
        }

        public void ClearWaves()
        {
            waves.Clear();
        }

        public void ValidateOrThrow()
        {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(stageId))
            {
                errors.Add("Stage ID is required.");
            }
            else if (!string.Equals(stageId, stageId.Trim(), StringComparison.Ordinal))
            {
                errors.Add($"Stage ID '{stageId}' has leading or trailing whitespace.");
            }

            if (displayOrder < 1)
            {
                errors.Add($"Display order must be at least 1, but is {displayOrder}.");
            }

            if (float.IsNaN(timeLimitSeconds) ||
                float.IsInfinity(timeLimitSeconds) ||
                timeLimitSeconds <= 0f)
            {
                errors.Add(
                    $"Time limit must be finite and greater than 0 seconds, but is {timeLimitSeconds}.");
            }

            if (float.IsNaN(enemyDifficultyMultiplier) ||
                float.IsInfinity(enemyDifficultyMultiplier) ||
                enemyDifficultyMultiplier < 1f)
            {
                errors.Add(
                    $"Enemy difficulty multiplier must be finite and at least 1, but is " +
                    $"{enemyDifficultyMultiplier}.");
            }

            if (startingTowerEnergy < 1)
            {
                errors.Add(
                    $"Starting tower energy must be at least 1, but is " +
                    $"{startingTowerEnergy}.");
            }

            if (baseCreditReward < 0)
            {
                errors.Add($"Base credit reward cannot be negative, but is {baseCreditReward}.");
            }

            if (baseExperienceReward < 0)
            {
                errors.Add($"Base experience reward cannot be negative, but is {baseExperienceReward}.");
            }

            if (waves == null || waves.Count == 0)
            {
                errors.Add("At least one wave is required.");
            }
            else
            {
                for (int index = 0; index < waves.Count; index++)
                {
                    WaveSchedule wave = waves[index];
                    if (wave == null)
                    {
                        errors.Add($"Wave entry {index + 1} is not assigned.");
                        continue;
                    }

                    try
                    {
                        wave.ValidateOrThrow();
                    }
                    catch (InvalidOperationException exception)
                    {
                        errors.Add(
                            $"Wave entry {index + 1} ('{wave.name}') is invalid: " +
                            exception.Message);
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{name} has {errors.Count} wave bundle error(s):" +
                    Environment.NewLine +
                    "- " +
                    string.Join(Environment.NewLine + "- ", errors));
            }
        }

        private void OnValidate()
        {
            try
            {
                ValidateOrThrow();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
            }

            if (usesPlaceholderTuning)
            {
                Debug.LogWarning($"[{name}] Placeholder tuning is enabled.", this);
            }
        }
    }
}
