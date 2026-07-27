using System;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public class CellCursor : MonoBehaviour
    {
        private const int CellCornerCount = 4;

        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Min(0f)] private float size = 1f;
        [SerializeField] private float yOffset = 0.1f;
        [SerializeField] private Color validColor = new(0f, 0.65f, 1f, 0.9f);
        [SerializeField] private Color invalidColor = new(1f, 0.15f, 0.1f, 0.9f);

        private void Awake()
        {
            EnsureConfigured();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        public void Show(Vector3 worldSurfaceCenter, bool canPlace)
        {
            Vector3 center = worldSurfaceCenter;
            center.y += yOffset;
            float halfSize = size * 0.5f;
            Color color = canPlace ? validColor : invalidColor;

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.enabled = true;
            lineRenderer.positionCount = CellCornerCount + 1;

            lineRenderer.SetPosition(0, center + new Vector3(-halfSize, 0f, -halfSize));
            lineRenderer.SetPosition(1, center + new Vector3(halfSize, 0f, -halfSize));
            lineRenderer.SetPosition(2, center + new Vector3(halfSize, 0f, halfSize));
            lineRenderer.SetPosition(3, center + new Vector3(-halfSize, 0f, halfSize));
            lineRenderer.SetPosition(4, lineRenderer.GetPosition(0));
        }

        public void Hide()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        private void EnsureConfigured()
        {
            if (lineRenderer == null)
            {
                throw new InvalidOperationException($"{nameof(CellCursor)} requires a {nameof(LineRenderer)} reference.");
            }

            if (size <= 0f)
            {
                throw new InvalidOperationException($"{nameof(CellCursor)} size must be greater than zero.");
            }
        }
    }
}
