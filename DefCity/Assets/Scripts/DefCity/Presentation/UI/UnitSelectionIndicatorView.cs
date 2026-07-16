using System;
using DefCity.Gameplay.Combat;
using UnityEngine;

namespace DefCity.Presentation.UI
{
    [DisallowMultipleComponent]
    public class UnitSelectionIndicatorView : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Min(3)] private int segmentCount = 48;
        [SerializeField, Min(0f)] private float padding = 0.25f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.05f;

        public Damageable TargetDamageable => targetDamageable;
        public bool IsBound => isBound;

        private Damageable targetDamageable;
        private Collider targetCollider;
        private Vector3 targetLocalBottomCenter;
        private bool isBound;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                throw new InvalidOperationException($"{name} requires a LineRenderer.");
            }

            if (lineRenderer.transform != transform)
            {
                throw new InvalidOperationException(
                    $"{name} requires its LineRenderer on the same GameObject.");
            }

            if (segmentCount < 3)
            {
                throw new InvalidOperationException(
                    $"{name} requires at least three indicator segments.");
            }

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.positionCount = segmentCount;
            lineRenderer.enabled = false;
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void LateUpdate()
        {
            if (!isBound)
            {
                return;
            }

            if (targetDamageable == null || targetCollider == null)
            {
                Unbind();
                return;
            }

            UpdateIndicatorPose();
        }

        public void Bind(Damageable damageable)
        {
            if (damageable == null)
            {
                throw new ArgumentNullException(nameof(damageable));
            }

            if (!damageable.IsAlive)
            {
                throw new InvalidOperationException(
                    $"Cannot bind {name} to a dead Damageable.");
            }

            Collider damageCollider = damageable.DamageCollider;
            if (damageCollider == null)
            {
                throw new InvalidOperationException(
                    $"{damageable.name} requires a DamageCollider.");
            }

            GetIndicatorGeometry(
                damageCollider,
                out Vector3 localBottomCenter,
                out float radiusX,
                out float radiusZ);

            Unbind();

            targetDamageable = damageable;
            targetCollider = damageCollider;
            targetLocalBottomCenter = localBottomCenter;
            isBound = true;

            targetDamageable.OnDeath.AddListener(OnTargetDeath);

            SetRingPositions(radiusX, radiusZ);
            UpdateIndicatorPose();
            lineRenderer.enabled = true;
        }

        public void Unbind()
        {
            if (targetDamageable != null)
            {
                targetDamageable.OnDeath.RemoveListener(OnTargetDeath);
            }

            targetDamageable = null;
            targetCollider = null;
            targetLocalBottomCenter = default;
            isBound = false;
            lineRenderer.enabled = false;
        }

        private void OnTargetDeath(GameObject sender, DamageEventArgs args)
        {
            Unbind();
        }

        private void GetIndicatorGeometry(
            Collider damageCollider,
            out Vector3 localBottomCenter,
            out float radiusX,
            out float radiusZ)
        {
            Vector3 scale = damageCollider.transform.lossyScale;

            if (damageCollider is BoxCollider boxCollider)
            {
                localBottomCenter = boxCollider.center +
                                    Vector3.down * (boxCollider.size.y * 0.5f);
                radiusX = boxCollider.size.x * Mathf.Abs(scale.x) * 0.5f + padding;
                radiusZ = boxCollider.size.z * Mathf.Abs(scale.z) * 0.5f + padding;
                return;
            }

            if (damageCollider is CapsuleCollider capsuleCollider)
            {
                if (capsuleCollider.direction != 1)
                {
                    throw new InvalidOperationException(
                        $"{damageCollider.name} requires a Y-axis CapsuleCollider.");
                }

                localBottomCenter = capsuleCollider.center +
                                    Vector3.down * (capsuleCollider.height * 0.5f);
                radiusX = capsuleCollider.radius * Mathf.Abs(scale.x) + padding;
                radiusZ = capsuleCollider.radius * Mathf.Abs(scale.z) + padding;
                return;
            }

            throw new InvalidOperationException(
                $"{damageCollider.name} uses unsupported collider type " +
                $"{damageCollider.GetType().Name}.");
        }

        private void SetRingPositions(float radiusX, float radiusZ)
        {
            lineRenderer.positionCount = segmentCount;

            for (int index = 0; index < segmentCount; index++)
            {
                float angle = Mathf.PI * 2f * index / segmentCount;
                lineRenderer.SetPosition(index, new Vector3(
                    Mathf.Cos(angle) * radiusX,
                    0f,
                    Mathf.Sin(angle) * radiusZ));
            }
        }

        private void UpdateIndicatorPose()
        {
            Transform colliderTransform = targetCollider.transform;
            Vector3 worldBottomCenter = colliderTransform.TransformPoint(
                targetLocalBottomCenter);
            worldBottomCenter += Vector3.up * surfaceOffset;

            transform.SetPositionAndRotation(
                worldBottomCenter,
                Quaternion.Euler(0f, colliderTransform.eulerAngles.y, 0f));
        }
    }
}
