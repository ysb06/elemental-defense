using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageMapGenerationSettings
    {
        public const double DefaultBlockedCellRatio = 0.15d;
        public const int DefaultMinimumDeployableCellCount = 12;
        public const int DefaultMinimumDeployableCellCountPerElement = 3;
        public const int DefaultMinimumDeployableNeighborsPerRoadCell = 1;
        public const int DefaultEndpointProtectionRadius = 1;
        public const int DefaultMaximumBlockedClusterSize = 3;
        public const int DefaultMaxBlockedCellPlacementAttempts = 16;

        public StageRouteGenerationSettings RouteSettings { get; }
        public double BlockedCellRatio { get; }
        public int MinimumDeployableCellCount { get; }
        public int MinimumDeployableCellCountPerElement { get; }
        public int MinimumDeployableNeighborsPerRoadCell { get; }
        public int EndpointProtectionRadius { get; }
        public int MaximumBlockedClusterSize { get; }
        public int MaxBlockedCellPlacementAttempts { get; }
        public bool RequireAcyclicRoutes { get; }
        public bool RequireRoadAdjacencyMatchesGraph { get; }

        public StageMapGenerationSettings(
            StageRouteGenerationSettings routeSettings,
            double blockedCellRatio = DefaultBlockedCellRatio,
            int minimumDeployableCellCount = DefaultMinimumDeployableCellCount,
            int minimumDeployableCellCountPerElement = DefaultMinimumDeployableCellCountPerElement,
            int minimumDeployableNeighborsPerRoadCell = DefaultMinimumDeployableNeighborsPerRoadCell,
            int endpointProtectionRadius = DefaultEndpointProtectionRadius,
            int maximumBlockedClusterSize = DefaultMaximumBlockedClusterSize,
            int maxBlockedCellPlacementAttempts = DefaultMaxBlockedCellPlacementAttempts,
            bool requireAcyclicRoutes = true,
            bool requireRoadAdjacencyMatchesGraph = true)
        {
            RouteSettings = routeSettings ?? throw new ArgumentNullException(nameof(routeSettings));

            if (double.IsNaN(blockedCellRatio) || double.IsInfinity(blockedCellRatio) || blockedCellRatio < 0d || blockedCellRatio > 1d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(blockedCellRatio),
                    blockedCellRatio,
                    "The blocked cell ratio must be between zero and one.");
            }

            if (minimumDeployableCellCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCount),
                    minimumDeployableCellCount,
                    "The minimum deployable cell count cannot be negative.");
            }

            if (minimumDeployableCellCountPerElement < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCountPerElement),
                    minimumDeployableCellCountPerElement,
                    "The per-element deployable cell minimum cannot be negative.");
            }

            if (minimumDeployableNeighborsPerRoadCell < 0 ||
                minimumDeployableNeighborsPerRoadCell > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableNeighborsPerRoadCell),
                    minimumDeployableNeighborsPerRoadCell,
                    "A Road cell can require between zero and four deployable neighbors.");
            }

            if (maxBlockedCellPlacementAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxBlockedCellPlacementAttempts),
                    maxBlockedCellPlacementAttempts,
                    "At least one blocked-cell placement attempt is required.");
            }

            if (endpointProtectionRadius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endpointProtectionRadius),
                    endpointProtectionRadius,
                    "The endpoint protection radius cannot be negative.");
            }

            if (maximumBlockedClusterSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumBlockedClusterSize),
                    maximumBlockedClusterSize,
                    "The maximum blocked cluster size must be positive.");
            }

            BlockedCellRatio = blockedCellRatio;
            MinimumDeployableCellCount = minimumDeployableCellCount;
            MinimumDeployableCellCountPerElement =
                minimumDeployableCellCountPerElement;
            MinimumDeployableNeighborsPerRoadCell =
                minimumDeployableNeighborsPerRoadCell;
            EndpointProtectionRadius = endpointProtectionRadius;
            MaximumBlockedClusterSize = maximumBlockedClusterSize;
            MaxBlockedCellPlacementAttempts = maxBlockedCellPlacementAttempts;
            RequireAcyclicRoutes = requireAcyclicRoutes;
            RequireRoadAdjacencyMatchesGraph =
                requireRoadAdjacencyMatchesGraph;
        }

        public int GetTargetBlockedCellCount(int groundCellCount)
        {
            if (groundCellCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(groundCellCount));
            }

            if (groundCellCount == 0 || BlockedCellRatio <= 0d)
            {
                return 0;
            }

            int roundedCount = checked((int)Math.Round(
                groundCellCount * BlockedCellRatio,
                MidpointRounding.AwayFromZero));
            return Math.Min(groundCellCount, Math.Max(1, roundedCount));
        }
    }
}
