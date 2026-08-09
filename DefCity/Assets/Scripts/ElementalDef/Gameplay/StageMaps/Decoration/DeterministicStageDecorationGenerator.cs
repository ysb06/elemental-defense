using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public sealed class DeterministicStageDecorationGenerator
    {
        public const string GeneratorVersion =
            "deterministic-stage-decoration-v5";

        private static readonly Vector2Int[] NeighborDirections =
        {
            new(-1, -1),
            new(0, -1),
            new(1, -1),
            new(-1, 0),
            new(1, 0),
            new(-1, 1),
            new(0, 1),
            new(1, 1),
        };

        private readonly IStageDecorationExpansionStrategy expansionStrategy;

        public DeterministicStageDecorationGenerator(
            IStageDecorationExpansionStrategy expansionStrategy = null)
        {
            this.expansionStrategy = expansionStrategy ??
                new DeterministicBorderRegionExpansionStrategy();

            if (string.IsNullOrWhiteSpace(this.expansionStrategy.StrategyId))
            {
                throw new ArgumentException(
                    "A decoration expansion strategy requires an ID.",
                    nameof(expansionStrategy));
            }

            if (string.IsNullOrWhiteSpace(this.expansionStrategy.Version))
            {
                throw new ArgumentException(
                    "A decoration expansion strategy requires a version.",
                    nameof(expansionStrategy));
            }
        }

        public StageDecorationGenerationResult Generate(
            GeneratedStageMap map,
            StageDecorationGenerationSettings settings)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            EnsureStrategyIdentity(
                expansionStrategy.StrategyId,
                expansionStrategy.Version,
                nameof(expansionStrategy));

            Vector2Int centerCell = new(
                checked(map.Bounds.xMin + map.Bounds.width / 2),
                checked(map.Bounds.yMin + map.Bounds.height / 2));

            if (!settings.GenerateGroundDecoration)
            {
                RectInt rectangularBounds =
                    CreateRectangularBoundaryBounds(map.Bounds);
                List<StageDecorationCellEntry> rectangularWalls =
                    CollectRectangularBoundaryWallCells(
                        map.Bounds,
                        rectangularBounds);
                return StageDecorationGenerationResult.Success(
                    new GeneratedStageDecoration(
                        map.Seed,
                        BuildGeneratorVersion(),
                        centerCell,
                        0,
                        StageDecorationBoundaryShape.MapBoundsRectangle,
                        rectangularBounds,
                        rectangularWalls));
            }

            int minimumRadius = CalculateMinimumRadius(
                map.Bounds,
                centerCell);
            int radius = checked(
                minimumRadius + settings.OuterPadding);
            RectInt decorationBounds = CreateDiskBounds(
                centerCell,
                checked(radius + 1));
            List<StageDecorationCellEntry> boundaryWalls =
                CollectBoundaryWallCells(
                    centerCell,
                    radius,
                    decorationBounds);

            RectInt elementalBounds = CreateDiskBounds(
                centerCell,
                radius);
            List<Vector2Int> decorationCells = CollectDecorationCells(
                map,
                centerCell,
                radius,
                elementalBounds);
            if (decorationCells.Count == 0)
            {
                return StageDecorationGenerationResult.Success(
                    new GeneratedStageDecoration(
                        map.Seed,
                        BuildGeneratorVersion(),
                        centerCell,
                        radius,
                        StageDecorationBoundaryShape.CircularDisk,
                        decorationBounds,
                        boundaryWalls));
            }

            if (!HasGroundSourceCell(map))
            {
                return StageDecorationGenerationResult.Failure(
                    StageDecorationGenerationFailureReason.
                        NoElementalSourceCells,
                    "The play map has no Neutral, Water, Fire, or Earth " +
                    "ground cell from which decoration regions can expand.");
            }

            IReadOnlyList<Vector2Int> readOnlyCandidates =
                decorationCells.AsReadOnly();
            IReadOnlyList<StageDecorationCellEntry> expandedCells =
                expansionStrategy.Expand(
                    map,
                    centerCell,
                    radius,
                    readOnlyCandidates,
                    map.Seed);

            if (!TryValidateExpansion(
                    elementalBounds,
                    decorationCells,
                    expandedCells,
                    out string validationMessage))
            {
                return StageDecorationGenerationResult.Failure(
                    StageDecorationGenerationFailureReason.
                        InvalidExpansionResult,
                    validationMessage);
            }

            List<StageDecorationCellEntry> allCells = new(
                checked(expandedCells.Count + boundaryWalls.Count));
            for (int index = 0; index < expandedCells.Count; index++)
            {
                allCells.Add(expandedCells[index]);
            }

            allCells.AddRange(boundaryWalls);

            GeneratedStageDecoration decoration = new(
                map.Seed,
                BuildGeneratorVersion(),
                centerCell,
                radius,
                StageDecorationBoundaryShape.CircularDisk,
                decorationBounds,
                allCells);
            return StageDecorationGenerationResult.Success(decoration);
        }

        private string BuildGeneratorVersion()
        {
            return $"{GeneratorVersion}|expansion=" +
                   $"{expansionStrategy.StrategyId}@" +
                   $"{expansionStrategy.Version}";
        }

        private static int CalculateMinimumRadius(
            RectInt mapBounds,
            Vector2Int centerCell)
        {
            Vector2Int[] corners =
            {
                new(mapBounds.xMin, mapBounds.yMin),
                new(mapBounds.xMax - 1, mapBounds.yMin),
                new(mapBounds.xMin, mapBounds.yMax - 1),
                new(mapBounds.xMax - 1, mapBounds.yMax - 1),
            };

            long maximumSquaredDistance = 0L;
            for (int index = 0; index < corners.Length; index++)
            {
                long deltaX = (long)corners[index].x - centerCell.x;
                long deltaY = (long)corners[index].y - centerCell.y;
                long squaredDistance = checked(
                    deltaX * deltaX + deltaY * deltaY);
                maximumSquaredDistance = Math.Max(
                    maximumSquaredDistance,
                    squaredDistance);
            }

            long radius = (long)Math.Ceiling(
                Math.Sqrt(maximumSquaredDistance));
            while (radius > 0 &&
                   checked((radius - 1) * (radius - 1)) >=
                   maximumSquaredDistance)
            {
                radius--;
            }

            while (checked(radius * radius) < maximumSquaredDistance)
            {
                radius++;
            }

            return checked((int)radius);
        }

        private static RectInt CreateDiskBounds(
            Vector2Int centerCell,
            int radius)
        {
            long xMin = (long)centerCell.x - radius;
            long yMin = (long)centerCell.y - radius;
            long xMaxExclusive = (long)centerCell.x + radius + 1L;
            long yMaxExclusive = (long)centerCell.y + radius + 1L;

            if (xMin < int.MinValue ||
                yMin < int.MinValue ||
                xMaxExclusive > int.MaxValue ||
                yMaxExclusive > int.MaxValue)
            {
                throw new OverflowException(
                    $"The decoration disk centered at {centerCell} with " +
                    $"radius {radius} cannot be represented by RectInt.");
            }

            int diameter = checked((int)(xMaxExclusive - xMin));
            return new RectInt(
                (int)xMin,
                (int)yMin,
                diameter,
                diameter);
        }

        private static RectInt CreateRectangularBoundaryBounds(
            RectInt mapBounds)
        {
            long xMin = (long)mapBounds.xMin - 1L;
            long yMin = (long)mapBounds.yMin - 1L;
            long width = (long)mapBounds.width + 2L;
            long height = (long)mapBounds.height + 2L;
            long xMaxExclusive = checked(xMin + width);
            long yMaxExclusive = checked(yMin + height);
            long storageLength = checked(width * height);

            if (xMin < int.MinValue ||
                yMin < int.MinValue ||
                xMaxExclusive > int.MaxValue ||
                yMaxExclusive > int.MaxValue ||
                width > int.MaxValue ||
                height > int.MaxValue ||
                storageLength > int.MaxValue)
            {
                throw new OverflowException(
                    $"Map bounds {mapBounds} cannot be expanded by one " +
                    "decoration boundary cell in every direction.");
            }

            return new RectInt(
                (int)xMin,
                (int)yMin,
                (int)width,
                (int)height);
        }

        private static List<StageDecorationCellEntry>
            CollectRectangularBoundaryWallCells(
                RectInt mapBounds,
                RectInt decorationBounds)
        {
            long wallCellCount = checked(
                2L * mapBounds.width +
                2L * mapBounds.height +
                4L);
            if (wallCellCount > int.MaxValue)
            {
                throw new OverflowException(
                    $"The rectangular decoration boundary around " +
                    $"{mapBounds} contains too many cells.");
            }

            List<StageDecorationCellEntry> cells =
                new((int)wallCellCount);
            long xMaxExclusive =
                (long)decorationBounds.xMin + decorationBounds.width;
            long yMaxExclusive =
                (long)mapBounds.yMin + mapBounds.height;
            int bottomY = decorationBounds.yMin;
            int topY = checked((int)yMaxExclusive);
            int leftX = decorationBounds.xMin;
            int rightX = checked((int)(
                (long)mapBounds.xMin + mapBounds.width));

            for (long x = decorationBounds.xMin;
                 x < xMaxExclusive;
                 x++)
            {
                cells.Add(
                    StageDecorationCellEntry.CreateBoundaryWall(
                        new Vector2Int((int)x, bottomY)));
            }

            for (long y = mapBounds.yMin;
                 y < yMaxExclusive;
                 y++)
            {
                cells.Add(
                    StageDecorationCellEntry.CreateBoundaryWall(
                        new Vector2Int(leftX, (int)y)));
                cells.Add(
                    StageDecorationCellEntry.CreateBoundaryWall(
                        new Vector2Int(rightX, (int)y)));
            }

            for (long x = decorationBounds.xMin;
                 x < xMaxExclusive;
                 x++)
            {
                cells.Add(
                    StageDecorationCellEntry.CreateBoundaryWall(
                        new Vector2Int((int)x, topY)));
            }

            if (cells.Count != wallCellCount)
            {
                throw new InvalidOperationException(
                    $"The rectangular decoration boundary around " +
                    $"{mapBounds} produced {cells.Count} cells; " +
                    $"{wallCellCount} were required.");
            }

            return cells;
        }

        private static List<Vector2Int> CollectDecorationCells(
            GeneratedStageMap map,
            Vector2Int centerCell,
            int radius,
            RectInt decorationBounds)
        {
            long squaredRadius = checked((long)radius * radius);
            List<Vector2Int> cells = new();
            for (int y = decorationBounds.yMin;
                 y < decorationBounds.yMax;
                 y++)
            {
                for (int x = decorationBounds.xMin;
                     x < decorationBounds.xMax;
                     x++)
                {
                    Vector2Int cell = new(x, y);
                    if (map.Contains(cell))
                    {
                        continue;
                    }

                    long deltaX = (long)x - centerCell.x;
                    long deltaY = (long)y - centerCell.y;
                    long squaredDistance = checked(
                        deltaX * deltaX + deltaY * deltaY);
                    if (squaredDistance <= squaredRadius)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells;
        }

        private static List<StageDecorationCellEntry>
            CollectBoundaryWallCells(
                Vector2Int centerCell,
                int radius,
                RectInt decorationBounds)
        {
            long squaredRadius = checked((long)radius * radius);
            List<StageDecorationCellEntry> cells = new();
            for (int y = decorationBounds.yMin;
                 y < decorationBounds.yMax;
                 y++)
            {
                for (int x = decorationBounds.xMin;
                     x < decorationBounds.xMax;
                     x++)
                {
                    Vector2Int cell = new(x, y);
                    if (IsInsideDisk(
                            cell,
                            centerCell,
                            squaredRadius) ||
                        !HasNeighborInsideDisk(
                            cell,
                            centerCell,
                            squaredRadius))
                    {
                        continue;
                    }

                    cells.Add(
                        StageDecorationCellEntry.CreateBoundaryWall(cell));
                }
            }

            return cells;
        }

        private static bool HasNeighborInsideDisk(
            Vector2Int cell,
            Vector2Int centerCell,
            long squaredRadius)
        {
            for (int index = 0; index < NeighborDirections.Length; index++)
            {
                long neighborX =
                    (long)cell.x + NeighborDirections[index].x;
                long neighborY =
                    (long)cell.y + NeighborDirections[index].y;
                if (IsInsideDisk(
                        neighborX,
                        neighborY,
                        centerCell,
                        squaredRadius))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInsideDisk(
            Vector2Int cell,
            Vector2Int centerCell,
            long squaredRadius)
        {
            return IsInsideDisk(
                cell.x,
                cell.y,
                centerCell,
                squaredRadius);
        }

        private static bool IsInsideDisk(
            long cellX,
            long cellY,
            Vector2Int centerCell,
            long squaredRadius)
        {
            long deltaX = cellX - centerCell.x;
            long deltaY = cellY - centerCell.y;
            long squaredDistance = checked(
                deltaX * deltaX + deltaY * deltaY);
            return squaredDistance <= squaredRadius;
        }

        private static bool HasGroundSourceCell(
            GeneratedStageMap map)
        {
            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (StageDecorationCellEntry.IsSupportedGroundSource(
                        entry.Cell))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryValidateExpansion(
            RectInt decorationBounds,
            IReadOnlyList<Vector2Int> expectedCells,
            IReadOnlyList<StageDecorationCellEntry> actualCells,
            out string message)
        {
            if (actualCells == null)
            {
                message = "The decoration expansion strategy returned no result.";
                return false;
            }

            if (actualCells.Count != expectedCells.Count)
            {
                message =
                    $"The decoration expansion strategy returned " +
                    $"{actualCells.Count} cells but {expectedCells.Count} " +
                    "cells were required.";
                return false;
            }

            int storageLength = checked(
                decorationBounds.width * decorationBounds.height);
            bool[] expected = new bool[storageLength];
            bool[] visited = new bool[storageLength];
            for (int index = 0; index < expectedCells.Count; index++)
            {
                int cellIndex = GetCellIndex(
                    decorationBounds,
                    expectedCells[index]);
                expected[cellIndex] = true;
            }

            for (int index = 0; index < actualCells.Count; index++)
            {
                StageDecorationCellEntry entry = actualCells[index];
                if (!decorationBounds.Contains(entry.Coordinates))
                {
                    message =
                        $"Decoration cell {entry.Coordinates} is outside " +
                        $"the required bounds {decorationBounds}.";
                    return false;
                }

                int cellIndex = GetCellIndex(
                    decorationBounds,
                    entry.Coordinates);
                if (!expected[cellIndex])
                {
                    message =
                        $"Decoration cell {entry.Coordinates} was not part " +
                        "of the requested expansion area.";
                    return false;
                }

                if (visited[cellIndex])
                {
                    message =
                        $"Decoration cell {entry.Coordinates} was returned " +
                        "more than once.";
                    return false;
                }

                if (entry.Kind != StageDecorationCellKind.ElementalGround ||
                    !StageDecorationCellEntry.IsSupportedElement(
                        entry.Element))
                {
                    message =
                        $"Decoration cell {entry.Coordinates} uses " +
                        $"unsupported ground kind/element " +
                        $"{entry.Kind}/{entry.Element}.";
                    return false;
                }

                visited[cellIndex] = true;
            }

            message = null;
            return true;
        }

        private static int GetCellIndex(
            RectInt bounds,
            Vector2Int cell)
        {
            int localX = cell.x - bounds.xMin;
            int localY = cell.y - bounds.yMin;
            return localY * bounds.width + localX;
        }

        private static void EnsureStrategyIdentity(
            string strategyId,
            string version,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(strategyId) ||
                string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException(
                    "A decoration expansion strategy requires a non-empty " +
                    "ID and version.",
                    parameterName);
            }
        }
    }
}
