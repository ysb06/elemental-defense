using UnityEngine;
using UnityEngine.InputSystem;
using DefCity.Gameplay.City.Construction;

namespace DefCity.Presentation.UI
{
    public class BuildMenuController : MonoBehaviour
    {
        private enum BuildMenuState
        {
            Root,
            MainBuildWindow,
            TempMilitaryWindow,
            BuildPlacement
        }

        private enum BuildPlacementType
        {
            None,
            Building,
            Road
        }

        [SerializeField] private GameObject buildingButton;
        [SerializeField] private GameObject mainBuildWindow;
        [SerializeField] private GameObject tempMilitaryWindow;
        [SerializeField] private Builder builder;
        [SerializeField] private RoadBuilder roadBuilder;

        private InputAction cancelAction;
        private BuildMenuState currentState;
        private BuildMenuState previousStateBeforeBuild = BuildMenuState.TempMilitaryWindow;
        private BuildPlacementType activeBuildPlacementType;
        private bool restoringPreviousStateAfterBuildCancel;
        private bool isSwitchingBuildModes;

        private void Awake()
        {
            ShowRoot();
        }

        private void OnEnable()
        {
            cancelAction = InputSystem.actions.FindAction("Cancel", true);
            cancelAction.performed += OnCancelPerformed;
            builder.OnBuildModeChanged.AddListener(OnBuilderBuildModeChanged);
            roadBuilder.OnBuildModeChanged.AddListener(OnRoadBuildModeChanged);

            if (builder.IsBuildModeActive)
            {
                HandleBuildModeChanged(BuildPlacementType.Building, true);
            }

            if (roadBuilder.IsBuildModeActive)
            {
                HandleBuildModeChanged(BuildPlacementType.Road, true);
            }
        }

        private void OnDisable()
        {
            if (cancelAction != null)
            {
                cancelAction.performed -= OnCancelPerformed;
                cancelAction = null;
            }

            builder.OnBuildModeChanged.RemoveListener(OnBuilderBuildModeChanged);
            roadBuilder.OnBuildModeChanged.RemoveListener(OnRoadBuildModeChanged);
        }

        public void ShowRoot()
        {
            currentState = BuildMenuState.Root;
            SetActive(buildingButton, true);
            SetActive(mainBuildWindow, false);
            SetActive(tempMilitaryWindow, false);
        }

        public void ShowMainBuildWindow()
        {
            currentState = BuildMenuState.MainBuildWindow;
            SetActive(buildingButton, false);
            SetActive(mainBuildWindow, true);
            SetActive(tempMilitaryWindow, false);
        }

        public void ShowTempMilitaryWindow()
        {
            currentState = BuildMenuState.TempMilitaryWindow;
            SetActive(buildingButton, false);
            SetActive(mainBuildWindow, false);
            SetActive(tempMilitaryWindow, true);
        }

        public void Cancel()
        {
            switch (currentState)
            {
                case BuildMenuState.BuildPlacement:
                    restoringPreviousStateAfterBuildCancel = true;
                    if (!EndBuildMode(activeBuildPlacementType))
                    {
                        restoringPreviousStateAfterBuildCancel = false;
                        activeBuildPlacementType = BuildPlacementType.None;
                        ShowState(previousStateBeforeBuild);
                    }
                    break;
                case BuildMenuState.TempMilitaryWindow:
                    ShowMainBuildWindow();
                    break;
                case BuildMenuState.MainBuildWindow:
                    ShowRoot();
                    break;
                case BuildMenuState.Root:
                    ShowRoot();
                    break;
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            Cancel();
        }

        private void OnBuilderBuildModeChanged(GameObject buildingTarget, BuilderEventArgs eventArgs)
        {
            HandleBuildModeChanged(BuildPlacementType.Building, eventArgs.IsBuildModeActive);
        }

        private void OnRoadBuildModeChanged(GameObject sender, RoadBuilderEventArgs eventArgs)
        {
            HandleBuildModeChanged(BuildPlacementType.Road, eventArgs.IsBuildModeActive);
        }

        private void HandleBuildModeChanged(BuildPlacementType buildPlacementType, bool isActive)
        {
            if (isActive)
            {
                EndOtherBuildMode(buildPlacementType);
                EnterBuildPlacement(buildPlacementType);
                return;
            }

            if (isSwitchingBuildModes || activeBuildPlacementType != buildPlacementType)
            {
                return;
            }

            activeBuildPlacementType = BuildPlacementType.None;
            if (currentState != BuildMenuState.BuildPlacement)
            {
                restoringPreviousStateAfterBuildCancel = false;
                return;
            }

            if (restoringPreviousStateAfterBuildCancel)
            {
                restoringPreviousStateAfterBuildCancel = false;
                ShowState(previousStateBeforeBuild);
                return;
            }

            ShowRoot();
        }

        private void EnterBuildPlacement(BuildPlacementType buildPlacementType)
        {
            if (currentState != BuildMenuState.BuildPlacement)
            {
                previousStateBeforeBuild = currentState;
            }

            activeBuildPlacementType = buildPlacementType;
            currentState = BuildMenuState.BuildPlacement;
            SetActive(buildingButton, false);
            SetActive(mainBuildWindow, false);
            SetActive(tempMilitaryWindow, false);
        }

        private void EndOtherBuildMode(BuildPlacementType buildPlacementType)
        {
            BuildPlacementType otherBuildMode = buildPlacementType == BuildPlacementType.Building
                ? BuildPlacementType.Road
                : BuildPlacementType.Building;

            if (!IsBuildModeActive(otherBuildMode))
            {
                return;
            }

            isSwitchingBuildModes = true;
            try
            {
                EndBuildMode(otherBuildMode);
            }
            finally
            {
                isSwitchingBuildModes = false;
            }
        }

        private bool IsBuildModeActive(BuildPlacementType buildPlacementType)
        {
            return buildPlacementType switch
            {
                BuildPlacementType.Building => builder.IsBuildModeActive,
                BuildPlacementType.Road => roadBuilder.IsBuildModeActive,
                _ => false
            };
        }

        private bool EndBuildMode(BuildPlacementType buildPlacementType)
        {
            if (!IsBuildModeActive(buildPlacementType))
            {
                return false;
            }

            switch (buildPlacementType)
            {
                case BuildPlacementType.Building:
                    builder.EndBuild();
                    return true;
                case BuildPlacementType.Road:
                    roadBuilder.EndBuild();
                    return true;
                case BuildPlacementType.None:
                    return false;
                default:
                    return false;
            }
        }

        private void ShowState(BuildMenuState state)
        {
            switch (state)
            {
                case BuildMenuState.TempMilitaryWindow:
                    ShowTempMilitaryWindow();
                    break;
                case BuildMenuState.MainBuildWindow:
                    ShowMainBuildWindow();
                    break;
                case BuildMenuState.Root:
                case BuildMenuState.BuildPlacement:
                    ShowRoot();
                    break;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            target.SetActive(active);
        }
    }
}
