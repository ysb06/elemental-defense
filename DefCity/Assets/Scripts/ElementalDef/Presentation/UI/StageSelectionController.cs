using System;
using System.Collections.Generic;
using ElementalDef.Data;
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
        [SerializeField] private Sprite[] activeStageSprites = new Sprite[StageCatalog.RequiredStageCount];
        [SerializeField] private Sprite[] inactiveStageSprites = new Sprite[StageCatalog.RequiredStageCount];
        [SerializeField] private string gameplaySceneName = "ElementalDefGame";

        private UnityAction[] stageButtonHandlers;
        private Image[] stageButtonImages;
        private bool isLaunchRequested;
        private int unlockedStageCount = 1;

        private void Awake()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                Debug.LogError($"[{name}] {errorMessage}", this);
                SetStageButtonsInteractable(false);
                return;
            }

            BindStageButtons();
            RefreshStageAvailability();
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
            }
        }

        private void RefreshStageAvailability()
        {
            int maxStageProgress = 0;
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;

            if (applicationRoot?.PlayerProgress == null)
            {
                Debug.LogError(
                    $"[{name}] ElementalDef player-progress services are unavailable. " +
                    "Stage 1 will be used as the fallback availability.",
                    this);
            }
            else
            {
                try
                {
                    PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                    maxStageProgress = progress.MaxStageProgress;
                }
                catch (Exception exception)
                {
                    Debug.LogException(
                        new InvalidOperationException(
                            "ElementalDef stage progress could not be loaded. " +
                            "Stage 1 will be used as the fallback availability.",
                            exception),
                        this);
                }
            }

            unlockedStageCount = Mathf.Clamp(
                maxStageProgress + 1,
                1,
                StageCatalog.RequiredStageCount);
            ApplyStageAvailability();
        }

        private void ApplyStageAvailability()
        {
            for (int index = 0; index < stageButtons.Length; index++)
            {
                bool isUnlocked = index < unlockedStageCount;
                stageButtonImages[index].sprite = isUnlocked
                    ? activeStageSprites[index]
                    : inactiveStageSprites[index];
                stageButtons[index].interactable = isUnlocked && !isLaunchRequested;
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

            if (stageIndex >= unlockedStageCount)
            {
                Debug.LogWarning($"[{name}] Stage {stageIndex + 1} is locked.", this);
                ApplyStageAvailability();
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
                isLaunchRequested = true;
                SetStageButtonsInteractable(false);
                applicationRoot.StageLaunch.Prepare(selectedStage);
                SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
            }
            catch (Exception exception)
            {
                isLaunchRequested = false;
                ApplyStageAvailability();
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

            stageButtonImages = new Image[stageButtons.Length];
            HashSet<Button> uniqueButtons = new();
            HashSet<Image> uniqueImages = new();
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

                if (button.targetGraphic is not Image buttonImage)
                {
                    errorMessage = $"Stage button {index + 1} requires an Image target graphic.";
                    return false;
                }

                if (!uniqueImages.Add(buttonImage))
                {
                    errorMessage = $"Stage button image {index + 1} is assigned more than once.";
                    return false;
                }

                stageButtonImages[index] = buttonImage;
            }

            if (!TryValidateSpriteArray(activeStageSprites, "Active", out errorMessage) ||
                !TryValidateSpriteArray(inactiveStageSprites, "Inactive", out errorMessage))
            {
                return false;
            }

            for (int index = 0; index < StageCatalog.RequiredStageCount; index++)
            {
                if (activeStageSprites[index] == inactiveStageSprites[index])
                {
                    errorMessage = $"Stage {index + 1} active and inactive sprites must differ.";
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

        private static bool TryValidateSpriteArray(
            Sprite[] sprites,
            string arrayName,
            out string errorMessage)
        {
            if (sprites == null || sprites.Length != StageCatalog.RequiredStageCount)
            {
                errorMessage = $"{arrayName} stage sprites must contain exactly " +
                               $"{StageCatalog.RequiredStageCount} entries.";
                return false;
            }

            HashSet<Sprite> uniqueSprites = new();
            for (int index = 0; index < sprites.Length; index++)
            {
                Sprite sprite = sprites[index];
                if (sprite == null)
                {
                    errorMessage = $"{arrayName} stage sprite {index + 1} is not assigned.";
                    return false;
                }

                if (!uniqueSprites.Add(sprite))
                {
                    errorMessage = $"{arrayName} stage sprite {index + 1} is assigned more than once.";
                    return false;
                }
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
