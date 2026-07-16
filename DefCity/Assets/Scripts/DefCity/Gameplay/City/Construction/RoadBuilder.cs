using System;
using DefCity.Gameplay.City.Roads;
using DefCity.Gameplay.Interaction;
using DefCity.Gameplay.World;
using UnityEngine;
using UnityEngine.Events;

namespace DefCity.Gameplay.City.Construction
{
    public readonly struct RoadBuilderEventArgs
    {
        public bool IsBuildModeActive { get; }

        public RoadBuilderEventArgs(bool isBuildModeActive)
        {
            IsBuildModeActive = isBuildModeActive;
        }
    }

    public class RoadBuilder : MonoBehaviour
    {
        [SerializeField] private TerrainMouseEventManager terrainMouseEventManager;
        [SerializeField] private RoadNetwork roadNetwork;
        [SerializeField] private RoadBuildSettings roadSettings = RoadBuildSettings.Default;

        private TerrainCell? startCell;
        private bool isBuildModeActive;
        private RoadConstructionService constructionService;
        private RoadNetwork constructionServiceNetwork;

        public RoadBuilderEvent OnBuildModeChanged = new();

        public bool IsBuildModeActive => isBuildModeActive;
        public TerrainCell? StartCell => startCell;
        public RoadBuildSettings BuildSettings => roadSettings;

        private void OnEnable()
        {
            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseClick.AddListener(OnTerrainCellMouseClick);
            }
        }

        private void OnDisable()
        {
            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseClick.RemoveListener(OnTerrainCellMouseClick);
            }

            EndBuild();
        }

        public void BeginBuild()
        {
            EnsureInteractiveConfiguration();
            roadSettings.Validate();

            if (!enabled)
            {
                enabled = true;
            }

            if (isBuildModeActive)
            {
                return;
            }

            startCell = null;
            isBuildModeActive = true;
            OnBuildModeChanged?.Invoke(gameObject, new RoadBuilderEventArgs(true));
        }

        public void EndBuild()
        {
            bool wasBuildModeActive = isBuildModeActive;
            isBuildModeActive = false;
            startCell = null;

            if (wasBuildModeActive)
            {
                OnBuildModeChanged?.Invoke(gameObject, new RoadBuilderEventArgs(false));
            }
        }

        public void OnTerrainCellMouseClick(GameObject sender, TerrainCellEventArgs eventArgs)
        {
            if (!isBuildModeActive)
            {
                return;
            }

            if (!startCell.HasValue)
            {
                startCell = eventArgs.Cell;
                return;
            }

            TerrainCell selectedStart = startCell.Value;
            if (Build(selectedStart, eventArgs.Cell, out _, out string failureReason))
            {
                startCell = eventArgs.Cell;
                return;
            }

            Debug.LogWarning(
                $"Cannot build road from {selectedStart.RefPosition} to {eventArgs.Cell.RefPosition}: {failureReason}",
                this);
        }

        public bool Build(
            TerrainCell start,
            TerrainCell end,
            out RoadSegment segment,
            out string failureReason)
        {
            return GetConstructionService().TryBuildStraightSegment(
                start,
                end,
                roadSettings,
                out segment,
                out failureReason);
        }

        public bool CanBuild(
            TerrainCell start,
            TerrainCell end,
            out string failureReason)
        {
            return GetConstructionService().CanBuildStraightSegment(
                start,
                end,
                roadSettings,
                out failureReason);
        }

        private RoadConstructionService GetConstructionService()
        {
            if (roadNetwork == null)
            {
                throw new InvalidOperationException($"{nameof(RoadBuilder)} requires a {nameof(RoadNetwork)} reference.");
            }

            if (constructionService == null || !ReferenceEquals(constructionServiceNetwork, roadNetwork))
            {
                constructionService = new RoadConstructionService(roadNetwork);
                constructionServiceNetwork = roadNetwork;
            }

            return constructionService;
        }

        private void EnsureInteractiveConfiguration()
        {
            if (terrainMouseEventManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(RoadBuilder)} requires a {nameof(TerrainMouseEventManager)} reference.");
            }

            if (roadNetwork == null)
            {
                throw new InvalidOperationException($"{nameof(RoadBuilder)} requires a {nameof(RoadNetwork)} reference.");
            }
        }
    }

    [Serializable]
    public class RoadBuilderEvent : UnityEvent<GameObject, RoadBuilderEventArgs> { }
}
