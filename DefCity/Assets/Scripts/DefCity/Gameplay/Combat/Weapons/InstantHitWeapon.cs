using System;
using UnityEngine;
using DefCity.Gameplay.Combat;

namespace DefCity.Gameplay.Combat.Weapons
{
    public class InstantHitWeapon : WeaponBase
    {
        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float attackRange = 11f;
        [SerializeField] private float attackCooldown = 1f;

        public override float AttackPower => attackPower;
        public override float AttackRange => attackRange;
        public override float AttackCooldown => attackCooldown;

        public override bool TryStartAttack(AttackInfoArgs attackInfo, Action<WeaponBase, AttackResolvedEventArgs> onResolved)
        {
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

            if (onResolved == null)
            {
                throw new ArgumentNullException(nameof(onResolved));
            }

            float damage = attackInfo.WeaponSnapshot.AttackPower;
            Vector3 impactPoint = attackInfo.Target.GetClosestPoint(attackInfo.Attacker.transform.position);

            attackInfo.Target.TakeDamage(attackInfo.Attacker.gameObject, damage);
            AttackHitEntry[] hits = { new(attackInfo.Target, damage, impactPoint) };
            onResolved(this, new AttackResolvedEventArgs(
                attackInfo,
                AttackResolveStatus.Succeeded,
                hits,
                impactPoint));

            return true;
        }
    }
}
