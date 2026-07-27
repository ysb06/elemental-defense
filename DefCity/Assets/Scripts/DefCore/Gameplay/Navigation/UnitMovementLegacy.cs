using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DefCore.Gameplay.World;

namespace DefCore.Gameplay.Navigation
{
    public enum MovementTargetKind
    {
        None,
        Coordinates,
        Cell
    }

    public enum MovementFailureReason
    {
        TargetNotConfigured,
        CellNotFound,
        InvalidCell,
        NavMeshSampleNotFound,
        AgentDisabled,
        AgentNotOnNavMesh,
        DestinationRejected,
        PathPartial,
        PathInvalid,
        RequestLocked
    }

    public enum MovementCancellationReason
    {
        ExplicitStop,
        Superseded,
        ComponentDisabled
    }

    [Serializable]
    public readonly struct MovementInfoArgs
    {
        public int MovementId { get; }
        public UnitMovementLegacy Mover { get; }
        public MovementTargetKind TargetKind { get; }
        public bool HasTargetCoordinates => TargetKind == MovementTargetKind.Coordinates;
        public Vector2Int TargetCoordinates { get; }
        public bool HasTargetCell => TargetKind == MovementTargetKind.Cell;
        public CellRef TargetCell { get; }
        public bool HasResolvedCell { get; }
        public CellRef ResolvedCell { get; }
        public bool HasSampledDestination { get; }
        public Vector3 SampledDestination { get; }

        private MovementInfoArgs(
            int movementId,
            UnitMovementLegacy mover,
            MovementTargetKind targetKind,
            Vector2Int targetCoordinates,
            CellRef targetCell,
            bool hasResolvedCell,
            CellRef resolvedCell,
            bool hasSampledDestination,
            Vector3 sampledDestination)
        {
            MovementId = movementId;
            Mover = mover;
            TargetKind = targetKind;
            TargetCoordinates = targetCoordinates;
            TargetCell = targetCell;
            HasResolvedCell = hasResolvedCell;
            ResolvedCell = resolvedCell;
            HasSampledDestination = hasSampledDestination;
            SampledDestination = sampledDestination;
        }

        internal static MovementInfoArgs ForMissingTarget(int movementId, UnitMovementLegacy mover)
        {
            return new MovementInfoArgs(
                movementId,
                mover,
                MovementTargetKind.None,
                default,
                default,
                false,
                default,
                false,
                default);
        }

        internal static MovementInfoArgs ForCoordinates(
            int movementId,
            UnitMovementLegacy mover,
            Vector2Int targetCoordinates)
        {
            return new MovementInfoArgs(
                movementId,
                mover,
                MovementTargetKind.Coordinates,
                targetCoordinates,
                default,
                false,
                default,
                false,
                default);
        }

        internal static MovementInfoArgs ForCell(int movementId, UnitMovementLegacy mover, CellRef targetCell)
        {
            return new MovementInfoArgs(
                movementId,
                mover,
                MovementTargetKind.Cell,
                targetCell.Coordinates,
                targetCell,
                false,
                default,
                false,
                default);
        }

        internal MovementInfoArgs WithResolvedCell(CellRef resolvedCell)
        {
            return new MovementInfoArgs(
                MovementId,
                Mover,
                TargetKind,
                TargetCoordinates,
                TargetCell,
                true,
                resolvedCell,
                HasSampledDestination,
                SampledDestination);
        }

        internal MovementInfoArgs WithSampledDestination(Vector3 sampledDestination)
        {
            return new MovementInfoArgs(
                MovementId,
                Mover,
                TargetKind,
                TargetCoordinates,
                TargetCell,
                HasResolvedCell,
                ResolvedCell,
                true,
                sampledDestination);
        }
    }

    [Serializable]
    public readonly struct MovementRequestedEventArgs
    {
        public MovementInfoArgs Info { get; }

        public MovementRequestedEventArgs(MovementInfoArgs info)
        {
            Info = info;
        }
    }

    [Serializable]
    public readonly struct MovementStartedEventArgs
    {
        public MovementInfoArgs Info { get; }

        public MovementStartedEventArgs(MovementInfoArgs info)
        {
            Info = info;
        }
    }

    [Serializable]
    public readonly struct MovementReachedEventArgs
    {
        public MovementInfoArgs Info { get; }
        public Vector3 ReachedPosition { get; }

        public MovementReachedEventArgs(MovementInfoArgs info, Vector3 reachedPosition)
        {
            Info = info;
            ReachedPosition = reachedPosition;
        }
    }

    [Serializable]
    public readonly struct MovementFailedEventArgs
    {
        public MovementInfoArgs Info { get; }
        public MovementFailureReason FailureReason { get; }

        public MovementFailedEventArgs(MovementInfoArgs info, MovementFailureReason failureReason)
        {
            Info = info;
            FailureReason = failureReason;
        }
    }

    [Serializable]
    public readonly struct MovementCancelledEventArgs
    {
        public MovementInfoArgs Info { get; }
        public MovementCancellationReason CancellationReason { get; }

        public MovementCancelledEventArgs(
            MovementInfoArgs info,
            MovementCancellationReason cancellationReason)
        {
            Info = info;
            CancellationReason = cancellationReason;
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class UnitMovementLegacy : MonoBehaviour
    {
        private enum ActiveMovementPhase
        {
            None,
            PathPending,
            Moving
        }

        [SerializeField] private CellSpace cellManager;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private float navSampleRadius = 2f;
        [SerializeField] private bool hasTargetCellCoordinates = true;
        [SerializeField] private Vector2Int targetCellCoordinates = new(0, 0);

        private int nextMovementId;
        private int acceptedMovementFrame = -1;
        private ActiveMovementPhase activeMovementPhase;
        private MovementInfoArgs activeMovement;
        private bool requiresAgentPathCleanup;
        private bool isProcessingMovementRequest;
        private bool isDispatchingMovingStateChanged;
        private bool isDispatchingRequestLockedFailure;
        private bool isMoving;

        public CellSpace CellManager
        {
            set
            {
                if (cellManager == null)
                {
                    cellManager = value;
                }
                else
                {
                    Debug.LogWarning($"CellManager is already set for {gameObject.name}. Changing it is not allowed.");
                }
            }
        }

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

        public bool IsMoving => isMoving;

        public MovingEvent OnMovingStateChanged = new();
        public MovementRequestedEvent OnMovementRequested = new();
        public MovementStartedEvent OnMovementStarted = new();
        public MovementReachedEvent OnMovementReached = new();
        public MovementFailedEvent OnMovementFailed = new();
        public MovementCancelledEvent OnMovementCancelled = new();

        private bool HasActiveMovement => activeMovementPhase != ActiveMovementPhase.None;

        private void OnEnable()
        {
            TryCompleteDeferredAgentPathCleanup();
        }

        private void OnDisable()
        {
            if (HasActiveMovement)
            {
                CancelActiveMovement(MovementCancellationReason.ComponentDisabled);
            }
        }

        private void Update()
        {
            TryCompleteDeferredAgentPathCleanup();

            if (!HasActiveMovement)
            {
                return;
            }

            if (!navMeshAgent.isActiveAndEnabled)
            {
                FailActiveMovement(MovementFailureReason.AgentDisabled);
                return;
            }

            if (!navMeshAgent.isOnNavMesh)
            {
                FailActiveMovement(MovementFailureReason.AgentNotOnNavMesh);
                return;
            }

            if (activeMovementPhase == ActiveMovementPhase.PathPending &&
                Time.frameCount == acceptedMovementFrame)
            {
                return;
            }

            if (navMeshAgent.pathPending)
            {
                return;
            }

            switch (navMeshAgent.pathStatus)
            {
                case NavMeshPathStatus.PathPartial:
                    FailActiveMovement(MovementFailureReason.PathPartial);
                    return;
                case NavMeshPathStatus.PathInvalid:
                    FailActiveMovement(MovementFailureReason.PathInvalid);
                    return;
            }

            if (HasReachedDestination())
            {
                ReachActiveMovement();
                return;
            }

            if (!navMeshAgent.hasPath)
            {
                FailActiveMovement(MovementFailureReason.PathInvalid);
                return;
            }

            if (activeMovementPhase == ActiveMovementPhase.PathPending)
            {
                StartActiveMovement();
            }
        }

        public void TryMoveToCell()
        {
            MovementInfoArgs info = hasTargetCellCoordinates
                ? MovementInfoArgs.ForCoordinates(nextMovementId++, this, targetCellCoordinates)
                : MovementInfoArgs.ForMissingTarget(nextMovementId++, this);

            ProcessMovementRequest(info);
        }

        public void TryMoveToCell(Vector2Int cellCoordinates)
        {
            ProcessMovementRequest(
                MovementInfoArgs.ForCoordinates(nextMovementId++, this, cellCoordinates));
        }

        public void TryMoveToCell(CellRef cell)
        {
            ProcessMovementRequest(MovementInfoArgs.ForCell(nextMovementId++, this, cell));
        }

        public void StopMoving()
        {
            if (HasActiveMovement)
            {
                CancelActiveMovement(MovementCancellationReason.ExplicitStop);
                return;
            }

            CleanupAgentPathOrDefer();
            SetMoving(false);
        }

        private void ProcessMovementRequest(MovementInfoArgs info)
        {
            if (isProcessingMovementRequest || isDispatchingMovingStateChanged)
            {
                PublishRequestLockedFailure(info);
                return;
            }

            MovementFailedEventArgs? activeFailure = null;
            MovementFailedEventArgs? requestFailure = null;
            MovementCancelledEventArgs? cancellation = null;
            int? acceptedMovementId = null;

            isProcessingMovementRequest = true;
            try
            {
                OnMovementRequested.Invoke(gameObject, new MovementRequestedEventArgs(info));
                ProcessMovementRequestCore(
                    info,
                    out activeFailure,
                    out requestFailure,
                    out cancellation,
                    out acceptedMovementId);
            }
            finally
            {
                isProcessingMovementRequest = false;
            }

            if (activeFailure.HasValue)
            {
                OnMovementFailed.Invoke(gameObject, activeFailure.Value);
            }

            if (cancellation.HasValue)
            {
                OnMovementCancelled.Invoke(gameObject, cancellation.Value);
            }

            if (requestFailure.HasValue)
            {
                OnMovementFailed.Invoke(gameObject, requestFailure.Value);
            }

            if (acceptedMovementId.HasValue &&
                HasActiveMovement &&
                activeMovementPhase == ActiveMovementPhase.PathPending &&
                activeMovement.MovementId == acceptedMovementId.Value)
            {
                StartActiveMovement();
            }
        }

        private void ProcessMovementRequestCore(
            MovementInfoArgs info,
            out MovementFailedEventArgs? activeFailure,
            out MovementFailedEventArgs? requestFailure,
            out MovementCancelledEventArgs? cancellation,
            out int? acceptedMovementId)
        {
            activeFailure = null;
            requestFailure = null;
            cancellation = null;
            acceptedMovementId = null;

            if (!TryResolveTargetCell(info, out info, out MovementFailureReason targetFailureReason))
            {
                requestFailure = new MovementFailedEventArgs(info, targetFailureReason);
                return;
            }

            if (!isActiveAndEnabled || !navMeshAgent.isActiveAndEnabled)
            {
                FailForInvalidAgentState(
                    info,
                    MovementFailureReason.AgentDisabled,
                    out activeFailure,
                    out requestFailure);
                return;
            }

            if (!navMeshAgent.isOnNavMesh)
            {
                FailForInvalidAgentState(
                    info,
                    MovementFailureReason.AgentNotOnNavMesh,
                    out activeFailure,
                    out requestFailure);
                return;
            }

            TryCompleteDeferredAgentPathCleanup();

            NavMeshQueryFilter navMeshFilter = new()
            {
                agentTypeID = navMeshAgent.agentTypeID,
                areaMask = navMeshAgent.areaMask
            };
            if (!NavMesh.SamplePosition(
                    info.ResolvedCell.SurfaceCenter,
                    out NavMeshHit navMeshHit,
                    navSampleRadius,
                    navMeshFilter))
            {
                requestFailure = new MovementFailedEventArgs(
                    info,
                    MovementFailureReason.NavMeshSampleNotFound);
                return;
            }

            info = info.WithSampledDestination(navMeshHit.position);
            NavMeshPath calculatedPath = new();
            if (!NavMesh.CalculatePath(
                    navMeshAgent.nextPosition,
                    navMeshHit.position,
                    navMeshFilter,
                    calculatedPath))
            {
                requestFailure = new MovementFailedEventArgs(
                    info,
                    MovementFailureReason.PathInvalid);
                return;
            }

            switch (calculatedPath.status)
            {
                case NavMeshPathStatus.PathPartial:
                    requestFailure = new MovementFailedEventArgs(
                        info,
                        MovementFailureReason.PathPartial);
                    return;
                case NavMeshPathStatus.PathInvalid:
                    requestFailure = new MovementFailedEventArgs(
                        info,
                        MovementFailureReason.PathInvalid);
                    return;
            }

            bool hadUsableActivePath =
                HasActiveMovement &&
                navMeshAgent.hasPath &&
                navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid;
            if (!navMeshAgent.SetPath(calculatedPath))
            {
                bool activePathWasLost =
                    hadUsableActivePath &&
                    (!navMeshAgent.hasPath ||
                     navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid);
                activeFailure = activePathWasLost
                    ? FailActiveMovementWithoutPublishing(
                        MovementFailureReason.PathInvalid)
                    : null;
                requestFailure = new MovementFailedEventArgs(
                    info,
                    MovementFailureReason.DestinationRejected);
                return;
            }

            AcceptMovementRequest(info, out cancellation);
            acceptedMovementId = info.MovementId;
        }

        private bool TryResolveTargetCell(
            MovementInfoArgs requestInfo,
            out MovementInfoArgs resolvedInfo,
            out MovementFailureReason failureReason)
        {
            resolvedInfo = requestInfo;
            failureReason = default;

            switch (requestInfo.TargetKind)
            {
                case MovementTargetKind.None:
                    failureReason = MovementFailureReason.TargetNotConfigured;
                    return false;
                case MovementTargetKind.Coordinates:
                    if (!cellManager.TryGetCell(requestInfo.TargetCoordinates, out CellRef cell))
                    {
                        failureReason = MovementFailureReason.CellNotFound;
                        return false;
                    }

                    resolvedInfo = requestInfo.WithResolvedCell(cell);
                    return true;
                case MovementTargetKind.Cell:
                    if (!requestInfo.TargetCell.IsValid)
                    {
                        failureReason = MovementFailureReason.InvalidCell;
                        return false;
                    }

                    resolvedInfo = requestInfo.WithResolvedCell(requestInfo.TargetCell);
                    return true;
                default:
                    failureReason = MovementFailureReason.TargetNotConfigured;
                    return false;
            }
        }

        private void FailForInvalidAgentState(
            MovementInfoArgs requestInfo,
            MovementFailureReason failureReason,
            out MovementFailedEventArgs? activeFailure,
            out MovementFailedEventArgs? requestFailure)
        {
            activeFailure = HasActiveMovement
                ? FailActiveMovementWithoutPublishing(failureReason)
                : null;
            requestFailure = new MovementFailedEventArgs(requestInfo, failureReason);
        }

        private void AcceptMovementRequest(
            MovementInfoArgs info,
            out MovementCancelledEventArgs? cancellation)
        {
            cancellation = HasActiveMovement
                ? new MovementCancelledEventArgs(
                    activeMovement,
                    MovementCancellationReason.Superseded)
                : null;

            navMeshAgent.isStopped = true;
            activeMovement = info;
            activeMovementPhase = ActiveMovementPhase.PathPending;
            acceptedMovementFrame = Time.frameCount;
            requiresAgentPathCleanup = false;
            SetMoving(false);
        }

        private void StartActiveMovement()
        {
            MovementInfoArgs startedMovement = activeMovement;

            activeMovementPhase = ActiveMovementPhase.Moving;
            navMeshAgent.isStopped = false;
            SetMoving(true);

            if (!HasActiveMovement ||
                activeMovementPhase != ActiveMovementPhase.Moving ||
                activeMovement.MovementId != startedMovement.MovementId)
            {
                return;
            }

            OnMovementStarted.Invoke(
                gameObject,
                new MovementStartedEventArgs(startedMovement));
        }

        private void ReachActiveMovement()
        {
            MovementInfoArgs completedMovement = activeMovement;

            CleanupAgentPathOrDefer();
            Vector3 reachedPosition = transform.position;
            ClearActiveMovement();
            SetMoving(false);
            OnMovementReached.Invoke(
                gameObject,
                new MovementReachedEventArgs(completedMovement, reachedPosition));
        }

        private void FailActiveMovement(MovementFailureReason failureReason)
        {
            MovementFailedEventArgs failedEventArgs =
                FailActiveMovementWithoutPublishing(failureReason);
            OnMovementFailed.Invoke(gameObject, failedEventArgs);
        }

        private MovementFailedEventArgs FailActiveMovementWithoutPublishing(
            MovementFailureReason failureReason)
        {
            MovementInfoArgs failedMovement = activeMovement;

            CleanupAgentPathOrDefer();
            ClearActiveMovement();
            SetMoving(false);
            return new MovementFailedEventArgs(failedMovement, failureReason);
        }

        private void CancelActiveMovement(MovementCancellationReason cancellationReason)
        {
            MovementInfoArgs cancelledMovement = activeMovement;

            CleanupAgentPathOrDefer();
            ClearActiveMovement();
            SetMoving(false);
            OnMovementCancelled.Invoke(
                gameObject,
                new MovementCancelledEventArgs(cancelledMovement, cancellationReason));
        }

        private void ClearActiveMovement()
        {
            activeMovement = default;
            activeMovementPhase = ActiveMovementPhase.None;
            acceptedMovementFrame = -1;
        }

        private bool HasReachedDestination()
        {
            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                return false;
            }

            return activeMovementPhase == ActiveMovementPhase.PathPending ||
                !navMeshAgent.hasPath ||
                navMeshAgent.velocity.sqrMagnitude < 0.01f;
        }

        private void CleanupAgentPathOrDefer()
        {
            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
                requiresAgentPathCleanup = false;
                return;
            }

            requiresAgentPathCleanup = true;
        }

        private void TryCompleteDeferredAgentPathCleanup()
        {
            if (!requiresAgentPathCleanup ||
                !navMeshAgent.isActiveAndEnabled ||
                !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            requiresAgentPathCleanup = false;
        }

        private void SetMoving(bool value)
        {
            if (isMoving == value)
            {
                return;
            }

            isMoving = value;
            bool wasDispatchingMovingStateChanged = isDispatchingMovingStateChanged;
            isDispatchingMovingStateChanged = true;
            try
            {
                OnMovingStateChanged.Invoke(gameObject, isMoving);
            }
            finally
            {
                isDispatchingMovingStateChanged = wasDispatchingMovingStateChanged;
            }
        }

        private void PublishRequestLockedFailure(MovementInfoArgs info)
        {
            if (isDispatchingRequestLockedFailure)
            {
                return;
            }

            isDispatchingRequestLockedFailure = true;
            try
            {
                OnMovementRequested.Invoke(
                    gameObject,
                    new MovementRequestedEventArgs(info));
                OnMovementFailed.Invoke(
                    gameObject,
                    new MovementFailedEventArgs(info, MovementFailureReason.RequestLocked));
            }
            finally
            {
                isDispatchingRequestLockedFailure = false;
            }
        }
    }

    [Serializable]
    public class MovingEvent : UnityEvent<GameObject, bool> { }

    [Serializable]
    public class MovementRequestedEvent : UnityEvent<GameObject, MovementRequestedEventArgs> { }

    [Serializable]
    public class MovementStartedEvent : UnityEvent<GameObject, MovementStartedEventArgs> { }

    [Serializable]
    public class MovementReachedEvent : UnityEvent<GameObject, MovementReachedEventArgs> { }

    [Serializable]
    public class MovementFailedEvent : UnityEvent<GameObject, MovementFailedEventArgs> { }

    [Serializable]
    public class MovementCancelledEvent : UnityEvent<GameObject, MovementCancelledEventArgs> { }
}
