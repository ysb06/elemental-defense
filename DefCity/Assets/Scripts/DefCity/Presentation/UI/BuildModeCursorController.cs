using UnityEngine;
using DefCity.Gameplay.City.Construction;

namespace DefCity.Presentation.UI
{
    public class BuildModeCursorController : MonoBehaviour
    {
        [SerializeField] private Builder builder;
        [SerializeField] private TerrainCursor terrainCursor;

        private bool targetMouseCursorVisible = true;

        private void Awake()
        {
            SetBuildCursorActive(false);
        }

        private void OnEnable()
        {
            if (builder != null)
            {
                builder.OnBuildModeChanged.AddListener(OnBuildModeChanged);
            }
        }

        private void OnDisable()
        {
            if (builder != null)
            {
                builder.OnBuildModeChanged.RemoveListener(OnBuildModeChanged);
            }

            SetBuildCursorActive(false);
        }

        private void OnBuildModeChanged(GameObject buildingTarget, BuilderEventArgs eventArgs)
        {
            SetBuildCursorActive(eventArgs.IsBuildModeActive);
        }

        private void SetBuildCursorActive(bool active)
        {
            if (terrainCursor == null)
            {
                SetMouseCursorVisible(true);
                return;
            }

            terrainCursor.gameObject.SetActive(active);
            SetMouseCursorVisible(!active);
        }

        private void SetMouseCursorVisible(bool visible)
        {
            targetMouseCursorVisible = visible;
            Cursor.visible = targetMouseCursorVisible;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
