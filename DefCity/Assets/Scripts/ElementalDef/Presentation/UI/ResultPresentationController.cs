using System;
using System.Collections.Generic;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class ResultPresentationController : MonoBehaviour
    {
        private const string HealthNumberFormat = "0.##";
        private const string CountNumberFormat = "N0";

        [Header("Result Images")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image bannerImage;
        [SerializeField] private Sprite victoryBackgroundSprite;
        [SerializeField] private Sprite defeatBackgroundSprite;
        [SerializeField] private Sprite victoryBannerSprite;
        [SerializeField] private Sprite defeatBannerSprite;

        [Header("Battle Statistics")]
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text currentHealthText;
        [SerializeField] private TMP_Text healthSeparatorText;
        [SerializeField] private TMP_Text maximumHealthText;
        [SerializeField] private TMP_Text killCountText;
        [SerializeField] private TMP_Text attackCountText;
        [SerializeField] private TMP_Text accumulatedDefeatCountText;

        [Header("Rewards")]
        [SerializeField] private TMP_Text creditRewardText;
        [SerializeField] private TMP_Text experienceRewardText;

        private StageRunContext resultContext;

        private void OnEnable()
        {
            ClearPresentation();

            if (!TryValidateConfiguration(out string errorMessage))
            {
                Debug.LogError(
                    $"[{name}] {nameof(ResultPresentationController)}: {errorMessage}",
                    this);
                enabled = false;
                return;
            }

            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            resultContext ??= applicationRoot?.StageLaunch?.Current;
            if (applicationRoot == null || resultContext == null ||
                applicationRoot.RunStore == null)
            {
                Debug.LogError(
                    $"[{name}] The current stage context and run-store service are required.",
                    this);
                return;
            }

            CompletedStageRunRecord completedRun;
            try
            {
                if (!applicationRoot.RunStore.TryGetRun(resultContext.RunId, out completedRun))
                {
                    Debug.LogError(
                        $"[{name}] No completed run was found for RunId " +
                        $"'{resultContext.RunId}'.",
                        this);
                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The completed ElementalDef stage run could not be loaded for presentation.",
                    exception), this);
                return;
            }

            if (!string.Equals(
                    completedRun.StageId,
                    resultContext.StageId,
                    StringComparison.Ordinal) ||
                completedRun.StageDisplayOrder != resultContext.DisplayOrder)
            {
                Debug.LogError(
                    $"[{name}] The completed run does not match the current stage context.",
                    this);
                return;
            }

            ApplyCompletedRun(completedRun);
            RefreshAccumulatedDefeatCount(applicationRoot);
        }

        private void ApplyCompletedRun(CompletedStageRunRecord completedRun)
        {
            ApplyOutcomeImages(completedRun.Outcome);

            timeText.text = FormatPlayDuration(completedRun.PlayDurationMilliseconds);
            currentHealthText.text = completedRun.HeadquartersRemainingHealth.ToString(
                HealthNumberFormat,
                CultureInfo.InvariantCulture);
            ApplyMaximumHealth(completedRun.HeadquartersMaxHealth);
            killCountText.text = completedRun.DefeatedEnemyCount.ToString(
                CountNumberFormat,
                CultureInfo.InvariantCulture);
            attackCountText.text = completedRun.AttackCount.ToString(
                CountNumberFormat,
                CultureInfo.InvariantCulture);
            creditRewardText.text = completedRun.EarnedCredits.ToString(
                CountNumberFormat,
                CultureInfo.InvariantCulture);
            experienceRewardText.text = completedRun.EarnedExperience.ToString(
                CountNumberFormat,
                CultureInfo.InvariantCulture);
        }

        private void ApplyOutcomeImages(StageRunOutcome outcome)
        {
            switch (outcome)
            {
                case StageRunOutcome.Victory:
                    backgroundImage.sprite = victoryBackgroundSprite;
                    bannerImage.sprite = victoryBannerSprite;
                    break;
                case StageRunOutcome.Defeat:
                    backgroundImage.sprite = defeatBackgroundSprite;
                    bannerImage.sprite = defeatBannerSprite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }

            backgroundImage.enabled = true;
            bannerImage.enabled = true;
        }

        private void ApplyMaximumHealth(double? maximumHealth)
        {
            if (!maximumHealth.HasValue)
            {
                healthSeparatorText.text = string.Empty;
                maximumHealthText.text = string.Empty;
                return;
            }

            healthSeparatorText.text = "/";
            maximumHealthText.text = maximumHealth.Value.ToString(
                HealthNumberFormat,
                CultureInfo.InvariantCulture);
        }

        private void RefreshAccumulatedDefeatCount(
            ElementalDefApplicationRoot applicationRoot)
        {
            accumulatedDefeatCountText.text = string.Empty;
            if (applicationRoot.PlayerProgress == null)
            {
                Debug.LogError(
                    $"[{name}] ElementalDef player-progress services are unavailable.",
                    this);
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                accumulatedDefeatCountText.text = progress.TotalDefeatCount.ToString(
                    CountNumberFormat,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The accumulated ElementalDef defeat count could not be displayed.",
                    exception), this);
            }
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (backgroundImage == null || bannerImage == null)
            {
                errorMessage = "Background and banner Image references are required.";
                return false;
            }

            if (backgroundImage == bannerImage)
            {
                errorMessage = "Background and banner Images must be unique.";
                return false;
            }

            if (victoryBackgroundSprite == null || defeatBackgroundSprite == null ||
                victoryBannerSprite == null || defeatBannerSprite == null)
            {
                errorMessage = "Victory and defeat background and banner Sprites are required.";
                return false;
            }

            if (timeText == null || currentHealthText == null ||
                healthSeparatorText == null || maximumHealthText == null ||
                killCountText == null || attackCountText == null ||
                accumulatedDefeatCountText == null || creditRewardText == null ||
                experienceRewardText == null)
            {
                errorMessage = "All result statistic and reward text references are required.";
                return false;
            }

            var uniqueTexts = new HashSet<TMP_Text>
            {
                timeText,
                currentHealthText,
                healthSeparatorText,
                maximumHealthText,
                killCountText,
                attackCountText,
                accumulatedDefeatCountText,
                creditRewardText,
                experienceRewardText
            };
            if (uniqueTexts.Count != 9)
            {
                errorMessage = "Result statistic and reward texts must be unique.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ClearPresentation()
        {
            if (backgroundImage != null)
            {
                backgroundImage.enabled = false;
            }

            if (bannerImage != null)
            {
                bannerImage.enabled = false;
            }

            ClearText(timeText);
            ClearText(currentHealthText);
            ClearText(healthSeparatorText);
            ClearText(maximumHealthText);
            ClearText(killCountText);
            ClearText(attackCountText);
            ClearText(accumulatedDefeatCountText);
            ClearText(creditRewardText);
            ClearText(experienceRewardText);
        }

        private static string FormatPlayDuration(long playDurationMilliseconds)
        {
            long totalSeconds = playDurationMilliseconds / 1000L;
            long totalMinutes = totalSeconds / 60L;
            long seconds = totalSeconds % 60L;
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}",
                totalMinutes,
                seconds);
        }

        private static void ClearText(TMP_Text text)
        {
            if (text != null)
            {
                text.text = string.Empty;
            }
        }
    }
}
