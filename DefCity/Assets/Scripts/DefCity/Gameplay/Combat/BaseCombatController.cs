using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DefCity.Gameplay.Entities;
using DefCity.Gameplay.Navigation;
using DefCity.Gameplay.World;
using DefCore.Gameplay.Combat;

namespace DefCity.Gameplay.Combat
{
    public enum CombatState
    {
        Searching,
        Attacking
    }

    /// <summary>
    /// 기본적인 전투 행동을 관리하는 AI. 공격 가능한 타겟을 주기적으로 검색하여, 발견 시 공격 상태로 전환합니다. 타겟이 사망하거나 공격 범위를 벗어나면 다시 검색 상태로 돌아갑니다.
    /// </summary>
    [RequireComponent(typeof(Entity))]
    public class BaseCombatController : MonoBehaviour
    {
        [SerializeField] private Entity entity;
        [SerializeField] private float scanInterval = 0.5f;
        private WaitForSeconds _waitForInterval = new(0.5f);
        public float ScanInterval
        {
            get => scanInterval;
            set
            {
                scanInterval = value;
                _waitForInterval = new WaitForSeconds(scanInterval);
            }
        }
        [SerializeField] private Movable movable;
        [SerializeField] private AttackCapable attacker;
        [SerializeField] private Health damageable;
        [SerializeField] private TerrainCellManager terrainCellManager;
        public TerrainCellManager TerrainCellManager
        {
            get => terrainCellManager;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (terrainCellManager == null)
                {
                    terrainCellManager = value;
                    if (hasStarted)
                    {
                        TryStartCombatLoop();
                    }
                }
                else
                {
                    Debug.LogWarning($"TerrainCellManager is already set for {gameObject.name}. Changing it is not allowed.");
                }
            }
        }
        [SerializeField] private LayerMask targetLayerMask;
        public float ScanRadius => attacker.EquippedWeapon.AttackRange + 0.1f;
        [SerializeField] private Health currentTarget;
        public Health CurrentTarget => currentTarget;
        [SerializeField] private CombatState currentState = CombatState.Searching;
        public CombatState CurrentState
        {
            get => currentState;
            private set
            {
                if (currentState != value)
                {
                    currentState = value;
                    OnStateChanged.Invoke(gameObject, currentState);
                }
            }
        }

        private readonly Collider[] scanBuffer = new Collider[32];
        private Coroutine combatLoopRoutine;
        private bool hasStarted;
        private bool wasMovingBeforeAttack;
        public ControllerEvent OnStateChanged = new();


        private void Awake()
        {
            ScanInterval = scanInterval;

            // Keep existing scene behavior even before the new layer mask is assigned in the inspector.
            if (targetLayerMask == 0)
            {
                targetLayerMask = LayerMask.GetMask("Game Entity");
            }
        }

        private void OnEnable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged.AddListener(OnDamaged);
            }

            if (hasStarted)
            {
                TryStartCombatLoop();
            }
        }

        private void Start()
        {
            hasStarted = true;
            if (!TryStartCombatLoop())
            {
                Debug.LogError($"{name} cannot start combat loop because required combat references are not assigned.", this);
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged.RemoveListener(OnDamaged);
            }

            if (combatLoopRoutine != null)
            {
                StopCoroutine(combatLoopRoutine);
                combatLoopRoutine = null;
            }

            ClearTargetSubscription();
            currentTarget = null;
            wasMovingBeforeAttack = false;
            CurrentState = CombatState.Searching;
        }

        private bool TryStartCombatLoop()
        {
            if (!isActiveAndEnabled || combatLoopRoutine != null)
            {
                return false;
            }

            if (!CanRunCombatLoop())
            {
                return false;
            }

            combatLoopRoutine = StartCoroutine(LoopCombat());
            return true;
        }

        private bool CanRunCombatLoop()
        {
            return entity != null &&
                entity.Team != null &&
                movable != null &&
                attacker != null &&
                damageable != null &&
                terrainCellManager != null;
        }

        private IEnumerator LoopCombat()
        {
            while (true)
            {
                switch (CurrentState)
                {
                    case CombatState.Searching:
                        Health target = FindClosestHostileTarget();
                        if (target != null)
                        {
                            EnterAttacking(target);
                            yield return null;
                            continue;
                        }

                        yield return _waitForInterval;
                        continue;

                    case CombatState.Attacking:
                        if (currentTarget == null || !currentTarget.IsAlive)
                        {
                            EnterSearching(true);
                            yield return null;
                            continue;
                        }

                        AttackRejectReason attackRejectReason = attacker.GetAttackStartRejectReason(currentTarget);
                        if (attackRejectReason == AttackRejectReason.OnCooldown)
                        {
                            yield return null;
                            continue;
                        }

                        if (attackRejectReason != AttackRejectReason.None)
                        {
                            EnterSearching(true);
                            yield return null;
                            continue;
                        }

                        attacker.TryAttack(currentTarget);
                        yield return null;
                        continue;
                }

                yield return null;
            }
        }

        private void EnterSearching(bool resumeMovement)
        {
            ClearTargetSubscription();
            currentTarget = null;
            CurrentState = CombatState.Searching;

            if (resumeMovement && wasMovingBeforeAttack)
            {
                movable.MoveToCell();
            }

            wasMovingBeforeAttack = false;
        }

        private void EnterAttacking(Health target)
        {
            ClearTargetSubscription();
            wasMovingBeforeAttack = movable.IsMoving;
            currentTarget = target;
            currentTarget.OnDeath.AddListener(OnTargetDeath);
            CurrentState = CombatState.Attacking;
            movable.StopMoving();
        }

        private void ClearTargetSubscription()
        {
            if (currentTarget != null)
            {
                currentTarget.OnDeath.RemoveListener(OnTargetDeath);
            }
        }

        private void OnTargetDeath(GameObject sender, DamageEventArgs args)
        {
            EnterSearching(true);
        }

        private void OnDamaged(GameObject sender, DamageEventArgs args)
        {
            if (!ShouldMoveToDamageInstigator(args.Instigator))
            {
                return;
            }

            if (terrainCellManager == null)
            {
                Debug.LogError($"{name} cannot move toward damage instigator because TerrainCellManager is not assigned.", this);
                return;
            }

            if (movable == null)
            {
                Debug.LogError($"{name} cannot move toward damage instigator because Movable is not assigned.", this);
                return;
            }

            TerrainCell targetCell = terrainCellManager.GetTerrainCell(args.Instigator.transform.position);
            movable.MoveToCell(targetCell);
        }

        private bool ShouldMoveToDamageInstigator(GameObject instigator)
        {
            if (CurrentState != CombatState.Searching)
            {
                return false;
            }

            if (damageable == null || !damageable.IsAlive)
            {
                return false;
            }

            if (instigator == null || instigator == gameObject)
            {
                return false;
            }

            Entity instigatorEntity = instigator.GetComponentInParent<Entity>();
            if (instigatorEntity == null || instigatorEntity == entity)
            {
                return false;
            }

            if (entity == null || entity.Team == null || instigatorEntity.Team == null)
            {
                return false;
            }

            return !entity.Team.IsAlliedWith(instigatorEntity.Team);
        }

        private Health FindClosestHostileTarget()
        {
            Vector3 origin = transform.position;
            float scanRadiusSqr = ScanRadius * ScanRadius;

            int hitCount = Physics.OverlapSphereNonAlloc(
                origin,
                ScanRadius,
                scanBuffer,
                targetLayerMask);

            Health closestTarget = null;
            float closestDistanceSqr = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = scanBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                Health damageable = collider.GetComponentInParent<Health>();
                if (AttackCapable.GetTargetRejectReason(this.entity.Team, damageable, true, out _, out _) != AttackRejectReason.None)
                {
                    continue;
                }

                float distanceSqr = damageable.GetDistanceSqrTo(origin);
                if (distanceSqr > scanRadiusSqr)
                {
                    continue;
                }

                if (distanceSqr >= closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                closestTarget = damageable;
            }

            return closestTarget;
        }
    }

    public class ControllerEvent : UnityEvent<GameObject, CombatState> { }
}
