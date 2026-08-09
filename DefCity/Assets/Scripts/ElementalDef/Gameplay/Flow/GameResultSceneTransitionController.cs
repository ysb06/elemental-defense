using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementalDef.Gameplay.Flow
{
    [DisallowMultipleComponent]
    public sealed class GameResultSceneTransitionController : MonoBehaviour
    {
        [SerializeField] private GameFlowController gameFlowController;
        [SerializeField] private string resultSceneName = "ElementalDefResult";

        private bool isSubscribed;
        private bool isTransitionScheduled;
        private bool isLoadRequested;
        private int transitionRequestedFrame = -1;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            if (isSubscribed || isTransitionScheduled || isLoadRequested)
            {
                return;
            }

            gameFlowController.OnVictory.AddListener(HandleVictory);
            gameFlowController.OnDefeat.AddListener(HandleDefeat);
            isSubscribed = true;
        }

        private void Update()
        {
            if (!isTransitionScheduled ||
                isLoadRequested ||
                Time.frameCount <= transitionRequestedFrame)
            {
                return;
            }

            isLoadRequested = true;
            isTransitionScheduled = false;
            Unsubscribe();
            SceneManager.LoadScene(resultSceneName, LoadSceneMode.Single);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleVictory(GameObject sender)
        {
            if (sender != gameFlowController.gameObject)
            {
                return;
            }

            ScheduleTransition();
        }

        private void HandleDefeat(GameObject sender)
        {
            if (sender != gameFlowController.gameObject)
            {
                return;
            }

            ScheduleTransition();
        }

        private void ScheduleTransition()
        {
            if (isTransitionScheduled || isLoadRequested)
            {
                return;
            }

            transitionRequestedFrame = Time.frameCount;
            isTransitionScheduled = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            gameFlowController?.OnVictory.RemoveListener(HandleVictory);
            gameFlowController?.OnDefeat.RemoveListener(HandleDefeat);
            isSubscribed = false;
        }

        private void EnsureConfigured()
        {
            if (gameFlowController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultSceneTransitionController)} requires a " +
                    $"{nameof(GameFlowController)} reference.");
            }

            EnsureSceneIsLoadable(resultSceneName, nameof(resultSceneName));
        }

        private static void EnsureSceneIsLoadable(string sceneName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new InvalidOperationException(
                    $"{nameof(GameResultSceneTransitionController)} requires a value for {fieldName}.");
            }

            if (!CanLoadScene(sceneName))
            {
                throw new InvalidOperationException(
                    $"Scene '{sceneName}' configured in {fieldName} is not available to load. " +
                    "Add it to the enabled scenes in Build Settings.");
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
