using System;
using DefCore.Gameplay.Combat.Weapons;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Weapons
{
    public abstract class ElementalWeaponBase : WeaponBase
    {
        [SerializeField] private ElementType weaponElement = ElementType.Neutral;

        private ElementalDamageCalculator damageCalculator;

        public ElementType AttackElement => weaponElement;

        protected virtual void Awake()
        {
            if (!Enum.IsDefined(typeof(ElementType), weaponElement))
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalWeaponBase)} has an undefined {nameof(AttackElement)} value: {(int)weaponElement}.");
            }
        }

        public void Initialize(ElementalDamageCalculator calculator)
        {
            if (calculator == null)
            {
                throw new ArgumentNullException(nameof(calculator));
            }

            if (damageCalculator == calculator)
            {
                return;
            }

            if (damageCalculator != null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalWeaponBase)} is already initialized with " +
                    $"{nameof(ElementalDamageCalculator)} '{damageCalculator.name}'.");
            }

            damageCalculator = calculator;
        }

        public void Initialize(float attackPower, float attackRange, float attackCooldown, ElementType element)
        {
            Initialize(attackPower, attackRange, attackCooldown);
            weaponElement = element;
        }

        protected float CalculateElementalDamage(
            float baseAttackPower,
            float skillMultiplier,
            ElementalCombatant defender,
            Vector3 attackerWorldPosition)
        {
            if (damageCalculator == null)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalWeaponBase)} must be initialized with an " +
                    $"{nameof(ElementalDamageCalculator)} before it can calculate damage.");
            }

            return damageCalculator.CalculateDamage(
                baseAttackPower,
                skillMultiplier,
                weaponElement,
                attackerWorldPosition,
                defender);
        }
    }
}
