using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class DeterministicStageRouteGenerator
    {
        public const string GeneratorVersion = "deterministic-stage-route-v4";

        private readonly IStageRoutePatternStrategy patternStrategy;

        public DeterministicStageRouteGenerator()
            : this(new QuadrantStageRoutePatternStrategy())
        {
        }

        public DeterministicStageRouteGenerator(
            IStageRoutePatternStrategy patternStrategy)
        {
            this.patternStrategy = patternStrategy ??
                throw new ArgumentNullException(nameof(patternStrategy));
            EnsureStrategyIdentity(patternStrategy);
        }

        public StageRouteGenerationResult Generate(
            StageRouteGenerationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            EnsureStrategyIdentity(patternStrategy);
            StageRoutePatternCandidateSet candidateSet =
                patternStrategy.CreateCandidates(settings) ??
                throw new InvalidOperationException(
                    $"Pattern strategy '{patternStrategy.StrategyId}' returned null.");

            if (!candidateSet.Succeeded)
            {
                StageRouteGenerationFailureReason failureReason =
                    candidateSet.FailureReason ==
                    StageRoutePatternCandidateFailureReason.NoValidPassageOrder
                        ? StageRouteGenerationFailureReason.NoValidPassageOrder
                        : StageRouteGenerationFailureReason.NoFeasiblePatternLayout;
                StageRouteGenerationDiagnostics diagnostics =
                    CreateDiagnostics(
                        candidateSet,
                        Array.Empty<StageRouteCandidateDiagnostic>(),
                        totalSearchBudgetExceeded: false);
                return StageRouteGenerationResult.Failure(
                    failureReason,
                    failureReason == StageRouteGenerationFailureReason.NoValidPassageOrder
                        ? "No valid logical passage order satisfies the slot constraints."
                        : "No feasible physical pattern layout could be generated.",
                    diagnostics);
            }

            if (candidateSet.CandidateRecords.Count >
                settings.MaxRouteCandidateCount)
            {
                throw new InvalidOperationException(
                    $"Pattern strategy '{patternStrategy.StrategyId}' returned " +
                    $"{candidateSet.CandidateRecords.Count} candidates, exceeding " +
                    $"the configured limit of {settings.MaxRouteCandidateCount}.");
            }

            IReadOnlyList<StageRoutePatternCandidateRecord> candidates =
                candidateSet.CandidateRecords;
            CandidateSearchAccumulator[] candidateSearches =
                CreateCandidateSearchAccumulators(candidates);
            int totalSearchWork = 0;
            bool totalSearchBudgetExceeded = false;
            int totalWorkLimitedCandidateIndex = -1;

            // Candidate records are already arranged in deterministic variant-major
            // order by the strategy. Search each candidate exactly once so its full
            // per-candidate allowance contributes to one bounded solver run instead
            // of being lost to repeated restarts.
            for (int index = 0; index < candidates.Count; index++)
            {
                if (totalSearchWork >= settings.MaxTotalSearchWork)
                {
                    totalSearchBudgetExceeded = true;
                    break;
                }

                CandidateSearchAccumulator candidate = candidateSearches[index];
                int remainingTotalWork =
                    settings.MaxTotalSearchWork - totalSearchWork;
                int workLimit = Math.Min(
                    settings.MaxSearchWorkPerCandidate,
                    remainingTotalWork);
                OrderedPatternPathSearchResult searchResult = SearchCandidate(
                    settings,
                    candidate,
                    workLimit,
                    ref totalSearchWork);
                if (searchResult.Succeeded)
                {
                    return CreateSuccessResult(
                        settings,
                        candidateSet,
                        candidateSearches,
                        candidate,
                        searchResult);
                }

                if (searchResult.Outcome ==
                        OrderedPatternPathSearchOutcome.SearchLimitExceeded &&
                    searchResult.LimitKind ==
                        StageRouteSearchLimitKind.PerCandidateWork &&
                    workLimit < settings.MaxSearchWorkPerCandidate &&
                    totalSearchWork >= settings.MaxTotalSearchWork)
                {
                    totalSearchBudgetExceeded = true;
                    totalWorkLimitedCandidateIndex = index;
                    break;
                }
            }

            if (totalSearchWork >= settings.MaxTotalSearchWork &&
                candidateSearches.Any(candidate =>
                    !candidate.WasAttempted ||
                    candidate.CanRetry(settings.MaxSearchWorkPerCandidate)))
            {
                totalSearchBudgetExceeded = true;
            }

            IReadOnlyList<StageRouteCandidateDiagnostic> candidateDiagnostics =
                FreezeCandidateDiagnostics(
                    candidateSearches,
                    totalWorkLimitedCandidateIndex);
            StageRouteGenerationDiagnostics finalDiagnostics =
                CreateDiagnostics(
                    candidateSet,
                    candidateDiagnostics,
                    totalSearchBudgetExceeded);
            StageRouteGenerationFailureReason finalFailure;
            if (finalDiagnostics.TotalSearchBudgetExceeded)
            {
                finalFailure =
                    StageRouteGenerationFailureReason.TotalSearchBudgetExceeded;
            }
            else if (finalDiagnostics.PerCandidateBudgetExceededCount > 0)
            {
                finalFailure =
                    StageRouteGenerationFailureReason.SearchBudgetExceeded;
            }
            else
            {
                finalFailure = StageRouteGenerationFailureReason.PathNotFound;
            }

            string message = BuildFailureMessage(
                finalFailure,
                finalDiagnostics,
                settings.MaxTotalSearchWork);
            return StageRouteGenerationResult.Failure(
                finalFailure,
                message,
                finalDiagnostics);
        }

        private StageRouteGenerationResult CreateSuccessResult(
            StageRouteGenerationSettings settings,
            StageRoutePatternCandidateSet candidateSet,
            IReadOnlyList<CandidateSearchAccumulator> candidateSearches,
            CandidateSearchAccumulator successfulCandidate,
            OrderedPatternPathSearchResult searchResult)
        {
            GeneratedStageRoute route = BuildRoute(
                settings,
                successfulCandidate.Record.Layout,
                searchResult);
            IReadOnlyList<StageRouteCandidateDiagnostic> candidateDiagnostics =
                FreezeCandidateDiagnostics(
                    candidateSearches,
                    totalWorkLimitedCandidateIndex: -1);
            StageRouteGenerationDiagnostics diagnostics = CreateDiagnostics(
                candidateSet,
                candidateDiagnostics,
                totalSearchBudgetExceeded: false);
            return StageRouteGenerationResult.Success(route, diagnostics);
        }

        private static OrderedPatternPathSearchResult SearchCandidate(
            StageRouteGenerationSettings settings,
            CandidateSearchAccumulator candidate,
            int workLimit,
            ref int totalSearchWork)
        {
            ValidatePatternLayout(settings, candidate.Record.Layout);
            OrderedPatternPathSolver solver = new(
                settings,
                candidate.Record.Layout,
                workLimit,
                settings.MaxConnectorAlternatives,
                settings.ConnectorDetourAllowance,
                candidate.CandidateIndex);
            OrderedPatternPathSearchResult searchResult = solver.Solve();
            if (searchResult.TotalWorkUnits > workLimit)
            {
                throw new InvalidOperationException(
                    $"Candidate '{candidate.Record.Layout.LayoutId}' exceeded " +
                    $"its assigned search work limit of {workLimit}.");
            }

            candidate.Accumulate(searchResult);
            totalSearchWork = checked(
                totalSearchWork + searchResult.TotalWorkUnits);
            if (candidate.WorkUnits > settings.MaxSearchWorkPerCandidate)
            {
                throw new InvalidOperationException(
                    $"Candidate '{candidate.Record.Layout.LayoutId}' exceeded " +
                    $"the cumulative search limit of " +
                    $"{settings.MaxSearchWorkPerCandidate}.");
            }

            if (totalSearchWork > settings.MaxTotalSearchWork)
            {
                throw new InvalidOperationException(
                    "The route scheduler exceeded the total search work limit.");
            }

            return searchResult;
        }

        private static CandidateSearchAccumulator[]
            CreateCandidateSearchAccumulators(
                IReadOnlyList<StageRoutePatternCandidateRecord> candidates)
        {
            CandidateSearchAccumulator[] accumulators =
                new CandidateSearchAccumulator[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                accumulators[index] = new CandidateSearchAccumulator(
                    index,
                    candidates[index]);
            }

            return accumulators;
        }

        private static IReadOnlyList<StageRouteCandidateDiagnostic>
            FreezeCandidateDiagnostics(
                IReadOnlyList<CandidateSearchAccumulator> candidates,
                int totalWorkLimitedCandidateIndex)
        {
            if (totalWorkLimitedCandidateIndex < -1 ||
                totalWorkLimitedCandidateIndex >= candidates.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalWorkLimitedCandidateIndex));
            }

            StageRouteCandidateDiagnostic[] diagnostics =
                new StageRouteCandidateDiagnostic[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                diagnostics[index] = candidates[index].FreezeDiagnostic(
                    markAsTotalWorkLimit:
                        index == totalWorkLimitedCandidateIndex);
            }

            return Array.AsReadOnly(diagnostics);
        }

        private static StageRouteGenerationDiagnostics CreateDiagnostics(
            StageRoutePatternCandidateSet candidateSet,
            IReadOnlyList<StageRouteCandidateDiagnostic> candidates,
            bool totalSearchBudgetExceeded)
        {
            int aStarNodeExpansionCount = 0;
            int connectorAlternativeCount = 0;
            int backtrackCount = 0;
            int reachabilityCheckCount = 0;
            int reachabilityVisitedCellCount = 0;
            int totalWorkUnits = 0;
            foreach (StageRouteCandidateDiagnostic candidate in candidates)
            {
                aStarNodeExpansionCount = checked(
                    aStarNodeExpansionCount +
                    candidate.AStarNodeExpansionCount);
                connectorAlternativeCount = checked(
                    connectorAlternativeCount +
                    candidate.ConnectorAlternativeCount);
                backtrackCount = checked(
                    backtrackCount + candidate.BacktrackCount);
                reachabilityCheckCount = checked(
                    reachabilityCheckCount +
                    candidate.ReachabilityCheckCount);
                reachabilityVisitedCellCount = checked(
                    reachabilityVisitedCellCount +
                    candidate.ReachabilityVisitedCellCount);
                totalWorkUnits = checked(
                    totalWorkUnits + candidate.WorkUnits);
            }

            return new StageRouteGenerationDiagnostics(
                candidateSet.PhysicalLayoutDrawCount,
                candidateSet.PhysicalPlacementRejectedCount,
                candidateSet.DuplicatePhysicalLayoutCount,
                candidateSet.UnselectedPhysicalLayoutCount,
                candidateSet.PhysicalLayoutCount,
                candidateSet.PassageOrderDrawCount,
                candidateSet.DuplicatePassageOrderCount,
                candidateSet.PassageOrderVariantCount,
                candidateSet.LayoutsWithoutValidOrderCount,
                aStarNodeExpansionCount,
                connectorAlternativeCount,
                backtrackCount,
                reachabilityCheckCount,
                reachabilityVisitedCellCount,
                totalWorkUnits,
                totalSearchBudgetExceeded,
                candidates,
                candidateSet.HasPreferredPatternComposition,
                candidateSet.PreferredStraightPatternCount,
                candidateSet.PreferredCornerPatternCount,
                candidateSet.PreferredCrossPatternCount);
        }

        private static string BuildFailureMessage(
            StageRouteGenerationFailureReason failureReason,
            StageRouteGenerationDiagnostics diagnostics,
            int totalSearchWorkLimit)
        {
            string prefix = failureReason switch
            {
                StageRouteGenerationFailureReason.TotalSearchBudgetExceeded =>
                    "The total route search work budget was exhausted.",
                StageRouteGenerationFailureReason.SearchBudgetExceeded =>
                    "One or more candidates reached a bounded connector search limit.",
                _ =>
                    "No route was found within the configured connector policy.",
            };

            return $"{prefix} Candidates attempted/generated: " +
                   $"{diagnostics.CandidatesAttempted}/" +
                   $"{diagnostics.GeneratedCandidateCount}; " +
                   $"prevalidation rejected: " +
                   $"{diagnostics.PrevalidationRejectedCandidateCount}; " +
                   $"path not found: " +
                   $"{diagnostics.PathNotFoundCandidateCount}; " +
                   $"candidate limits: " +
                   $"{diagnostics.PerCandidateBudgetExceededCount}; " +
                   $"unattempted: {diagnostics.CandidatesNotAttempted}; " +
                   $"work: {diagnostics.TotalWorkUnits}/" +
                   $"{totalSearchWorkLimit}.";
        }

        private GeneratedStageRoute BuildRoute(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout,
            OrderedPatternPathSearchResult searchResult)
        {
            MutableEnemyRouteGraphBuilder graphBuilder = new();
            for (int nodeId = 0; nodeId < searchResult.Path.Count; nodeId++)
            {
                graphBuilder.AddNode(
                    new RouteNode(nodeId, searchResult.Path[nodeId]));
                if (nodeId > 0)
                {
                    graphBuilder.AddEdge(nodeId - 1, nodeId);
                }
            }

            graphBuilder.AddSpawn(settings.SpawnId, startNodeId: 0);
            graphBuilder.SetGoal(searchResult.Path.Count - 1);

            foreach (StageRoutePatternPlacement placement in layout.Placements)
            {
                if (placement.Kind != StageRoutePatternKind.DisconnectedCross)
                {
                    continue;
                }

                StageRoutePatternPassage horizontal = placement.Passages[0];
                StageRoutePatternPassage vertical = placement.Passages[1];
                int horizontalAnchorOffset = IndexOfCell(
                    horizontal.Cells,
                    placement.AnchorCell);
                int verticalAnchorOffset = IndexOfCell(
                    vertical.Cells,
                    placement.AnchorCell);
                int horizontalNodeId = checked(
                    searchResult.PassageStartNodeIds[horizontal.PassageId] +
                    horizontalAnchorOffset);
                int verticalNodeId = checked(
                    searchResult.PassageStartNodeIds[vertical.PassageId] +
                    verticalAnchorOffset);
                graphBuilder.AddDisconnectedCrossing(
                    placement.AnchorCell,
                    horizontalNodeId,
                    verticalNodeId);
            }

            EnemyRouteGraph graph = graphBuilder.Freeze();
            SpawnDefinition spawn = new(settings.SpawnId, settings.SpawnCell, 0);
            Vector2Int[] roadCells = searchResult.Path
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();

            if (!graph.TryBuildPrimaryPath(settings.SpawnId, out var graphPath) ||
                !PathsEqual(searchResult.Path, graphPath))
            {
                throw new InvalidOperationException(
                    "The frozen route graph does not reproduce the generated ordered path.");
            }

            return new GeneratedStageRoute(
                settings,
                GeneratorVersion,
                patternStrategy.StrategyId,
                patternStrategy.Version,
                layout,
                searchResult.Path,
                roadCells,
                spawn,
                graph);
        }

        private static void ValidatePatternLayout(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout)
        {
            if (layout == null)
            {
                throw new InvalidOperationException(
                    "A pattern strategy returned a null layout.");
            }

            if (layout.Placements.Count != settings.PatternCount)
            {
                throw new InvalidOperationException(
                    $"Pattern layout '{layout.LayoutId}' contains " +
                    $"{layout.Placements.Count} physical placements; " +
                    $"{settings.PatternCount} were requested.");
            }

            Dictionary<Vector2Int, int> roadOccurrenceCounts = new();
            HashSet<CellPair> declaredPatternEdges = new();
            HashSet<Vector2Int> uniquePatternRoadCells = new();
            foreach (StageRoutePatternPlacement placement in layout.Placements)
            {
                if (!IsKindAllowed(settings.AllowedPatternKinds, placement.Kind))
                {
                    throw new InvalidOperationException(
                        $"Layout '{layout.LayoutId}' uses disabled pattern kind " +
                        $"{placement.Kind}.");
                }

                if (!GetAnchorRegion(settings, placement.Slot)
                        .Contains(placement.AnchorCell))
                {
                    throw new InvalidOperationException(
                        $"Pattern '{placement.Id}' anchor {placement.AnchorCell} is " +
                        $"outside its {placement.Slot} anchor region.");
                }

                ValidatePlacementShape(placement);
                foreach (StageRoutePatternPassage passage in placement.Passages)
                {
                    for (int index = 0; index < passage.Cells.Count; index++)
                    {
                        Vector2Int cell = passage.Cells[index];
                        if (!settings.Bounds.Contains(cell) ||
                            cell == settings.SpawnCell ||
                            cell == settings.RouteGoalCell ||
                            cell == settings.HeadquartersCell)
                        {
                            throw new InvalidOperationException(
                                $"Pattern passage '{passage.PassageId}' uses reserved " +
                                $"or out-of-bounds cell {cell}.");
                        }

                        roadOccurrenceCounts[cell] =
                            roadOccurrenceCounts.TryGetValue(cell, out int count)
                                ? count + 1
                                : 1;
                        uniquePatternRoadCells.Add(cell);
                        if (index > 0)
                        {
                            declaredPatternEdges.Add(
                                new CellPair(passage.Cells[index - 1], cell));
                        }
                    }
                }
            }

            ValidatePatternRoadOccurrences(layout, roadOccurrenceCounts);
            ValidatePatternAdjacency(
                uniquePatternRoadCells,
                declaredPatternEdges,
                layout.LayoutId);
            ValidatePassageOrder(layout);
            ValidateEndpointAdjacency(settings, layout, uniquePatternRoadCells);
        }

        private static void ValidatePlacementShape(
            StageRoutePatternPlacement placement)
        {
            foreach (StageRoutePatternPassage passage in placement.Passages)
            {
                if (passage.Cells.Count != 3 ||
                    passage.Cells[1] != placement.AnchorCell)
                {
                    throw new InvalidOperationException(
                        $"Pattern passage '{passage.PassageId}' must contain exactly " +
                        "three cells with the anchor in the middle.");
                }
            }

            switch (placement.Kind)
            {
                case StageRoutePatternKind.Straight:
                    if (!AreOppositeCardinalNeighbors(
                            placement.Passages[0].Cells[0],
                            placement.AnchorCell,
                            placement.Passages[0].Cells[2]))
                    {
                        throw new InvalidOperationException(
                            $"Straight pattern '{placement.Id}' is not straight.");
                    }

                    break;

                case StageRoutePatternKind.Corner:
                    if (!ArePerpendicularCardinalNeighbors(
                            placement.Passages[0].Cells[0],
                            placement.AnchorCell,
                            placement.Passages[0].Cells[2]))
                    {
                        throw new InvalidOperationException(
                            $"Corner pattern '{placement.Id}' is not a compact corner.");
                    }

                    break;

                case StageRoutePatternKind.DisconnectedCross:
                    StageRoutePatternPassage horizontal = placement.Passages[0];
                    StageRoutePatternPassage vertical = placement.Passages[1];
                    if (horizontal.Axis != StageRoutePassageAxis.Horizontal ||
                        vertical.Axis != StageRoutePassageAxis.Vertical ||
                        !AreOppositeCardinalNeighbors(
                            horizontal.Cells[0],
                            placement.AnchorCell,
                            horizontal.Cells[2]) ||
                        !AreOppositeCardinalNeighbors(
                            vertical.Cells[0],
                            placement.AnchorCell,
                            vertical.Cells[2]))
                    {
                        throw new InvalidOperationException(
                            $"Disconnected cross '{placement.Id}' has invalid passages.");
                    }

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Pattern '{placement.Id}' has an unknown kind.");
            }
        }

        private static void ValidatePatternRoadOccurrences(
            StageRoutePatternLayout layout,
            IReadOnlyDictionary<Vector2Int, int> occurrenceCounts)
        {
            HashSet<Vector2Int> crossingAnchors = new(
                layout.Placements
                    .Where(placement =>
                        placement.Kind == StageRoutePatternKind.DisconnectedCross)
                    .Select(placement => placement.AnchorCell));

            foreach (KeyValuePair<Vector2Int, int> occurrence in occurrenceCounts)
            {
                int expectedCount = crossingAnchors.Contains(occurrence.Key) ? 2 : 1;
                if (occurrence.Value != expectedCount)
                {
                    throw new InvalidOperationException(
                        $"Pattern Road cell {occurrence.Key} occurs {occurrence.Value} " +
                        $"times; expected {expectedCount}.");
                }
            }
        }

        private static void ValidatePatternAdjacency(
            IEnumerable<Vector2Int> roadCells,
            ISet<CellPair> declaredEdges,
            string layoutId)
        {
            HashSet<Vector2Int> roadCellSet = new(roadCells);
            foreach (Vector2Int cell in roadCellSet)
            {
                Vector2Int right = cell + Vector2Int.right;
                Vector2Int up = cell + Vector2Int.up;
                if ((roadCellSet.Contains(right) &&
                     !declaredEdges.Contains(new CellPair(cell, right))) ||
                    (roadCellSet.Contains(up) &&
                     !declaredEdges.Contains(new CellPair(cell, up))))
                {
                    throw new InvalidOperationException(
                        $"Pattern layout '{layoutId}' contains accidental Road adjacency.");
                }
            }
        }

        private static void ValidatePassageOrder(StageRoutePatternLayout layout)
        {
            for (int index = 1; index < layout.OrderedPassages.Count; index++)
            {
                StageRoutePatternSlot previous =
                    layout.OrderedPassages[index - 1].Slot;
                StageRoutePatternSlot current = layout.OrderedPassages[index].Slot;
                if (AreOppositeSlots(previous, current))
                {
                    throw new InvalidOperationException(
                        $"Pattern layout '{layout.LayoutId}' directly connects " +
                        $"opposite slots {previous} and {current}.");
                }
            }
        }

        private static void ValidateEndpointAdjacency(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout,
            IEnumerable<Vector2Int> roadCells)
        {
            Vector2Int firstEntry = layout.OrderedPassages[0].EntryCell;
            Vector2Int lastExit =
                layout.OrderedPassages[layout.OrderedPassages.Count - 1].ExitCell;
            foreach (Vector2Int roadCell in roadCells)
            {
                if (GetManhattanDistance(roadCell, settings.SpawnCell) == 1 &&
                    roadCell != firstEntry)
                {
                    throw new InvalidOperationException(
                        $"Pattern Road cell {roadCell} touches Spawn without being " +
                        "the first passage entry.");
                }

                if (GetManhattanDistance(roadCell, settings.RouteGoalCell) == 1 &&
                    roadCell != lastExit)
                {
                    throw new InvalidOperationException(
                        $"Pattern Road cell {roadCell} touches RouteGoal without being " +
                        "the final passage exit.");
                }
            }
        }

        private static RectInt GetAnchorRegion(
            StageRouteGenerationSettings settings,
            StageRoutePatternSlot slot)
        {
            RectInt bounds = settings.Bounds;
            int leftWidth = bounds.width / 2;
            int bottomHeight = bounds.height / 2;
            int xSplit = bounds.xMin + leftWidth;
            int ySplit = bounds.yMin + bottomHeight;

            switch (slot)
            {
                case StageRoutePatternSlot.Quadrant1:
                    return new RectInt(
                        bounds.xMin,
                        bounds.yMin,
                        leftWidth,
                        bottomHeight);
                case StageRoutePatternSlot.Quadrant2:
                    return new RectInt(
                        xSplit,
                        bounds.yMin,
                        bounds.width - leftWidth,
                        bottomHeight);
                case StageRoutePatternSlot.Quadrant3:
                    return new RectInt(
                        xSplit,
                        ySplit,
                        bounds.width - leftWidth,
                        bounds.height - bottomHeight);
                case StageRoutePatternSlot.Quadrant4:
                    return new RectInt(
                        bounds.xMin,
                        ySplit,
                        leftWidth,
                        bounds.height - bottomHeight);
                case StageRoutePatternSlot.Center:
                    long radius = settings.CenterBandRadius;
                    long lowerX = bounds.xMin + (bounds.width - 1L) / 2L;
                    long upperX = bounds.xMin + bounds.width / 2L;
                    long lowerY = bounds.yMin + (bounds.height - 1L) / 2L;
                    long upperY = bounds.yMin + bounds.height / 2L;
                    int xMin = (int)Math.Max(bounds.xMin, lowerX - radius);
                    int xMaxInclusive = (int)Math.Min(
                        bounds.xMax - 1L,
                        upperX + radius);
                    int yMin = (int)Math.Max(bounds.yMin, lowerY - radius);
                    int yMaxInclusive = (int)Math.Min(
                        bounds.yMax - 1L,
                        upperY + radius);
                    return new RectInt(
                        xMin,
                        yMin,
                        xMaxInclusive - xMin + 1,
                        yMaxInclusive - yMin + 1);
                default:
                    throw new InvalidOperationException(
                        $"Unknown pattern slot {slot}.");
            }
        }

        private static bool IsKindAllowed(
            StageRoutePatternKinds allowedKinds,
            StageRoutePatternKind kind)
        {
            StageRoutePatternKinds flag = kind switch
            {
                StageRoutePatternKind.Straight => StageRoutePatternKinds.Straight,
                StageRoutePatternKind.Corner => StageRoutePatternKinds.Corner,
                StageRoutePatternKind.DisconnectedCross =>
                    StageRoutePatternKinds.DisconnectedCross,
                _ => StageRoutePatternKinds.None,
            };
            return flag != StageRoutePatternKinds.None &&
                   (allowedKinds & flag) != 0;
        }

        private static bool AreOppositeSlots(
            StageRoutePatternSlot first,
            StageRoutePatternSlot second)
        {
            return (first == StageRoutePatternSlot.Quadrant1 &&
                    second == StageRoutePatternSlot.Quadrant3) ||
                   (first == StageRoutePatternSlot.Quadrant3 &&
                    second == StageRoutePatternSlot.Quadrant1) ||
                   (first == StageRoutePatternSlot.Quadrant2 &&
                    second == StageRoutePatternSlot.Quadrant4) ||
                   (first == StageRoutePatternSlot.Quadrant4 &&
                    second == StageRoutePatternSlot.Quadrant2);
        }

        private static bool AreOppositeCardinalNeighbors(
            Vector2Int first,
            Vector2Int center,
            Vector2Int second)
        {
            Vector2Int firstOffset = first - center;
            Vector2Int secondOffset = second - center;
            return GetManhattanDistance(first, center) == 1 &&
                   GetManhattanDistance(second, center) == 1 &&
                   firstOffset == -secondOffset;
        }

        private static bool ArePerpendicularCardinalNeighbors(
            Vector2Int first,
            Vector2Int center,
            Vector2Int second)
        {
            Vector2Int firstOffset = first - center;
            Vector2Int secondOffset = second - center;
            return GetManhattanDistance(first, center) == 1 &&
                   GetManhattanDistance(second, center) == 1 &&
                   firstOffset.x * secondOffset.x +
                   firstOffset.y * secondOffset.y == 0;
        }

        private static int IndexOfCell(
            IReadOnlyList<Vector2Int> cells,
            Vector2Int expected)
        {
            for (int index = 0; index < cells.Count; index++)
            {
                if (cells[index] == expected)
                {
                    return index;
                }
            }

            throw new InvalidOperationException(
                $"Pattern passage does not contain expected anchor {expected}.");
        }

        private static bool PathsEqual(
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

        private static int GetManhattanDistance(
            Vector2Int first,
            Vector2Int second)
        {
            return Math.Abs(first.x - second.x) +
                   Math.Abs(first.y - second.y);
        }

        private static void EnsureStrategyIdentity(
            IStageRoutePatternStrategy strategy)
        {
            if (string.IsNullOrWhiteSpace(strategy.StrategyId) ||
                string.IsNullOrWhiteSpace(strategy.Version))
            {
                throw new ArgumentException(
                    "A pattern strategy requires stable non-empty ID and Version values.",
                    nameof(strategy));
            }
        }

        private sealed class CandidateSearchAccumulator
        {
            internal int CandidateIndex { get; }
            internal StageRoutePatternCandidateRecord Record { get; }
            internal bool WasAttempted { get; private set; }
            internal OrderedPatternPathSearchOutcome LastOutcome { get; private set; }
            internal StageRouteSearchLimitKind LastLimitKind { get; private set; }
            internal StageRouteCandidateRejectionReason RejectionReason
            {
                get;
                private set;
            }
            internal int WorkUnits { get; private set; }
            internal int AStarNodeExpansionCount { get; private set; }
            internal int ConnectorAlternativeCount { get; private set; }
            internal int BacktrackCount { get; private set; }
            internal int ReachabilityCheckCount { get; private set; }
            internal int ReachabilityVisitedCellCount { get; private set; }

            internal CandidateSearchAccumulator(
                int candidateIndex,
                StageRoutePatternCandidateRecord record)
            {
                CandidateIndex = candidateIndex;
                Record = record ?? throw new ArgumentNullException(nameof(record));
            }

            internal void Accumulate(OrderedPatternPathSearchResult result)
            {
                if (result == null)
                {
                    throw new ArgumentNullException(nameof(result));
                }

                if (WasAttempted &&
                    LastOutcome !=
                    OrderedPatternPathSearchOutcome.SearchLimitExceeded)
                {
                    throw new InvalidOperationException(
                        $"Completed candidate {CandidateIndex} cannot be searched again.");
                }

                WasAttempted = true;
                LastOutcome = result.Outcome;
                LastLimitKind = result.LimitKind;
                RejectionReason = result.PrevalidationRejectionReason;
                WorkUnits = checked(WorkUnits + result.TotalWorkUnits);
                AStarNodeExpansionCount = checked(
                    AStarNodeExpansionCount + result.AStarStatesExpanded);
                ConnectorAlternativeCount = checked(
                    ConnectorAlternativeCount +
                    result.ConnectorAlternativesTried);
                BacktrackCount = checked(
                    BacktrackCount + result.BacktrackCount);
                ReachabilityCheckCount = checked(
                    ReachabilityCheckCount + result.ReachabilityCheckCount);
                ReachabilityVisitedCellCount = checked(
                    ReachabilityVisitedCellCount +
                    result.ReachabilityVisitedCellCount);
            }

            internal bool CanRetry(int maxCandidateWork)
            {
                if (!WasAttempted ||
                    WorkUnits >= maxCandidateWork ||
                    LastOutcome !=
                    OrderedPatternPathSearchOutcome.SearchLimitExceeded)
                {
                    return false;
                }

                return LastLimitKind ==
                       StageRouteSearchLimitKind.PerCandidateWork ||
                       LastLimitKind == StageRouteSearchLimitKind.OpenSetCapacity;
            }

            internal StageRouteCandidateDiagnostic FreezeDiagnostic(
                bool markAsTotalWorkLimit)
            {
                if (!WasAttempted)
                {
                    return CreateDiagnostic(
                        StageRouteCandidateOutcome.NotAttempted,
                        StageRouteCandidateRejectionReason.None,
                        StageRouteSearchLimitKind.None);
                }

                if (LastOutcome == OrderedPatternPathSearchOutcome.Succeeded)
                {
                    return CreateDiagnostic(
                        StageRouteCandidateOutcome.Succeeded,
                        StageRouteCandidateRejectionReason.None,
                        StageRouteSearchLimitKind.None);
                }

                if (RejectionReason !=
                    StageRouteCandidateRejectionReason.None)
                {
                    return CreateDiagnostic(
                        StageRouteCandidateOutcome.RejectedByPrevalidation,
                        RejectionReason,
                        StageRouteSearchLimitKind.None);
                }

                if (LastOutcome == OrderedPatternPathSearchOutcome.PathNotFound)
                {
                    return CreateDiagnostic(
                        StageRouteCandidateOutcome.PathNotFound,
                        StageRouteCandidateRejectionReason.None,
                        StageRouteSearchLimitKind.None);
                }

                StageRouteSearchLimitKind limitKind = LastLimitKind;
                if (markAsTotalWorkLimit)
                {
                    if (LastOutcome !=
                        OrderedPatternPathSearchOutcome.SearchLimitExceeded)
                    {
                        throw new InvalidOperationException(
                            $"Candidate {CandidateIndex} was marked as total-work " +
                            "limited without a search-limit outcome.");
                    }

                    limitKind = StageRouteSearchLimitKind.TotalWork;
                }

                return CreateDiagnostic(
                    StageRouteCandidateOutcome.SearchBudgetExceeded,
                    StageRouteCandidateRejectionReason.None,
                    limitKind);
            }

            private StageRouteCandidateDiagnostic CreateDiagnostic(
                StageRouteCandidateOutcome outcome,
                StageRouteCandidateRejectionReason rejectionReason,
                StageRouteSearchLimitKind limitKind)
            {
                return new StageRouteCandidateDiagnostic(
                    CandidateIndex,
                    Record.Layout.LayoutId,
                    Record.PhysicalLayoutIndex,
                    Record.VariantIndex,
                    Record.PhysicalLayoutDrawIndex,
                    Record.PassageOrderDrawIndex,
                    outcome,
                    rejectionReason,
                    limitKind,
                    WorkUnits,
                    AStarNodeExpansionCount,
                    ConnectorAlternativeCount,
                    BacktrackCount,
                    ReachabilityCheckCount,
                    ReachabilityVisitedCellCount);
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
                return HashCode.Combine(first, second);
            }

            private static int CompareCells(Vector2Int left, Vector2Int right)
            {
                int yComparison = left.y.CompareTo(right.y);
                return yComparison != 0
                    ? yComparison
                    : left.x.CompareTo(right.x);
            }
        }
    }
}
