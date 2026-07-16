using System.Collections.Generic;
using UnityEngine;
using DefCity.Gameplay.City.Construction;
using DefCity.Gameplay.Interaction;
using DefCity.Gameplay.World;

namespace DefCity.Presentation.UI
{
    public class BuildFootprintCursorController : MonoBehaviour
    {
        [SerializeField] private Builder builder;
        [SerializeField] private TerrainMouseEventManager terrainMouseEventManager;
        [SerializeField] private TerrainCellManager terrainCellManager;
        [SerializeField] private PlacementValidator placementValidator;
        [SerializeField] private TerrainCursor terrainCursor;
        [SerializeField] private BuildGhostPreview buildGhostPreview;
        [SerializeField] private Color validColor = new(0f, 0.65f, 1f, 0.9f);
        [SerializeField] private Color invalidColor = new(1f, 0.15f, 0.1f, 0.9f);

        private readonly List<TerrainCell> footprintCells = new();
        private bool isBuildModeActive;

        private void OnEnable()
        {
            if (builder != null)
            {
                builder.OnBuildModeChanged.AddListener(OnBuildModeChanged);
            }

            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseOver.AddListener(OnTerrainCellMouseOver);
            }
        }

        private void OnDisable()
        {
            if (builder != null)
            {
                builder.OnBuildModeChanged.RemoveListener(OnBuildModeChanged);
            }

            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseOver.RemoveListener(OnTerrainCellMouseOver);
            }

            ClearCursor();
        }

        private void Update()
        {
            if (!isBuildModeActive)
            {
                return;
            }

            if (terrainMouseEventManager == null)
            {
                ClearCursor();
                return;
            }

            if (!terrainMouseEventManager.TryGetTerrainCellEventArgs(out TerrainCellEventArgs eventArgs))
            {
                ClearCursor();
                return;
            }

            RefreshCursor(eventArgs.Cell);
        }

        private void OnBuildModeChanged(GameObject sender, BuilderEventArgs eventArgs)
        {
            isBuildModeActive = eventArgs.IsBuildModeActive;
            if (!isBuildModeActive)
            {
                ClearCursor();
                return;
            }

            if (buildGhostPreview != null)
            {
                buildGhostPreview.SetTarget(eventArgs.BuildingTarget);
            }
        }

        private void OnTerrainCellMouseOver(GameObject sender, TerrainCellEventArgs eventArgs)
        {
            RefreshCursor(eventArgs.Cell);
        }

        private void RefreshCursor(TerrainCell cell)
        {
            if (!isBuildModeActive || builder == null || builder.EntityTarget == null)
            {
                ClearCursor();
                return;
            }

            if (placementValidator == null || terrainCellManager == null || terrainCursor == null)
            {
                ClearCursor();
                return;
            }

            Vector3 entityPosition = GetBuildPosition(cell);
            Quaternion entityRotation = Quaternion.identity;
            bool canBuild = builder.CanBuild(cell, builder.EntityTarget, out _);
            Color cursorColor = canBuild ? validColor : invalidColor;

            if (buildGhostPreview != null)
            {
                buildGhostPreview.SetTarget(builder.EntityTarget);
                buildGhostPreview.SetPose(entityPosition, entityRotation);
                buildGhostPreview.SetValid(canBuild);
            }

            if (!placementValidator.TryGetPlacementBounds(
                    builder.EntityTarget.gameObject,
                    entityPosition,
                    entityRotation,
                    out Bounds placementBounds,
                    out _))
            {
                terrainCursor.Clear();
                return;
            }

            footprintCells.Clear();
            footprintCells.AddRange(terrainCellManager.EnumerateTerrainCellsOverlappingBounds(placementBounds));
            terrainCursor.SetCells(footprintCells, cursorColor);
        }

        private void ClearCursor()
        {
            footprintCells.Clear();
            if (terrainCursor != null)
            {
                terrainCursor.Clear();
            }

            if (buildGhostPreview != null)
            {
                buildGhostPreview.Clear();
            }
        }

        private static Vector3 GetBuildPosition(TerrainCell cell)
        {
            Vector3 entityPosition = cell.Center;
            entityPosition.y = cell.AverageWorldHeight;
            return entityPosition;
        }
    }
}
