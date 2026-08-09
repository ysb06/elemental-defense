using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Flow.Settings;
using ElementalDef.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class ResultNavigationController : MonoBehaviour
    {
        [SerializeField] private StageCatalog stageCatalog;
        [SerializeField] private Button nextStageButton;
        [SerializeField] private string gameplaySceneName = "ElementalDefGame";

        private bool isListenerBound;
        private bool isNavigationRequested;
        private StageRunContext resultContext;
        private WaveBundle nextStage;

        private void OnEnable()
        {
            isNavigationRequested = false;

            if (!TryValidateConfiguration(out string errorMessage))
            {
                SetNextStageButtonInteractable(false);
                Debug.LogError($"[{name}] {errorMessage}", this);
                return;
            }

            resultContext ??= ElementalDefApplicationRoot.Instance?.StageLaunch?.Current;
            BindButtonListener();
            RefreshNextStageAvailability();
        }

        private void OnDisable()
        {
            UnbindButtonListener();
            isNavigationRequested = false;
            nextStage = null;
        }

        private void BindButtonListener()
        {
            if (isListenerBound)
            {
                return;
            }

            nextStageButton.onClick.AddListener(HandleNextStageClicked);
            isListenerBound = true;
        }

        private void UnbindButtonListener()
        {
            if (!isListenerBound)
            {
                return;
            }

            if (nextStageButton != null)
            {
                nextStageButton.onClick.RemoveListener(HandleNextStageClicked);
            }

            isListenerBound = false;
        }

        private void RefreshNextStageAvailability()
        {
            nextStage = null;
            SetNextStageButtonInteractable(false);

            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (resultContext == null || applicationRoot?.RunStore == null)
            {
                Debug.LogError(
                    $"[{name}] The current stage context and run-store service are required.",
                    this);
                return;
            }

            try
            {
                if (!applicationRoot.RunStore.TryGetRun(
                        resultContext.RunId,
                        out CompletedStageRunRecord completedRun))
                {
                    Debug.LogError(
                        $"[{name}] No completed run was found for RunId " +
                        $"'{resultContext.RunId}'.",
                        this);
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

                if (completedRun.Outcome != StageRunOutcome.Victory)
                {
                    return;
                }

                int nextStageIndex = resultContext.DisplayOrder % StageCatalog.RequiredStageCount;
                nextStage = stageCatalog.Stages[nextStageIndex];
                SetNextStageButtonInteractable(true);
            }
            catch (Exception exception)
            {
                nextStage = null;
                SetNextStageButtonInteractable(false);
                Debug.LogException(new InvalidOperationException(
                    "The next ElementalDef stage could not be resolved from the completed run.",
                    exception),
                    this);
            }
        }

        private void HandleNextStageClicked()
        {
            if (isNavigationRequested)
            {
                return;
            }

            if (nextStage == null)
            {
                RefreshNextStageAvailability();
                return;
            }

            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot?.StageLaunch == null)
            {
                Debug.LogError(
                    $"[{name}] ElementalDef stage-launch services are unavailable.",
                    this);
                RefreshNextStageAvailability();
                return;
            }

            WaveBundle stageToLaunch = nextStage;
            try
            {
                isNavigationRequested = true;
                SetNextStageButtonInteractable(false);
                applicationRoot.StageLaunch.Prepare(stageToLaunch);
                SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                isNavigationRequested = false;
                Debug.LogException(new InvalidOperationException(
                    $"Failed to launch the next stage " +
                    $"'{stageToLaunch?.StageId ?? "<null>"}'.",
                    exception),
                    this);
                RefreshNextStageAvailability();
            }
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (stageCatalog == null)
            {
                errorMessage = "A StageCatalog reference is required.";
                return false;
            }

            try
            {
                stageCatalog.ValidateOrThrow();
            }
            catch (InvalidOperationException exception)
            {
                errorMessage = $"StageCatalog validation failed: {exception.Message}";
                return false;
            }

            if (nextStageButton == null)
            {
                errorMessage = "A next-stage button reference is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                errorMessage = "A gameplay scene name is required.";
                return false;
            }

            if (!CanLoadScene(gameplaySceneName))
            {
                errorMessage = $"Gameplay scene '{gameplaySceneName}' is not available to load.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void SetNextStageButtonInteractable(bool interactable)
        {
            if (nextStageButton != null)
            {
                nextStageButton.interactable = interactable && !isNavigationRequested;
            }
        }

        private static bool CanLoadScene(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return true;
            }

#if UNITY_EDITOR
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                {
                    continue;
                }

                if (string.Equals(buildScene.path, sceneName, StringComparison.Ordinal) ||
                    string.Equals(
                        Path.GetFileNameWithoutExtension(buildScene.path),
                        sceneName,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }
#endif

            return false;
        }
    }
}
