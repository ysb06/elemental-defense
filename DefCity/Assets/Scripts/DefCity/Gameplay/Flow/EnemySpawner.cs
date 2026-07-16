using System;
using System.Collections.Generic;
using UnityEngine;
using DefCity.Gameplay.AI;
using DefCity.Gameplay.City;
using DefCity.Gameplay.City.Construction;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Entities;
using DefCity.Gameplay.Navigation;
using DefCity.Gameplay.World;

namespace DefCity.Gameplay.Flow
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpawnPlace))]
    [RequireComponent(typeof(PlacementValidator))]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private WaveSchedule waveSchedule;
        [SerializeField] private CityCenter cityCenter;
        [SerializeField] private SpawnPlace spawnPlace;
        [SerializeField] private PlacementValidator placementValidator;

        // Reference for enemys
        [SerializeField] private Team enemyTeam;
        [SerializeField] private TerrainCellManager terrainCellManager;

        private readonly Queue<Entity> spawnQueue = new();
        private readonly List<TerrainCell> shuffledSpawnCells = new();

        public int PendingCount => spawnQueue.Count;
        public bool HasPendingSpawns => spawnQueue.Count > 0;

        private void OnEnable()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnChanged.AddListener(OnTurnChanged);
            }
        }

        private void OnDisable()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnChanged.RemoveListener(OnTurnChanged);
            }
        }

        public void Enqueue(Entity entityPrefab)
        {
            spawnQueue.Enqueue(entityPrefab);
        }

        public void EnqueueWaveTurn(int turn)
        {
            if (waveSchedule == null)
            {
                return;
            }

            foreach (Entity entity in waveSchedule.GetEntitiesForTurn(turn))
            {
                Enqueue(entity);
            }
        }

        public int TrySpawnQueuedEnemies()
        {
            int spawnedCount = 0;
            spawnPlace.RecalculateSpawnableCells();

            while (spawnQueue.Count > 0)
            {
                Entity entityPrefab = spawnQueue.Peek();
                if (!IsValidEntityPrefab(entityPrefab))
                {
                    spawnQueue.Dequeue();
                    continue;
                }

                if (!TrySpawn(entityPrefab))
                {
                    break;
                }

                spawnQueue.Dequeue();
                spawnedCount++;
                spawnPlace.RecalculateSpawnableCells();
            }

            return spawnedCount;
        }

        private void OnTurnChanged(GameObject sender, TurnChangedEventArgs args)
        {
            EnqueueWaveTurn(args.CurrentTurn);
            TrySpawnQueuedEnemies();
        }

        private bool TrySpawn(Entity entityPrefab)
        {
            PrepareShuffledSpawnCells();

            if (shuffledSpawnCells.Count == 0)
            {
                Debug.LogWarning($"{name} has no spawnable cells for {entityPrefab.name}. Spawn will retry next turn.", this);
                return false;
            }

            string lastFailureReason = string.Empty;
            foreach (TerrainCell cell in shuffledSpawnCells)
            {
                Vector3 position = cell.Center;
                position.y = cell.AverageWorldHeight;
                Quaternion rotation = Quaternion.identity;

                if (!placementValidator.CanPlace(entityPrefab.gameObject, position, rotation, out lastFailureReason))
                {
                    continue;
                }

                Entity spawnedEntity = Instantiate(entityPrefab, position, rotation);
                ConfigureSpawnedEntity(spawnedEntity);
                spawnedEntity.gameObject.SetActive(true);
                return true;
            }

            Debug.LogWarning(
                $"{name} could not place {entityPrefab.name} in any spawnable cell. Last failure: {lastFailureReason}",
                this);
            return false;
        }

        private void ConfigureSpawnedEntity(Entity spawnedEntity)
        {
            Vector2Int targetCellCoordinates = GetCityCenterTargetCellCoordinates();

            spawnedEntity.Team = enemyTeam;

            if (spawnedEntity.TryGetComponent<Movable>(out var movable))
            {
                movable.TargetCellCoordinates = targetCellCoordinates;
                movable.TerrainCellManager = terrainCellManager;
            }

            if (spawnedEntity.TryGetComponent<BaseCombatController>(out var combatController))
            {
                combatController.TerrainCellManager = terrainCellManager;
            }

            if (spawnedEntity.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                enemyAI.SetCityCenter(cityCenter, terrainCellManager);

                if (Application.isPlaying)
                {
                    enemyAI.MoveToConfiguredTarget();
                }

                return;
            }

            if (Application.isPlaying)
            {
                movable.MoveToCell();
            }
        }

        private void PrepareShuffledSpawnCells()
        {
            shuffledSpawnCells.Clear();
            foreach (TerrainCell cell in spawnPlace.SpawnableCells)
            {
                shuffledSpawnCells.Add(cell);
            }

            for (int i = shuffledSpawnCells.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (shuffledSpawnCells[i], shuffledSpawnCells[swapIndex]) = (shuffledSpawnCells[swapIndex], shuffledSpawnCells[i]);
            }
        }

        private bool IsValidEntityPrefab(Entity entityPrefab)
        {
            if (entityPrefab == null)
            {
                Debug.LogError($"{name} received a null entity prefab. It will be removed from the spawn queue.", this);
                return false;
            }

            if (entityPrefab.GetComponent<Movable>() == null)
            {
                Debug.LogError($"{entityPrefab.name} requires a Movable component to be spawned by {name}.", entityPrefab);
                return false;
            }

            return true;
        }

        private Vector2Int GetCityCenterTargetCellCoordinates()
        {
            if (cityCenter == null)
            {
                throw new InvalidOperationException($"{name} requires a CityCenter target.");
            }

            if (terrainCellManager == null)
            {
                throw new InvalidOperationException($"{name} requires a TerrainCellManager.");
            }

            TerrainCell targetCell = terrainCellManager.GetTerrainCell(cityCenter.CurrentPosition);
            return new Vector2Int(targetCell.RefPosition.x, targetCell.RefPosition.y);
        }
    }
}
