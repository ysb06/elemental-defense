using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Skills;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Entities.Settings;
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
        public TowerUnitEvent OnTowerRegistered = new();
        public TowerUnitEvent OnTowerUnregistered = new();

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
            InitializeSkill(tower, elementalWeapon);

            towers.Add(tower);
            tower.OnDestroyed.AddListener(HandleTowerDestroyed);
            OnTowerRegistered?.Invoke(tower.gameObject);
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
            OnTowerUnregistered?.Invoke(tower.gameObject);
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

        private void InitializeSkill(TowerUnit tower, ElementalWeaponBase elementalWeapon)
        {
            TowerSkillController[] controllers = tower.GetComponents<TowerSkillController>();
            SkillExecutorBase[] executors = tower.GetComponents<SkillExecutorBase>();
            TowerUnitSpec towerSpec = tower.Spec;
            SkillDefinition skillDefinition = towerSpec != null ? towerSpec.Skill : null;

            if (skillDefinition == null)
            {
                if (controllers.Length != 0 || executors.Length != 0)
                {
                    throw new InvalidOperationException(
                        $"[{tower.name}] A tower without a skill definition cannot have a " +
                        $"{nameof(TowerSkillController)} or {nameof(SkillExecutorBase)} component.");
                }

                return;
            }

            if (controllers.Length != 1)
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] Skill '{skillDefinition.SkillId}' requires exactly one " +
                    $"{nameof(TowerSkillController)}, but found {controllers.Length}.");
            }

            if (executors.Length != 1)
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] Skill '{skillDefinition.SkillId}' requires exactly one " +
                    $"{nameof(SkillExecutorBase)}, but found {executors.Length}.");
            }

            if (!tower.TryGetComponent(out DefCore.Gameplay.Combat.Attacker attacker) ||
                attacker.EquippedWeapon != elementalWeapon)
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] A configured skill requires the registered " +
                    $"{nameof(ElementalWeaponBase)} to be the attacker's equipped weapon.");
            }

            TowerSkillController controller = controllers[0];
            if (controller.Executor != executors[0])
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] {nameof(TowerSkillController)} does not reference the " +
                    $"{nameof(SkillExecutorBase)} configured on the same tower.");
            }

            if (tower.SkillController != controller)
            {
                throw new InvalidOperationException(
                    $"[{tower.name}] {nameof(TowerUnit)} does not reference its configured " +
                    $"{nameof(TowerSkillController)}.");
            }

            controller.Initialize(tower, skillDefinition, elementalDamageCalculator);
        }
    }
}
