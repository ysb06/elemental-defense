using System;
using UnityEngine;
using DefCity.Gameplay.Combat;

namespace DefCity.Gameplay.Combat.Weapons
{
    public class HomingProjectileWeapon : WeaponBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform muzzleTransform;
        [SerializeField] private float splashRadius;
        [SerializeField] private float attackPower = 50f;
        [SerializeField] private float attackRange = 100f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private DamageType damageType = DamageType.SplashFalloff;

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

            if (projectilePrefab == null)
            {
                Debug.LogError($"[{name}] Projectile prefab is not assigned.", this);
                return false;
            }

            if (muzzleTransform == null)
            {
                Debug.LogError($"[{name}] Muzzle transform is not assigned.", this);
                return false;
            }

            if (!projectilePrefab.TryGetComponent(out HomingProjectile _))
            {
                Debug.LogError($"[{name}] Projectile prefab '{projectilePrefab.name}' does not contain a HomingProjectile component.", projectilePrefab);
                return false;
            }

            GameObject projectileObject = Instantiate(projectilePrefab, muzzleTransform.position, muzzleTransform.rotation);
            HomingProjectile projectile = projectileObject.GetComponent<HomingProjectile>();
            DamageType projectileDamageType = GetProjectileDamageType();
            float projectileSplashRadius = projectileDamageType == DamageType.Single ? 0f : Mathf.Max(0f, splashRadius);
            projectile.Initialize(this, attackInfo, projectileDamageType, projectileSplashRadius, onResolved);
            return true;
        }

        private DamageType GetProjectileDamageType()
        {
            if (damageType == DamageType.Single)
            {
                return DamageType.Single;
            }

            return splashRadius > 0f ? damageType : DamageType.Single;
        }
    }
}
