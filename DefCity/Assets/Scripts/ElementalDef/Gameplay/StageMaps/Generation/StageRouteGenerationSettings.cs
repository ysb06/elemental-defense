using System;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageRouteGenerationSettings
    {
        public const int MaxSupportedMapCells = 1_024;
        public const int MaxSupportedPhysicalLayoutDraws = 65_536;
        public const int MaxSupportedPhysicalLayoutCount = 256;
        public const int MaxSupportedOrderVariantsPerPhysicalLayout = 64;
        public const int MaxSupportedRouteCandidateCount = 4_096;
        public const int MaxSupportedSearchWorkPerCandidate = 5_000_000;
        public const int MaxSupportedTotalSearchWork = 50_000_000;
        public const int MaxSupportedConnectorAlternatives = 64;
        public const int DefaultMaxPhysicalLayoutDraws = 1_024;
        public const int DefaultMaxPhysicalLayoutCount = 32;
        public const int DefaultOrderVariantsPerPhysicalLayout = 8;
        public const int DefaultMaxRouteCandidateCount = 256;
        public const int DefaultMaxSearchWorkPerCandidate = 25_000;
        public const int DefaultMaxTotalSearchWork = 500_000;
        public const int DefaultMaxConnectorAlternatives = 8;
        public const int DefaultConnectorDetourAllowance = 8;

        [Obsolete("Use DefaultMaxPhysicalLayoutCount instead.")]
        public const int DefaultMaxGenerationAttempts = DefaultMaxPhysicalLayoutCount;

        [Obsolete("Use DefaultMaxSearchWorkPerCandidate instead.")]
        public const int DefaultMaxPathSearchNodes = DefaultMaxSearchWorkPerCandidate;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public string SpawnId { get; }
        public Vector2Int SpawnCell { get; }
        public Vector2Int RouteGoalCell { get; }
        public RectInt HeadquartersFootprint { get; }
        public int PatternCount { get; }
        public int CenterBandRadius { get; }
        public StageRoutePatternKinds AllowedPatternKinds { get; }
        public int MaxPhysicalLayoutDraws { get; }
        public int MaxPhysicalLayoutCount { get; }
        public int OrderVariantsPerPhysicalLayout { get; }
        public int MaxRouteCandidateCount { get; }
        public int MaxSearchWorkPerCandidate { get; }
        public int MaxTotalSearchWork { get; }
        public int MaxConnectorAlternatives { get; }
        public int ConnectorDetourAllowance { get; }

        [Obsolete("Use MaxPhysicalLayoutCount instead.")]
        public int MaxGenerationAttempts => MaxPhysicalLayoutCount;

        [Obsolete("Use MaxSearchWorkPerCandidate instead.")]
        public int MaxPathSearchNodes => MaxSearchWorkPerCandidate;

        public StageRouteGenerationSettings(
            RectInt bounds,
            int seed,
            string spawnId,
            Vector2Int spawnCell,
            Vector2Int routeGoalCell,
            RectInt headquartersFootprint,
            int patternCount,
            int centerBandRadius = 0,
            StageRoutePatternKinds allowedPatternKinds = StageRoutePatternKinds.All,
            int maxGenerationAttempts = DefaultMaxPhysicalLayoutCount,
            int maxPathSearchNodes = DefaultMaxSearchWorkPerCandidate,
            int maxPhysicalLayoutDraws = DefaultMaxPhysicalLayoutDraws,
            int orderVariantsPerPhysicalLayout = DefaultOrderVariantsPerPhysicalLayout,
            int maxRouteCandidateCount = DefaultMaxRouteCandidateCount,
            int maxTotalSearchWork = DefaultMaxTotalSearchWork,
            int maxConnectorAlternatives = DefaultMaxConnectorAlternatives,
            int connectorDetourAllowance = DefaultConnectorDetourAllowance)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bounds),
                    bounds,
                    "Route generation bounds must have positive dimensions.");
            }

            long cellCount = (long)bounds.width * bounds.height;
            if (cellCount > MaxSupportedMapCells)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bounds),
                    bounds,
                    $"Route generation supports at most {MaxSupportedMapCells} map cells.");
            }

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                throw new ArgumentException("A spawn ID is required.", nameof(spawnId));
            }

            EnsureCellInBounds(bounds, spawnCell, nameof(spawnCell));
            EnsureCellInBounds(bounds, routeGoalCell, nameof(routeGoalCell));
            EnsureFootprintInBounds(
                bounds,
                headquartersFootprint,
                nameof(headquartersFootprint));

            if (!IsPerimeterCell(bounds, spawnCell))
            {
                throw new ArgumentException(
                    "The spawn cell must be on the map perimeter.",
                    nameof(spawnCell));
            }

            if (spawnCell == routeGoalCell ||
                headquartersFootprint.Contains(spawnCell) ||
                headquartersFootprint.Contains(routeGoalCell))
            {
                throw new ArgumentException(
                    "Spawn and route goal must be distinct and outside the " +
                    "Headquarters footprint.");
            }

            if (!IsCardinallyAdjacent(
                    routeGoalCell,
                    headquartersFootprint))
            {
                throw new ArgumentException(
                    "The route goal must be cardinally adjacent to the " +
                    "Headquarters footprint.",
                    nameof(headquartersFootprint));
            }

            if (patternCount < 1 || patternCount > 5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(patternCount),
                    patternCount,
                    "Pattern count must be between 1 and 5.");
            }

            if (centerBandRadius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(centerBandRadius),
                    centerBandRadius,
                    "Center band radius cannot be negative.");
            }

            const StageRoutePatternKinds allKinds = StageRoutePatternKinds.All;
            if (allowedPatternKinds == StageRoutePatternKinds.None ||
                (allowedPatternKinds & ~allKinds) != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(allowedPatternKinds),
                    allowedPatternKinds,
                    "At least one defined route pattern kind must be enabled.");
            }

            if (maxGenerationAttempts <= 0 ||
                maxGenerationAttempts > MaxSupportedPhysicalLayoutCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxGenerationAttempts),
                    maxGenerationAttempts,
                    $"The physical layout count must be between 1 and " +
                    $"{MaxSupportedPhysicalLayoutCount}.");
            }

            if (maxPathSearchNodes <= 0 ||
                maxPathSearchNodes > MaxSupportedSearchWorkPerCandidate)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxPathSearchNodes),
                    maxPathSearchNodes,
                    $"The per-candidate search work limit must be between 1 and " +
                    $"{MaxSupportedSearchWorkPerCandidate}.");
            }

            if (maxPhysicalLayoutDraws <= 0 ||
                maxPhysicalLayoutDraws > MaxSupportedPhysicalLayoutDraws)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxPhysicalLayoutDraws),
                    maxPhysicalLayoutDraws,
                    $"The physical layout draw limit must be between 1 and " +
                    $"{MaxSupportedPhysicalLayoutDraws}.");
            }

            if (orderVariantsPerPhysicalLayout <= 0 ||
                orderVariantsPerPhysicalLayout >
                MaxSupportedOrderVariantsPerPhysicalLayout)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(orderVariantsPerPhysicalLayout),
                    orderVariantsPerPhysicalLayout,
                    $"The order variant count must be between 1 and " +
                    $"{MaxSupportedOrderVariantsPerPhysicalLayout}.");
            }

            if (maxRouteCandidateCount <= 0 ||
                maxRouteCandidateCount > MaxSupportedRouteCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxRouteCandidateCount),
                    maxRouteCandidateCount,
                    $"The route candidate limit must be between 1 and " +
                    $"{MaxSupportedRouteCandidateCount}.");
            }

            if (maxTotalSearchWork <= 0 ||
                maxTotalSearchWork > MaxSupportedTotalSearchWork)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxTotalSearchWork),
                    maxTotalSearchWork,
                    $"The total search work limit must be between 1 and " +
                    $"{MaxSupportedTotalSearchWork}.");
            }

            if (maxConnectorAlternatives <= 0 ||
                maxConnectorAlternatives > MaxSupportedConnectorAlternatives)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxConnectorAlternatives),
                    maxConnectorAlternatives,
                    $"The connector alternative limit must be between 1 and " +
                    $"{MaxSupportedConnectorAlternatives}.");
            }

            if (connectorDetourAllowance < 0 ||
                connectorDetourAllowance > MaxSupportedMapCells)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(connectorDetourAllowance),
                    connectorDetourAllowance,
                    $"The connector detour allowance must be between 0 and " +
                    $"{MaxSupportedMapCells}.");
            }

            Bounds = bounds;
            Seed = seed;
            SpawnId = spawnId;
            SpawnCell = spawnCell;
            RouteGoalCell = routeGoalCell;
            HeadquartersFootprint = headquartersFootprint;
            PatternCount = patternCount;
            CenterBandRadius = centerBandRadius;
            AllowedPatternKinds = allowedPatternKinds;
            MaxPhysicalLayoutDraws = maxPhysicalLayoutDraws;
            MaxPhysicalLayoutCount = maxGenerationAttempts;
            OrderVariantsPerPhysicalLayout = orderVariantsPerPhysicalLayout;
            MaxRouteCandidateCount = maxRouteCandidateCount;
            MaxSearchWorkPerCandidate = maxPathSearchNodes;
            MaxTotalSearchWork = maxTotalSearchWork;
            MaxConnectorAlternatives = maxConnectorAlternatives;
            ConnectorDetourAllowance = connectorDetourAllowance;
        }

        public bool IsHeadquartersCell(Vector2Int cell)
        {
            return HeadquartersFootprint.Contains(cell);
        }

        private static void EnsureCellInBounds(
            RectInt bounds,
            Vector2Int cell,
            string parameterName)
        {
            if (!bounds.Contains(cell))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    cell,
                    $"Cell {cell} must be inside route generation bounds {bounds}.");
            }
        }

        private static bool IsPerimeterCell(RectInt bounds, Vector2Int cell)
        {
            return cell.x == bounds.xMin ||
                   cell.x == bounds.xMax - 1 ||
                   cell.y == bounds.yMin ||
                   cell.y == bounds.yMax - 1;
        }

        private static void EnsureFootprintInBounds(
            RectInt bounds,
            RectInt footprint,
            string parameterName)
        {
            if (footprint.width <= 0 || footprint.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    footprint,
                    "The Headquarters footprint must have positive dimensions.");
            }

            long footprintXMax = (long)footprint.xMin + footprint.width;
            long footprintYMax = (long)footprint.yMin + footprint.height;
            if (footprint.xMin < bounds.xMin ||
                footprint.yMin < bounds.yMin ||
                footprintXMax > bounds.xMax ||
                footprintYMax > bounds.yMax)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    footprint,
                    $"The Headquarters footprint {footprint} must be fully " +
                    $"inside route generation bounds {bounds}.");
            }
        }

        private static bool IsCardinallyAdjacent(
            Vector2Int cell,
            RectInt footprint)
        {
            long cellX = cell.x;
            long cellY = cell.y;
            long xMin = footprint.xMin;
            long yMin = footprint.yMin;
            long xMax = xMin + footprint.width;
            long yMax = yMin + footprint.height;

            bool touchesVerticalEdge =
                (cellX == xMin - 1L || cellX == xMax) &&
                cellY >= yMin &&
                cellY < yMax;
            bool touchesHorizontalEdge =
                (cellY == yMin - 1L || cellY == yMax) &&
                cellX >= xMin &&
                cellX < xMax;
            return touchesVerticalEdge || touchesHorizontalEdge;
        }
    }
}
