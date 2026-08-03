using System;
using System.Collections.Generic;
using DefCore.Gameplay.Entities;
using DefCore.Gameplay.Flow;
using DefCore.Gameplay.Navigation;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.AI;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.World;
using UnityEngine;
using UnityEngine.Events;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Gameplay.Flow
{
    public class EnemySpawner : MonoBehaviour
    {
        private enum WaveRuntimeState
        {
            Idle,
            Prepared,
            Running,
            Completed,
            Stopped,
        }

        [SerializeField] private TurnManager turnManager;
        [SerializeField] private CellSpace cellWorld;
        [SerializeField] private Team enemyTeam;
        [SerializeField] private EnemyRoute enemyRoute;
        [SerializeField] private ElementalDamageCalculator elementalDamageCalculator;

        private readonly HashSet<EnemyUnit> activeEnemies = new();
        private WaveSchedule currentWaveSchedule;
        private WaveRuntimeState waveState;
        private int remainingSpawnCount;
        private bool isSubscribedToTurnChanges;

        public EnemySpawnerEvent OnWaveStarted = new();
        public EnemySpawnerEvent OnWaveCompleted = new();

        private void Awake()
        {
            if (elementalDamageCalculator == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemySpawner)} requires an {nameof(ElementalDamageCalculator)} reference.");
            }
        }

        private void OnEnable()
        {
            waveState = WaveRuntimeState.Idle;
            currentWaveSchedule = null;
            remainingSpawnCount = 0;
            activeEnemies.Clear();
        }

        private void OnDisable()
        {
            UnsubscribeFromTurnChanges();

            foreach (EnemyUnit activeEnemy in activeEnemies)
            {
                if (activeEnemy == null)
                {
                    continue;
                }

                activeEnemy.OnDefeated.RemoveListener(HandleEnemyDefeated);
                if (activeEnemy.TryGetComponent(out EnemyRouteFollower activeEnemyFollower))
                {
                    activeEnemyFollower.OnRouteFailed.RemoveListener(HandleEnemyRouteFailed);
                }

                Destroy(activeEnemy.gameObject);
            }

            activeEnemies.Clear();
            currentWaveSchedule = null;
            remainingSpawnCount = 0;
            waveState = WaveRuntimeState.Idle;
        }

        public void PrepareWave(WaveSchedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            if (schedule.Entries.Count == 0)
            {
                throw new InvalidOperationException("A wave schedule must contain at least one entry.");
            }

            if (!isActiveAndEnabled)
            {
                throw new InvalidOperationException($"{nameof(EnemySpawner)} must be active and enabled to prepare a wave.");
            }

            if (turnManager == null)
            {
                throw new InvalidOperationException($"{nameof(EnemySpawner)} requires a {nameof(TurnManager)} reference.");
            }

            if (waveState != WaveRuntimeState.Idle && waveState != WaveRuntimeState.Completed)
            {
                throw new InvalidOperationException($"Cannot prepare a wave while the spawner is {waveState}.");
            }

            if (activeEnemies.Count != 0)
            {
                throw new InvalidOperationException("Cannot prepare a wave while active enemies remain.");
            }

            if (turnManager.CurrentTurn != 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(TurnManager)} must be reset to turn 0 before preparing a wave. " +
                    $"Current turn is {turnManager.CurrentTurn}.");
            }

            currentWaveSchedule = schedule;
            remainingSpawnCount = schedule.Entries.Count;
            waveState = WaveRuntimeState.Prepared;

            SubscribeToTurnChanges();
        }

        public void Shutdown()
        {
            if (waveState == WaveRuntimeState.Stopped)
            {
                return;
            }

            waveState = WaveRuntimeState.Stopped;
            UnsubscribeFromTurnChanges();
            currentWaveSchedule = null;
            remainingSpawnCount = 0;

            foreach (EnemyUnit activeEnemy in activeEnemies)
            {
                if (activeEnemy == null)
                {
                    continue;
                }

                activeEnemy.OnDefeated.RemoveListener(HandleEnemyDefeated);
                if (activeEnemy.TryGetComponent(out EnemyRouteFollower activeEnemyFollower))
                {
                    activeEnemyFollower.OnRouteFailed.RemoveListener(HandleEnemyRouteFailed);
                }

                activeEnemy.Shutdown();
            }
        }

        private void HandleTurnChanged(GameObject sender, TurnChangedEventArgs args)
        {
            if ((waveState != WaveRuntimeState.Prepared && waveState != WaveRuntimeState.Running) ||
                currentWaveSchedule == null)
            {
                return;
            }

            if (currentWaveSchedule.TryGetNextWaveTurn(args.CurrentTurn, out int nextWaveTurn) &&
                nextWaveTurn == args.CurrentTurn)
            {
                Entity entity = currentWaveSchedule.GetEntityForTurn(args.CurrentTurn);
                if (entity != null)
                {
                    Vector3 spawnPosition = cellWorld.GetSurfaceCenter(enemyRoute.EntryCell);
                    GameObject spawnedEntity = Instantiate(entity.gameObject, spawnPosition, Quaternion.identity);

                    EnemyUnit spawnedEnemy;
                    EnemyRouteFollower spawnedEntityFollower;
                    try
                    {
                        spawnedEnemy = GetRequiredComponent<EnemyUnit>(spawnedEntity);
                        spawnedEntityFollower = GetRequiredComponent<EnemyRouteFollower>(spawnedEntity);
                        UnitMovement spawnedEntityMovement = GetRequiredComponent<UnitMovement>(spawnedEntity);
                        Entity spawnedEntityComponent = GetRequiredComponent<Entity>(spawnedEntity);
                        GetRequiredComponent<ElementalCombatant>(spawnedEntity);
                        ElementalWeaponBase elementalWeapon =
                            GetRequiredComponent<ElementalWeaponBase>(spawnedEntity);

                        spawnedEntityComponent.Initialize(enemyTeam);
                        spawnedEntityMovement.Initialize(cellWorld);
                        spawnedEntityFollower.Route = enemyRoute;
                        elementalWeapon.Initialize(elementalDamageCalculator);
                    }
                    catch
                    {
                        spawnedEntity.SetActive(false);
                        Destroy(spawnedEntity);
                        throw;
                    }

                    spawnedEntityFollower.OnRouteFailed.AddListener(HandleEnemyRouteFailed);
                    spawnedEnemy.OnDefeated.AddListener(HandleEnemyDefeated);

                    activeEnemies.Add(spawnedEnemy);
                    remainingSpawnCount--;

                    if (waveState == WaveRuntimeState.Prepared)
                    {
                        waveState = WaveRuntimeState.Running;
                        OnWaveStarted?.Invoke(gameObject);
                    }

                    if (!isActiveAndEnabled || waveState != WaveRuntimeState.Running || spawnedEnemy == null ||
                        !activeEnemies.Contains(spawnedEnemy))
                    {
                        return;
                    }

                    spawnedEntityFollower.FollowRoute();
                }
            }
        }

        private static T GetRequiredComponent<T>(GameObject target) where T : Component
        {
            if (target.TryGetComponent(out T component))
            {
                return component;
            }

            throw new InvalidOperationException(
                $"[{target.name}] Spawned enemy requires a {typeof(T).Name} component.");
        }

        private void HandleEnemyRouteFailed(GameObject sender)
        {
            // 무조건 발생하면 안 되는 상황이므로 예외를 발생시켜서 개발자가 인지하도록 합니다.
            // 의도적으로 어떠한 Fallback도 구현하지 않았습니다.
            throw new InvalidOperationException("Enemy route failed to complete. This should not happen.");
        }

        private void HandleEnemyDefeated(GameObject sender)
        {
            if (sender == null || !sender.TryGetComponent(out EnemyUnit defeatedEnemy) || !activeEnemies.Remove(defeatedEnemy))
            {
                return;
            }

            defeatedEnemy.OnDefeated.RemoveListener(HandleEnemyDefeated);
            if (defeatedEnemy.TryGetComponent(out EnemyRouteFollower defeatedEnemyFollower))
            {
                defeatedEnemyFollower.OnRouteFailed.RemoveListener(HandleEnemyRouteFailed);
            }

            if (waveState != WaveRuntimeState.Running || remainingSpawnCount != 0 || activeEnemies.Count != 0)
            {
                return;
            }

            waveState = WaveRuntimeState.Completed;
            UnsubscribeFromTurnChanges();
            currentWaveSchedule = null;
            OnWaveCompleted?.Invoke(gameObject);
        }

        private void SubscribeToTurnChanges()
        {
            if (isSubscribedToTurnChanges)
            {
                return;
            }

            turnManager.OnTurnChanged.AddListener(HandleTurnChanged);
            isSubscribedToTurnChanges = true;
        }

        private void UnsubscribeFromTurnChanges()
        {
            if (!isSubscribedToTurnChanges)
            {
                return;
            }

            if (turnManager != null)
            {
                turnManager.OnTurnChanged.RemoveListener(HandleTurnChanged);
            }

            isSubscribedToTurnChanges = false;
        }
    }

    [Serializable]
    public class EnemySpawnerEvent : UnityEvent<GameObject> { }
}
