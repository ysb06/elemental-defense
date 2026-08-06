using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.Flow.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Stage Catalog")]
    public sealed class StageCatalog : ScriptableObject
    {
        public const int RequiredStageCount = 10;

        [SerializeField] private List<WaveBundle> stages = new();

        public IReadOnlyList<WaveBundle> Stages => stages;

        public bool TryGetStage(string stageId, out WaveBundle stage)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                stage = null;
                return false;
            }

            foreach (WaveBundle candidate in stages)
            {
                if (candidate != null && string.Equals(candidate.StageId, stageId, StringComparison.Ordinal))
                {
                    stage = candidate;
                    return true;
                }
            }

            stage = null;
            return false;
        }

        public WaveBundle GetRequiredStage(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                throw new ArgumentException("A stage ID is required.", nameof(stageId));
            }

            if (TryGetStage(stageId, out WaveBundle stage))
            {
                return stage;
            }

            throw new KeyNotFoundException($"{name} does not contain stage '{stageId}'.");
        }

        public void ValidateOrThrow()
        {
            List<string> errors = new();
            List<string> placeholderStages = new();
            HashSet<string> stageIds = new(StringComparer.Ordinal);
            HashSet<int> displayOrders = new();

            if (stages == null)
            {
                throw new InvalidOperationException($"{name} has no stage list.");
            }

            if (stages.Count != RequiredStageCount)
            {
                errors.Add($"Stage count must be exactly {RequiredStageCount}, but is {stages.Count}.");
            }

            for (int index = 0; index < stages.Count; index++)
            {
                WaveBundle stage = stages[index];
                int expectedDisplayOrder = index + 1;

                if (stage == null)
                {
                    errors.Add($"Stage entry {expectedDisplayOrder} is not assigned.");
                    continue;
                }

                try
                {
                    stage.ValidateOrThrow();
                }
                catch (InvalidOperationException exception)
                {
                    errors.Add(exception.Message);
                }

                if (!string.IsNullOrWhiteSpace(stage.StageId) && !stageIds.Add(stage.StageId))
                {
                    errors.Add($"Stage ID '{stage.StageId}' is duplicated.");
                }

                if (!displayOrders.Add(stage.DisplayOrder))
                {
                    errors.Add($"Display order {stage.DisplayOrder} is duplicated.");
                }

                if (stage.DisplayOrder != expectedDisplayOrder)
                {
                    errors.Add(
                        $"Stage '{stage.StageId}' is at catalog position {expectedDisplayOrder}, " +
                        $"but its display order is {stage.DisplayOrder}.");
                }

                if (stage.UsesPlaceholderTuning)
                {
                    placeholderStages.Add(string.IsNullOrWhiteSpace(stage.StageId) ? $"entry {expectedDisplayOrder}" : stage.StageId);
                }
            }

            for (int displayOrder = 1; displayOrder <= RequiredStageCount; displayOrder++)
            {
                if (!displayOrders.Contains(displayOrder))
                {
                    errors.Add($"Display order {displayOrder} is missing.");
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{name} has {errors.Count} stage catalog error(s):" +
                    Environment.NewLine +
                    "- " +
                    string.Join(Environment.NewLine + "- ", errors));
            }

            if (placeholderStages.Count > 0)
            {
                Debug.LogWarning(
                    $"[{name}] Placeholder tuning is enabled for: {string.Join(", ", placeholderStages)}.",
                    this);
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
        }
    }
}
