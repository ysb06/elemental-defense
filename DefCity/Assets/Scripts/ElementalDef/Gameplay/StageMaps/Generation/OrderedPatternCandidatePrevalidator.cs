using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    internal readonly struct OrderedPatternCandidatePrevalidationResult
    {
        internal bool Succeeded =>
            RejectionReason == StageRouteCandidateRejectionReason.None;
        internal StageRouteCandidateRejectionReason RejectionReason { get; }

        internal OrderedPatternCandidatePrevalidationResult(
            StageRouteCandidateRejectionReason rejectionReason)
        {
            RejectionReason = rejectionReason;
        }
    }

    /// <summary>
    /// Rejects only candidates that cannot possibly complete in a relaxed
    /// residual grid. Exact Road-clearance constraints remain the solver's job.
    /// </summary>
    internal sealed class OrderedPatternCandidatePrevalidator
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
        };

        private readonly RectInt bounds;
        private readonly Vector2Int spawn;
        private readonly Vector2Int goal;
        private readonly RectInt headquartersFootprint;
        private readonly IReadOnlyList<StageRoutePatternPassage> passages;
        private readonly HashSet<Vector2Int> reservedPatternCells = new();
        private readonly HashSet<CellPair> declaredPatternEdges = new();
        private readonly Dictionary<Vector2Int, int> patternCellOccurrences = new();
        private readonly HashSet<Vector2Int> crossingAnchors = new();
        private readonly int[] visited;
        private readonly Vector2Int[] queue;

        internal OrderedPatternCandidatePrevalidator(
            StageRouteGenerationSettings settings,
            StageRoutePatternLayout layout)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            bounds = settings.Bounds;
            spawn = settings.SpawnCell;
            goal = settings.RouteGoalCell;
            headquartersFootprint = settings.HeadquartersFootprint;
            passages = layout.OrderedPassages;

            foreach (StageRoutePatternPlacement placement in layout.Placements)
            {
                if (placement.Kind == StageRoutePatternKind.DisconnectedCross)
                {
                    crossingAnchors.Add(placement.AnchorCell);
                }

                foreach (StageRoutePatternPassage passage in placement.Passages)
                {
                    for (int index = 0; index < passage.Cells.Count; index++)
                    {
                        Vector2Int cell = passage.Cells[index];
                        reservedPatternCells.Add(cell);
                        patternCellOccurrences[cell] =
                            patternCellOccurrences.TryGetValue(
                                cell,
                                out int occurrenceCount)
                                ? occurrenceCount + 1
                                : 1;

                        if (index > 0)
                        {
                            declaredPatternEdges.Add(
                                new CellPair(passage.Cells[index - 1], cell));
                        }
                    }
                }
            }

            int mapCellCount = checked(bounds.width * bounds.height);
            visited = new int[mapCellCount];
            queue = new Vector2Int[mapCellCount];
        }

        internal OrderedPatternCandidatePrevalidationResult EvaluateInitial(
            IReadOnlyDictionary<Vector2Int, int> usedCells,
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            if (usedCells == null)
            {
                throw new ArgumentNullException(nameof(usedCells));
            }

            if (metrics == null)
            {
                throw new ArgumentNullException(nameof(metrics));
            }

            if (!HasValidFixedPassages(metrics))
            {
                return Failure(
                    StageRouteCandidateRejectionReason.FixedPassageConflict);
            }

            return EvaluateRemaining(spawn, 0, usedCells, metrics);
        }

        internal OrderedPatternCandidatePrevalidationResult EvaluateRemaining(
            Vector2Int current,
            int nextPassageIndex,
            IReadOnlyDictionary<Vector2Int, int> usedCells,
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            if (nextPassageIndex < 0 || nextPassageIndex > passages.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(nextPassageIndex));
            }

            if (usedCells == null)
            {
                throw new ArgumentNullException(nameof(usedCells));
            }

            if (metrics == null)
            {
                throw new ArgumentNullException(nameof(metrics));
            }

            if (!BuildResidualComponents(
                    usedCells,
                    metrics,
                    out int availableResidualCellCount))
            {
                return Success();
            }

            if (!HasEnoughResidualCells(
                    current,
                    nextPassageIndex,
                    availableResidualCellCount))
            {
                if (metrics.IsLimited)
                {
                    return Success();
                }

                return Failure(
                    StageRouteCandidateRejectionReason.InsufficientResidualCells);
            }

            Vector2Int connectorSource = current;
            for (int passageIndex = nextPassageIndex;
                 passageIndex < passages.Count;
                 passageIndex++)
            {
                StageRoutePatternPassage passage = passages[passageIndex];
                if (!HasEntryPort(
                        connectorSource,
                        passage,
                        metrics))
                {
                    if (metrics.IsLimited)
                    {
                        return Success();
                    }

                    return Failure(
                        StageRouteCandidateRejectionReason.EntryPortUnavailable);
                }

                if (!CanReachThroughResidualComponents(
                        connectorSource,
                        passage.EntryCell,
                        metrics))
                {
                    if (metrics.IsLimited)
                    {
                        return Success();
                    }

                    return Failure(
                        StageRouteCandidateRejectionReason
                            .ResidualConnectivityUnavailable);
                }

                Vector2Int connectorTarget = passageIndex + 1 < passages.Count
                    ? passages[passageIndex + 1].EntryCell
                    : goal;
                if (!HasExitPort(
                        passage,
                        connectorTarget,
                        metrics))
                {
                    if (metrics.IsLimited)
                    {
                        return Success();
                    }

                    return Failure(
                        StageRouteCandidateRejectionReason.ExitPortUnavailable);
                }

                connectorSource = passage.ExitCell;
            }

            if (!CanReachThroughResidualComponents(
                    connectorSource,
                    goal,
                    metrics))
            {
                if (metrics.IsLimited)
                {
                    return Success();
                }

                return Failure(
                    StageRouteCandidateRejectionReason
                        .ResidualConnectivityUnavailable);
            }

            return Success();
        }

        private bool HasValidFixedPassages(
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            foreach (KeyValuePair<Vector2Int, int> occurrence in
                     patternCellOccurrences)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return true;
                }

                Vector2Int cell = occurrence.Key;
                int allowedOccurrenceCount = crossingAnchors.Contains(cell) ? 2 : 1;
                if (!bounds.Contains(cell) ||
                    cell == spawn ||
                    cell == goal ||
                    headquartersFootprint.Contains(cell) ||
                    occurrence.Value != allowedOccurrenceCount)
                {
                    return false;
                }

                foreach (Vector2Int direction in CardinalDirections)
                {
                    Vector2Int neighbor = cell + direction;
                    if (reservedPatternCells.Contains(neighbor) &&
                        !declaredPatternEdges.Contains(
                            new CellPair(cell, neighbor)))
                    {
                        return false;
                    }
                }
            }

            StageRoutePatternPassage firstPassage = passages[0];
            StageRoutePatternPassage lastPassage = passages[passages.Count - 1];
            foreach (Vector2Int patternCell in reservedPatternCells)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return true;
                }

                if (GetManhattanDistance(spawn, patternCell) == 1 &&
                    patternCell != firstPassage.EntryCell)
                {
                    return false;
                }

                if (GetManhattanDistance(goal, patternCell) == 1 &&
                    patternCell != lastPassage.ExitCell)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasEnoughResidualCells(
            Vector2Int current,
            int nextPassageIndex,
            int availableResidualCellCount)
        {
            long requiredIntermediateCellCount = 0;
            Vector2Int connectorSource = current;
            for (int passageIndex = nextPassageIndex;
                 passageIndex < passages.Count;
                 passageIndex++)
            {
                StageRoutePatternPassage passage = passages[passageIndex];
                requiredIntermediateCellCount += Math.Max(
                    0,
                    GetManhattanDistance(connectorSource, passage.EntryCell) - 1);
                connectorSource = passage.ExitCell;
            }

            requiredIntermediateCellCount += Math.Max(
                0,
                GetManhattanDistance(connectorSource, goal) - 1);

            return requiredIntermediateCellCount <= availableResidualCellCount;
        }

        private bool BuildResidualComponents(
            IReadOnlyDictionary<Vector2Int, int> usedCells,
            OrderedPatternPathSearchMetricsBuilder metrics,
            out int availableResidualCellCount)
        {
            Array.Clear(visited, 0, visited.Length);
            metrics.BeginReachabilityCheck();
            availableResidualCellCount = 0;
            int componentId = 0;

            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    Vector2Int seedCell = new(x, y);
                    int seedIndex = GetCellIndex(seedCell);
                    if (visited[seedIndex] != 0)
                    {
                        continue;
                    }

                    if (!metrics.TryRecordReachabilityCell())
                    {
                        return false;
                    }

                    if (!IsResidualFreeCell(seedCell, usedCells))
                    {
                        visited[seedIndex] = -1;
                        continue;
                    }

                    componentId++;
                    int queueStart = 0;
                    int queueEnd = 0;
                    queue[queueEnd++] = seedCell;
                    visited[seedIndex] = componentId;
                    availableResidualCellCount++;

                    while (queueStart < queueEnd)
                    {
                        Vector2Int current = queue[queueStart++];
                        foreach (Vector2Int direction in CardinalDirections)
                        {
                            Vector2Int next = current + direction;
                            if (!bounds.Contains(next))
                            {
                                continue;
                            }

                            int nextIndex = GetCellIndex(next);
                            if (visited[nextIndex] != 0)
                            {
                                continue;
                            }

                            if (!metrics.TryRecordReachabilityCell())
                            {
                                return false;
                            }

                            if (!IsResidualFreeCell(next, usedCells))
                            {
                                visited[nextIndex] = -1;
                                continue;
                            }

                            visited[nextIndex] = componentId;
                            queue[queueEnd++] = next;
                            availableResidualCellCount++;
                        }
                    }
                }
            }

            return true;
        }

        private bool HasEntryPort(
            Vector2Int connectorSource,
            StageRoutePatternPassage passage,
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            Vector2Int entry = passage.EntryCell;
            Vector2Int? nextFixedCell = passage.Cells.Count > 1
                ? passage.Cells[1]
                : null;

            metrics.BeginReachabilityCheck();
            foreach (Vector2Int direction in CardinalDirections)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return false;
                }

                Vector2Int neighbor = entry + direction;
                if (nextFixedCell.HasValue && neighbor == nextFixedCell.Value)
                {
                    continue;
                }

                if (neighbor == connectorSource ||
                    IsResidualComponentCell(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasExitPort(
            StageRoutePatternPassage passage,
            Vector2Int connectorTarget,
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            Vector2Int exit = passage.ExitCell;
            Vector2Int? previousFixedCell = passage.Cells.Count > 1
                ? passage.Cells[passage.Cells.Count - 2]
                : null;

            metrics.BeginReachabilityCheck();
            foreach (Vector2Int direction in CardinalDirections)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return false;
                }

                Vector2Int neighbor = exit + direction;
                if (previousFixedCell.HasValue &&
                    neighbor == previousFixedCell.Value)
                {
                    continue;
                }

                if (neighbor == connectorTarget ||
                    IsResidualComponentCell(neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanReachThroughResidualComponents(
            Vector2Int source,
            Vector2Int target,
            OrderedPatternPathSearchMetricsBuilder metrics)
        {
            metrics.BeginReachabilityCheck();
            if (source == target || GetManhattanDistance(source, target) == 1)
            {
                return true;
            }

            int[] sourceComponents = new int[CardinalDirections.Length];
            int sourceComponentCount = 0;
            foreach (Vector2Int direction in CardinalDirections)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return false;
                }

                Vector2Int neighbor = source + direction;
                if (!IsResidualComponentCell(neighbor))
                {
                    continue;
                }

                int component = visited[GetCellIndex(neighbor)];
                if (!ContainsComponent(
                        sourceComponents,
                        sourceComponentCount,
                        component))
                {
                    sourceComponents[sourceComponentCount++] = component;
                }
            }

            foreach (Vector2Int direction in CardinalDirections)
            {
                if (!metrics.TryRecordReachabilityCell())
                {
                    return false;
                }

                Vector2Int neighbor = target + direction;
                if (!IsResidualComponentCell(neighbor))
                {
                    continue;
                }

                int targetComponent = visited[GetCellIndex(neighbor)];
                if (ContainsComponent(
                        sourceComponents,
                        sourceComponentCount,
                        targetComponent))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsResidualFreeCell(
            Vector2Int cell,
            IReadOnlyDictionary<Vector2Int, int> usedCells)
        {
            return !headquartersFootprint.Contains(cell) &&
                   cell != goal &&
                   !usedCells.ContainsKey(cell) &&
                   !reservedPatternCells.Contains(cell);
        }

        private bool IsResidualComponentCell(Vector2Int cell)
        {
            return bounds.Contains(cell) && visited[GetCellIndex(cell)] > 0;
        }

        private static bool ContainsComponent(
            IReadOnlyList<int> components,
            int componentCount,
            int component)
        {
            for (int index = 0; index < componentCount; index++)
            {
                if (components[index] == component)
                {
                    return true;
                }
            }

            return false;
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

        private static OrderedPatternCandidatePrevalidationResult Success()
        {
            return new OrderedPatternCandidatePrevalidationResult(
                StageRouteCandidateRejectionReason.None);
        }

        private static OrderedPatternCandidatePrevalidationResult Failure(
            StageRouteCandidateRejectionReason reason)
        {
            return new OrderedPatternCandidatePrevalidationResult(reason);
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
