using System;
using DefCity.Gameplay.Combat;
using UnityEngine;

namespace DefCity.Presentation.UI
{
    [DisallowMultipleComponent]
    public class UnitAttackRangeView : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField, Min(3)] private int minSegmentCount = 48;
        [SerializeField, Min(3)] private int maxSegmentCount = 256;
        [SerializeField, Min(0.01f)] private float maxSegmentLength = 2f;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.05f;

        public AttackCapable TargetAttackCapable => targetAttackCapable;
        public Damageable TargetDamageable => targetDamageable;
        public bool IsBound => isBound;

        private AttackCapable targetAttackCapable;
        private Damageable targetDamageable;
        private Collider targetCollider;
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

            if (minSegmentCount < 3)
            {
                throw new InvalidOperationException(
                    $"{name} requires at least three minimum segments.");
            }

            if (maxSegmentCount < minSegmentCount)
            {
                throw new InvalidOperationException(
                    $"{name} requires Max Segment Count to be at least Min Segment Count.");
            }

            if (maxSegmentLength <= 0f)
            {
                throw new InvalidOperationException(
                    $"{name} requires a positive Max Segment Length.");
            }

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.positionCount = 0;
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

            if (targetAttackCapable == null ||
                targetDamageable == null ||
                targetCollider == null ||
                !targetDamageable.isActiveAndEnabled ||
                !targetDamageable.gameObject.activeInHierarchy)
            {
                Unbind();
                return;
            }

            UpdateRangePose();
        }

        public void Bind(AttackCapable attackCapable, Damageable damageable)
        {
            if (attackCapable == null)
            {
                throw new ArgumentNullException(nameof(attackCapable));
            }

            if (damageable == null)
            {
                throw new ArgumentNullException(nameof(damageable));
            }

            if (!damageable.IsAlive)
            {
                throw new InvalidOperationException(
                    $"Cannot bind {name} to a dead Damageable.");
            }

            if (damageable.DamageCollider == null)
            {
                throw new InvalidOperationException(
                    $"{damageable.name} requires a DamageCollider.");
            }

            if (attackCapable.EquippedWeapon == null)
            {
                throw new InvalidOperationException(
                    $"{attackCapable.name} requires an equipped Weapon.");
            }

            float attackRange = attackCapable.EquippedWeapon.AttackRange;
            if (attackRange <= 0f)
            {
                throw new InvalidOperationException(
                    $"{attackCapable.EquippedWeapon.name} requires a positive AttackRange.");
            }

            Unbind();

            targetAttackCapable = attackCapable;
            targetDamageable = damageable;
            targetCollider = damageable.DamageCollider;
            isBound = true;

            targetDamageable.OnDeath.AddListener(OnTargetDeath);

            SetRingPositions(attackRange);
            UpdateRangePose();
            lineRenderer.enabled = true;
        }

        public void Unbind()
        {
            if (targetDamageable != null)
            {
                targetDamageable.OnDeath.RemoveListener(OnTargetDeath);
            }

            targetAttackCapable = null;
            targetDamageable = null;
            targetCollider = null;
            isBound = false;
            lineRenderer.enabled = false;
        }

        private void OnTargetDeath(GameObject sender, DamageEventArgs args)
        {
            Unbind();
        }

        private void SetRingPositions(float radius)
        {
            float circumference = Mathf.PI * 2f * radius;
            int segmentCount = Mathf.Clamp(
                Mathf.CeilToInt(circumference / maxSegmentLength),
                minSegmentCount,
                maxSegmentCount);

            lineRenderer.positionCount = segmentCount;

            for (int index = 0; index < segmentCount; index++)
            {
                float angle = Mathf.PI * 2f * index / segmentCount;
                lineRenderer.SetPosition(index, new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        private void UpdateRangePose()
        {
            Vector3 attackerPosition = targetAttackCapable.transform.position;
            Vector3 rangePosition = new(
                attackerPosition.x,
                targetCollider.bounds.min.y + surfaceOffset,
                attackerPosition.z);

            transform.SetPositionAndRotation(rangePosition, Quaternion.identity);
        }
    }
}
