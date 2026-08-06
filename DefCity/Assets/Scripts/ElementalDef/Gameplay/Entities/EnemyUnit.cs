using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Entities;
using DefCore.Gameplay.Navigation;
using ElementalDef.Gameplay.AI;
using UnityEngine;
using UnityEngine.Events;
using ElementalDef.Gameplay.Entities.Settings;
using DefCore.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Combat;

namespace ElementalDef.Gameplay.Entities
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity), typeof(Health), typeof(AutoCombatController))]
    [RequireComponent(typeof(HostileTargetScanner), typeof(Attacker), typeof(EnemyRouteFollower))]
    [RequireComponent(typeof(UnitMovement))]
    [DefaultExecutionOrder(-1000)]
    public class EnemyUnit : MonoBehaviour
    {
        [SerializeField] private EnemyUnitSpec spec;

        [SerializeField] private Entity entity;
        [SerializeField] private Health health;
        [SerializeField] private AutoCombatController autoCombatController;
        [SerializeField] private HostileTargetScanner scanner;
        [SerializeField] private Attacker attacker;
        [SerializeField] private ElementalWeaponBase weapon;
        [SerializeField] private ElementalCombatant elementalCombatant;
        [SerializeField] private UnitMovement movement;
        [SerializeField] private EnemyRouteFollower routeFollower;
        [SerializeField, Min(0f)] private float deathRemovalDelay = 5.5f;

        private bool isShutdown;

        public EnemyUnitSpec Spec => spec;

        public EnemyUnitEvent OnDefeated = new();

        private void Awake()
        {
            entity = entity != null ? entity : GetComponent<Entity>();
            health = health != null ? health : GetComponent<Health>();
            autoCombatController = autoCombatController != null ? autoCombatController : GetComponent<AutoCombatController>();
            scanner = scanner != null ? scanner : GetComponent<HostileTargetScanner>();
            attacker = attacker != null ? attacker : GetComponent<Attacker>();
            weapon = weapon != null ? weapon : GetComponentInChildren<ElementalWeaponBase>();
            elementalCombatant = elementalCombatant != null ? elementalCombatant : GetComponent<ElementalCombatant>();
            movement = movement != null ? movement : GetComponent<UnitMovement>();
            routeFollower = routeFollower != null ? routeFollower : GetComponent<EnemyRouteFollower>();

            InitializeSpecs();

            health.OnDeath.AddListener(HandleDeath);
        }

        private void InitializeSpecs()
        {
            if (spec != null)
            {
                health.Initialize(spec.Defense.MaxHealth);
                elementalCombatant.Initialize(spec.Defense.Element, spec.Defense.Defense);
                weapon.Initialize(spec.Attack.Power, spec.Attack.Range, spec.Attack.Cooldown, spec.Attack.Element);
                scanner.Initialize(
                    spec.Attack.Range + spec.Scanner.AcquisitionPadding,
                    spec.Scanner.Interval);
                movement.Initialize(spec.Movement.Speed, spec.Movement.Acceleration, spec.Movement.AngularSpeed, spec.Movement.StoppingDistance);
            }
            else
            {
                Debug.LogWarning($"[{name}] {nameof(EnemyUnit)} has no {nameof(EnemyUnitSpec)} assigned. Default values will be used.");
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(HandleDeath);
            }
        }

        private void HandleDeath(GameObject sender, DamageEventArgs args)
        {
            if (args.Victim != gameObject || !entity.TryMarkDead())
            {
                return;
            }

            health.OnDeath.RemoveListener(HandleDeath);

            Shutdown();

            OnDefeated?.Invoke(gameObject);
            Destroy(gameObject, deathRemovalDelay);
        }

        public void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }

            isShutdown = true;
            autoCombatController.Shutdown();
            scanner.enabled = false;
            attacker.enabled = false;
            routeFollower.CancelFollowing();
        }
    }

    [Serializable]
    public class EnemyUnitEvent : UnityEvent<GameObject> { }
}
