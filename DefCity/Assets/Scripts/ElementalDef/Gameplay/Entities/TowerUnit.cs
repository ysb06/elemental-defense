using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Entities;
using ElementalDef.Gameplay.AI;
using UnityEngine;
using UnityEngine.Events;
using ElementalDef.Gameplay.Entities.Settings;
using DefCore.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Economy;
using ElementalDef.Gameplay.Combat.Skills;

namespace ElementalDef.Gameplay.Entities
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity), typeof(Health), typeof(AutoCombatController))]
    [RequireComponent(typeof(HostileTargetScanner), typeof(Attacker))]
    [DefaultExecutionOrder(-1000)]
    public class TowerUnit : MonoBehaviour
    {
        [SerializeField] private TowerUnitSpec spec;
        [SerializeField] private Entity entity;
        [SerializeField] private Health health;
        [SerializeField] private AutoCombatController autoCombatController;
        [SerializeField] private HostileTargetScanner scanner;
        [SerializeField] private Attacker attacker;
        [SerializeField] private bool isShutdown;
        [SerializeField] private ElementalWeaponBase weapon;
        [SerializeField] private ElementalCombatant elementalCombatant;
        [SerializeField] private TowerCost towerCost;
        [SerializeField] private TowerSkillController skillController;
        [SerializeField, Min(0f)] private float deathRemovalDelay = 5.5f;

        public string InstanceId { get; private set; }
        public TowerUnitSpec Spec => spec;
        public TowerSkillController SkillController => skillController;

        public TowerUnitEvent OnDestroyed = new();

        private void Awake()
        {
            InstanceId = Guid.NewGuid().ToString("N");

            entity = entity != null ? entity : GetComponent<Entity>();
            health = health != null ? health : GetComponent<Health>();
            autoCombatController = autoCombatController != null ? autoCombatController : GetComponent<AutoCombatController>();
            scanner = scanner != null ? scanner : GetComponent<HostileTargetScanner>();
            attacker = attacker != null ? attacker : GetComponent<Attacker>();
            weapon = weapon != null ? weapon : GetComponentInChildren<ElementalWeaponBase>();
            elementalCombatant = elementalCombatant != null ? elementalCombatant : GetComponent<ElementalCombatant>();
            towerCost = towerCost != null ? towerCost : GetComponent<TowerCost>();
            skillController = skillController != null ? skillController : GetComponent<TowerSkillController>();

            health.OnDeath.AddListener(HandleDeath);
        }

        private void Start()
        {
            InitializeSpecs();
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
                towerCost.Initialize(spec.Cost);
            }
            else
            {
                Debug.LogWarning($"[{name}] {nameof(TowerUnit)} has no {nameof(TowerUnitSpec)} assigned. Default values will be used.");
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(HandleDeath);
            }

            skillController?.Shutdown();
        }

        private void HandleDeath(GameObject sender, DamageEventArgs args)
        {
            if (args.Victim != gameObject || !entity.TryMarkDead())
            {
                return;
            }

            health.OnDeath.RemoveListener(HandleDeath);

            Shutdown();

            try
            {
                OnDestroyed?.Invoke(gameObject);
            }
            finally
            {
                Destroy(gameObject, deathRemovalDelay);
            }
        }

        public void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }

            isShutdown = true;
            try
            {
                skillController?.Shutdown();
            }
            finally
            {
                autoCombatController.Shutdown();
                scanner.enabled = false;
                attacker.enabled = false;
            }
        }
    }

    [Serializable]
    public class TowerUnitEvent : UnityEvent<GameObject> { }
}
