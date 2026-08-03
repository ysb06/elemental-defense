using System;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using DefCore.Gameplay.Interaction;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class EntitySelectionIndicator : MonoBehaviour
    {
        [SerializeField] private EntitySelectionManager entitySelectionManager;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Min(3)] private int segmentCount = 48;
        [SerializeField, Min(0f)] private float padding = 0.25f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.05f;
        [SerializeField] private Color color = new(0.2f, 1f, 0.2f, 0.9f);

        private readonly List<Collider> colliderBuffer = new();
        private Entity selectedEntity;

        private void Awake()
        {
            EnsureConfigured();

            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            Clear();
        }

        private void OnEnable()
        {
            entitySelectionManager.OnEntitySelectionChanged.AddListener(
                HandleEntitySelectionChanged);
            selectedEntity = entitySelectionManager.CurrentEntity;
        }

        private void OnDisable()
        {
            if (entitySelectionManager != null)
            {
                entitySelectionManager.OnEntitySelectionChanged.RemoveListener(
                    HandleEntitySelectionChanged);
            }

            selectedEntity = null;
            Clear();
        }

        private void LateUpdate()
        {
            if (!ReferenceEquals(selectedEntity, entitySelectionManager.CurrentEntity))
            {
                selectedEntity = entitySelectionManager.CurrentEntity;
            }

            if (selectedEntity == null ||
                !selectedEntity.IsOperational ||
                !TryGetCombinedColliderBounds(selectedEntity, out Bounds bounds))
            {
                Clear();
                return;
            }

            DrawIndicator(bounds);
        }

        private void HandleEntitySelectionChanged(
            GameObject sender,
            EntitySelectionChangedEventArgs eventArgs)
        {
            if (sender != entitySelectionManager.gameObject)
            {
                return;
            }

            selectedEntity = eventArgs.CurrentEntity;

            if (selectedEntity == null || !selectedEntity.IsOperational)
            {
                Clear();
            }
        }

        private bool TryGetCombinedColliderBounds(Entity entity, out Bounds combinedBounds)
        {
            combinedBounds = default;
            colliderBuffer.Clear();
            entity.GetComponentsInChildren(false, colliderBuffer);

            bool hasBounds = false;
            foreach (Collider collider in colliderBuffer)
            {
                if (collider == null ||
                    !collider.enabled ||
                    collider.isTrigger ||
                    !collider.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    combinedBounds = collider.bounds;
                    hasBounds = true;
                    continue;
                }

                combinedBounds.Encapsulate(collider.bounds);
            }

            return hasBounds;
        }

        private void DrawIndicator(Bounds bounds)
        {
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) + padding;
            Vector3 center = new(
                bounds.center.x,
                bounds.min.y + surfaceOffset,
                bounds.center.z);

            lineRenderer.enabled = true;
            lineRenderer.positionCount = segmentCount;

            for (int index = 0; index < segmentCount; index++)
            {
                float angle = Mathf.PI * 2f * index / segmentCount;
                lineRenderer.SetPosition(index, center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        private void Clear()
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
            if (entitySelectionManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionIndicator)} requires an " +
                    $"{nameof(EntitySelectionManager)} reference.");
            }

            if (lineRenderer == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionIndicator)} requires a " +
                    $"{nameof(LineRenderer)} reference.");
            }

            if (segmentCount < 3)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionIndicator)} requires at least three segments.");
            }

            if (padding < 0f)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionIndicator)} padding cannot be negative.");
            }

            if (surfaceOffset < 0f)
            {
                throw new InvalidOperationException(
                    $"{nameof(EntitySelectionIndicator)} surface offset cannot be negative.");
            }
        }
    }
}
