using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Flow.Settings;
using ElementalDef.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class StageSelectionController : MonoBehaviour
    {
        [SerializeField] private StageCatalog stageCatalog;
        [SerializeField] private Button[] stageButtons = new Button[StageCatalog.RequiredStageCount];
        [SerializeField] private string gameplaySceneName = "ElementalDefMain";

        private UnityAction[] stageButtonHandlers;
        private bool isLaunchRequested;

        private void Awake()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                Debug.LogError($"[{name}] {errorMessage}", this);
                SetStageButtonsInteractable(false);
                return;
            }

            BindStageButtons();
        }

        private void OnDestroy()
        {
            UnbindStageButtons();
        }

        private void BindStageButtons()
        {
            stageButtonHandlers = new UnityAction[stageButtons.Length];
            for (int index = 0; index < stageButtons.Length; index++)
            {
                int capturedIndex = index;
                UnityAction handler = () => LaunchStage(capturedIndex);
                stageButtonHandlers[index] = handler;
                stageButtons[index].onClick.AddListener(handler);
                stageButtons[index].interactable = true;
            }
        }

        private void UnbindStageButtons()
        {
            if (stageButtonHandlers == null || stageButtons == null)
            {
                return;
            }

            int count = Math.Min(stageButtonHandlers.Length, stageButtons.Length);
            for (int index = 0; index < count; index++)
            {
                if (stageButtons[index] != null && stageButtonHandlers[index] != null)
                {
                    stageButtons[index].onClick.RemoveListener(stageButtonHandlers[index]);
                }
            }

            stageButtonHandlers = null;
        }

        private void LaunchStage(int stageIndex)
        {
            if (isLaunchRequested)
            {
                return;
            }

            if (stageIndex < 0 || stageIndex >= stageCatalog.Stages.Count)
            {
                Debug.LogError($"[{name}] Stage index {stageIndex} is outside the catalog.", this);
                SetStageButtonsInteractable(false);
                return;
            }

            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot == null || applicationRoot.StageLaunch == null)
            {
                Debug.LogError($"[{name}] ElementalDef application services are unavailable.", this);
                SetStageButtonsInteractable(false);
                return;
            }

            WaveBundle selectedStage = stageCatalog.Stages[stageIndex];
            try
            {
                applicationRoot.StageLaunch.Prepare(selectedStage);
                isLaunchRequested = true;
                SetStageButtonsInteractable(false);
                SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                isLaunchRequested = false;
                SetStageButtonsInteractable(true);
                Debug.LogException(new InvalidOperationException(
                    $"Failed to launch stage '{selectedStage?.StageId ?? "<null>"}'.",
                    exception),
                    this);
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

            if (stageButtons == null || stageButtons.Length != StageCatalog.RequiredStageCount)
            {
                errorMessage = $"Exactly {StageCatalog.RequiredStageCount} stage buttons are required.";
                return false;
            }

            HashSet<Button> uniqueButtons = new();
            for (int index = 0; index < stageButtons.Length; index++)
            {
                Button button = stageButtons[index];
                if (button == null)
                {
                    errorMessage = $"Stage button {index + 1} is not assigned.";
                    return false;
                }

                if (!uniqueButtons.Add(button))
                {
                    errorMessage = $"Stage button {index + 1} is assigned more than once.";
                    return false;
                }
            }

            if (ElementalDefApplicationRoot.Instance == null ||
                ElementalDefApplicationRoot.Instance.StageLaunch == null)
            {
                errorMessage = "ElementalDefApplicationRoot and StageLaunchService must be initialized.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                errorMessage = "A gameplay scene name is required.";
                return false;
            }

            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(gameplaySceneName))
            {
                errorMessage = $"Gameplay scene '{gameplaySceneName}' is not available to load.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void SetStageButtonsInteractable(bool interactable)
        {
            if (stageButtons == null)
            {
                return;
            }

            foreach (Button button in stageButtons)
            {
                if (button != null)
                {
                    button.interactable = interactable;
                }
            }
        }
    }
}
