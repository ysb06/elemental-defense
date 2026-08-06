using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Weapons
{
    public enum HomingProjectileDamageType
    {
        Single,
        SplashEqual,
        SplashFalloff
    }

    public class HomingProjectileWeapon : ElementalWeaponBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform muzzleTransform;
        [SerializeField] private float splashRadius;
        [SerializeField] private float attackPower = 50f;
        [SerializeField] private float attackRange = 100f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private HomingProjectileDamageType damageType = HomingProjectileDamageType.SplashFalloff;

        public override float AttackPower { get { return attackPower; } protected set { attackPower = value; } }
        public override float AttackRange { get { return attackRange; } protected set { attackRange = value; } }
        public override float AttackCooldown { get { return attackCooldown; } protected set { attackCooldown = value; } }

        public override bool TryStartAttack(AttackInfoArgs attackInfo, Action<WeaponBase, AttackResolvedEventArgs> onResolved)
        {
            Vector3 attackerLaunchPosition = attackInfo.Attacker.transform.position;
            GameObject projectileObject = Instantiate(projectilePrefab, muzzleTransform.position, muzzleTransform.rotation);
            HomingProjectile projectile = projectileObject.GetComponent<HomingProjectile>();
            HomingProjectileDamageType projectileDamageType = GetProjectileDamageType();
            float projectileSplashRadius = projectileDamageType == HomingProjectileDamageType.Single ? 0f : Mathf.Max(0f, splashRadius);

            projectile.Initialize(
                this,
                attackInfo,
                attackerLaunchPosition,
                projectileDamageType,
                projectileSplashRadius,
                onResolved);
            return true;
        }

        internal bool TryCreateProjectileHit(
            AttackInfoArgs attackInfo,
            Health target,
            Vector3 attackerLaunchPosition,
            Vector3 impactPoint,
            float damageMultiplier,
            out AttackHitEntry hit)
        {
            hit = default;

            ElementalCombatant targetCombatant = target.GetComponent<ElementalCombatant>();
            if (targetCombatant == null)
            {
                throw new InvalidOperationException(
                    $"Attack target '{target.name}' has no {nameof(ElementalCombatant)} component.");
            }

            float damage = CalculateElementalDamage(attackInfo.WeaponSnapshot.AttackPower, damageMultiplier, targetCombatant, attackerLaunchPosition);

            if (damage <= 0f)
            {
                return false;
            }

            DamageEventArgs damageEventArgs = target.TakeDamage(attackInfo.Attacker.gameObject, damage);
            hit = new AttackHitEntry(target, damageEventArgs.DamageAmount, impactPoint);
            return true;
        }

        private HomingProjectileDamageType GetProjectileDamageType()
        {
            if (damageType == HomingProjectileDamageType.Single)
            {
                return HomingProjectileDamageType.Single;
            }

            return splashRadius > 0f ? damageType : HomingProjectileDamageType.Single;
        }
    }
}
