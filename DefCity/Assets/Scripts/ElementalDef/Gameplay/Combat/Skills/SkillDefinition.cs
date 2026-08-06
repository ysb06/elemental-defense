using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    [CreateAssetMenu(menuName = "ElementalDef/Combat/Skill Definition")]
    public sealed class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0.0001f)] private float fullChargeDurationSeconds = 1f;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject castVfxPrefab;
        [SerializeField] private string animatorTrigger;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public float FullChargeDurationSeconds => fullChargeDurationSeconds;
        public Sprite Icon => icon;
        public GameObject CastVfxPrefab => castVfxPrefab;
        public string AnimatorTrigger => animatorTrigger;

        public void ValidateOrThrow()
        {
            List<string> errors = new();

            if (string.IsNullOrWhiteSpace(skillId))
            {
                errors.Add("Skill ID is required.");
            }
            else if (!string.Equals(skillId, skillId.Trim(), StringComparison.Ordinal))
            {
                errors.Add($"Skill ID '{skillId}' has leading or trailing whitespace.");
            }

            if (float.IsNaN(fullChargeDurationSeconds) ||
                float.IsInfinity(fullChargeDurationSeconds) ||
                fullChargeDurationSeconds <= 0f)
            {
                errors.Add(
                    $"Full charge duration must be a finite value greater than zero, " +
                    $"but is {fullChargeDurationSeconds}.");
            }

            if (!string.IsNullOrEmpty(animatorTrigger) &&
                !string.Equals(animatorTrigger, animatorTrigger.Trim(), StringComparison.Ordinal))
            {
                errors.Add("Animation trigger has leading or trailing whitespace.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{name} has {errors.Count} skill definition error(s):" +
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
        }
    }
}
