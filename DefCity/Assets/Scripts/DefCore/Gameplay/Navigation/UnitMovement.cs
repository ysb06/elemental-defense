using System;
using UnityEngine;
using UnityEngine.AI;
using DefCore.Gameplay.World;
using UnityEngine.Events;

namespace DefCore.Gameplay.Navigation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitMovement : MonoBehaviour
    {
        [SerializeField] private CellSpace cellManager;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private float navSampleRadius = 0.4f;

        private bool hasActiveMovement;
        private bool isMovingReady;
        private bool isPaused;
        private int acceptedMovementFrame = -1;
        private UnitMovementEventArgs activeMovementArgs;

        public bool HasActiveMovement => hasActiveMovement;
        public bool IsPaused => isPaused;

        public UnitMovementEvent OnMovingRequested = new();
        public UnitMovementEvent OnMovingFailed = new();
        public UnitMovementEvent OnMovingStart = new();
        public UnitMovementEvent OnMovingComplete = new();
        public UnitMovementEvent OnMovingStopped = new();
        public UnitMovementEvent OnMovingReady = new();

        public void Initialize(CellSpace cellManager)
        {
            if (cellManager == null)
            {
                throw new ArgumentNullException(nameof(cellManager), "CellSpace cannot be null.");
            }

            bool isCellManagerNotExisted = this.cellManager == null;
            this.cellManager = cellManager;
            if (isCellManagerNotExisted)
            {
                PublishMovingReady(default);
            }
        }

        public void Initialize(float speed, float acceleration, float angularSpeed, float stoppingDistance)
        {
            if (navMeshAgent == null)
            {
                throw new InvalidOperationException("NavMeshAgent is not assigned.");
            }

            navMeshAgent.speed = speed;
            navMeshAgent.angularSpeed = angularSpeed;
            navMeshAgent.acceleration = acceleration;
            navMeshAgent.stoppingDistance = stoppingDistance;
        }

        private void OnEnable()
        {
            isPaused = false;
            StopAgent();
            if (cellManager != null)
            {
                PublishMovingReady(default);
            }
        }

        private void OnDisable()
        {
            isMovingReady = false;
            Stop(publishReady: false);
        }

        public void MoveToCell(Vector2Int cellCoordinates)
        {
            if (cellManager == null)
            {
                Debug.LogError($"CellSpace is not assigned. Cannot move to cell {cellCoordinates}.");
                return;
            }

            isMovingReady = false;

            if (cellManager.TryGetCell(cellCoordinates, out CellRef cellRef))
            {
                MoveToCell(cellRef);
            }
            else
            {
                PublishMovingToCellFailed(cellCoordinates);
            }
        }

        public void MoveToCell(CellRef cell)
        {
            isMovingReady = false;

            if (!cell.IsValid)
            {
                PublishMovingToCellFailed(cell.Coordinates);
                return;
            }

            Vector3 targetPosition = cell.SurfaceCenter;
            Move(targetPosition, cell);
        }

        private void PublishMovingToCellFailed(Vector2Int cellCoordinates)
        {
            Debug.LogWarning($"CellRef is invalid. Cannot move to cell {cellCoordinates}.");

            UnitMovementEventArgs movementArgs = new()
            {
                State = UnitMovementState.IsValidating,
                PathStatus = null,
                TargetPosition = default,
                TargetCellCoordinates = cellCoordinates,
                TargetCell = default
            };
            OnMovingRequested?.Invoke(gameObject, movementArgs);

            movementArgs.State = UnitMovementState.InvalidTarget;
            OnMovingFailed?.Invoke(gameObject, movementArgs);
            PublishMovingReady(movementArgs);
        }

        private void Move(Vector3 worldPosition, CellRef targetCell = default)
        {
            UnitMovementEventArgs movementArgs = new()
            {
                State = UnitMovementState.IsValidating,
                PathStatus = null,
                TargetPosition = worldPosition,
                TargetCellCoordinates = targetCell.Coordinates,
                TargetCell = targetCell
            };
            OnMovingRequested?.Invoke(gameObject, movementArgs);

            NavMeshQueryFilter navMeshFilter = new()
            {
                agentTypeID = navMeshAgent.agentTypeID,
                areaMask = navMeshAgent.areaMask
            };
            if (!NavMesh.SamplePosition(
                worldPosition,
                out NavMeshHit navMeshHit,
                navSampleRadius,
                navMeshFilter
                ))
            {
                Debug.LogError($"Failed to find nav mesh position for {worldPosition}");
                movementArgs.State = UnitMovementState.NavMeshSampleFailed;
                OnMovingFailed?.Invoke(gameObject, movementArgs);
                PublishMovingReady(movementArgs);
                return;
            }

            NavMeshPath calculatedPath = new();
            if (!NavMesh.CalculatePath(
                navMeshAgent.nextPosition,
                navMeshHit.position,
                navMeshFilter,
                calculatedPath
                ))
            {
                movementArgs.State = UnitMovementState.PathNotFound;
                OnMovingFailed?.Invoke(gameObject, movementArgs);
                PublishMovingReady(movementArgs);
                return;
            }

            switch (calculatedPath.status)
            {
                case NavMeshPathStatus.PathPartial:
                case NavMeshPathStatus.PathInvalid:
                    Debug.LogWarning($"Calculated path to {navMeshHit.position} is invalid or partial.");
                    movementArgs.State = UnitMovementState.PathNotFound;
                    movementArgs.PathStatus = calculatedPath.status;
                    OnMovingFailed?.Invoke(gameObject, movementArgs);
                    PublishMovingReady(movementArgs);
                    return;
            }

            if (!navMeshAgent.SetPath(calculatedPath))
            {
                Debug.LogError($"Failed to set path for {gameObject.name} to {navMeshHit.position}");
                movementArgs.State = UnitMovementState.DestinationRejected;
                OnMovingFailed?.Invoke(gameObject, movementArgs);
                PublishMovingReady(movementArgs);
                return;
            }

            movementArgs.State = UnitMovementState.IsMoving;
            movementArgs.PathStatus = calculatedPath.status;

            activeMovementArgs = movementArgs;
            hasActiveMovement = true;
            isMovingReady = false;
            acceptedMovementFrame = Time.frameCount;
            navMeshAgent.autoBraking = false;
            navMeshAgent.isStopped = false;
            isPaused = false;

            OnMovingStart?.Invoke(gameObject, movementArgs);
        }

        public bool TryPause()
        {
            if (isPaused || !CanControlActiveMovement())
            {
                return false;
            }

            navMeshAgent.isStopped = true;
            isPaused = true;
            return true;
        }

        public bool TryResume()
        {
            if (!isPaused ||
                !CanControlActiveMovement() ||
                navMeshAgent.pathPending ||
                !navMeshAgent.hasPath ||
                navMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            navMeshAgent.isStopped = false;
            isPaused = false;
            return true;
        }

        public void Stop()
        {
            Stop(publishReady: true);
        }

        private void Stop(bool publishReady)
        {
            bool stoppedActiveMovement = hasActiveMovement;
            UnitMovementEventArgs stoppedMovementArgs = activeMovementArgs;

            activeMovementArgs = default;
            hasActiveMovement = false;
            isPaused = false;
            acceptedMovementFrame = -1;
            StopAgent();

            if (stoppedActiveMovement)
            {
                stoppedMovementArgs.State = UnitMovementState.IsStopped;
                OnMovingStopped?.Invoke(gameObject, stoppedMovementArgs);
                if (publishReady)
                {
                    PublishMovingReady(stoppedMovementArgs);
                }
            }
        }

        private void Update()
        {
            if (!hasActiveMovement ||
                isPaused ||
                Time.frameCount <= acceptedMovementFrame ||
                navMeshAgent.pathPending)
            {
                return;
            }

            float remainingDistance = navMeshAgent.remainingDistance;
            float arrivalDistance = Mathf.Max(navMeshAgent.radius, navMeshAgent.stoppingDistance);
            bool hasValidRemainingDistance = !float.IsInfinity(remainingDistance) && !float.IsNaN(remainingDistance);
            bool hasReachedDestination = hasValidRemainingDistance && remainingDistance <= arrivalDistance;

            if (hasReachedDestination)
            {
                // 목적지에 도착 시, Complete Active Movement
                if (!hasActiveMovement)
                {
                    return;
                }

                UnitMovementEventArgs completedMovementArgs = activeMovementArgs;

                activeMovementArgs = default;
                hasActiveMovement = false;
                isPaused = false;
                acceptedMovementFrame = -1;

                // Keep the current path so a command issued synchronously from
                // OnMovingReady can replace it without losing the agent's velocity.
                // When no command follows, the agent naturally brakes on this path.
                navMeshAgent.autoBraking = true;

                completedMovementArgs.State = UnitMovementState.IsCompleted;
                OnMovingComplete?.Invoke(gameObject, completedMovementArgs);
                PublishMovingReady(completedMovementArgs);
            }
        }

        private void PublishMovingReady(UnitMovementEventArgs movementArgs)
        {
            if (isMovingReady || !isActiveAndEnabled || hasActiveMovement)
            {
                return;
            }

            isMovingReady = true;
            OnMovingReady?.Invoke(gameObject, movementArgs);
        }

        private bool CanControlActiveMovement()
        {
            return hasActiveMovement &&
                isActiveAndEnabled &&
                navMeshAgent != null &&
                navMeshAgent.isActiveAndEnabled &&
                navMeshAgent.isOnNavMesh;
        }

        private void StopAgent()
        {
            if (navMeshAgent == null ||
                !navMeshAgent.isActiveAndEnabled ||
                !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.autoBraking = true;
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }
    }

    public enum UnitMovementState
    {
        None,
        IsValidating,
        IsMoving,
        InvalidTarget,
        PathNotFound,
        NavMeshSampleFailed,
        DestinationRejected,
        IsStopped,
        IsCompleted,
    }

    public struct UnitMovementEventArgs
    {
        public UnitMovementState State;
        public NavMeshPathStatus? PathStatus;
        public Vector3 TargetPosition;
        public Vector2Int TargetCellCoordinates;
        public CellRef TargetCell;
    }

    [Serializable]
    public class UnitMovementEvent : UnityEvent<GameObject, UnitMovementEventArgs> { }
}
