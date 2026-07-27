using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Weapons;
using UnityEngine;

namespace ElementalDef.Gameplay.Entities
{
    [DisallowMultipleComponent]
    public sealed class TowerRegistry : MonoBehaviour
    {
        [SerializeField] private TowerUnit[] initialTowers = Array.Empty<TowerUnit>();
        [SerializeField] private ElementalDamageCalculator elementalDamageCalculator;

        private readonly HashSet<TowerUnit> towers = new();
        private bool isShutdown;

        public IReadOnlyCollection<TowerUnit> Towers => towers;

        private void Awake()
        {
            if (elementalDamageCalculator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerRegistry)} requires an {nameof(ElementalDamageCalculator)} reference.");
            }

            if (initialTowers == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerRegistry)} requires an initial {nameof(TowerUnit)} array.");
            }

            HashSet<TowerUnit> validatedInitialTowers = new();
            for (int i = 0; i < initialTowers.Length; i++)
            {
                TowerUnit initialTower = initialTowers[i];
                if (initialTower == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(TowerRegistry)} has a missing {nameof(TowerUnit)} reference at index {i}.");
                }

                if (!validatedInitialTowers.Add(initialTower))
                {
                    throw new InvalidOperationException(
                        $"{initialTower.name} is registered more than once in {nameof(initialTowers)}.");
                }
            }

            foreach (TowerUnit initialTower in initialTowers)
            {
                RegisterTower(initialTower);
            }
        }

        private void OnDestroy()
        {
            foreach (TowerUnit tower in towers)
            {
                if (tower != null)
                {
                    tower.OnDestroyed.RemoveListener(HandleTowerDestroyed);
                }
            }

            towers.Clear();
        }

        public void RegisterTower(TowerUnit tower)
        {
            if (tower == null)
            {
                throw new ArgumentNullException(nameof(tower));
            }

            if (isShutdown)
            {
                throw new InvalidOperationException(
                    $"Cannot register {tower.name} after {nameof(TowerRegistry)} has shut down.");
            }

            if (towers.Contains(tower))
            {
                throw new InvalidOperationException($"{tower.name} is already registered.");
            }

            if (!tower.TryGetComponent(out ElementalCombatant _))
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] Registered tower requires an {nameof(ElementalCombatant)} component.");
            }

            if (!tower.TryGetComponent(out ElementalWeaponBase elementalWeapon))
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] Registered tower requires an {nameof(ElementalWeaponBase)} component.");
            }

            elementalWeapon.Initialize(elementalDamageCalculator);

            towers.Add(tower);
            tower.OnDestroyed.AddListener(HandleTowerDestroyed);
        }

        public bool UnregisterTower(TowerUnit tower)
        {
            if (tower == null)
            {
                throw new ArgumentNullException(nameof(tower));
            }

            if (!towers.Remove(tower))
            {
                return false;
            }

            tower.OnDestroyed.RemoveListener(HandleTowerDestroyed);
            return true;
        }

        public void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }

            isShutdown = true;

            TowerUnit[] towerSnapshot = new TowerUnit[towers.Count];
            towers.CopyTo(towerSnapshot);

            foreach (TowerUnit tower in towerSnapshot)
            {
                if (tower != null)
                {
                    tower.Shutdown();
                }
            }
        }

        private void HandleTowerDestroyed(GameObject sender)
        {
            if (sender == null || !sender.TryGetComponent(out TowerUnit destroyedTower))
            {
                return;
            }

            UnregisterTower(destroyedTower);
        }
    }
}
