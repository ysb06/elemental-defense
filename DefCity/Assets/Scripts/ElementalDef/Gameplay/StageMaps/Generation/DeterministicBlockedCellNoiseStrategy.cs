using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class DeterministicBlockedCellNoiseStrategy :
        IStageBlockedCellPlacementStrategy
    {
        private const int RandomStream = unchecked((int)0xB5297A4D);

        public string StrategyId => "independent-blocked-cell-noise";
        public string Version => "2";

        public IReadOnlyList<Vector2Int> SelectBlockedCells(
            StageBlockedCellPlacementContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.TargetBlockedCellCount == 0)
            {
                return Array.Empty<Vector2Int>();
            }

            List<Vector2Int> bestSelection = new();
            for (int attempt = 0; attempt < context.MaxAttempts; attempt++)
            {
                List<Vector2Int> candidates =
                    new(context.CandidateCells);
                StageRouteDeterministicRandom random = new(
                    context.Seed,
                    unchecked(RandomStream + attempt));
                random.Shuffle(candidates);

                Dictionary<ElementType, int> deployableCountsByElement =
                    CountElements(context);
                Dictionary<Vector2Int, int> deployableNeighborsByRoadCell =
                    CountCandidateNeighborsByRoadCell(context);
                Dictionary<Vector2Int, int> requiredNeighborsByRoadCell =
                    GetRequiredNeighborsByRoadCell(
                        context,
                        deployableNeighborsByRoadCell);

                List<Vector2Int> selected = new(
                    context.TargetBlockedCellCount);
                HashSet<Vector2Int> selectedSet = new();
                int remainingDeployableCount = context.CandidateCells.Count;
                foreach (Vector2Int candidate in candidates)
                {
                    if (selected.Count == context.TargetBlockedCellCount)
                    {
                        break;
                    }

                    ElementType element = context.ElementsByCell[candidate];
                    if (IsInsideEndpointProtectionArea(candidate, context) ||
                        WouldExceedMaximumClusterSize(
                            candidate,
                            selectedSet,
                            context.MaximumBlockedClusterSize) ||
                        remainingDeployableCount - 1 <
                            context.MinimumDeployableCellCount ||
                        deployableCountsByElement[element] - 1 <
                            context.MinimumDeployableCellCountPerElement ||
                        WouldViolateRoadNeighborMinimum(
                            candidate,
                            deployableNeighborsByRoadCell,
                            requiredNeighborsByRoadCell))
                    {
                        continue;
                    }

                    selected.Add(candidate);
                    selectedSet.Add(candidate);
                    remainingDeployableCount--;
                    deployableCountsByElement[element]--;
                    DecrementAdjacentRoadNeighborCounts(
                        candidate,
                        deployableNeighborsByRoadCell);
                }

                if (selected.Count > bestSelection.Count)
                {
                    bestSelection = selected;
                }

                if (selected.Count == context.TargetBlockedCellCount)
                {
                    SortRowMajor(selected);
                    return Array.AsReadOnly(selected.ToArray());
                }
            }

            SortRowMajor(bestSelection);
            return Array.AsReadOnly(bestSelection.ToArray());
        }

        private static bool IsInsideEndpointProtectionArea(
            Vector2Int candidate,
            StageBlockedCellPlacementContext context)
        {
            foreach (Vector2Int endpoint in context.EndpointCells)
            {
                long distance = Math.Abs((long)candidate.x - endpoint.x) +
                                Math.Abs((long)candidate.y - endpoint.y);
                if (distance <= context.EndpointProtectionRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool WouldExceedMaximumClusterSize(
            Vector2Int candidate,
            ISet<Vector2Int> selectedCells,
            int maximumClusterSize)
        {
            HashSet<Vector2Int> visited = new() { candidate };
            Queue<Vector2Int> pending = new();
            pending.Enqueue(candidate);

            while (pending.Count > 0)
            {
                Vector2Int cell = pending.Dequeue();
                if (visited.Count > maximumClusterSize)
                {
                    return true;
                }

                foreach (Vector2Int offset in CardinalOffsets)
                {
                    Vector2Int neighbor = cell + offset;
                    if (selectedCells.Contains(neighbor) &&
                        visited.Add(neighbor))
                    {
                        pending.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        private static Dictionary<ElementType, int> CountElements(
            StageBlockedCellPlacementContext context)
        {
            Dictionary<ElementType, int> counts = new(
                StageGroundElementTypes.Count);
            foreach (ElementType element in StageGroundElementTypes.Ordered)
            {
                counts.Add(element, 0);
            }

            foreach (Vector2Int cell in context.CandidateCells)
            {
                counts[context.ElementsByCell[cell]]++;
            }

            return counts;
        }

        private static Dictionary<Vector2Int, int>
            CountCandidateNeighborsByRoadCell(
                StageBlockedCellPlacementContext context)
        {
            HashSet<Vector2Int> candidateSet =
                new(context.CandidateCells);
            Dictionary<Vector2Int, int> counts =
                new(context.RoadCells.Count);
            foreach (Vector2Int roadCell in context.RoadCells)
            {
                int count = 0;
                foreach (Vector2Int offset in CardinalOffsets)
                {
                    if (candidateSet.Contains(roadCell + offset))
                    {
                        count++;
                    }
                }

                counts.Add(roadCell, count);
            }

            return counts;
        }

        private static Dictionary<Vector2Int, int>
            GetRequiredNeighborsByRoadCell(
                StageBlockedCellPlacementContext context,
                IReadOnlyDictionary<Vector2Int, int> initialCounts)
        {
            Dictionary<Vector2Int, int> required =
                new(initialCounts.Count);
            foreach (KeyValuePair<Vector2Int, int> entry in initialCounts)
            {
                required.Add(
                    entry.Key,
                    Math.Min(
                        context.MinimumDeployableNeighborsPerRoadCell,
                        entry.Value));
            }

            return required;
        }

        private static bool WouldViolateRoadNeighborMinimum(
            Vector2Int candidate,
            IReadOnlyDictionary<Vector2Int, int> currentCounts,
            IReadOnlyDictionary<Vector2Int, int> requiredCounts)
        {
            foreach (Vector2Int offset in CardinalOffsets)
            {
                Vector2Int adjacentCell = candidate + offset;
                if (currentCounts.TryGetValue(adjacentCell, out int currentCount) &&
                    currentCount - 1 < requiredCounts[adjacentCell])
                {
                    return true;
                }
            }

            return false;
        }

        private static void DecrementAdjacentRoadNeighborCounts(
            Vector2Int candidate,
            IDictionary<Vector2Int, int> currentCounts)
        {
            foreach (Vector2Int offset in CardinalOffsets)
            {
                Vector2Int adjacentCell = candidate + offset;
                if (currentCounts.TryGetValue(adjacentCell, out int currentCount))
                {
                    currentCounts[adjacentCell] = currentCount - 1;
                }
            }
        }

        private static void SortRowMajor(List<Vector2Int> cells)
        {
            cells.Sort((left, right) =>
            {
                int yComparison = left.y.CompareTo(right.y);
                return yComparison != 0
                    ? yComparison
                    : left.x.CompareTo(right.x);
            });
        }

        private static readonly Vector2Int[] CardinalOffsets =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };
    }
}
