using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public sealed class DeterministicBorderRegionExpansionStrategy :
        IStageDecorationExpansionStrategy
    {
        private const ulong DecorationTraversalHashDomain =
            0x8A7F6D359E20C14BUL;
        private const int BaseTraversalCost = 1024;
        private const int TraversalCostVariation = 256;

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        public string StrategyId => "border-region-growth";
        public string Version => "2";

        public IReadOnlyList<StageDecorationCellEntry> Expand(
            GeneratedStageMap map,
            Vector2Int centerCell,
            int radius,
            IReadOnlyList<Vector2Int> decorationCells,
            int seed)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(radius),
                    radius,
                    "The decoration radius cannot be negative.");
            }

            if (decorationCells == null)
            {
                throw new ArgumentNullException(nameof(decorationCells));
            }

            if (decorationCells.Count == 0)
            {
                return Array.Empty<StageDecorationCellEntry>();
            }

            AssignMapElements(
                map,
                out long[] mapCosts,
                out ElementType[] mapElements);

            RectInt decorationBounds = CreateDiskBounds(
                centerCell,
                radius);
            int storageLength = checked(
                decorationBounds.width * decorationBounds.height);
            bool[] candidateCells = new bool[storageLength];
            for (int index = 0; index < decorationCells.Count; index++)
            {
                Vector2Int cell = decorationCells[index];
                if (!decorationBounds.Contains(cell))
                {
                    throw new ArgumentException(
                        $"Decoration candidate {cell} is outside disk bounds " +
                        $"{decorationBounds}.",
                        nameof(decorationCells));
                }

                int cellIndex = GetCellIndex(decorationBounds, cell);
                if (candidateCells[cellIndex])
                {
                    throw new ArgumentException(
                        $"Decoration candidate {cell} is duplicated.",
                        nameof(decorationCells));
                }

                candidateCells[cellIndex] = true;
            }

            long[] decorationCosts = new long[storageLength];
            Array.Fill(decorationCosts, long.MaxValue);
            ElementType[] decorationElements =
                new ElementType[storageLength];
            FrontierMinHeap frontier = new();

            SeedFromMapBoundary(
                map,
                seed,
                decorationBounds,
                candidateCells,
                mapCosts,
                mapElements,
                decorationCosts,
                decorationElements,
                frontier);

            ExpandDecorationFrontier(
                seed,
                decorationBounds,
                candidateCells,
                decorationCosts,
                decorationElements,
                frontier);

            List<StageDecorationCellEntry> entries =
                new(decorationCells.Count);
            for (int index = 0; index < decorationCells.Count; index++)
            {
                Vector2Int cell = decorationCells[index];
                int cellIndex = GetCellIndex(decorationBounds, cell);
                if (decorationCosts[cellIndex] == long.MaxValue)
                {
                    continue;
                }

                entries.Add(new StageDecorationCellEntry(
                    cell,
                    decorationElements[cellIndex]));
            }

            return entries.AsReadOnly();
        }

        private static void AssignMapElements(
            GeneratedStageMap map,
            out long[] costs,
            out ElementType[] elements)
        {
            costs = new long[map.CellCount];
            Array.Fill(costs, long.MaxValue);
            elements = new ElementType[map.CellCount];
            FrontierMinHeap frontier = new();

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (!StageDecorationCellEntry.IsSupportedGroundSource(
                        entry.Cell))
                {
                    continue;
                }

                ElementType element = entry.Cell.Element;
                int index = GetCellIndex(map.Bounds, entry.Coordinates);
                costs[index] = 0L;
                elements[index] = element;
                frontier.Push(new FrontierNode(
                    entry.Coordinates,
                    0L,
                    element));
            }

            while (frontier.Count > 0)
            {
                FrontierNode current = frontier.Pop();
                int currentIndex = GetCellIndex(map.Bounds, current.Cell);
                if (costs[currentIndex] != current.Cost ||
                    elements[currentIndex] != current.Element)
                {
                    continue;
                }

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int neighbor =
                        current.Cell + CardinalDirections[directionIndex];
                    if (!map.Contains(neighbor))
                    {
                        continue;
                    }

                    // The in-map bridge uses exact four-neighbor distance so a
                    // non-source perimeter cell inherits its nearest ground
                    // source. Seeded noise is reserved for the exterior growth.
                    long candidateCost = checked(current.Cost + 1L);
                    int neighborIndex = GetCellIndex(map.Bounds, neighbor);
                    if (!IsBetterAssignment(
                            candidateCost,
                            current.Element,
                            costs[neighborIndex],
                            elements[neighborIndex]))
                    {
                        continue;
                    }

                    costs[neighborIndex] = candidateCost;
                    elements[neighborIndex] = current.Element;
                    frontier.Push(new FrontierNode(
                        neighbor,
                        candidateCost,
                        current.Element));
                }
            }
        }

        private static void SeedFromMapBoundary(
            GeneratedStageMap map,
            int seed,
            RectInt decorationBounds,
            IReadOnlyList<bool> candidateCells,
            IReadOnlyList<long> mapCosts,
            IReadOnlyList<ElementType> mapElements,
            IList<long> decorationCosts,
            IList<ElementType> decorationElements,
            FrontierMinHeap frontier)
        {
            RectInt mapBounds = map.Bounds;
            for (int y = mapBounds.yMin; y < mapBounds.yMax; y++)
            {
                for (int x = mapBounds.xMin; x < mapBounds.xMax; x++)
                {
                    if (x != mapBounds.xMin &&
                        x != mapBounds.xMax - 1 &&
                        y != mapBounds.yMin &&
                        y != mapBounds.yMax - 1)
                    {
                        continue;
                    }

                    Vector2Int boundaryCell = new(x, y);
                    int mapIndex = GetCellIndex(mapBounds, boundaryCell);
                    if (mapCosts[mapIndex] == long.MaxValue)
                    {
                        continue;
                    }

                    ElementType element = mapElements[mapIndex];
                    for (int directionIndex = 0;
                         directionIndex < CardinalDirections.Length;
                         directionIndex++)
                    {
                        Vector2Int candidate =
                            boundaryCell + CardinalDirections[directionIndex];
                        if (!decorationBounds.Contains(candidate))
                        {
                            continue;
                        }

                        int candidateIndex = GetCellIndex(
                            decorationBounds,
                            candidate);
                        if (!candidateCells[candidateIndex])
                        {
                            continue;
                        }

                        // Every inferred perimeter label starts the exterior
                        // expansion at equal cost. The internal distance only
                        // selected the perimeter label and must not bias which
                        // perimeter segment owns an exterior cell.
                        long candidateCost = GetTraversalCost(
                            seed,
                            candidate,
                            DecorationTraversalHashDomain);
                        if (!IsBetterAssignment(
                                candidateCost,
                                element,
                                decorationCosts[candidateIndex],
                                decorationElements[candidateIndex]))
                        {
                            continue;
                        }

                        decorationCosts[candidateIndex] = candidateCost;
                        decorationElements[candidateIndex] = element;
                        frontier.Push(new FrontierNode(
                            candidate,
                            candidateCost,
                            element));
                    }
                }
            }
        }

        private static void ExpandDecorationFrontier(
            int seed,
            RectInt decorationBounds,
            IReadOnlyList<bool> candidateCells,
            IList<long> costs,
            IList<ElementType> elements,
            FrontierMinHeap frontier)
        {
            while (frontier.Count > 0)
            {
                FrontierNode current = frontier.Pop();
                int currentIndex = GetCellIndex(
                    decorationBounds,
                    current.Cell);
                if (costs[currentIndex] != current.Cost ||
                    elements[currentIndex] != current.Element)
                {
                    continue;
                }

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int neighbor =
                        current.Cell + CardinalDirections[directionIndex];
                    if (!decorationBounds.Contains(neighbor))
                    {
                        continue;
                    }

                    int neighborIndex = GetCellIndex(
                        decorationBounds,
                        neighbor);
                    if (!candidateCells[neighborIndex])
                    {
                        continue;
                    }

                    long candidateCost = checked(
                        current.Cost + GetTraversalCost(
                            seed,
                            neighbor,
                            DecorationTraversalHashDomain));
                    if (!IsBetterAssignment(
                            candidateCost,
                            current.Element,
                            costs[neighborIndex],
                            elements[neighborIndex]))
                    {
                        continue;
                    }

                    costs[neighborIndex] = candidateCost;
                    elements[neighborIndex] = current.Element;
                    frontier.Push(new FrontierNode(
                        neighbor,
                        candidateCost,
                        current.Element));
                }
            }
        }

        private static bool IsBetterAssignment(
            long candidateCost,
            ElementType candidateElement,
            long currentCost,
            ElementType currentElement)
        {
            return candidateCost < currentCost ||
                   (candidateCost == currentCost &&
                    (int)candidateElement < (int)currentElement);
        }

        private static int GetTraversalCost(
            int seed,
            Vector2Int cell,
            ulong domain)
        {
            ulong seedValue = (uint)seed;
            ulong coordinateValue =
                ((ulong)(uint)cell.x << 32) | (uint)cell.y;
            ulong hash = Mix64(domain ^ seedValue);
            hash = Mix64(hash ^ coordinateValue);
            return BaseTraversalCost +
                   (int)(hash % TraversalCostVariation);
        }

        private static ulong Mix64(ulong value)
        {
            value = (value ^ (value >> 30)) *
                    0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) *
                    0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }

        private static RectInt CreateDiskBounds(
            Vector2Int centerCell,
            int radius)
        {
            int diameter = checked(radius * 2 + 1);
            return new RectInt(
                checked(centerCell.x - radius),
                checked(centerCell.y - radius),
                diameter,
                diameter);
        }

        private static int GetCellIndex(
            RectInt bounds,
            Vector2Int cell)
        {
            int localX = cell.x - bounds.xMin;
            int localY = cell.y - bounds.yMin;
            return localY * bounds.width + localX;
        }

        private readonly struct FrontierNode
        {
            internal Vector2Int Cell { get; }
            internal long Cost { get; }
            internal ElementType Element { get; }

            internal FrontierNode(
                Vector2Int cell,
                long cost,
                ElementType element)
            {
                Cell = cell;
                Cost = cost;
                Element = element;
            }
        }

        private sealed class FrontierMinHeap
        {
            private readonly List<FrontierNode> nodes = new();

            internal int Count => nodes.Count;

            internal void Push(FrontierNode node)
            {
                nodes.Add(node);
                int index = nodes.Count - 1;
                while (index > 0)
                {
                    int parentIndex = (index - 1) / 2;
                    if (Compare(nodes[parentIndex], nodes[index]) <= 0)
                    {
                        break;
                    }

                    (nodes[parentIndex], nodes[index]) =
                        (nodes[index], nodes[parentIndex]);
                    index = parentIndex;
                }
            }

            internal FrontierNode Pop()
            {
                if (nodes.Count == 0)
                {
                    throw new InvalidOperationException(
                        "The decoration frontier is empty.");
                }

                FrontierNode result = nodes[0];
                int lastIndex = nodes.Count - 1;
                nodes[0] = nodes[lastIndex];
                nodes.RemoveAt(lastIndex);

                int index = 0;
                while (true)
                {
                    int leftIndex = index * 2 + 1;
                    if (leftIndex >= nodes.Count)
                    {
                        break;
                    }

                    int rightIndex = leftIndex + 1;
                    int smallestIndex =
                        rightIndex < nodes.Count &&
                        Compare(nodes[rightIndex], nodes[leftIndex]) < 0
                            ? rightIndex
                            : leftIndex;
                    if (Compare(nodes[index], nodes[smallestIndex]) <= 0)
                    {
                        break;
                    }

                    (nodes[index], nodes[smallestIndex]) =
                        (nodes[smallestIndex], nodes[index]);
                    index = smallestIndex;
                }

                return result;
            }

            private static int Compare(
                FrontierNode left,
                FrontierNode right)
            {
                int costComparison = left.Cost.CompareTo(right.Cost);
                if (costComparison != 0)
                {
                    return costComparison;
                }

                int elementComparison = ((int)left.Element).CompareTo(
                    (int)right.Element);
                if (elementComparison != 0)
                {
                    return elementComparison;
                }

                int yComparison = left.Cell.y.CompareTo(right.Cell.y);
                return yComparison != 0
                    ? yComparison
                    : left.Cell.x.CompareTo(right.Cell.x);
            }
        }
    }
}
