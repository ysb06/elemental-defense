using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DefCore.Gameplay.Combat;

namespace DefCity.Gameplay.Combat.Weapons
{
    public enum AttackResolveStatus
    {
        None = 0,
        Succeeded = 1,
        Missed = 2,
        TargetLost = 3,
        Cancelled = 4,
        Failed = 5
    }

    public interface IWeapon
    {
        public float AttackPower { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }
    }

    public struct WeaponSnapshot : IWeapon
    {
        public float AttackPower { get; }

        public float AttackRange { get; }

        public float AttackCooldown { get; }

        public WeaponSnapshot(IWeapon weapon)
        {
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon), "Weapon cannot be null when creating a WeaponSnapshot.");
            }
            else
            {
                AttackPower = weapon.AttackPower;
                AttackRange = weapon.AttackRange;
                AttackCooldown = weapon.AttackCooldown;
            }
        }
    }

    [Serializable]
    public readonly struct AttackHitEntry
    {
        public Health Target { get; }
        public float Damage { get; }
        public Vector3 ImpactPoint { get; }

        public AttackHitEntry(Health target, float damage, Vector3 impactPoint)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Target = target;
            Damage = damage;
            ImpactPoint = impactPoint;
        }
    }

    [Serializable]
    public readonly struct AttackResolvedEventArgs
    {
        public AttackInfoArgs Info { get; }
        public AttackResolveStatus ResolveStatus { get; }
        public float TotalDamage { get; }
        public IReadOnlyList<Health> Targets { get; }
        public IReadOnlyList<AttackHitEntry> Hits { get; }
        public Vector3 ImpactPoint { get; }

        public AttackResolvedEventArgs(
            AttackInfoArgs info,
            AttackResolveStatus resolveStatus,
            IReadOnlyList<AttackHitEntry> hits,
            Vector3 impactPoint)
        {
            Info = info;
            ResolveStatus = resolveStatus;
            ImpactPoint = impactPoint;

            if (hits == null || hits.Count == 0)
            {
                TotalDamage = 0f;
                Targets = Array.Empty<Health>();
                Hits = Array.Empty<AttackHitEntry>();
                return;
            }

            AttackHitEntry[] hitArray = new AttackHitEntry[hits.Count];
            Health[] targetArray = new Health[hits.Count];
            float totalDamage = 0f;

            for (int i = 0; i < hits.Count; i++)
            {
                AttackHitEntry hit = hits[i];
                if (hit.Target == null)
                {
                    throw new ArgumentException("AttackResolvedEventArgs cannot contain a hit entry with a null target.", nameof(hits));
                }

                hitArray[i] = hit;
                targetArray[i] = hit.Target;
                totalDamage += hit.Damage;
            }

            TotalDamage = totalDamage;
            Targets = targetArray;
            Hits = hitArray;
        }
    }

    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        public abstract float AttackPower { get; }
        public abstract float AttackRange { get; }
        public abstract float AttackCooldown { get; }

        public abstract bool TryStartAttack(AttackInfoArgs attackInfo, Action<WeaponBase, AttackResolvedEventArgs> onResolved);
    }
}
