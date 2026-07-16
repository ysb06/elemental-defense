using System;
using System.Collections.Generic;
using UnityEngine;
using DefCity.Gameplay.World;

namespace DefCity.Presentation.UI
{
    public class TerrainCursor : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float yOffset = 0f;

        private readonly List<LineRenderer> lineRenderers = new();

        private void OnEnable()
        {
            Clear();
        }

        public void SetCells(IReadOnlyList<TerrainCell> cells)
        {
            Color color = lineRenderer != null ? lineRenderer.startColor : Color.white;
            SetCells(cells, color);
        }

        public void SetCells(IReadOnlyList<TerrainCell> cells, Color color)
        {
            if (cells == null || cells.Count == 0)
            {
                Clear();
                return;
            }

            EnsureRendererPool(cells.Count);
            for (int i = 0; i < lineRenderers.Count; i++)
            {
                LineRenderer renderer = lineRenderers[i];
                bool isActive = i < cells.Count;
                renderer.enabled = isActive;
                if (isActive)
                {
                    SetRendererColor(renderer, color);
                    DrawCell(renderer, cells[i]);
                }
                else
                {
                    renderer.positionCount = 0;
                }
            }
        }

        public void Clear()
        {
            if (lineRenderers.Count == 0 && lineRenderer != null)
            {
                lineRenderers.Add(lineRenderer);
            }

            foreach (LineRenderer renderer in lineRenderers)
            {
                renderer.enabled = false;
                renderer.positionCount = 0;
            }
        }

        private void EnsureRendererPool(int count)
        {
            if (lineRenderer == null)
            {
                throw new InvalidOperationException($"{name} requires a LineRenderer.");
            }

            if (lineRenderers.Count == 0)
            {
                lineRenderers.Add(lineRenderer);
            }

            while (lineRenderers.Count < count)
            {
                GameObject rendererObject = new($"{lineRenderer.name} ({lineRenderers.Count})");
                rendererObject.transform.SetParent(transform, false);
                LineRenderer renderer = rendererObject.AddComponent<LineRenderer>();
                CopyRendererSettings(lineRenderer, renderer);
                lineRenderers.Add(renderer);
            }
        }

        private static void CopyRendererSettings(LineRenderer source, LineRenderer target)
        {
            target.sharedMaterials = source.sharedMaterials;
            target.widthMultiplier = source.widthMultiplier;
            target.widthCurve = source.widthCurve;
            target.colorGradient = source.colorGradient;
            target.numCornerVertices = source.numCornerVertices;
            target.numCapVertices = source.numCapVertices;
            target.alignment = source.alignment;
            target.textureMode = source.textureMode;
            target.textureScale = source.textureScale;
            target.shadowCastingMode = source.shadowCastingMode;
            target.receiveShadows = source.receiveShadows;
            target.useWorldSpace = source.useWorldSpace;
            target.loop = source.loop;
            target.sortingLayerID = source.sortingLayerID;
            target.sortingOrder = source.sortingOrder;
        }

        private static void SetRendererColor(LineRenderer renderer, Color color)
        {
            renderer.startColor = color;
            renderer.endColor = color;
        }

        private void DrawCell(LineRenderer renderer, TerrainCell cell)
        {
            Vector3[] corners = cell.CornerWorldPositions;
            int cornerCount = corners.Length;
            renderer.positionCount = cornerCount + 1;

            for (int i = 0; i < cornerCount; i++)
            {
                Vector3 position = corners[i];
                position.y += yOffset;
                renderer.SetPosition(i, position);
            }

            Vector3 firstPosition = corners[0];
            firstPosition.y += yOffset;
            renderer.SetPosition(cornerCount, firstPosition);
        }
    }
}
