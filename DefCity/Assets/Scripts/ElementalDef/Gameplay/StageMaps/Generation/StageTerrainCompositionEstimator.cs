using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public static class StageTerrainCompositionEstimator
    {
        public static StageTerrainCompositionResult Estimate(
            StageMapGenerationProfile profile,
            int seed)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            RectInt bounds = profile.Bounds;
            int totalCellCount = checked(bounds.width * bounds.height);
            if (totalCellCount <= 0)
            {
                throw new InvalidOperationException(
                    "The stage map profile must have positive bounds.");
            }

            StageMapGenerationSettings settings = profile.CreateSettings(seed);
            List<Vector2Int> candidateCells = CreateCandidateCells(bounds, totalCellCount);
            StageElementPlacementContext context = new(bounds, seed, candidateCells, settings.MinimumDeployableCellCountPerElement);

            DeterministicElementRegionPlacementStrategy strategy = new();
            IReadOnlyDictionary<Vector2Int, ElementType> elementsByCell = strategy.PlaceElements(context);
            if (elementsByCell.Count != totalCellCount)
            {
                throw new InvalidOperationException(
                    $"The terrain strategy assigned {elementsByCell.Count} cells " +
                    $"instead of the expected {totalCellCount}.");
            }

            int neutralCellCount = 0;
            int waterCellCount = 0;
            int fireCellCount = 0;
            int earthCellCount = 0;
            foreach (Vector2Int cell in candidateCells)
            {
                if (!elementsByCell.TryGetValue(cell, out ElementType element))
                {
                    throw new InvalidOperationException(
                        $"The terrain strategy did not assign cell {cell}.");
                }

                switch (element)
                {
                    case ElementType.Neutral:
                        neutralCellCount++;
                        break;
                    case ElementType.Water:
                        waterCellCount++;
                        break;
                    case ElementType.Fire:
                        fireCellCount++;
                        break;
                    case ElementType.Earth:
                        earthCellCount++;
                        break;
                    default:
                        throw new InvalidOperationException($"The terrain strategy returned unsupported element {element}.");
                }
            }

            return new StageTerrainCompositionResult(totalCellCount, neutralCellCount, waterCellCount, fireCellCount, earthCellCount);
        }

        private static List<Vector2Int> CreateCandidateCells(
            RectInt bounds,
            int capacity)
        {
            List<Vector2Int> cells = new(capacity);
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }

            return cells;
        }
    }
}
