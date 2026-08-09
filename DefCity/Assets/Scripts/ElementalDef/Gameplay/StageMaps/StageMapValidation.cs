using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public enum StageMapValidationErrorCode
    {
        HeadquartersMarkerMismatch,
        UnexpectedHeadquartersMarker,
        RouteGoalMarkerMismatch,
        UnexpectedRouteGoalMarker,
        SpawnMarkerMismatch,
        UnexpectedSpawnMarker,
        MissingGraphSpawn,
        UnexpectedGraphSpawn,
        SpawnStartNodeMismatch,
        RouteGoalNodeMismatch,
        RouteNodeOutOfBounds,
        DuplicateRouteNodeCell,
        RouteNodeNotOnRoad,
        RoadCellMissingRouteNode,
        NonCardinalRouteEdge,
        SpawnHasIncomingEdge,
        GoalHasOutgoingEdge,
        IsolatedRouteNode,
        RouteDeadEnd,
        RouteNodeUnreachableFromSpawn,
        RouteNodeCannotReachGoal,
        RouteCycleDetected,
        InsufficientDeployableCells,
        InsufficientWaterCells,
        InsufficientFireCells,
        InsufficientEarthCells,
        InvalidDisconnectedCrossing,
        UnexpectedRoadAdjacency,
        InsufficientRoadAdjacentDeployableCells,
        BlockedCellInsideEndpointProtectionRadius,
        BlockedCellClusterTooLarge,
        HeadquartersFootprintOverlapsSpawn,
        HeadquartersFootprintOverlapsRouteGoal,
        HeadquartersFootprintOverlapsRoad,
        HeadquartersFootprintOverlapsRouteNode,
        RouteGoalNotAdjacentToHeadquarters,
        InsufficientNeutralCells,
    }

    public readonly struct StageMapValidationError
    {
        public StageMapValidationErrorCode Code { get; }
        public string Message { get; }
        public Vector2Int? Cell { get; }
        public int? NodeId { get; }

        internal StageMapValidationError(
            StageMapValidationErrorCode code,
            string message,
            Vector2Int? cell = null,
            int? nodeId = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "A validation error message is required.",
                    nameof(message));
            }

            Code = code;
            Message = message;
            Cell = cell;
            NodeId = nodeId;
        }
    }

    public sealed class StageMapValidationReport
    {
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyList<StageMapValidationError> Errors { get; }

        internal StageMapValidationReport(
            IReadOnlyList<StageMapValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            StageMapValidationError[] copies =
                new StageMapValidationError[errors.Count];
            for (int index = 0; index < errors.Count; index++)
            {
                copies[index] = errors[index];
            }

            Errors = Array.AsReadOnly(copies);
        }
    }

    public sealed class StageMapValidationRules
    {
        public int MinimumDeployableCellCount { get; }
        public int MinimumCellCountPerElement { get; }
        public bool RequireAcyclicRoutes { get; }
        public bool RequireRoadAdjacencyMatchesGraph { get; }
        public int MinimumDeployableNeighborsPerRoadCell { get; }
        public int EndpointProtectionRadius { get; }
        public int MaximumBlockedClusterSize { get; }

        public StageMapValidationRules(
            int minimumDeployableCellCount,
            int minimumCellCountPerElement,
            bool requireAcyclicRoutes)
            : this(
                minimumDeployableCellCount,
                minimumCellCountPerElement,
                requireAcyclicRoutes,
                requireRoadAdjacencyMatchesGraph: false,
                minimumDeployableNeighborsPerRoadCell: 0,
                endpointProtectionRadius: 0,
                maximumBlockedClusterSize: 0)
        {
        }

        public StageMapValidationRules(
            int minimumDeployableCellCount,
            int minimumCellCountPerElement,
            bool requireAcyclicRoutes,
            bool requireRoadAdjacencyMatchesGraph)
            : this(
                minimumDeployableCellCount,
                minimumCellCountPerElement,
                requireAcyclicRoutes,
                requireRoadAdjacencyMatchesGraph,
                minimumDeployableNeighborsPerRoadCell: 0,
                endpointProtectionRadius: 0,
                maximumBlockedClusterSize: 0)
        {
        }

        public StageMapValidationRules(
            int minimumDeployableCellCount,
            int minimumCellCountPerElement,
            bool requireAcyclicRoutes,
            bool requireRoadAdjacencyMatchesGraph,
            int minimumDeployableNeighborsPerRoadCell)
            : this(
                minimumDeployableCellCount,
                minimumCellCountPerElement,
                requireAcyclicRoutes,
                requireRoadAdjacencyMatchesGraph,
                minimumDeployableNeighborsPerRoadCell,
                endpointProtectionRadius: 0,
                maximumBlockedClusterSize: 0)
        {
        }

        public StageMapValidationRules(
            int minimumDeployableCellCount,
            int minimumCellCountPerElement,
            bool requireAcyclicRoutes,
            bool requireRoadAdjacencyMatchesGraph,
            int minimumDeployableNeighborsPerRoadCell,
            int endpointProtectionRadius,
            int maximumBlockedClusterSize)
        {
            if (minimumDeployableCellCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCount),
                    minimumDeployableCellCount,
                    "The minimum deployable cell count cannot be negative.");
            }

            if (minimumCellCountPerElement < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumCellCountPerElement),
                    minimumCellCountPerElement,
                    "The minimum ground-type cell count cannot be negative.");
            }

            if (minimumDeployableNeighborsPerRoadCell < 0 ||
                minimumDeployableNeighborsPerRoadCell > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableNeighborsPerRoadCell),
                    minimumDeployableNeighborsPerRoadCell,
                    "A Road cell can require between zero and four deployable neighbors.");
            }

            if (endpointProtectionRadius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endpointProtectionRadius),
                    endpointProtectionRadius,
                    "The endpoint protection radius cannot be negative.");
            }

            if (maximumBlockedClusterSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumBlockedClusterSize),
                    maximumBlockedClusterSize,
                    "The maximum blocked cluster size cannot be negative.");
            }

            MinimumDeployableCellCount = minimumDeployableCellCount;
            MinimumCellCountPerElement = minimumCellCountPerElement;
            RequireAcyclicRoutes = requireAcyclicRoutes;
            RequireRoadAdjacencyMatchesGraph =
                requireRoadAdjacencyMatchesGraph;
            MinimumDeployableNeighborsPerRoadCell =
                minimumDeployableNeighborsPerRoadCell;
            EndpointProtectionRadius = endpointProtectionRadius;
            MaximumBlockedClusterSize = maximumBlockedClusterSize;
        }
    }
}
