using System;
using DefCity.Gameplay.City.Construction;
using DefCity.Gameplay.City.Roads;
using DefCity.Gameplay.Interaction;
using DefCity.Gameplay.World;
using UnityEngine;

namespace DefCity.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class RoadBuildPreviewController : MonoBehaviour
    {
        [SerializeField] private RoadBuilder roadBuilder;
        [SerializeField] private TerrainMouseEventManager terrainMouseEventManager;
        [SerializeField] private LineRenderer cellCursorRenderer;
        [SerializeField] private LineRenderer roadPreviewRenderer;
        [SerializeField, Min(3)] private int circleSegmentCount = 32;
        [SerializeField, Range(0.05f, 0.5f)] private float circleRadiusScale = 0.4f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.1f;
        [SerializeField] private Color neutralColor = new(1f, 0.92f, 0.02f, 0.9f);
        [SerializeField] private Color validColor = new(0f, 0.65f, 1f, 0.9f);
        [SerializeField] private Color invalidColor = new(1f, 0.15f, 0.1f, 0.9f);

        private bool isRoadBuildModeActive;

        private void Awake()
        {
            ValidateConfiguration();

            cellCursorRenderer.useWorldSpace = true;
            cellCursorRenderer.loop = true;
            roadPreviewRenderer.useWorldSpace = true;
            roadPreviewRenderer.loop = false;
            ClearPreview();
        }

        private void OnEnable()
        {
            roadBuilder.OnBuildModeChanged.AddListener(OnRoadBuildModeChanged);
            isRoadBuildModeActive = roadBuilder.IsBuildModeActive;
            if (!isRoadBuildModeActive)
            {
                ClearPreview();
            }
        }

        private void OnDisable()
        {
            if (roadBuilder != null)
            {
                roadBuilder.OnBuildModeChanged.RemoveListener(OnRoadBuildModeChanged);
            }

            isRoadBuildModeActive = false;
            ClearPreview();
        }

        private void Update()
        {
            if (!isRoadBuildModeActive)
            {
                return;
            }

            bool hasHoveredCell = terrainMouseEventManager.TryGetTerrainCellEventArgs(
                out TerrainCellEventArgs eventArgs);
            UpdatePreview(hasHoveredCell, eventArgs.Cell);
        }

        private void UpdatePreview(bool hasHoveredCell, TerrainCell hoveredCell)
        {
            if (!hasHoveredCell)
            {
                ClearPreview();
                return;
            }

            RefreshPreview(hoveredCell);
        }

        private void OnRoadBuildModeChanged(GameObject sender, RoadBuilderEventArgs eventArgs)
        {
            isRoadBuildModeActive = eventArgs.IsBuildModeActive;
            if (!isRoadBuildModeActive)
            {
                ClearPreview();
            }
        }

        private void RefreshPreview(TerrainCell hoveredCell)
        {
            TerrainCell? startCell = roadBuilder.StartCell;
            if (!startCell.HasValue)
            {
                DrawCellCursor(hoveredCell, neutralColor);
                ClearRenderer(roadPreviewRenderer);
                return;
            }

            bool canBuild = roadBuilder.CanBuild(startCell.Value, hoveredCell, out _);
            DrawCellCursor(hoveredCell, canBuild ? validColor : invalidColor);

            if (!canBuild)
            {
                ClearRenderer(roadPreviewRenderer);
                return;
            }

            DrawRoadPreview(startCell.Value, hoveredCell);
        }

        private void DrawCellCursor(TerrainCell cell, Color color)
        {
            Vector3[] corners = cell.CornerWorldPositions;
            float firstEdgeLength = GetHorizontalDistance(corners[0], corners[1]);
            float secondEdgeLength = GetHorizontalDistance(corners[0], corners[3]);
            float radius = Mathf.Min(firstEdgeLength, secondEdgeLength) * circleRadiusScale;

            Vector3 center = cell.Center;
            center.y = cell.AverageWorldHeight + surfaceOffset;

            cellCursorRenderer.positionCount = circleSegmentCount;
            SetRendererColor(cellCursorRenderer, color);
            for (int index = 0; index < circleSegmentCount; index++)
            {
                float angle = Mathf.PI * 2f * index / circleSegmentCount;
                cellCursorRenderer.SetPosition(index, center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }

            cellCursorRenderer.enabled = true;
        }

        private void DrawRoadPreview(TerrainCell start, TerrainCell end)
        {
            RoadBuildSettings settings = roadBuilder.BuildSettings;
            float heightOffset = settings.YOffset + settings.Thickness + surfaceOffset;

            roadPreviewRenderer.positionCount = 2;
            roadPreviewRenderer.SetPosition(0, GetPreviewPosition(start, heightOffset));
            roadPreviewRenderer.SetPosition(1, GetPreviewPosition(end, heightOffset));
            SetRendererColor(roadPreviewRenderer, validColor);
            roadPreviewRenderer.enabled = true;
        }

        private void ClearPreview()
        {
            ClearRenderer(cellCursorRenderer);
            ClearRenderer(roadPreviewRenderer);
        }

        private void ValidateConfiguration()
        {
            if (roadBuilder == null)
            {
                throw new InvalidOperationException($"{name} requires a {nameof(RoadBuilder)} reference.");
            }

            if (terrainMouseEventManager == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a {nameof(TerrainMouseEventManager)} reference.");
            }

            if (cellCursorRenderer == null || roadPreviewRenderer == null)
            {
                throw new InvalidOperationException($"{name} requires both road preview LineRenderers.");
            }

            if (ReferenceEquals(cellCursorRenderer, roadPreviewRenderer))
            {
                throw new InvalidOperationException($"{name} requires separate road preview LineRenderers.");
            }

            if (circleSegmentCount < 3)
            {
                throw new InvalidOperationException($"{name} requires at least three circle segments.");
            }

            if (circleRadiusScale <= 0f || float.IsNaN(circleRadiusScale) || float.IsInfinity(circleRadiusScale))
            {
                throw new InvalidOperationException($"{name} requires a positive finite circle radius scale.");
            }

            if (surfaceOffset < 0f || float.IsNaN(surfaceOffset) || float.IsInfinity(surfaceOffset))
            {
                throw new InvalidOperationException($"{name} requires a non-negative finite surface offset.");
            }
        }

        private static Vector3 GetPreviewPosition(TerrainCell cell, float heightOffset)
        {
            Vector3 position = cell.Center;
            position.y = cell.AverageWorldHeight + heightOffset;
            return position;
        }

        private static float GetHorizontalDistance(Vector3 first, Vector3 second)
        {
            return new Vector2(second.x - first.x, second.z - first.z).magnitude;
        }

        private static void SetRendererColor(LineRenderer renderer, Color color)
        {
            renderer.startColor = color;
            renderer.endColor = color;
        }

        private static void ClearRenderer(LineRenderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.enabled = false;
            renderer.positionCount = 0;
        }
    }
}
