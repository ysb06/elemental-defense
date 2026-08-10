using System;
using System.Collections.Generic;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class DifficultyDebugPanelController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_Dropdown outcomeDropdown;
        [SerializeField] private TMP_InputField playDurationSecondsInput;
        [SerializeField] private TMP_InputField headquartersRemainingHealthInput;
        [SerializeField] private TMP_InputField headquartersMaxHealthInput;
        [SerializeField] private TMP_InputField defeatedEnemyCountInput;

        [Header("Actions")]
        [SerializeField] private Button injectButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button refreshButton;

        [Header("Output")]
        [SerializeField] private TMP_Text databaseDifficultyText;
        [SerializeField] private TMP_Text injectedDifficultyText;
        [SerializeField] private TMP_Text injectionStateText;
        [SerializeField] private TMP_Text statusText;

        private UnityAction injectHandler;
        private UnityAction clearHandler;
        private UnityAction refreshHandler;
        private bool listenersBound;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                Debug.LogError($"[{name}] {errorMessage}", this);
                SetButtonsInteractable(false);
                enabled = false;
                return;
            }

            ConfigureInputDefaults();
            BindListeners();
            RefreshDiagnostics(false);
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        public void Refresh()
        {
            RefreshDiagnostics(true);
        }

        private void RefreshDiagnostics(bool reportSuccess)
        {
            if (!TryGetServices(
                    out ElementalDefApplicationRoot applicationRoot,
                    out DifficultyDebugRunStore debugStore,
                    out string errorMessage))
            {
                ApplyStatus(errorMessage, true);
                SetButtonsInteractable(false);
                return;
            }

            try
            {
                PerformanceStageDifficultySnapshot databaseSnapshot =
                    new StageDifficultyService(applicationRoot.RunStore)
                        .GetPerformanceDifficulty();
                PerformanceStageDifficultySnapshot injectedSnapshot =
                    applicationRoot.StageDifficulty.GetPerformanceDifficulty();

                databaseDifficultyText.text = FormatDifficulty(databaseSnapshot);
                injectedDifficultyText.text = FormatDifficulty(injectedSnapshot);
                injectionStateText.text = FormatInjectionState(debugStore);
                SetButtonsInteractable(true);
                if (reportSuccess)
                {
                    ApplyStatus("난이도 상태를 새로고침했습니다.", false);
                }
            }
            catch (Exception exception)
            {
                ApplyStatus(exception.Message, true);
                SetButtonsInteractable(false);
                Debug.LogException(new InvalidOperationException(
                    "The difficulty debug panel could not refresh its diagnostics.",
                    exception), this);
            }
        }

        private void Inject()
        {
            if (!TryCreateInput(out DifficultyDebugRunInput input, out string errorMessage))
            {
                ApplyStatus(errorMessage, true);
                return;
            }

            if (!TryGetServices(
                    out _,
                    out DifficultyDebugRunStore debugStore,
                    out errorMessage))
            {
                ApplyStatus(errorMessage, true);
                return;
            }

            try
            {
                bool willEvictOldest =
                    debugStore.InjectedRunCount >=
                    DifficultyDebugRunStore.MaxInjectedRunCount;
                debugStore.Inject(input);
                string statusMessage = willEvictOldest
                    ? $"난이도 결과를 주입했습니다. " +
                      $"누적 {debugStore.InjectedRunCount}/" +
                      $"{DifficultyDebugRunStore.MaxInjectedRunCount}, " +
                      "가장 오래된 주입 1건을 제거했습니다."
                    : $"난이도 결과를 주입했습니다. " +
                      $"누적 {debugStore.InjectedRunCount}/" +
                      $"{DifficultyDebugRunStore.MaxInjectedRunCount}.";
                ApplyStatus(statusMessage, false);
                RefreshDiagnostics(false);
            }
            catch (Exception exception)
            {
                ApplyStatus(exception.Message, true);
            }
        }

        private void Clear()
        {
            if (!TryGetServices(
                    out _,
                    out DifficultyDebugRunStore debugStore,
                    out string errorMessage))
            {
                ApplyStatus(errorMessage, true);
                return;
            }

            int clearedCount = debugStore.InjectedRunCount;
            debugStore.ClearInjectedRuns();
            ApplyStatus(
                $"주입된 난이도 결과 {clearedCount}건을 모두 해제했습니다.",
                false);
            RefreshDiagnostics(false);
        }

        private void ConfigureInputDefaults()
        {
            outcomeDropdown.ClearOptions();
            outcomeDropdown.AddOptions(new List<string> { "Victory", "Defeat" });
            outcomeDropdown.SetValueWithoutNotify(0);
            SetDefaultIfEmpty(playDurationSecondsInput, "0");
            SetDefaultIfEmpty(headquartersRemainingHealthInput, "0");
            SetDefaultIfEmpty(headquartersMaxHealthInput, "100");
            SetDefaultIfEmpty(defeatedEnemyCountInput, "0");
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            injectHandler = Inject;
            clearHandler = Clear;
            refreshHandler = Refresh;
            injectButton.onClick.AddListener(injectHandler);
            clearButton.onClick.AddListener(clearHandler);
            refreshButton.onClick.AddListener(refreshHandler);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            injectButton?.onClick.RemoveListener(injectHandler);
            clearButton?.onClick.RemoveListener(clearHandler);
            refreshButton?.onClick.RemoveListener(refreshHandler);
            injectHandler = null;
            clearHandler = null;
            refreshHandler = null;
            listenersBound = false;
        }

        private bool TryCreateInput(
            out DifficultyDebugRunInput input,
            out string errorMessage)
        {
            input = default;
            StageRunOutcome outcome = outcomeDropdown.value == 0
                ? StageRunOutcome.Victory
                : StageRunOutcome.Defeat;

            if (!TryParseDouble(
                    playDurationSecondsInput.text,
                    "플레이 시간",
                    out double playDurationSeconds,
                    out errorMessage) ||
                !TryParseDouble(
                    headquartersRemainingHealthInput.text,
                    "HQ 잔여 HP",
                    out double remainingHealth,
                    out errorMessage) ||
                !TryParseDouble(
                    headquartersMaxHealthInput.text,
                    "HQ 최대 HP",
                    out double maximumHealth,
                    out errorMessage))
            {
                return false;
            }

            if (!long.TryParse(
                    defeatedEnemyCountInput.text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long defeatedEnemyCount))
            {
                errorMessage = "처치 수에는 정수를 입력해야 합니다.";
                return false;
            }

            input = new DifficultyDebugRunInput(
                outcome,
                playDurationSeconds,
                remainingHealth,
                maximumHealth,
                defeatedEnemyCount);
            errorMessage = null;
            return true;
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (outcomeDropdown == null || playDurationSecondsInput == null ||
                headquartersRemainingHealthInput == null ||
                headquartersMaxHealthInput == null || defeatedEnemyCountInput == null)
            {
                errorMessage = "All difficulty input references are required.";
                return false;
            }

            if (injectButton == null || clearButton == null || refreshButton == null)
            {
                errorMessage = "Inject, Clear, and Refresh Button references are required.";
                return false;
            }

            if (databaseDifficultyText == null || injectedDifficultyText == null ||
                injectionStateText == null || statusText == null)
            {
                errorMessage = "All difficulty output Text references are required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool TryGetServices(
            out ElementalDefApplicationRoot applicationRoot,
            out DifficultyDebugRunStore debugStore,
            out string errorMessage)
        {
            applicationRoot = ElementalDefApplicationRoot.Instance;
            debugStore = applicationRoot?.DifficultyDebug;

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

            if (applicationRoot.RunStore == null || applicationRoot.StageDifficulty == null ||
                debugStore == null)
            {
                errorMessage = "난이도 디버그 서비스를 사용할 수 없습니다.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool TryParseDouble(
            string value,
            string label,
            out double parsedValue,
            out string errorMessage)
        {
            if (!double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out parsedValue))
            {
                errorMessage = $"{label}에는 '.'을 소수점으로 사용하는 숫자를 입력해야 합니다.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static string FormatDifficulty(PerformanceStageDifficultySnapshot snapshot)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:F4} (원본 {1:F4}, 기록 {2}, 승리 {3})",
                snapshot.DifficultyMultiplier,
                snapshot.RawDifficultyMultiplier,
                snapshot.ConsideredRunCount,
                snapshot.VictoryCount);
        }

        private static string FormatInjectionState(DifficultyDebugRunStore debugStore)
        {
            if (!debugStore.HasInjectedRun)
            {
                return $"저장된 주입 0/{DifficultyDebugRunStore.MaxInjectedRunCount}\n" +
                       $"현재 최근 {StageDifficultyService.RecentRunLimit}건 반영 0건";
            }

            IReadOnlyList<CompletedStageRunRecord> injectedRuns = debugStore.InjectedRuns;
            var injectedRunIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < injectedRuns.Count; i++)
            {
                injectedRunIds.Add(injectedRuns[i].RunId);
            }

            IReadOnlyList<CompletedStageRunRecord> recentRuns =
                debugStore.GetRecentRuns(StageDifficultyService.RecentRunLimit);
            int reflectedInjectedRunCount = 0;
            for (int i = 0; i < recentRuns.Count; i++)
            {
                if (injectedRunIds.Contains(recentRuns[i].RunId))
                {
                    reflectedInjectedRunCount++;
                }
            }

            return $"저장된 주입 {debugStore.InjectedRunCount}/" +
                   $"{DifficultyDebugRunStore.MaxInjectedRunCount}\n" +
                   $"현재 최근 {StageDifficultyService.RecentRunLimit}건 반영 " +
                   $"{reflectedInjectedRunCount}건\n최신: {debugStore.InjectedRun.RunId}";
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (injectButton != null)
            {
                injectButton.interactable = interactable;
            }

            if (clearButton != null)
            {
                clearButton.interactable = interactable;
            }

            if (refreshButton != null)
            {
                refreshButton.interactable = interactable;
            }
        }

        private void ApplyStatus(string message, bool isError)
        {
            statusText.text = message ?? string.Empty;
            statusText.color = isError
                ? new Color(0.85f, 0.2f, 0.2f)
                : new Color(0.1f, 0.55f, 0.2f);
        }

        private static void SetDefaultIfEmpty(TMP_InputField inputField, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(inputField.text))
            {
                inputField.SetTextWithoutNotify(defaultValue);
            }
        }
    }
}
