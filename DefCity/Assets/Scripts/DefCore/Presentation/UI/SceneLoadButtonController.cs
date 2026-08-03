using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefCore.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class SceneLoadButtonController : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        private bool isLoadRequested;

        private void Awake()
        {
            EnsureSceneIsLoadable(sceneName);
        }

        public void LoadConfiguredScene()
        {
            if (isLoadRequested)
            {
                return;
            }

            EnsureSceneIsLoadable(sceneName);
            isLoadRequested = true;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private static void EnsureSceneIsLoadable(string targetSceneName)
        {
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                throw new InvalidOperationException(
                    $"{nameof(SceneLoadButtonController)} requires a scene name.");
            }

            if (!CanLoadScene(targetSceneName))
            {
                throw new InvalidOperationException(
                    $"Scene '{targetSceneName}' is not available to load. " +
                    "Add it to the enabled scenes in Build Settings.");
            }
        }

        private static bool CanLoadScene(string targetSceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
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

                if (string.Equals(buildScene.path, targetSceneName, StringComparison.Ordinal) ||
                    string.Equals(
                        Path.GetFileNameWithoutExtension(buildScene.path),
                        targetSceneName,
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
