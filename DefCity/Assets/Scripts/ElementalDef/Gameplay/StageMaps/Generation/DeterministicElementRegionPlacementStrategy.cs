using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class DeterministicElementRegionPlacementStrategy :
        IStageElementPlacementStrategy
    {
        private const int RandomStream = unchecked((int)0x6E624EB7);

        public string StrategyId => "element-region-growth";
        public string Version => "2";

        public IReadOnlyDictionary<Vector2Int, ElementType> PlaceElements(
            StageElementPlacementContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            int candidateCount = context.CandidateCells.Count;
            int requiredCellCount = checked(
                context.MinimumDeployableCellCountPerElement *
                StageGroundElementTypes.Count);
            if (candidateCount < requiredCellCount)
            {
                throw new InvalidOperationException(
                    $"Ground-type placement has {candidateCount} cells but requires " +
                    $"at least {requiredCellCount} to satisfy its per-ground-type quota.");
            }

            Dictionary<Vector2Int, ElementType> assignments =
                new(candidateCount);
            if (candidateCount == 0)
            {
                return new ReadOnlyDictionary<Vector2Int, ElementType>(assignments);
            }

            List<Vector2Int> randomizedCells =
                new(context.CandidateCells);
            ElementType[] randomizedElements =
                StageGroundElementTypes.CreateOrderedCopy();
            StageRouteDeterministicRandom random =
                new(context.Seed, RandomStream);
            random.Shuffle(randomizedCells);
            random.Shuffle(randomizedElements);

            Dictionary<Vector2Int, int> randomRanks =
                new(randomizedCells.Count);
            for (int index = 0; index < randomizedCells.Count; index++)
            {
                randomRanks.Add(randomizedCells[index], index);
            }

            Dictionary<ElementType, Vector2Int> seedByElement = new();
            int seedCount = Math.Min(
                randomizedElements.Length,
                randomizedCells.Count);
            for (int index = 0; index < seedCount; index++)
            {
                Vector2Int selectedSeed = index == 0
                    ? randomizedCells[0]
                    : SelectFarthestSeed(
                        randomizedCells,
                        seedByElement.Values,
                        randomRanks);
                seedByElement.Add(randomizedElements[index], selectedSeed);
            }

            for (int quotaIndex = 0;
                 quotaIndex < context.MinimumDeployableCellCountPerElement;
                 quotaIndex++)
            {
                foreach (ElementType element in randomizedElements)
                {
                    Vector2Int selectedCell = SelectClosestUnassignedCell(
                        context.CandidateCells,
                        assignments,
                        seedByElement[element],
                        randomRanks);
                    assignments.Add(selectedCell, element);
                }
            }

            foreach (Vector2Int cell in context.CandidateCells)
            {
                if (assignments.ContainsKey(cell))
                {
                    continue;
                }

                ElementType selectedElement = SelectClosestElement(
                    cell,
                    randomizedElements,
                    seedByElement);
                assignments.Add(cell, selectedElement);
            }

            return new ReadOnlyDictionary<Vector2Int, ElementType>(assignments);
        }

        private static Vector2Int SelectFarthestSeed(
            IReadOnlyList<Vector2Int> candidates,
            IEnumerable<Vector2Int> existingSeeds,
            IReadOnlyDictionary<Vector2Int, int> randomRanks)
        {
            HashSet<Vector2Int> existingSeedSet = new(existingSeeds);
            Vector2Int selected = default;
            long selectedDistance = long.MinValue;
            int selectedRank = int.MaxValue;
            bool found = false;

            foreach (Vector2Int candidate in candidates)
            {
                if (existingSeedSet.Contains(candidate))
                {
                    continue;
                }

                long minimumDistance = long.MaxValue;
                foreach (Vector2Int seed in existingSeedSet)
                {
                    minimumDistance = Math.Min(
                        minimumDistance,
                        GetManhattanDistance(candidate, seed));
                }

                int rank = randomRanks[candidate];
                if (!found ||
                    minimumDistance > selectedDistance ||
                    (minimumDistance == selectedDistance && rank < selectedRank))
                {
                    selected = candidate;
                    selectedDistance = minimumDistance;
                    selectedRank = rank;
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    "A distinct ground-type region seed could not be selected.");
            }

            return selected;
        }

        private static Vector2Int SelectClosestUnassignedCell(
            IReadOnlyList<Vector2Int> candidates,
            IReadOnlyDictionary<Vector2Int, ElementType> assignments,
            Vector2Int seed,
            IReadOnlyDictionary<Vector2Int, int> randomRanks)
        {
            Vector2Int selected = default;
            long selectedDistance = long.MaxValue;
            int selectedRank = int.MaxValue;
            bool found = false;

            foreach (Vector2Int candidate in candidates)
            {
                if (assignments.ContainsKey(candidate))
                {
                    continue;
                }

                long distance = GetManhattanDistance(candidate, seed);
                int rank = randomRanks[candidate];
                if (!found ||
                    distance < selectedDistance ||
                    (distance == selectedDistance && rank < selectedRank))
                {
                    selected = candidate;
                    selectedDistance = distance;
                    selectedRank = rank;
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    "An unassigned ground cell could not be selected.");
            }

            return selected;
        }

        private static ElementType SelectClosestElement(
            Vector2Int cell,
            IReadOnlyList<ElementType> orderedElements,
            IReadOnlyDictionary<ElementType, Vector2Int> seedByElement)
        {
            ElementType selected = default;
            long selectedDistance = long.MaxValue;
            bool found = false;

            foreach (ElementType element in orderedElements)
            {
                if (!seedByElement.TryGetValue(element, out Vector2Int seed))
                {
                    continue;
                }

                long distance = GetManhattanDistance(cell, seed);
                if (!found || distance < selectedDistance)
                {
                    selected = element;
                    selectedDistance = distance;
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    "No ground-type region seed is available.");
            }

            return selected;
        }

        private static long GetManhattanDistance(
            Vector2Int first,
            Vector2Int second)
        {
            return Math.Abs((long)first.x - second.x) +
                   Math.Abs((long)first.y - second.y);
        }
    }
}
