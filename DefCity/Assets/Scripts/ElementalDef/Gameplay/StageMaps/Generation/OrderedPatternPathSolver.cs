using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    internal sealed class OrderedPatternPathSearchResult
    {
        private static readonly IReadOnlyList<Vector2Int> EmptyPath =
            Array.Empty<Vector2Int>();
        private static readonly IReadOnlyDictionary<string, int> EmptyPassageStarts =
            new Dictionary<string, int>();

        internal OrderedPatternPathSearchOutcome Outcome { get; }
        internal StageRouteSearchLimitKind LimitKind { get; }
        internal StageRouteCandidateRejectionReason PrevalidationRejectionReason
        {
            get;
        }
        internal IReadOnlyList<Vector2Int> Path { get; }
        internal IReadOnlyDictionary<string, int> PassageStartNodeIds { get; }
        internal OrderedPatternPathSearchMetrics Metrics { get; }

        internal bool Succeeded =>
            Outcome == OrderedPatternPathSearchOutcome.Succeeded;
        internal bool SearchBudgetExceeded =>
            Outcome == OrderedPatternPathSearchOutcome.SearchLimitExceeded;

        // Compatibility alias for the v2 generator while diagnostics are migrated.
        internal int ExploredNodeCount => Metrics.TotalWorkUnits;
        internal int TotalWorkUnits => Metrics.TotalWorkUnits;
        internal int AStarStatesExpanded => Metrics.AStarStatesExpanded;
        internal int ConnectorAlternativesTried =>
            Metrics.ConnectorAlternativesTried;
        internal int BacktrackCount => Metrics.BacktrackCount;
        internal int ReachabilityCheckCount => Metrics.ReachabilityCheckCount;
        internal int ReachabilityVisitedCellCount =>
            Metrics.ReachabilityVisitedCellCount;

        private OrderedPatternPathSearchResult(
            OrderedPatternPathSearchOutcome outcome,
            StageRouteSearchLimitKind limitKind,
            StageRouteCandidateRejectionReason prevalidationRejectionReason,
            IReadOnlyList<Vector2Int> path,
            IReadOnlyDictionary<string, int> passageStartNodeIds,
            OrderedPatternPathSearchMetrics metrics)
        {
            Outcome = outcome;
            LimitKind = limitKind;
            PrevalidationRejectionReason = prevalidationRejectionReason;
            Path = path;
            PassageStartNodeIds = passageStartNodeIds;
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        internal static OrderedPatternPathSearchResult Success(
            IReadOnlyList<Vector2Int> sourcePath,
            IReadOnlyDictionary<string, int> sourcePassageStarts,
            OrderedPatternPathSearchMetrics metrics)
        {
            Vector2Int[] path = new Vector2Int[sourcePath.Count];
            for (int index = 0; index < sourcePath.Count; index++)
            {
                path[index] = sourcePath[index];
            }

            Dictionary<string, int> passageStarts =
                new(sourcePassageStarts, StringComparer.Ordinal);
            return new OrderedPatternPathSearchResult(
                OrderedPatternPathSearchOutcome.Succeeded,
                StageRouteSearchLimitKind.None,
                StageRouteCandidateRejectionReason.None,
                Array.AsReadOnly(path),
                passageStarts,
                metrics);
        }

        internal static OrderedPatternPathSearchResult Rejected(
            StageRouteCandidateRejectionReason rejectionReason,
            OrderedPatternPathSearchMetrics metrics)
        {
            if (rejectionReason == StageRouteCandidateRejectionReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(rejectionReason));
            }

            return new OrderedPatternPathSearchResult(
                OrderedPatternPathSearchOutcome.PathNotFound,
                StageRouteSearchLimitKind.None,
                rejectionReason,
                EmptyPath,
                EmptyPassageStarts,
                metrics);
        }

        internal static OrderedPatternPathSearchResult Failure(
            OrderedPatternPathSearchOutcome outcome,
            StageRouteSearchLimitKind limitKind,
            OrderedPatternPathSearchMetrics metrics)
        {
            if (outcome == OrderedPatternPathSearchOutcome.Succeeded)
            {
                throw new ArgumentOutOfRangeException(nameof(outcome));
            }

            bool isLimited =
                outcome == OrderedPatternPathSearchOutcome.SearchLimitExceeded;
            if (isLimited != (limitKind != StageRouteSearchLimitKind.None))
            {
                throw new ArgumentException(
                    "Only a search-limit failure may carry a limit kind.",
                    nameof(limitKind));
            }

            return new OrderedPatternPathSearchResult(
                outcome,
                limitKind,
                StageRouteCandidateRejectionReason.None,
                EmptyPath,
                EmptyPassageStarts,
                metrics);
        }
    }

    /// <summary>
    /// Connects an already ordered set of fixed pattern passages. Passage order
    /// is never changed: bounded A* produces only connector alternatives, and
    /// rollback occurs only at passage boundaries.
    /// </summary>
    internal sealed class OrderedPatternPathSolver
    {
        private const int AbsoluteOpenSetCapacity = 16_384;

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
        };

        private readonly RectInt bounds;
        private readonly int seed;
        private readonly int candidateIndex;
        private readonly Vector2Int goal;
        private readonly RectInt headquartersFootprint;
        private readonly IReadOnlyList<StageRoutePatternPassage> passages;
        private readonly int maxConnectorAlternatives;
        private readonly int connectorDetourAllowance;
        private readonly int maxOpenSetCapacity;
        private readonly HashSet<Vector2Int> reservedPatternCells = new();
        private readonly Dictionary<Vector2Int, HashSet<Vector2Int>>
            crossingArmCells = new();
        private readonly Dictionary<Vector2Int, HashSet<string>>
            crossingPassageIds = new();
        private readonly Dictionary<Vector2Int, int> usedCellCounts = new();
        private readonly List<Vector2Int> path = new();
        private readonly Dictionary<string, int> passageStartNodeIds =
            new(StringComparer.Ordinal);
        private readonly int[] reverseDistances;
        private readonly Vector2Int[] reachabilityQueue;
        private readonly int[] shortestPathVisitIds;
        private readonly int[] shortestPathCosts;
        private readonly Vector2Int[] shortestPathParents;
        private readonly OrderedPatternPathSearchMetricsBuilder metrics;
        private readonly OrderedPatternCandidatePrevalidator prevalidator;

        private int shortestPathVisitId;
        private bool connectorAlternativeLimitEncountered;

        internal OrderedPatternPathSolver(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout)
            : this(
                settings,
                layout,
                settings?.MaxSearchWorkPerCandidate ?? 0,
                maxConnectorAlternatives: 8,
                connectorDetourAllowance: 8,
                candidateIndex: 0)
        {
        }

        internal OrderedPatternPathSolver(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout,
            int maxWorkUnits,
            int maxConnectorAlternatives,
            int connectorDetourAllowance,
            int candidateIndex)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (maxWorkUnits <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxWorkUnits));
            }

            if (maxConnectorAlternatives <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxConnectorAlternatives));
            }

            if (connectorDetourAllowance < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(connectorDetourAllowance));
            }

            if (candidateIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(candidateIndex));
            }

            bounds = settings.Bounds;
            seed = settings.Seed;
            this.candidateIndex = candidateIndex;
            goal = settings.RouteGoalCell;
            headquartersFootprint = settings.HeadquartersFootprint;
            passages = layout.OrderedPassages;
            this.maxConnectorAlternatives = maxConnectorAlternatives;
            this.connectorDetourAllowance = connectorDetourAllowance;
            maxOpenSetCapacity = Math.Max(
                64,
                Math.Min(maxWorkUnits, AbsoluteOpenSetCapacity));
            metrics = new OrderedPatternPathSearchMetricsBuilder(maxWorkUnits);
            prevalidator = new OrderedPatternCandidatePrevalidator(
                settings,
                layout);

            foreach (StageRoutePatternPlacement placement in layout.Placements)
            {
                foreach (Vector2Int roadCell in placement.RoadCells)
                {
                    reservedPatternCells.Add(roadCell);
                }

                if (placement.Kind != StageRoutePatternKind.DisconnectedCross)
                {
                    continue;
                }

                HashSet<Vector2Int> arms = new();
                HashSet<string> passageIds = new(StringComparer.Ordinal);
                foreach (StageRoutePatternPassage passage in placement.Passages)
                {
                    passageIds.Add(passage.PassageId);
                    foreach (Vector2Int cell in passage.Cells)
                    {
                        if (cell != placement.AnchorCell)
                        {
                            arms.Add(cell);
                        }
                    }
                }

                crossingArmCells.Add(placement.AnchorCell, arms);
                crossingPassageIds.Add(placement.AnchorCell, passageIds);
            }

            path.Add(settings.SpawnCell);
            IncrementUsedCell(settings.SpawnCell);

            int mapCellCount = checked(bounds.width * bounds.height);
            reverseDistances = new int[mapCellCount];
            reachabilityQueue = new Vector2Int[mapCellCount];
            shortestPathVisitIds = new int[mapCellCount];
            shortestPathCosts = new int[mapCellCount];
            shortestPathParents = new Vector2Int[mapCellCount];
        }

        internal OrderedPatternPathSearchResult Solve()
        {
            OrderedPatternCandidatePrevalidationResult prevalidation =
                prevalidator.EvaluateInitial(usedCellCounts, metrics);
            if (metrics.IsLimited)
            {
                return CreateLimitFailure();
            }

            if (!prevalidation.Succeeded)
            {
                return OrderedPatternPathSearchResult.Rejected(
                    prevalidation.RejectionReason,
                    metrics.Freeze());
            }

            if (SearchPassageBoundary(0))
            {
                return OrderedPatternPathSearchResult.Success(
                    path,
                    passageStartNodeIds,
                    metrics.Freeze());
            }

            if (metrics.IsLimited)
            {
                return CreateLimitFailure();
            }

            if (connectorAlternativeLimitEncountered)
            {
                metrics.SetLimit(
                    StageRouteSearchLimitKind.ConnectorAlternativeCount);
                return CreateLimitFailure();
            }

            return OrderedPatternPathSearchResult.Failure(
                OrderedPatternPathSearchOutcome.PathNotFound,
                StageRouteSearchLimitKind.None,
                metrics.Freeze());
        }

        private bool SearchPassageBoundary(int passageIndex)
        {
            if (metrics.IsLimited)
            {
                return false;
            }

            Vector2Int current = path[path.Count - 1];
            Vector2Int target = passageIndex < passages.Count
                ? passages[passageIndex].EntryCell
                : goal;
            return SearchConnectorAlternatives(
                current,
                target,
                passageIndex);
        }

        private bool TryConnectorAlternative(
            Vector2Int[] connector,
            int passageIndex)
        {
            if (!metrics.TryRecordConnectorAlternative())
            {
                return false;
            }

            int originalPathCount = path.Count;
            AppendConnector(connector);

            if (passageIndex == passages.Count)
            {
                if (HasOnlyGraphBackedRoadAdjacency())
                {
                    return true;
                }
            }
            else
            {
                StageRoutePatternPassage passage = passages[passageIndex];
                passageStartNodeIds.Add(
                    passage.PassageId,
                    path.Count - 1);
                bool appendedPassage = AppendFixedPassage(
                    passageIndex,
                    passage);
                if (appendedPassage && !metrics.IsLimited)
                {
                    OrderedPatternCandidatePrevalidationResult forwardCheck =
                        prevalidator.EvaluateRemaining(
                            path[path.Count - 1],
                            passageIndex + 1,
                            usedCellCounts,
                            metrics);
                    if (!metrics.IsLimited &&
                        forwardCheck.Succeeded &&
                        SearchPassageBoundary(passageIndex + 1))
                    {
                        return true;
                    }
                }

                passageStartNodeIds.Remove(passage.PassageId);
            }

            RollBackPath(originalPathCount);
            metrics.RecordBacktrack();
            return false;
        }

        private bool SearchConnectorAlternatives(
            Vector2Int start,
            Vector2Int target,
            int passageIndex)
        {
            if (start == target)
            {
                return TryConnectorAlternative(
                    new[] { start },
                    passageIndex);
            }

            if (usedCellCounts.ContainsKey(target))
            {
                return false;
            }

            if (!BuildReverseDistanceField(start, target))
            {
                return false;
            }

            int startIndex = GetCellIndex(start);
            if (reverseDistances[startIndex] < 0)
            {
                return false;
            }

            int[] connectorDistances = new int[reverseDistances.Length];
            Array.Copy(
                reverseDistances,
                connectorDistances,
                reverseDistances.Length);

            // The common case stays cheap: apply one globally shortest
            // connector before constructing the more diverse port-pair set.
            Vector2Int[] firstConnector = FindShortestConnectorPath(
                start,
                target,
                start,
                passageIndex,
                blockedCells: null,
                searchStream: 0,
                connectorDistances,
                maximumLength: int.MaxValue);
            if (metrics.IsLimited || firstConnector == null)
            {
                return false;
            }

            firstConnector = RemoveInducedPathChords(firstConnector);
            if (!IsValidConnectorPath(
                    firstConnector,
                    target,
                    passageIndex))
            {
                throw new InvalidOperationException(
                    "The normalized shortest connector is not an induced path.");
            }

            if (TryConnectorAlternative(firstConnector, passageIndex))
            {
                return true;
            }

            if (metrics.IsLimited)
            {
                return false;
            }

            int maximumLength = checked(
                firstConnector.Length - 1 + connectorDetourAllowance);
            List<ConnectorPathCandidate> connectorCandidates = new(16);
            for (int startDirectionIndex = 0;
                 startDirectionIndex < CardinalDirections.Length;
                 startDirectionIndex++)
            {
                Vector2Int requiredFirstCell =
                    start + CardinalDirections[startDirectionIndex];
                if (!bounds.Contains(requiredFirstCell) ||
                    requiredFirstCell == target ||
                    !CanUseConnectorTransition(
                        start,
                        requiredFirstCell,
                        start,
                        target,
                        passageIndex))
                {
                    continue;
                }

                for (int targetDirectionIndex = 0;
                     targetDirectionIndex < CardinalDirections.Length;
                     targetDirectionIndex++)
                {
                    Vector2Int requiredTargetPredecessor =
                        target + CardinalDirections[targetDirectionIndex];
                    if (!bounds.Contains(requiredTargetPredecessor) ||
                        requiredTargetPredecessor == start ||
                        !CanUseConnectorTransition(
                            start,
                            requiredTargetPredecessor,
                            start,
                            target,
                            passageIndex))
                    {
                        continue;
                    }

                    int searchStream =
                        1 +
                        startDirectionIndex * CardinalDirections.Length +
                        targetDirectionIndex;
                    Vector2Int[] connector = FindWeightedConnectorPath(
                        start,
                        target,
                        passageIndex,
                        searchStream,
                        connectorDistances,
                        maximumLength,
                        requiredFirstCell,
                        requiredTargetPredecessor);
                    if (metrics.IsLimited)
                    {
                        return false;
                    }

                    if (connector == null)
                    {
                        continue;
                    }

                    connector = RemoveInducedPathChords(connector);
                    if (connector.Length - 1 > maximumLength ||
                        !IsValidConnectorPath(
                            connector,
                            target,
                            passageIndex) ||
                        AreSamePath(firstConnector, connector) ||
                        ContainsConnectorPath(
                            connectorCandidates,
                            connector))
                    {
                        continue;
                    }

                    connectorCandidates.Add(new ConnectorPathCandidate(
                        connector,
                        CreateConnectorPathTieBreaker(
                            passageIndex,
                            connector)));

                    if (maxConnectorAlternatives == 1)
                    {
                        connectorAlternativeLimitEncountered = true;
                        return false;
                    }
                }
            }

            if (connectorCandidates.Count == 0)
            {
                return false;
            }

            connectorCandidates.Sort(CompareConnectorPathCandidates);
            int candidateCountWithinDetour = 0;
            while (candidateCountWithinDetour < connectorCandidates.Count &&
                   connectorCandidates[candidateCountWithinDetour].Path.Length - 1 <=
                   maximumLength)
            {
                candidateCountWithinDetour++;
            }

            int attemptedCount = Math.Min(
                maxConnectorAlternatives - 1,
                candidateCountWithinDetour);
            for (int index = 0; index < attemptedCount; index++)
            {
                if (TryConnectorAlternative(
                        connectorCandidates[index].Path,
                        passageIndex))
                {
                    return true;
                }

                if (metrics.IsLimited)
                {
                    return false;
                }
            }

            if (candidateCountWithinDetour > maxConnectorAlternatives - 1)
            {
                connectorAlternativeLimitEncountered = true;
            }

            return false;
        }

        private static bool AreSamePath(
            IReadOnlyList<Vector2Int> first,
            IReadOnlyList<Vector2Int> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (int index = 0; index < first.Count; index++)
            {
                if (first[index] != second[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsConnectorPath(
            IReadOnlyList<ConnectorPathCandidate> candidates,
            IReadOnlyList<Vector2Int> pathCandidate)
        {
            foreach (ConnectorPathCandidate candidate in candidates)
            {
                if (candidate.Path.Length != pathCandidate.Count)
                {
                    continue;
                }

                bool equal = true;
                for (int index = 0; index < pathCandidate.Count; index++)
                {
                    if (candidate.Path[index] == pathCandidate[index])
                    {
                        continue;
                    }

                    equal = false;
                    break;
                }

                if (equal)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareConnectorPathCandidates(
            ConnectorPathCandidate left,
            ConnectorPathCandidate right)
        {
            int comparison = left.Path.Length.CompareTo(right.Path.Length);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = left.TieBreaker.CompareTo(right.TieBreaker);
            if (comparison != 0)
            {
                return comparison;
            }

            for (int index = 0; index < left.Path.Length; index++)
            {
                comparison = CompareCellsRowMajor(
                    left.Path[index],
                    right.Path[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return 0;
        }

        private static Vector2Int[] RemoveInducedPathChords(
            IReadOnlyList<Vector2Int> source)
        {
            List<Vector2Int> normalized = new(source.Count);
            foreach (Vector2Int cell in source)
            {
                int shortcutIndex = -1;
                for (int index = normalized.Count - 2; index >= 0; index--)
                {
                    if (GetManhattanDistance(normalized[index], cell) == 1)
                    {
                        shortcutIndex = index;
                        break;
                    }
                }

                if (shortcutIndex >= 0)
                {
                    normalized.RemoveRange(
                        shortcutIndex + 1,
                        normalized.Count - shortcutIndex - 1);
                }

                normalized.Add(cell);
            }

            return normalized.ToArray();
        }

        private Vector2Int[] FindShortestConnectorPath(
            Vector2Int searchStart,
            Vector2Int target,
            Vector2Int connectorStart,
            int passageIndex,
            ISet<Vector2Int> blockedCells,
            int searchStream,
            IReadOnlyList<int> connectorDistances,
            int maximumLength,
            Vector2Int? requiredFirstCell = null,
            Vector2Int? requiredTargetPredecessor = null)
        {
            if (searchStart == target)
            {
                return new[] { searchStart };
            }

            AdvanceShortestPathVisitId();
            ConnectorOpenSet openSet = new();
            int startCellIndex = GetCellIndex(searchStart);
            if (connectorDistances[startCellIndex] > maximumLength)
            {
                return null;
            }

            shortestPathVisitIds[startCellIndex] = shortestPathVisitId;
            shortestPathCosts[startCellIndex] = 0;
            shortestPathParents[startCellIndex] = searchStart;
            int sequence = 0;
            openSet.Push(new ConnectorSearchNode(
                searchStart,
                cost: 0,
                estimatedTotalCost: connectorDistances[startCellIndex],
                tieBreaker: CreateCellTieBreaker(
                    passageIndex,
                    searchStream,
                    searchStart,
                    cost: 0),
                sequence: sequence++));

            while (openSet.Count > 0)
            {
                ConnectorSearchNode current = openSet.Pop();
                int currentIndex = GetCellIndex(current.Cell);
                if (shortestPathVisitIds[currentIndex] != shortestPathVisitId ||
                    shortestPathCosts[currentIndex] != current.Cost)
                {
                    continue;
                }

                if (!metrics.TryRecordAStarExpansion())
                {
                    return null;
                }

                if (current.Cell == target)
                {
                    return ReconstructShortestPath(searchStart, target);
                }

                bool currentTouchesTarget =
                    GetManhattanDistance(current.Cell, target) == 1;
                if (currentTouchesTarget &&
                    requiredTargetPredecessor.HasValue &&
                    current.Cell != requiredTargetPredecessor.Value)
                {
                    // Entering the target later would create an unintended
                    // adjacency with this already visited cell.
                    continue;
                }

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int next =
                        current.Cell + CardinalDirections[directionIndex];
                    if (current.Cell == searchStart &&
                        requiredFirstCell.HasValue &&
                        next != requiredFirstCell.Value)
                    {
                        continue;
                    }

                    if (next == target &&
                        requiredTargetPredecessor.HasValue &&
                        current.Cell != requiredTargetPredecessor.Value)
                    {
                        continue;
                    }

                    if (currentTouchesTarget && next != target)
                    {
                        continue;
                    }

                    if (!bounds.Contains(next) ||
                        (blockedCells != null &&
                         blockedCells.Contains(next)) ||
                        !CanUseConnectorTransition(
                            current.Cell,
                            next,
                            connectorStart,
                            target,
                            passageIndex))
                    {
                        continue;
                    }

                    int nextIndex = GetCellIndex(next);
                    int relaxedDistance = connectorDistances[nextIndex];
                    if (relaxedDistance < 0)
                    {
                        continue;
                    }

                    int nextCost = current.Cost + 1;
                    if (nextCost + relaxedDistance > maximumLength)
                    {
                        continue;
                    }

                    if (shortestPathVisitIds[nextIndex] == shortestPathVisitId &&
                        shortestPathCosts[nextIndex] <= nextCost)
                    {
                        continue;
                    }

                    shortestPathVisitIds[nextIndex] = shortestPathVisitId;
                    shortestPathCosts[nextIndex] = nextCost;
                    shortestPathParents[nextIndex] = current.Cell;
                    if (openSet.Count >= maxOpenSetCapacity)
                    {
                        metrics.SetLimit(
                            StageRouteSearchLimitKind.OpenSetCapacity);
                        return null;
                    }

                    openSet.Push(new ConnectorSearchNode(
                        next,
                        nextCost,
                        nextCost + relaxedDistance,
                        CreateCellTieBreaker(
                            passageIndex,
                            searchStream,
                            next,
                            nextCost),
                        sequence++));
                }
            }

            return null;
        }

        /// <summary>
        /// Finds one deterministic, topology-constrained connector within the
        /// hard step bound. Positive per-cell weights vary by stream, allowing
        /// a bounded detour to win without ever blocking a mandatory bottleneck.
        /// The (cell, step) state prevents a cheap longer prefix from hiding a
        /// shorter prefix which is still able to satisfy the step bound.
        /// </summary>
        private Vector2Int[] FindWeightedConnectorPath(
            Vector2Int searchStart,
            Vector2Int target,
            int passageIndex,
            int searchStream,
            IReadOnlyList<int> connectorDistances,
            int maximumLength,
            Vector2Int requiredFirstCell,
            Vector2Int requiredTargetPredecessor)
        {
            if (searchStart == target)
            {
                return new[] { searchStart };
            }

            int cellCount = reverseDistances.Length;
            int boundedMaximumLength = Math.Min(
                maximumLength,
                cellCount - 1);
            int startIndex = GetCellIndex(searchStart);
            if (connectorDistances[startIndex] < 0 ||
                connectorDistances[startIndex] > boundedMaximumLength)
            {
                return null;
            }

            Dictionary<long, int> bestCosts = new();
            WeightedConnectorOpenSet openSet = new();
            int sequence = 0;
            WeightedConnectorSearchNode startNode =
                new(
                    searchStart,
                    steps: 0,
                    weightedCost: 0,
                    estimatedWeightedCost: connectorDistances[startIndex],
                    tieBreaker: CreateCellTieBreaker(
                        passageIndex,
                        searchStream,
                        searchStart,
                        cost: 0),
                    sequence: sequence++,
                    parent: null);
            bestCosts.Add(CreateWeightedStateKey(startIndex, 0), 0);
            openSet.Push(startNode);

            while (openSet.Count > 0)
            {
                WeightedConnectorSearchNode current = openSet.Pop();
                int currentIndex = GetCellIndex(current.Cell);
                long currentKey = CreateWeightedStateKey(
                    currentIndex,
                    current.Steps);
                if (!bestCosts.TryGetValue(currentKey, out int bestCost) ||
                    bestCost != current.WeightedCost)
                {
                    continue;
                }

                if (!metrics.TryRecordAStarExpansion())
                {
                    return null;
                }

                if (current.Cell == target)
                {
                    return ReconstructWeightedPath(current);
                }

                bool currentTouchesTarget =
                    GetManhattanDistance(current.Cell, target) == 1;
                if (currentTouchesTarget &&
                    current.Cell != requiredTargetPredecessor)
                {
                    continue;
                }

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int next =
                        current.Cell + CardinalDirections[directionIndex];
                    if (current.Steps == 0 && next != requiredFirstCell)
                    {
                        continue;
                    }

                    if (next == target &&
                        current.Cell != requiredTargetPredecessor)
                    {
                        continue;
                    }

                    if (currentTouchesTarget && next != target)
                    {
                        continue;
                    }

                    if (!bounds.Contains(next) ||
                        !CanUseConnectorTransition(
                            current.Cell,
                            next,
                            searchStart,
                            target,
                            passageIndex))
                    {
                        continue;
                    }

                    int nextIndex = GetCellIndex(next);
                    int relaxedDistance = connectorDistances[nextIndex];
                    if (relaxedDistance < 0)
                    {
                        continue;
                    }

                    int nextSteps = current.Steps + 1;
                    if (nextSteps + relaxedDistance > boundedMaximumLength)
                    {
                        continue;
                    }

                    int nextWeightedCost = checked(
                        current.WeightedCost +
                        GetDeterministicCellWeight(
                            passageIndex,
                            searchStream,
                            next));
                    if (IsDominatedWeightedState(
                            bestCosts,
                            nextIndex,
                            nextSteps,
                            nextWeightedCost))
                    {
                        continue;
                    }

                    long nextKey = CreateWeightedStateKey(
                        nextIndex,
                        nextSteps);
                    bestCosts[nextKey] = nextWeightedCost;
                    if (openSet.Count >= maxOpenSetCapacity)
                    {
                        metrics.SetLimit(
                            StageRouteSearchLimitKind.OpenSetCapacity);
                        return null;
                    }

                    openSet.Push(new WeightedConnectorSearchNode(
                        next,
                        nextSteps,
                        nextWeightedCost,
                        nextWeightedCost + relaxedDistance,
                        CreateCellTieBreaker(
                            passageIndex,
                            searchStream,
                            next,
                            nextSteps),
                        sequence++,
                        current));
                }
            }

            return null;
        }

        private static bool IsDominatedWeightedState(
            IReadOnlyDictionary<long, int> bestCosts,
            int cellIndex,
            int steps,
            int weightedCost)
        {
            for (int earlierSteps = 0;
                 earlierSteps <= steps;
                 earlierSteps++)
            {
                if (bestCosts.TryGetValue(
                        CreateWeightedStateKey(cellIndex, earlierSteps),
                        out int earlierCost) &&
                    earlierCost <= weightedCost)
                {
                    return true;
                }
            }

            return false;
        }

        private static long CreateWeightedStateKey(
            int cellIndex,
            int steps)
        {
            return ((long)steps << 32) | (uint)cellIndex;
        }

        private int GetDeterministicCellWeight(
            int passageIndex,
            int searchStream,
            Vector2Int cell)
        {
            ulong value = CreateCellTieBreaker(
                passageIndex,
                searchStream + 193,
                cell,
                cost: 0);
            return 1 + (int)(value & 3UL);
        }

        private static Vector2Int[] ReconstructWeightedPath(
            WeightedConnectorSearchNode targetNode)
        {
            Vector2Int[] result = new Vector2Int[targetNode.Steps + 1];
            WeightedConnectorSearchNode current = targetNode;
            for (int index = result.Length - 1; index >= 0; index--)
            {
                result[index] = current.Cell;
                current = current.Parent;
            }

            return result;
        }

        private bool IsValidConnectorPath(
            IReadOnlyList<Vector2Int> connector,
            Vector2Int target,
            int passageIndex)
        {
            if (connector.Count == 0 ||
                connector[connector.Count - 1] != target)
            {
                return false;
            }

            for (int index = 1; index < connector.Count; index++)
            {
                if (GetManhattanDistance(
                        connector[index - 1],
                        connector[index]) != 1 ||
                    !CanAddConnectorCell(
                        connector,
                        index,
                        target,
                        passageIndex))
                {
                    return false;
                }
            }

            return true;
        }

        private bool BuildReverseDistanceField(
            Vector2Int source,
            Vector2Int target)
        {
            for (int index = 0; index < reverseDistances.Length; index++)
            {
                reverseDistances[index] = -1;
            }

            metrics.BeginReachabilityCheck();
            int queueStart = 0;
            int queueEnd = 0;
            reachabilityQueue[queueEnd++] = target;
            reverseDistances[GetCellIndex(target)] = 0;

            while (queueStart < queueEnd)
            {
                Vector2Int current = reachabilityQueue[queueStart++];
                if (!metrics.TryRecordReachabilityCell())
                {
                    return false;
                }

                int nextDistance = reverseDistances[GetCellIndex(current)] + 1;
                foreach (Vector2Int direction in CardinalDirections)
                {
                    Vector2Int next = current + direction;
                    if (!CanUseRelaxedConnectorCell(next, source, target))
                    {
                        continue;
                    }

                    int nextIndex = GetCellIndex(next);
                    if (reverseDistances[nextIndex] >= 0)
                    {
                        continue;
                    }

                    reverseDistances[nextIndex] = nextDistance;
                    reachabilityQueue[queueEnd++] = next;
                }
            }

            return reverseDistances[GetCellIndex(source)] >= 0;
        }

        private bool CanUseRelaxedConnectorCell(
            Vector2Int cell,
            Vector2Int source,
            Vector2Int target)
        {
            if (!bounds.Contains(cell) ||
                headquartersFootprint.Contains(cell))
            {
                return false;
            }

            if (cell == source || cell == target)
            {
                return true;
            }

            return cell != goal &&
                   !usedCellCounts.ContainsKey(cell) &&
                   !reservedPatternCells.Contains(cell);
        }

        private bool CanUseConnectorTransition(
            Vector2Int current,
            Vector2Int candidate,
            Vector2Int connectorStart,
            Vector2Int target,
            int passageIndex)
        {
            if (!bounds.Contains(candidate) ||
                headquartersFootprint.Contains(candidate) ||
                usedCellCounts.ContainsKey(candidate))
            {
                return false;
            }

            bool isTarget = candidate == target;
            if (candidate == goal && !isTarget)
            {
                return false;
            }

            if (reservedPatternCells.Contains(candidate) && !isTarget)
            {
                return false;
            }

            Vector2Int? nextFixedCell = null;
            if (isTarget && passageIndex < passages.Count)
            {
                StageRoutePatternPassage passage = passages[passageIndex];
                if (passage.Cells.Count > 1)
                {
                    nextFixedCell = passage.Cells[1];
                }
            }

            foreach (Vector2Int direction in CardinalDirections)
            {
                Vector2Int neighbor = candidate + direction;
                bool isStartPredecessor =
                    neighbor == current && neighbor == connectorStart;
                bool isNextFixedCell =
                    nextFixedCell.HasValue && neighbor == nextFixedCell.Value;
                if (usedCellCounts.ContainsKey(neighbor) &&
                    !isStartPredecessor &&
                    !isNextFixedCell)
                {
                    return false;
                }

                bool isReservedNeighbor =
                    reservedPatternCells.Contains(neighbor) || neighbor == goal;
                if (!isReservedNeighbor)
                {
                    continue;
                }

                if (usedCellCounts.ContainsKey(neighbor) &&
                    (isStartPredecessor || isNextFixedCell))
                {
                    continue;
                }

                if (neighbor == target || isNextFixedCell)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool CanAddConnectorCell(
            IReadOnlyList<Vector2Int> connector,
            int candidateIndex,
            Vector2Int target,
            int passageIndex)
        {
            Vector2Int current = connector[candidateIndex - 1];
            Vector2Int candidate = connector[candidateIndex];
            if (!bounds.Contains(candidate) ||
                headquartersFootprint.Contains(candidate) ||
                usedCellCounts.ContainsKey(candidate) ||
                ContainsConnectorCell(
                    connector,
                    candidateIndex,
                    candidate))
            {
                return false;
            }

            bool isTarget = candidate == target;
            if (candidate == goal && !isTarget)
            {
                return false;
            }

            if (reservedPatternCells.Contains(candidate) && !isTarget)
            {
                return false;
            }

            Vector2Int? nextFixedCell = null;
            if (isTarget && passageIndex < passages.Count)
            {
                StageRoutePatternPassage passage = passages[passageIndex];
                if (passage.Cells.Count > 1)
                {
                    nextFixedCell = passage.Cells[1];
                }
            }

            foreach (Vector2Int direction in CardinalDirections)
            {
                Vector2Int neighbor = candidate + direction;
                bool isPredecessor = neighbor == current;
                bool isNextFixedCell =
                    nextFixedCell.HasValue && neighbor == nextFixedCell.Value;
                bool isConnectorPrefixCell = ContainsConnectorCell(
                    connector,
                    candidateIndex,
                    neighbor);
                if ((usedCellCounts.ContainsKey(neighbor) ||
                     isConnectorPrefixCell) &&
                    !isPredecessor &&
                    !isNextFixedCell)
                {
                    return false;
                }

                bool isReservedNeighbor =
                    reservedPatternCells.Contains(neighbor) || neighbor == goal;
                if (!isReservedNeighbor)
                {
                    continue;
                }

                if (usedCellCounts.ContainsKey(neighbor) &&
                    (isPredecessor || isNextFixedCell))
                {
                    continue;
                }

                if (neighbor == target || isNextFixedCell)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool AppendFixedPassage(
            int passageIndex,
            StageRoutePatternPassage passage)
        {
            for (int cellIndex = 1;
                 cellIndex < passage.Cells.Count;
                 cellIndex++)
            {
                Vector2Int cell = passage.Cells[cellIndex];
                if (!CanAddFixedPassageCell(
                        passageIndex,
                        passage,
                        cellIndex,
                        cell))
                {
                    return false;
                }

                path.Add(cell);
                IncrementUsedCell(cell);
            }

            return true;
        }

        private bool CanAddFixedPassageCell(
            int passageIndex,
            StageRoutePatternPassage passage,
            int cellIndex,
            Vector2Int cell)
        {
            if (!bounds.Contains(cell) ||
                headquartersFootprint.Contains(cell))
            {
                return false;
            }

            bool isRegisteredCrossUse =
                crossingPassageIds.TryGetValue(
                    cell,
                    out HashSet<string> allowedPassages) &&
                allowedPassages.Contains(passage.PassageId);
            int currentUseCount = usedCellCounts.TryGetValue(cell, out int count)
                ? count
                : 0;
            if (isRegisteredCrossUse)
            {
                if (currentUseCount >= 2)
                {
                    return false;
                }
            }
            else if (currentUseCount != 0)
            {
                return false;
            }

            Vector2Int previous = passage.Cells[cellIndex - 1];
            Vector2Int? next = cellIndex + 1 < passage.Cells.Count
                ? passage.Cells[cellIndex + 1]
                : null;
            bool isLastPassageExit =
                passageIndex == passages.Count - 1 &&
                cellIndex == passage.Cells.Count - 1;

            foreach (Vector2Int direction in CardinalDirections)
            {
                Vector2Int neighbor = cell + direction;
                if (usedCellCounts.ContainsKey(neighbor) &&
                    neighbor != previous &&
                    !IsCrossingArmRelationship(cell, neighbor))
                {
                    return false;
                }

                bool isReservedNeighbor =
                    reservedPatternCells.Contains(neighbor) || neighbor == goal;
                if (!isReservedNeighbor || usedCellCounts.ContainsKey(neighbor))
                {
                    continue;
                }

                if ((next.HasValue && neighbor == next.Value) ||
                    IsCrossingArmRelationship(cell, neighbor) ||
                    (isLastPassageExit && neighbor == goal))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool IsCrossingArmRelationship(
            Vector2Int first,
            Vector2Int second)
        {
            if (crossingArmCells.TryGetValue(
                    first,
                    out HashSet<Vector2Int> firstArms) &&
                firstArms.Contains(second))
            {
                return true;
            }

            return crossingArmCells.TryGetValue(
                       second,
                       out HashSet<Vector2Int> secondArms) &&
                   secondArms.Contains(first);
        }

        private void AppendConnector(IReadOnlyList<Vector2Int> connector)
        {
            for (int index = 1; index < connector.Count; index++)
            {
                Vector2Int cell = connector[index];
                path.Add(cell);
                IncrementUsedCell(cell);
            }
        }

        private bool HasOnlyGraphBackedRoadAdjacency()
        {
            HashSet<Vector2Int> roadCells = new(path);
            HashSet<CellPair> graphBackedPairs = new();
            for (int index = 1; index < path.Count; index++)
            {
                if (path[index - 1] != path[index])
                {
                    graphBackedPairs.Add(
                        new CellPair(path[index - 1], path[index]));
                }
            }

            foreach (Vector2Int roadCell in roadCells)
            {
                Vector2Int right = roadCell + Vector2Int.right;
                if (roadCells.Contains(right) &&
                    !graphBackedPairs.Contains(
                        new CellPair(roadCell, right)))
                {
                    return false;
                }

                Vector2Int up = roadCell + Vector2Int.up;
                if (roadCells.Contains(up) &&
                    !graphBackedPairs.Contains(new CellPair(roadCell, up)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsConnectorCell(
            IReadOnlyList<Vector2Int> connector,
            int exclusiveEndIndex,
            Vector2Int cell)
        {
            for (int index = 0; index < exclusiveEndIndex; index++)
            {
                if (connector[index] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        private void AdvanceShortestPathVisitId()
        {
            if (shortestPathVisitId == int.MaxValue)
            {
                Array.Clear(
                    shortestPathVisitIds,
                    0,
                    shortestPathVisitIds.Length);
                shortestPathVisitId = 1;
                return;
            }

            shortestPathVisitId++;
        }

        private Vector2Int[] ReconstructShortestPath(
            Vector2Int start,
            Vector2Int target)
        {
            int length = shortestPathCosts[GetCellIndex(target)] + 1;
            Vector2Int[] result = new Vector2Int[length];
            Vector2Int current = target;
            for (int index = length - 1; index >= 0; index--)
            {
                result[index] = current;
                if (index > 0)
                {
                    current = shortestPathParents[GetCellIndex(current)];
                }
            }

            if (result[0] != start)
            {
                throw new InvalidOperationException(
                    "The connector parent map did not return to its search start.");
            }

            return result;
        }

        private ulong CreateCellTieBreaker(
            int passageIndex,
            int searchStream,
            Vector2Int cell,
            int cost)
        {
            unchecked
            {
                ulong value = 0xD1B54A32D192ED03UL;
                value ^= (ulong)(uint)seed * 0x9E3779B97F4A7C15UL;
                value ^= (ulong)(uint)candidateIndex * 0xBF58476D1CE4E5B9UL;
                value ^= (ulong)(uint)passageIndex * 0x94D049BB133111EBUL;
                value ^= (ulong)(uint)searchStream * 0xD6E8FEB86659FD93UL;
                value ^= (ulong)(uint)cell.x << 32;
                value ^= (uint)cell.y;
                value ^= (ulong)(uint)cost * 0xA0761D6478BD642FUL;
                return Mix(value);
            }
        }

        private ulong CreateConnectorPathTieBreaker(
            int passageIndex,
            IReadOnlyList<Vector2Int> connector)
        {
            unchecked
            {
                ulong value = CreateCellTieBreaker(
                    passageIndex,
                    searchStream: 97,
                    connector[0],
                    connector.Count);
                for (int index = 1; index < connector.Count; index++)
                {
                    Vector2Int cell = connector[index];
                    value ^= (ulong)(uint)cell.x << 32;
                    value ^= (uint)cell.y;
                    value ^= (ulong)(uint)index * 0xE7037ED1A0B428DBUL;
                    value = Mix(value);
                }

                return Mix(value);
            }
        }

        private static ulong Mix(ulong value)
        {
            unchecked
            {
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }

        private OrderedPatternPathSearchResult CreateLimitFailure()
        {
            return OrderedPatternPathSearchResult.Failure(
                OrderedPatternPathSearchOutcome.SearchLimitExceeded,
                metrics.LimitKind,
                metrics.Freeze());
        }

        private int GetCellIndex(Vector2Int cell)
        {
            int localX = cell.x - bounds.xMin;
            int localY = cell.y - bounds.yMin;
            return localY * bounds.width + localX;
        }

        private static int GetManhattanDistance(
            Vector2Int first,
            Vector2Int second)
        {
            return Math.Abs(first.x - second.x) +
                   Math.Abs(first.y - second.y);
        }

        private static int CompareCellsRowMajor(
            Vector2Int first,
            Vector2Int second)
        {
            int yComparison = first.y.CompareTo(second.y);
            return yComparison != 0
                ? yComparison
                : first.x.CompareTo(second.x);
        }

        private void IncrementUsedCell(Vector2Int cell)
        {
            usedCellCounts[cell] = usedCellCounts.TryGetValue(cell, out int count)
                ? count + 1
                : 1;
        }

        private void RemoveLastPathCell()
        {
            int lastIndex = path.Count - 1;
            Vector2Int cell = path[lastIndex];
            path.RemoveAt(lastIndex);

            int count = usedCellCounts[cell] - 1;
            if (count == 0)
            {
                usedCellCounts.Remove(cell);
            }
            else
            {
                usedCellCounts[cell] = count;
            }
        }

        private void RollBackPath(int originalPathCount)
        {
            while (path.Count > originalPathCount)
            {
                RemoveLastPathCell();
            }
        }

        private sealed class ConnectorSearchNode
        {
            internal Vector2Int Cell { get; }
            internal int Cost { get; }
            internal int EstimatedTotalCost { get; }
            internal ulong TieBreaker { get; }
            internal int Sequence { get; }

            internal ConnectorSearchNode(
                Vector2Int cell,
                int cost,
                int estimatedTotalCost,
                ulong tieBreaker,
                int sequence)
            {
                Cell = cell;
                Cost = cost;
                EstimatedTotalCost = estimatedTotalCost;
                TieBreaker = tieBreaker;
                Sequence = sequence;
            }
        }

        private sealed class ConnectorPathCandidate
        {
            internal Vector2Int[] Path { get; }
            internal ulong TieBreaker { get; }

            internal ConnectorPathCandidate(
                Vector2Int[] path,
                ulong tieBreaker)
            {
                Path = path ?? throw new ArgumentNullException(nameof(path));
                TieBreaker = tieBreaker;
            }
        }

        private sealed class WeightedConnectorSearchNode
        {
            internal Vector2Int Cell { get; }
            internal int Steps { get; }
            internal int WeightedCost { get; }
            internal int EstimatedWeightedCost { get; }
            internal ulong TieBreaker { get; }
            internal int Sequence { get; }
            internal WeightedConnectorSearchNode Parent { get; }

            internal WeightedConnectorSearchNode(
                Vector2Int cell,
                int steps,
                int weightedCost,
                int estimatedWeightedCost,
                ulong tieBreaker,
                int sequence,
                WeightedConnectorSearchNode parent)
            {
                Cell = cell;
                Steps = steps;
                WeightedCost = weightedCost;
                EstimatedWeightedCost = estimatedWeightedCost;
                TieBreaker = tieBreaker;
                Sequence = sequence;
                Parent = parent;
            }
        }

        private sealed class WeightedConnectorOpenSet
        {
            private readonly List<WeightedConnectorSearchNode> heap = new();

            internal int Count => heap.Count;

            internal void Push(WeightedConnectorSearchNode node)
            {
                heap.Add(node);
                int index = heap.Count - 1;
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (Compare(heap[parentIndex], node) <= 0)
                    {
                        break;
                    }

                    heap[index] = heap[parentIndex];
                    index = parentIndex;
                }

                heap[index] = node;
            }

            internal WeightedConnectorSearchNode Pop()
            {
                WeightedConnectorSearchNode result = heap[0];
                int lastIndex = heap.Count - 1;
                WeightedConnectorSearchNode tail = heap[lastIndex];
                heap.RemoveAt(lastIndex);
                if (lastIndex == 0)
                {
                    return result;
                }

                int index = 0;
                while (true)
                {
                    int leftIndex = index * 2 + 1;
                    if (leftIndex >= heap.Count)
                    {
                        break;
                    }

                    int rightIndex = leftIndex + 1;
                    int smallerChildIndex =
                        rightIndex < heap.Count &&
                        Compare(heap[rightIndex], heap[leftIndex]) < 0
                            ? rightIndex
                            : leftIndex;
                    if (Compare(tail, heap[smallerChildIndex]) <= 0)
                    {
                        break;
                    }

                    heap[index] = heap[smallerChildIndex];
                    index = smallerChildIndex;
                }

                heap[index] = tail;
                return result;
            }

            private static int Compare(
                WeightedConnectorSearchNode left,
                WeightedConnectorSearchNode right)
            {
                int comparison = left.EstimatedWeightedCost.CompareTo(
                    right.EstimatedWeightedCost);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = right.Steps.CompareTo(left.Steps);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = left.TieBreaker.CompareTo(right.TieBreaker);
                return comparison != 0
                    ? comparison
                    : left.Sequence.CompareTo(right.Sequence);
            }
        }

        private sealed class ConnectorOpenSet
        {
            private readonly List<ConnectorSearchNode> heap = new();

            internal int Count => heap.Count;

            internal void Push(ConnectorSearchNode node)
            {
                heap.Add(node);
                int index = heap.Count - 1;
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (Compare(heap[parentIndex], node) <= 0)
                    {
                        break;
                    }

                    heap[index] = heap[parentIndex];
                    index = parentIndex;
                }

                heap[index] = node;
            }

            internal ConnectorSearchNode Pop()
            {
                ConnectorSearchNode result = heap[0];
                int lastIndex = heap.Count - 1;
                ConnectorSearchNode tail = heap[lastIndex];
                heap.RemoveAt(lastIndex);
                if (lastIndex == 0)
                {
                    return result;
                }

                int index = 0;
                while (true)
                {
                    int leftIndex = index * 2 + 1;
                    if (leftIndex >= heap.Count)
                    {
                        break;
                    }

                    int rightIndex = leftIndex + 1;
                    int smallerChildIndex =
                        rightIndex < heap.Count &&
                        Compare(heap[rightIndex], heap[leftIndex]) < 0
                            ? rightIndex
                            : leftIndex;
                    if (Compare(tail, heap[smallerChildIndex]) <= 0)
                    {
                        break;
                    }

                    heap[index] = heap[smallerChildIndex];
                    index = smallerChildIndex;
                }

                heap[index] = tail;
                return result;
            }

            private static int Compare(
                ConnectorSearchNode left,
                ConnectorSearchNode right)
            {
                int comparison = left.EstimatedTotalCost.CompareTo(
                    right.EstimatedTotalCost);
                if (comparison != 0)
                {
                    return comparison;
                }

                // For equal f-scores, deeper states reach a complete shortest
                // path sooner without changing path-cost ordering.
                comparison = right.Cost.CompareTo(left.Cost);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = left.TieBreaker.CompareTo(right.TieBreaker);
                return comparison != 0
                    ? comparison
                    : left.Sequence.CompareTo(right.Sequence);
            }
        }

        private readonly struct CellPair : IEquatable<CellPair>
        {
            private readonly Vector2Int first;
            private readonly Vector2Int second;

            internal CellPair(Vector2Int left, Vector2Int right)
            {
                if (CompareCells(left, right) <= 0)
                {
                    first = left;
                    second = right;
                }
                else
                {
                    first = right;
                    second = left;
                }
            }

            public bool Equals(CellPair other)
            {
                return first == other.first && second == other.second;
            }

            public override bool Equals(object obj)
            {
                return obj is CellPair other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + first.x;
                    hash = hash * 31 + first.y;
                    hash = hash * 31 + second.x;
                    hash = hash * 31 + second.y;
                    return hash;
                }
            }

            private static int CompareCells(
                Vector2Int left,
                Vector2Int right)
            {
                int yComparison = left.y.CompareTo(right.y);
                return yComparison != 0
                    ? yComparison
                    : left.x.CompareTo(right.x);
            }
        }
    }
}
