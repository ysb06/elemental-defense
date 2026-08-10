using System;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow.Settings;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerProgressDebugPanelController : MonoBehaviour
    {
        [Header("Current Progress")]
        [SerializeField] private TMP_Text totalCreditsText;
        [SerializeField] private TMP_Text totalExperienceText;
        [SerializeField] private TMP_Text totalDefeatCountText;
        [SerializeField] private TMP_Text currentMaxStageProgressText;
        [SerializeField] private TMP_Text currentLoopText;
        [SerializeField] private TMP_Text nextSequentialStageText;
        [SerializeField] private TMP_Text absoluteStageNumberText;

        [Header("New Progress")]
        [SerializeField] private TMP_InputField maxStageProgressInput;
        [SerializeField] private TMP_InputField loopInput;
        [SerializeField] private Button applyButton;
        [SerializeField] private TMP_Text statusText;

        private UnityAction applyHandler;
        private bool listenerBound;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                Debug.LogError($"[{name}] {errorMessage}", this);
                if (applyButton != null)
                {
                    applyButton.interactable = false;
                }

                enabled = false;
                return;
            }

            BindListener();
            Refresh();
        }

        private void OnDisable()
        {
            UnbindListener();
        }

        public void Refresh()
        {
            if (!TryGetServices(
                    out ElementalDefApplicationRoot applicationRoot,
                    out _,
                    out string errorMessage))
            {
                ApplyStatus(errorMessage, true);
                applyButton.interactable = false;
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                ApplyProgress(progress, true);
                applyButton.interactable = true;
            }
            catch (Exception exception)
            {
                ApplyStatus(exception.Message, true);
                applyButton.interactable = false;
                Debug.LogException(new InvalidOperationException(
                    "The player-progress debug panel could not load current progress.",
                    exception), this);
            }
        }

        private void Apply()
        {
            if (!int.TryParse(
                    maxStageProgressInput.text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int maxStageProgress))
            {
                ApplyStatus("maxStageProgress에는 정수를 입력해야 합니다.", true);
                return;
            }

            if (!long.TryParse(
                    loopInput.text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long loop))
            {
                ApplyStatus("Loop에는 정수를 입력해야 합니다.", true);
                return;
            }

            if (maxStageProgress < 0 ||
                maxStageProgress >= StageCatalog.RequiredStageCount)
            {
                ApplyStatus(
                    $"maxStageProgress는 0~{StageCatalog.RequiredStageCount - 1} 범위여야 합니다.",
                    true);
                return;
            }

            if (loop < 0)
            {
                ApplyStatus("Loop는 0 이상이어야 합니다.", true);
                return;
            }

            if (!TryGetServices(
                    out _,
                    out PlayerProgressDebugService debugService,
                    out string errorMessage))
            {
                ApplyStatus(errorMessage, true);
                return;
            }

            try
            {
                PlayerProgressSnapshot updatedProgress =
                    debugService.SetProgress(maxStageProgress, loop);
                ApplyProgress(updatedProgress, true);
                ApplyStatus("플레이어 진행도를 로컬 DB에 저장했습니다.", false);
            }
            catch (Exception exception)
            {
                ApplyStatus(exception.Message, true);
            }
        }

        private void ApplyProgress(PlayerProgressSnapshot progress, bool updateInputs)
        {
            totalCreditsText.text = progress.TotalCredits.ToString(
                "N0",
                CultureInfo.InvariantCulture);
            totalExperienceText.text = progress.TotalExperience.ToString(
                "N0",
                CultureInfo.InvariantCulture);
            totalDefeatCountText.text = progress.TotalDefeatCount.ToString(
                "N0",
                CultureInfo.InvariantCulture);
            currentMaxStageProgressText.text = progress.MaxStageProgress.ToString(
                CultureInfo.InvariantCulture);
            currentLoopText.text = progress.Loop.ToString(CultureInfo.InvariantCulture);

            int nextStage = progress.MaxStageProgress + 1;
            long absoluteStageNumber = checked(
                (progress.Loop * StageCatalog.RequiredStageCount) + nextStage);
            nextSequentialStageText.text = $"Stage {nextStage}";
            absoluteStageNumberText.text = absoluteStageNumber.ToString(
                CultureInfo.InvariantCulture);

            if (updateInputs)
            {
                maxStageProgressInput.SetTextWithoutNotify(
                    progress.MaxStageProgress.ToString(CultureInfo.InvariantCulture));
                loopInput.SetTextWithoutNotify(
                    progress.Loop.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void BindListener()
        {
            if (listenerBound)
            {
                return;
            }

            applyHandler = Apply;
            applyButton.onClick.AddListener(applyHandler);
            listenerBound = true;
        }

        private void UnbindListener()
        {
            if (!listenerBound)
            {
                return;
            }

            applyButton?.onClick.RemoveListener(applyHandler);
            applyHandler = null;
            listenerBound = false;
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (totalCreditsText == null || totalExperienceText == null ||
                totalDefeatCountText == null || currentMaxStageProgressText == null ||
                currentLoopText == null || nextSequentialStageText == null ||
                absoluteStageNumberText == null || statusText == null)
            {
                errorMessage = "All player-progress output Text references are required.";
                return false;
            }

            if (maxStageProgressInput == null || loopInput == null || applyButton == null)
            {
                errorMessage = "Player-progress Input and Apply Button references are required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool TryGetServices(
            out ElementalDefApplicationRoot applicationRoot,
            out PlayerProgressDebugService debugService,
            out string errorMessage)
        {
            applicationRoot = ElementalDefApplicationRoot.Instance;
            debugService = applicationRoot?.PlayerProgressDebug;

            if (applicationRoot == null)
            {
                errorMessage = "ElementalDefApplicationRoot를 찾을 수 없습니다.";
                return false;
            }

            if (applicationRoot.InitializationException != null)
            {
                errorMessage = "ElementalDef 애플리케이션 서비스 초기화에 실패했습니다.";
                return false;
            }

            if (applicationRoot.PlayerProgress == null || debugService == null)
            {
                errorMessage = "플레이어 진행도 디버그 서비스를 사용할 수 없습니다.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ApplyStatus(string message, bool isError)
        {
            statusText.text = message ?? string.Empty;
            statusText.color = isError
                ? new Color(0.85f, 0.2f, 0.2f)
                : new Color(0.1f, 0.55f, 0.2f);
        }
    }
}
