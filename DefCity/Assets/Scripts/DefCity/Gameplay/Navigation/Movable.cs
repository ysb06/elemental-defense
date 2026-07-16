using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DefCity.Gameplay.Entities;
using DefCity.Gameplay.World;

namespace DefCity.Gameplay.Navigation
{
    /// <summary>
    /// 이동 가능한 엔티티를 관리하는 컴포넌트입니다. NavMeshAgent를 활용하여 지정된 셀로 이동하며, 이동 상태 변경 이벤트를 제공합니다.
    /// </summary>
    [RequireComponent(typeof(Entity))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Movable : MonoBehaviour
    {
        [SerializeField] private TerrainCellManager terrainCellManager;
        public TerrainCellManager TerrainCellManager
        {
            set
            {
                if (terrainCellManager == null)
                {
                    terrainCellManager = value;
                }
                else
                {
                    Debug.LogWarning($"TerrainCellManager is already set for {gameObject.name}. Changing it is not allowed.");
                }
            }
        }
        [SerializeField] private Entity entity;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private float navSampleRadius = 2f;
        [SerializeField] private bool hasTargetCellCoordinates = true;
        [SerializeField] private Vector2Int targetCellCoordinates = new(0, 0);
        public bool HasTargetCellCoordinates
        {
            get { return hasTargetCellCoordinates; }
            set { hasTargetCellCoordinates = value; }
        }
        public Vector2Int TargetCellCoordinates
        {
            get { return targetCellCoordinates; }
            set
            {
                targetCellCoordinates = value;
                hasTargetCellCoordinates = true;
            }
        }
        private bool isMoving;
        public bool IsMoving
        {
            get { return isMoving; }
            set
            {
                isMoving = value;
                OnMovingStateChanged.Invoke(gameObject, isMoving);
            }
        }
        public MovingEvent OnMovingStateChanged = new();

        private void Update()
        {
            if (!IsMoving)
            {
                return;
            }

            if (navMeshAgent.pathPending)
            {
                return;
            }

            bool hasReachedDestination =
                navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance &&
                (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.01f);

            if (!hasReachedDestination)
            {
                return;
            }

            StopMoving();
        }

        public void MoveToCell()
        {
            if (!hasTargetCellCoordinates)
            {
                StopMoving();
                return;
            }

            MoveToCell(targetCellCoordinates);
        }

        public void MoveToCell(Vector2Int cellCoordinates)
        {
            TerrainCell cell = terrainCellManager.GetTerrainCell(cellCoordinates);
            MoveToCell(cell);
        }

        public void MoveToCell(TerrainCell cell)
        {
            ApplyNavigationAreaMask();

            Vector3 targetPosition = cell.Center;
            targetPosition.y = cell.AverageWorldHeight;

            if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit navMeshHit, navSampleRadius, navMeshAgent.areaMask))
            {
                Debug.LogError($"Failed to find nav mesh position for TerrainCell at RefPosition {cell.RefPosition}");
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(navMeshHit.position);
            IsMoving = true;
        }

        public void StopMoving()
        {
            if (navMeshAgent == null)
            {
                IsMoving = false;
                return;
            }

            if (!navMeshAgent.enabled)
            {
                IsMoving = false;
                return;
            }

            if (!navMeshAgent.isOnNavMesh)
            {
                IsMoving = false;
                return;
            }

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            IsMoving = false;
        }

        public void ApplyNavigationAreaMask()
        {
            if (entity == null)
            {
                throw new InvalidOperationException($"{name} requires an Entity to resolve team navigation.");
            }

            if (entity.Team == null)
            {
                throw new InvalidOperationException($"{name} has no Team assigned on its Entity.");
            }

            if (navMeshAgent == null)
            {
                throw new InvalidOperationException($"{name} requires a NavMeshAgent.");
            }

            navMeshAgent.areaMask = TeamNavigationPolicy.BuildAreaMaskForMover(entity.Team.Kind);
        }
    }

    [Serializable]
    public class MovingEvent : UnityEvent<GameObject, bool> { }
}
