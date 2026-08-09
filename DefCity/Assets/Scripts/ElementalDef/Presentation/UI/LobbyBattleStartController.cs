using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow.Settings;
using ElementalDef.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyBattleStartController : MonoBehaviour
    {
        [SerializeField] private StageCatalog stageCatalog;
        [SerializeField] private Button battleStartButton;
        [SerializeField] private string gameplaySceneName = "ElementalDefGame";

        private bool isListenerBound;
        private bool isLaunchRequested;
        private WaveBundle nextStage;

        private void OnEnable()
        {
            isLaunchRequested = false;

            if (!TryValidateConfiguration(out string errorMessage))
            {
                SetBattleStartButtonInteractable(false);
                Debug.LogError($"[{name}] {errorMessage}", this);
                return;
            }

            BindButtonListener();
            RefreshStageAvailability();
        }

        private void OnDisable()
        {
            UnbindButtonListener();
            isLaunchRequested = false;
            nextStage = null;
        }

        private void BindButtonListener()
        {
            if (isListenerBound)
            {
                return;
            }

            battleStartButton.onClick.AddListener(HandleBattleStartClicked);
            isListenerBound = true;
        }

        private void UnbindButtonListener()
        {
            if (!isListenerBound)
            {
                return;
            }

            if (battleStartButton != null)
            {
                battleStartButton.onClick.RemoveListener(HandleBattleStartClicked);
            }

            isListenerBound = false;
        }

        private void RefreshStageAvailability()
        {
            nextStage = null;
            SetBattleStartButtonInteractable(false);

            if (!TryGetReadyApplicationServices(
                    out ElementalDefApplicationRoot applicationRoot,
                    out string errorMessage))
            {
                Debug.LogError($"[{name}] {errorMessage}", this);
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                int stageIndex = progress.MaxStageProgress;
                if (stageIndex < 0 || stageIndex >= stageCatalog.Stages.Count)
                {
                    Debug.LogError(
                        $"[{name}] Player progress resolved to invalid stage index {stageIndex}.",
                        this);
                    return;
                }

                WaveBundle resolvedStage = stageCatalog.Stages[stageIndex];
                if (resolvedStage == null)
                {
                    Debug.LogError(
                        $"[{name}] StageCatalog entry {stageIndex + 1} is not assigned.",
                        this);
                    return;
                }

                nextStage = resolvedStage;
                SetBattleStartButtonInteractable(true);
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The next ElementalDef stage could not be resolved from player progress.",
                    exception),
                    this);
            }
        }

        private void HandleBattleStartClicked()
        {
            if (isLaunchRequested)
            {
                return;
            }

            // Re-read progress so a lobby that remained active cannot launch stale stage data.
            RefreshStageAvailability();
            if (nextStage == null)
            {
                return;
            }

            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot?.StageLaunch == null)
            {
                Debug.LogError(
                    $"[{name}] ElementalDef stage-launch services are unavailable.",
                    this);
                RefreshStageAvailability();
                return;
            }

            WaveBundle stageToLaunch = nextStage;
            try
            {
                isLaunchRequested = true;
                SetBattleStartButtonInteractable(false);
                applicationRoot.StageLaunch.Prepare(stageToLaunch);
                SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                isLaunchRequested = false;
                Debug.LogException(new InvalidOperationException(
                    $"Failed to launch stage '{stageToLaunch.StageId}'.",
                    exception),
                    this);
                RefreshStageAvailability();
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

            if (battleStartButton == null)
            {
                errorMessage = "A battle-start button reference is required.";
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

        private static bool TryGetReadyApplicationServices(
            out ElementalDefApplicationRoot applicationRoot,
            out string errorMessage)
        {
            applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot == null)
            {
                errorMessage = "ElementalDefApplicationRoot is unavailable.";
                return false;
            }

            if (applicationRoot.StageLaunch == null)
            {
                errorMessage = "ElementalDef StageLaunchService is unavailable.";
                return false;
            }

            if (applicationRoot.RunStore == null ||
                applicationRoot.RunStore.State != DataStoreState.Ready)
            {
                errorMessage = "The ElementalDef data store is not ready.";
                return false;
            }

            if (applicationRoot.PlayerProgress == null)
            {
                errorMessage = "ElementalDef player-progress services are unavailable.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void SetBattleStartButtonInteractable(bool interactable)
        {
            if (battleStartButton != null)
            {
                battleStartButton.interactable = interactable && !isLaunchRequested;
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
