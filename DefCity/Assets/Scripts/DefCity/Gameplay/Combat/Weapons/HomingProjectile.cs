using System;
using System.Collections.Generic;
using UnityEngine;
using DefCity.Gameplay.Combat;

namespace DefCity.Gameplay.Combat.Weapons
{
    public enum DamageType
    {
        Single,
        SplashEqual,
        SplashFalloff
    }

    public class HomingProjectile : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 25f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float arrivalRadius = 0.25f;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactAudioClip;
        [SerializeField] private float impactAudioVolume = 1f;
        [SerializeField] private float impactEffectLifetime = 6f;
        [SerializeField] private DamageType damageType;

        private WeaponBase ownerWeapon;
        private AttackInfoArgs attackInfo;
        private Action<WeaponBase, AttackResolvedEventArgs> onResolved;
        private Vector3 lastKnownImpactPoint;
        private float splashRadius;
        private bool isInitialized;
        private bool hasResolved;
        private readonly Collider[] splashOverlapResults = new Collider[32];

        public void Initialize(
            WeaponBase ownerWeapon,
            AttackInfoArgs attackInfo,
            DamageType damageType,
            float splashRadius,
            Action<WeaponBase, AttackResolvedEventArgs> onResolved)
        {
            if (ownerWeapon == null)
            {
                throw new ArgumentNullException(nameof(ownerWeapon));
            }

            if (attackInfo.Attacker == null)
            {
                throw new ArgumentException("AttackInfoArgs.Attacker must be assigned.", nameof(attackInfo));
            }

            if (attackInfo.Target == null)
            {
                throw new ArgumentException("AttackInfoArgs.Target must be assigned.", nameof(attackInfo));
            }

            if (attackInfo.WeaponSnapshot == null)
            {
                throw new ArgumentException("AttackInfoArgs.WeaponSnapshot must be assigned.", nameof(attackInfo));
            }

            if (isInitialized)
            {
                throw new InvalidOperationException("HomingProjectile has already been initialized.");
            }

            this.ownerWeapon = ownerWeapon;
            this.attackInfo = attackInfo;
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

            Damageable liveTarget = GetLiveTarget();
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

        private Damageable GetLiveTarget()
        {
            Damageable target = attackInfo.Target;
            if (target == null)
            {
                return null;
            }

            if (AttackCapable.GetTargetRejectReason(attackInfo.AttackerTeam, target, true, out _, out _) != AttackRejectReason.None)
            {
                return null;
            }

            return target;
        }

        private void MoveTowards(Vector3 destination)
        {
            Vector3 toDestination = destination - transform.position;
            if (toDestination.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toDestination.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * UnityEngine.Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * UnityEngine.Time.deltaTime);
        }

        private void ResolveSucceeded(Damageable liveTarget, Vector3 impactPoint)
        {
            if (hasResolved)
            {
                return;
            }

            List<AttackHitEntry> hits = ApplyDamageAtImpact(impactPoint, liveTarget);
            SpawnImpactEffect(impactPoint);
            Resolve(new AttackResolvedEventArgs(
                attackInfo,
                AttackResolveStatus.Succeeded,
                hits,
                impactPoint));
        }

        private void ResolveTargetLost()
        {
            if (hasResolved)
            {
                return;
            }

            List<AttackHitEntry> hits = ApplyDamageAtImpact(lastKnownImpactPoint, null);
            SpawnImpactEffect(lastKnownImpactPoint);
            Resolve(new AttackResolvedEventArgs(
                attackInfo,
                AttackResolveStatus.TargetLost,
                hits,
                lastKnownImpactPoint));
        }

        private void SpawnImpactEffect(Vector3 impactPoint)
        {
            if (impactEffectPrefab != null)
            {
                GameObject effectInstance = Instantiate(impactEffectPrefab, impactPoint, Quaternion.identity);
                if (impactEffectLifetime > 0f)
                {
                    Destroy(effectInstance, impactEffectLifetime);
                }
            }

            if (impactAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(impactAudioClip, impactPoint, impactAudioVolume);
            }
        }

        private List<AttackHitEntry> ApplyDamageAtImpact(Vector3 impactPoint, Damageable primaryTarget)
        {
            List<AttackHitEntry> hits = new(primaryTarget != null ? 4 : 3);

            if (primaryTarget != null)
            {
                TryApplyHit(primaryTarget, impactPoint, isPrimaryTarget: true, hits);
            }

            if (damageType == DamageType.Single || splashRadius <= 0f)
            {
                return hits;
            }

            foreach (Damageable splashTarget in CollectSplashTargets(impactPoint, primaryTarget))
            {
                TryApplyHit(splashTarget, impactPoint, isPrimaryTarget: false, hits);
            }

            return hits;
        }

        private List<Damageable> CollectSplashTargets(Vector3 impactPoint, Damageable primaryTarget)
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(impactPoint, splashRadius, splashOverlapResults);
            HashSet<Damageable> seenTargets = new();
            if (primaryTarget != null)
            {
                seenTargets.Add(primaryTarget);
            }

            List<Damageable> splashTargets = new(overlapCount);

            for (int i = 0; i < overlapCount; i++)
            {
                Collider overlapCollider = splashOverlapResults[i];
                splashOverlapResults[i] = null;

                if (overlapCollider == null)
                {
                    continue;
                }

                Damageable splashTarget = overlapCollider.GetComponentInParent<Damageable>();
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

        private bool IsValidSplashTarget(Damageable splashTarget)
        {
            return splashTarget != null &&
                splashTarget.gameObject != attackInfo.Attacker.gameObject &&
                AttackCapable.GetTargetRejectReason(attackInfo.AttackerTeam, splashTarget, true, out _, out _) == AttackRejectReason.None;
        }

        private void TryApplyHit(Damageable target, Vector3 impactPoint, bool isPrimaryTarget, List<AttackHitEntry> hits)
        {
            if (target == null || target.gameObject == attackInfo.Attacker.gameObject)
            {
                return;
            }

            if (AttackCapable.GetTargetRejectReason(attackInfo.AttackerTeam, target, true, out _, out _) != AttackRejectReason.None)
            {
                return;
            }

            float damage = GetDamageAmount(target, impactPoint, isPrimaryTarget);

            if (damage <= 0f)
            {
                return;
            }

            target.TakeDamage(attackInfo.Attacker.gameObject, damage);
            hits.Add(new AttackHitEntry(target, damage, impactPoint));
        }

        private float GetDamageAmount(Damageable target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            switch (damageType)
            {
                case DamageType.Single:
                    return GetSingleDamage(isPrimaryTarget);
                case DamageType.SplashEqual:
                    return GetSplashEqualDamage(target, impactPoint, isPrimaryTarget);
                case DamageType.SplashFalloff:
                    return GetSplashFalloffDamage(target, impactPoint, isPrimaryTarget);
                default:
                    return 0f;
            }
        }

        private float GetSingleDamage(bool isPrimaryTarget)
        {
            return isPrimaryTarget ? attackInfo.WeaponSnapshot.AttackPower : 0f;
        }

        private float GetSplashEqualDamage(Damageable target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            if (isPrimaryTarget)
            {
                return attackInfo.WeaponSnapshot.AttackPower;
            }

            if (splashRadius <= 0f)
            {
                return 0f;
            }

            return target.GetDistanceSqrTo(impactPoint) <= splashRadius * splashRadius
                ? attackInfo.WeaponSnapshot.AttackPower
                : 0f;
        }

        private float GetSplashFalloffDamage(Damageable target, Vector3 impactPoint, bool isPrimaryTarget)
        {
            if (isPrimaryTarget)
            {
                return attackInfo.WeaponSnapshot.AttackPower;
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

            float falloff = 1f - Mathf.Clamp01(distance / splashRadius);
            return attackInfo.WeaponSnapshot.AttackPower * falloff;
        }

        private void Resolve(AttackResolvedEventArgs resolvedArgs)
        {
            if (hasResolved)
            {
                return;
            }

            hasResolved = true;
            onResolved?.Invoke(ownerWeapon, resolvedArgs);
            Destroy(gameObject);
        }
    }
}
