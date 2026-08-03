using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageBlockedCellPlacementContext
    {
        private readonly IReadOnlyList<Vector2Int> candidateCells;
        private readonly IReadOnlyList<Vector2Int> roadCells;
        private readonly IReadOnlyList<Vector2Int> endpointCells;
        private readonly IReadOnlyDictionary<Vector2Int, ElementType> elementsByCell;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public IReadOnlyList<Vector2Int> CandidateCells => candidateCells;
        public IReadOnlyList<Vector2Int> RoadCells => roadCells;
        public IReadOnlyList<Vector2Int> EndpointCells => endpointCells;
        public IReadOnlyDictionary<Vector2Int, ElementType> ElementsByCell =>
            elementsByCell;
        public int TargetBlockedCellCount { get; }
        public int MinimumDeployableCellCount { get; }
        public int MinimumDeployableCellCountPerElement { get; }
        public int MinimumDeployableNeighborsPerRoadCell { get; }
        public int EndpointProtectionRadius { get; }
        public int MaximumBlockedClusterSize { get; }
        public int MaxAttempts { get; }

        public StageBlockedCellPlacementContext(
            RectInt bounds,
            int seed,
            IReadOnlyList<Vector2Int> sourceCandidateCells,
            IReadOnlyDictionary<Vector2Int, ElementType> sourceElementsByCell,
            IReadOnlyList<Vector2Int> sourceRoadCells,
            IReadOnlyList<Vector2Int> sourceEndpointCells,
            int targetBlockedCellCount,
            int minimumDeployableCellCount,
            int minimumDeployableCellCountPerElement,
            int minimumDeployableNeighborsPerRoadCell,
            int endpointProtectionRadius,
            int maximumBlockedClusterSize,
            int maxAttempts)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds));
            }

            if (sourceCandidateCells == null)
            {
                throw new ArgumentNullException(nameof(sourceCandidateCells));
            }

            if (sourceElementsByCell == null)
            {
                throw new ArgumentNullException(nameof(sourceElementsByCell));
            }

            if (sourceRoadCells == null)
            {
                throw new ArgumentNullException(nameof(sourceRoadCells));
            }

            if (sourceEndpointCells == null)
            {
                throw new ArgumentNullException(nameof(sourceEndpointCells));
            }

            if (targetBlockedCellCount < 0 ||
                targetBlockedCellCount > sourceCandidateCells.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(targetBlockedCellCount));
            }

            if (minimumDeployableCellCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCount));
            }

            if (minimumDeployableCellCountPerElement < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCountPerElement));
            }

            if (minimumDeployableNeighborsPerRoadCell < 0 ||
                minimumDeployableNeighborsPerRoadCell > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableNeighborsPerRoadCell));
            }

            if (maxAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAttempts));
            }

            if (endpointProtectionRadius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endpointProtectionRadius));
            }

            if (maximumBlockedClusterSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumBlockedClusterSize));
            }

            Vector2Int[] candidateCopies = CopyUniqueCells(
                bounds,
                sourceCandidateCells,
                nameof(sourceCandidateCells));
            Vector2Int[] roadCopies = CopyUniqueCells(
                bounds,
                sourceRoadCells,
                nameof(sourceRoadCells));
            Vector2Int[] endpointCopies = CopyUniqueCells(
                bounds,
                sourceEndpointCells,
                nameof(sourceEndpointCells));
            Array.Sort(candidateCopies, CompareCellsRowMajor);
            Array.Sort(roadCopies, CompareCellsRowMajor);
            Array.Sort(endpointCopies, CompareCellsRowMajor);

            HashSet<Vector2Int> candidateSet = new(candidateCopies);
            foreach (Vector2Int roadCell in roadCopies)
            {
                if (candidateSet.Contains(roadCell))
                {
                    throw new ArgumentException(
                        $"Road cell {roadCell} cannot also be a blocked-cell candidate.",
                        nameof(sourceRoadCells));
                }
            }

            Dictionary<Vector2Int, ElementType> elementCopies =
                new(candidateCopies.Length);
            foreach (Vector2Int cell in candidateCopies)
            {
                if (!sourceElementsByCell.TryGetValue(
                        cell,
                        out ElementType element))
                {
                    throw new ArgumentException(
                        $"Element placement is missing candidate cell {cell}.",
                        nameof(sourceElementsByCell));
                }

                if (element != ElementType.Water &&
                    element != ElementType.Fire &&
                    element != ElementType.Earth)
                {
                    throw new ArgumentException(
                        $"Candidate cell {cell} has unsupported element {element}.",
                        nameof(sourceElementsByCell));
                }

                elementCopies.Add(cell, element);
            }

            if (sourceElementsByCell.Count != candidateCopies.Length)
            {
                throw new ArgumentException(
                    "Element placement contains cells outside the candidate set.",
                    nameof(sourceElementsByCell));
            }

            Bounds = bounds;
            Seed = seed;
            candidateCells = Array.AsReadOnly(candidateCopies);
            roadCells = Array.AsReadOnly(roadCopies);
            endpointCells = Array.AsReadOnly(endpointCopies);
            elementsByCell =
                new ReadOnlyDictionary<Vector2Int, ElementType>(elementCopies);
            TargetBlockedCellCount = targetBlockedCellCount;
            MinimumDeployableCellCount = minimumDeployableCellCount;
            MinimumDeployableCellCountPerElement =
                minimumDeployableCellCountPerElement;
            MinimumDeployableNeighborsPerRoadCell =
                minimumDeployableNeighborsPerRoadCell;
            EndpointProtectionRadius = endpointProtectionRadius;
            MaximumBlockedClusterSize = maximumBlockedClusterSize;
            MaxAttempts = maxAttempts;
        }

        private static Vector2Int[] CopyUniqueCells(
            RectInt bounds,
            IReadOnlyList<Vector2Int> sourceCells,
            string parameterName)
        {
            Vector2Int[] copies = new Vector2Int[sourceCells.Count];
            HashSet<Vector2Int> uniqueCells = new();
            for (int index = 0; index < sourceCells.Count; index++)
            {
                Vector2Int cell = sourceCells[index];
                if (!bounds.Contains(cell))
                {
                    throw new ArgumentOutOfRangeException(
                        parameterName,
                        cell,
                        $"Cell {cell} is outside bounds {bounds}.");
                }

                if (!uniqueCells.Add(cell))
                {
                    throw new ArgumentException(
                        $"Cell {cell} is duplicated.",
                        parameterName);
                }

                copies[index] = cell;
            }

            return copies;
        }

        private static int CompareCellsRowMajor(Vector2Int left, Vector2Int right)
        {
            int yComparison = left.y.CompareTo(right.y);
            return yComparison != 0
                ? yComparison
                : left.x.CompareTo(right.x);
        }
    }
}
