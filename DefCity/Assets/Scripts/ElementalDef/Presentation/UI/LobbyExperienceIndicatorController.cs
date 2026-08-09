using System;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyExperienceIndicatorController : MonoBehaviour
    {
        private const long ExperiencePerLevel = 100L;

        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text nextLevelExperienceText;
        [SerializeField] private RectTransform experienceBarRect;

        private void OnEnable()
        {
            if (!TryGetExperienceBarParent(out RectTransform experienceBarParent))
            {
                enabled = false;
                return;
            }

            RefreshExperienceDisplay(experienceBarParent);
        }

        private bool TryGetExperienceBarParent(out RectTransform experienceBarParent)
        {
            experienceBarParent = null;
            if (levelText == null || nextLevelExperienceText == null || experienceBarRect == null)
            {
                Debug.LogError(
                    $"[{name}] Level text, experience text, and experience bar references are required.",
                    this);
                return false;
            }

            experienceBarParent = experienceBarRect.parent as RectTransform;

            return true;
        }

        private void RefreshExperienceDisplay(RectTransform experienceBarParent)
        {
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot == null || applicationRoot.PlayerProgress == null)
            {
                ClearDisplay();
                Debug.LogError(
                    $"[{name}] ElementalDef player-progress services are unavailable.",
                    this);
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                long level = progress.TotalExperience / ExperiencePerLevel;
                long nextLevelExperience = progress.TotalExperience % ExperiencePerLevel;

                levelText.text = level.ToString(CultureInfo.InvariantCulture);
                nextLevelExperienceText.text = string.Concat(nextLevelExperience.ToString(CultureInfo.InvariantCulture), "%");

                float normalizedExperience = nextLevelExperience / (float)ExperiencePerLevel;
                float barWidth = experienceBarParent.rect.width * normalizedExperience;
                experienceBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barWidth);
            }
            catch (Exception exception)
            {
                ClearDisplay();
                Debug.LogException(new InvalidOperationException("The current ElementalDef experience progress could not be displayed.", exception), this);
            }
        }

        private void ClearDisplay()
        {
            levelText.text = string.Empty;
            nextLevelExperienceText.text = string.Empty;
            experienceBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        }
    }
}
