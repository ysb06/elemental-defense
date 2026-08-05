using System;
using System.Collections.Generic;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Weapons
{
    public class HomingProjectile : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 25f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float arrivalRadius = 0.25f;

        private HomingProjectileWeapon ownerWeapon;
        private AttackInfoArgs attackInfo;
        private Action<WeaponBase, AttackResolvedEventArgs> onResolved;
        private Vector3 attackerLaunchPosition;
        private Vector3 lastKnownImpactPoint;
        private HomingProjectileDamageType damageType;
        private float splashRadius;
        private bool isInitialized;
        private bool hasResolved;
        private readonly Collider[] splashOverlapResults = new Collider[32];

        public void Initialize(
            HomingProjectileWeapon ownerWeapon,
            AttackInfoArgs attackInfo,
            Vector3 attackerLaunchPosition,
            HomingProjectileDamageType damageType,
            float splashRadius,
            Action<WeaponBase, AttackResolvedEventArgs> onResolved)
        {
            this.ownerWeapon = ownerWeapon;
            this.attackInfo = attackInfo;
            this.attackerLaunchPosition = attackerLaunchPosition;
            this.damageType = damageType;
            this.splashRadius = splashRadius;
            this.onResolved = onResolved ?? throw new ArgumentNullException(nameof(onResolved));
            lastKnownImpactPoint = attackInfo.Target.GetClosestPoint(transform.position);
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || hasResolved)
            {
                return;
            }

            Health liveTarget = GetLiveTarget();
            Vector3 targetPoint = lastKnownImpactPoint;

            if (liveTarget != null)
            {
                targetPoint = liveTarget.GetClosestPoint(transform.position);
                lastKnownImpactPoint = targetPoint;
            }

            MoveTowards(targetPoint);

            if ((transform.position - targetPoint).sqrMagnitude > arrivalRadius * arrivalRadius)
            {
                return;
            }

            if (liveTarget != null)
            {
                ResolveSucceeded(liveTarget, targetPoint);
                return;
            }

            ResolveTargetLost();
        }

        private Health GetLiveTarget()
        {
            Health target = attackInfo.Target;
            if (target == null)
            {
                return null;
            }

            return Attacker.GetTargetRejectReason(attackInfo.AttackerTeam, target, true, out _, out _) == AttackRejectReason.None ? target : null;
        }

        private void MoveTowards(Vector3 destination)
        {
            Vector3 toDestination = destination - transform.position;
            if (toDestination.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toDestination.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        }

        private void ResolveSucceeded(Health liveTarget, Vector3 impactPoint)
        {
            if (hasResolved)
            {
                return;
            }

            List<AttackHitEntry> hits = ApplyDamageAtImpact(impactPoint, liveTarget);
            Resolve(new AttackResolvedEventArgs(attackInfo, AttackResolveStatus.Succeeded, hits, impactPoint));
        }

        private void ResolveTargetLost()
        {
            if (hasResolved)
            {
                return;
            }

            List<AttackHitEntry> hits = ApplyDamageAtImpact(lastKnownImpactPoint, null);
            Resolve(new AttackResolvedEventArgs(attackInfo, AttackResolveStatus.TargetLost, hits, lastKnownImpactPoint));
        }

        private List<AttackHitEntry> ApplyDamageAtImpact(Vector3 impactPoint, Health primaryTarget)
        {
            List<AttackHitEntry> hits = new(primaryTarget != null ? 4 : 3);

            if (primaryTarget != null)
            {
                TryApplyHit(primaryTarget, impactPoint, isPrimaryTarget: true, hits);
            }

            if (damageType == HomingProjectileDamageType.Single || splashRadius <= 0f)
            {
                return hits;
            }

            foreach (Health splashTarget in CollectSplashTargets(impactPoint, primaryTarget))
            {
                TryApplyHit(splashTarget, impactPoint, isPrimaryTarget: false, hits);
            }

            return hits;
        }

        private List<Health> CollectSplashTargets(Vector3 impactPoint, Health primaryTarget)
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(impactPoint, splashRadius, splashOverlapResults);
            HashSet<Health> seenTargets = new();
            if (primaryTarget != null)
            {
                seenTargets.Add(primaryTarget);
            }

            List<Health> splashTargets = new(overlapCount);

            for (int i = 0; i < overlapCount; i++)
            {
                Collider overlapCollider = splashOverlapResults[i];
                splashOverlapResults[i] = null;

                if (overlapCollider == null)
                {
                    continue;
                }

                Health splashTarget = overlapCollider.GetComponentInParent<Health>();
                if (splashTarget == null || !seenTargets.Add(splashTarget))
                {
                    continue;
                }

                if (!IsValidSplashTarget(splashTarget))
                {
                    continue;
                }

                splashTargets.Add(splashTarget);
            }

            splashTargets.Sort((left, right) =>
            {
                float leftDistanceSqr = left.GetDistanceSqrTo(impactPoint);
                float rightDistanceSqr = right.GetDistanceSqrTo(impactPoint);
                int distanceCompare = leftDistanceSqr.CompareTo(rightDistanceSqr);
                if (distanceCompare != 0)
                {
                    return distanceCompare;
                }

                return left.GetInstanceID().CompareTo(right.GetInstanceID());
            });

            return splashTargets;
        }

        private bool IsValidSplashTarget(Health splashTarget)
        {
            return splashTarget != null && splashTarget.gameObject != attackInfo.Attacker.gameObject &&
                Attacker.GetTargetRejectReason(attackInfo.AttackerTeam, splashTarget, true, out _, out _) == AttackRejectReason.None;
        }

        private void TryApplyHit(
            Health target,
            Vector3 impactPoint,
            bool isPrimaryTarget,
            List<AttackHitEntry> hits)
        {
            if (target == null || target.gameObject == attackInfo.Attacker.gameObject)
            {
                return;
            }

            if (Attacker.GetTargetRejectReason(attackInfo.AttackerTeam, target, true, out _, out _) != AttackRejectReason.None)
            {
                return;
            }

            float damageMultiplier = GetDamageMultiplier(target, impactPoint, isPrimaryTarget);
            if (damageMultiplier <= 0f)
            {
                return;
            }

            if (ownerWeapon.TryCreateProjectileHit(
                    attackInfo,
                    target,
                    attackerLaunchPosition,
                    impactPoint,
                    damageMultiplier,
                    out AttackHitEntry hit))
            {
                hits.Add(hit);
            }
        }

        private float GetDamageMultiplier(Health target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            switch (damageType)
            {
                case HomingProjectileDamageType.Single:
                    return isPrimaryTarget ? 1f : 0f;
                case HomingProjectileDamageType.SplashEqual:
                    return GetSplashEqualMultiplier(target, impactPoint, isPrimaryTarget);
                case HomingProjectileDamageType.SplashFalloff:
                    return GetSplashFalloffMultiplier(target, impactPoint, isPrimaryTarget);
                default:
                    return 0f;
            }
        }

        private float GetSplashEqualMultiplier(Health target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            if (isPrimaryTarget)
            {
                return 1f;
            }

            if (splashRadius <= 0f)
            {
                return 0f;
            }

            return target.GetDistanceSqrTo(impactPoint) <= splashRadius * splashRadius ? 1f : 0f;
        }

        private float GetSplashFalloffMultiplier(Health target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            if (isPrimaryTarget)
            {
                return 1f;
            }

            if (splashRadius <= 0f)
            {
                return 0f;
            }

            float distance = target.GetDistanceTo(impactPoint);
            if (distance > splashRadius)
            {
                return 0f;
            }

            return 1f - Mathf.Clamp01(distance / splashRadius);
        }

        private void Resolve(AttackResolvedEventArgs resolvedArgs)
        {
            if (hasResolved)
            {
                return;
            }

            hasResolved = true;
            try
            {
                onResolved(ownerWeapon, resolvedArgs);
            }
            finally
            {
                Destroy(gameObject);
            }
        }
    }
}
