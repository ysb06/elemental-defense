using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefCity.Presentation.UI
{
    public class SceneLoadButtonController : MonoBehaviour
    {
        [SerializeField] private string sceneName = "SampleScene";

        public void LoadConfiguredScene()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new InvalidOperationException($"{nameof(SceneLoadButtonController)} requires a scene name.");
            }

            LoadScene(sceneName);
        }

        protected virtual void LoadScene(string targetSceneName)
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
