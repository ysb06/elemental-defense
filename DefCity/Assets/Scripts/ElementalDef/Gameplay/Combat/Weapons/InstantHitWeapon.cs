using System;
using UnityEngine;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;

namespace ElementalDef.Gameplay.Combat.Weapons
{
    public class InstantHitWeapon : ElementalWeaponBase
    {
        private const float NormalAttackSkillMultiplier = 1f;

        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float attackRange = 11f;
        [SerializeField] private float attackCooldown = 1f;

        public override float AttackPower { get { return attackPower; } protected set { attackPower = value; } }
        public override float AttackRange { get { return attackRange; } protected set { attackRange = value; } }
        public override float AttackCooldown { get { return attackCooldown; } protected set { attackCooldown = value; } }

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

            Vector3 attackerPosition = attackInfo.Attacker.transform.position;
            Vector3 impactPoint = attackInfo.Target.GetClosestPoint(attackerPosition);

            ElementalCombatant targetCombatant = attackInfo.Target.GetComponent<ElementalCombatant>();
            if (targetCombatant == null)
            {
                throw new InvalidOperationException(
                    $"Attack target '{attackInfo.Target.name}' has no {nameof(ElementalCombatant)} component.");
            }

            float damage = CalculateElementalDamage(
                attackInfo.WeaponSnapshot.AttackPower,
                NormalAttackSkillMultiplier,
                targetCombatant,
                attackerPosition);

            DamageEventArgs damageEventArgs = attackInfo.Target.TakeDamage(attackInfo.Attacker.gameObject, damage);

            AttackHitEntry[] hits = { new(attackInfo.Target, damageEventArgs.DamageAmount, impactPoint) };
            onResolved(this, new AttackResolvedEventArgs(
                attackInfo,
                AttackResolveStatus.Succeeded,
                hits,
                impactPoint));

            return true;
        }
    }
}
