using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageElementPlacementContext
    {
        private readonly IReadOnlyList<Vector2Int> candidateCells;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public IReadOnlyList<Vector2Int> CandidateCells => candidateCells;
        public int MinimumDeployableCellCountPerElement { get; }

        public StageElementPlacementContext(
            RectInt bounds,
            int seed,
            IReadOnlyList<Vector2Int> sourceCandidateCells,
            int minimumDeployableCellCountPerElement)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds));
            }

            if (sourceCandidateCells == null)
            {
                throw new ArgumentNullException(nameof(sourceCandidateCells));
            }

            if (minimumDeployableCellCountPerElement < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumDeployableCellCountPerElement));
            }

            Vector2Int[] copies = new Vector2Int[sourceCandidateCells.Count];
            HashSet<Vector2Int> uniqueCells = new();
            for (int index = 0; index < sourceCandidateCells.Count; index++)
            {
                Vector2Int cell = sourceCandidateCells[index];
                if (!bounds.Contains(cell))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(sourceCandidateCells),
                        cell,
                        $"Element candidate {cell} is outside bounds {bounds}.");
                }

                if (!uniqueCells.Add(cell))
                {
                    throw new ArgumentException(
                        $"Element candidate {cell} is duplicated.",
                        nameof(sourceCandidateCells));
                }

                copies[index] = cell;
            }

            Array.Sort(copies, CompareCellsRowMajor);
            Bounds = bounds;
            Seed = seed;
            candidateCells = Array.AsReadOnly(copies);
            MinimumDeployableCellCountPerElement =
                minimumDeployableCellCountPerElement;
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
