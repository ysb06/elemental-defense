using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public sealed class GeneratedStageDecoration
    {
        private readonly StageDecorationCellKind[] kinds;
        private readonly ElementType[] elements;
        private readonly bool[] assignedCells;

        public int Seed { get; }
        public string GeneratorVersion { get; }
        public Vector2Int CenterCell { get; }
        public int Radius { get; }
        public StageDecorationBoundaryShape BoundaryShape { get; }
        public RectInt Bounds { get; }
        public int CellCount { get; }
        public int ElementalGroundCellCount { get; }
        public int BoundaryWallCellCount { get; }
        public int BoundaryWallThickness => 1;

        internal GeneratedStageDecoration(
            int seed,
            string generatorVersion,
            Vector2Int centerCell,
            int radius,
            StageDecorationBoundaryShape boundaryShape,
            RectInt bounds,
            IReadOnlyList<StageDecorationCellEntry> sourceCells)
        {
            if (string.IsNullOrWhiteSpace(generatorVersion))
            {
                throw new ArgumentException(
                    "A decoration generator version is required.",
                    nameof(generatorVersion));
            }

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(radius),
                    radius,
                    "The decoration radius cannot be negative.");
            }

            if (!Enum.IsDefined(
                    typeof(StageDecorationBoundaryShape),
                    boundaryShape))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(boundaryShape),
                    boundaryShape,
                    "The decoration boundary shape must be defined.");
            }

            if (boundaryShape ==
                    StageDecorationBoundaryShape.MapBoundsRectangle &&
                radius != 0)
            {
                throw new ArgumentException(
                    "A rectangular map-bounds decoration must use radius zero.",
                    nameof(radius));
            }

            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bounds),
                    bounds,
                    "Decoration bounds must have a positive width and height.");
            }

            if (sourceCells == null)
            {
                throw new ArgumentNullException(nameof(sourceCells));
            }

            int storageLength = checked(bounds.width * bounds.height);
            kinds = new StageDecorationCellKind[storageLength];
            elements = new ElementType[storageLength];
            assignedCells = new bool[storageLength];

            StageDecorationCellEntry[] orderedCells =
                new StageDecorationCellEntry[sourceCells.Count];
            for (int index = 0; index < sourceCells.Count; index++)
            {
                orderedCells[index] = sourceCells[index];
            }

            Array.Sort(orderedCells, CompareCellsRowMajor);

            int elementalGroundCellCount = 0;
            int boundaryWallCellCount = 0;
            for (int index = 0; index < orderedCells.Length; index++)
            {
                StageDecorationCellEntry entry = orderedCells[index];
                if (!StageDecorationCellEntry.IsValid(
                        entry.Kind,
                        entry.Element))
                {
                    throw new ArgumentException(
                        $"Decoration cell {entry.Coordinates} uses invalid " +
                        $"kind/element pair {entry.Kind}/{entry.Element}.",
                        nameof(sourceCells));
                }

                if (!bounds.Contains(entry.Coordinates))
                {
                    throw new ArgumentException(
                        $"Decoration cell {entry.Coordinates} is outside " +
                        $"the declared bounds {bounds}.",
                        nameof(sourceCells));
                }

                int cellIndex = GetCellIndex(bounds, entry.Coordinates);
                if (assignedCells[cellIndex])
                {
                    throw new ArgumentException(
                        $"Decoration cell {entry.Coordinates} is duplicated.",
                        nameof(sourceCells));
                }

                assignedCells[cellIndex] = true;
                kinds[cellIndex] = entry.Kind;
                elements[cellIndex] = entry.Element;

                if (entry.Kind == StageDecorationCellKind.ElementalGround)
                {
                    elementalGroundCellCount++;
                }
                else
                {
                    boundaryWallCellCount++;
                }
            }

            Seed = seed;
            GeneratorVersion = generatorVersion;
            CenterCell = centerCell;
            Radius = radius;
            BoundaryShape = boundaryShape;
            Bounds = bounds;
            CellCount = orderedCells.Length;
            ElementalGroundCellCount = elementalGroundCellCount;
            BoundaryWallCellCount = boundaryWallCellCount;
        }

        public bool Contains(Vector2Int cell)
        {
            return TryGetCell(cell, out _);
        }

        public bool IsBoundaryWallCell(Vector2Int cell)
        {
            return TryGetCell(cell, out StageDecorationCellEntry entry) &&
                   entry.Kind == StageDecorationCellKind.BoundaryWall;
        }

        public bool TryGetCell(
            Vector2Int cell,
            out StageDecorationCellEntry entry)
        {
            if (!Bounds.Contains(cell))
            {
                entry = default;
                return false;
            }

            int index = GetCellIndex(Bounds, cell);
            if (!assignedCells[index])
            {
                entry = default;
                return false;
            }

            entry = CreateEntry(
                cell,
                kinds[index],
                elements[index]);
            return true;
        }

        public bool TryGetElement(
            Vector2Int cell,
            out ElementType element)
        {
            if (!TryGetCell(cell, out StageDecorationCellEntry entry) ||
                entry.Kind != StageDecorationCellKind.ElementalGround)
            {
                element = default;
                return false;
            }

            element = entry.Element;
            return true;
        }

        public IEnumerable<StageDecorationCellEntry> EnumerateCells()
        {
            for (int localY = 0; localY < Bounds.height; localY++)
            {
                for (int localX = 0; localX < Bounds.width; localX++)
                {
                    int index = localY * Bounds.width + localX;
                    if (!assignedCells[index])
                    {
                        continue;
                    }

                    Vector2Int coordinates = new(
                        Bounds.xMin + localX,
                        Bounds.yMin + localY);
                    yield return CreateEntry(
                        coordinates,
                        kinds[index],
                        elements[index]);
                }
            }
        }

        private static StageDecorationCellEntry CreateEntry(
            Vector2Int coordinates,
            StageDecorationCellKind kind,
            ElementType element)
        {
            return kind == StageDecorationCellKind.BoundaryWall
                ? StageDecorationCellEntry.CreateBoundaryWall(coordinates)
                : new StageDecorationCellEntry(coordinates, element);
        }

        private static int CompareCellsRowMajor(
            StageDecorationCellEntry left,
            StageDecorationCellEntry right)
        {
            int yComparison = left.Coordinates.y.CompareTo(
                right.Coordinates.y);
            return yComparison != 0
                ? yComparison
                : left.Coordinates.x.CompareTo(right.Coordinates.x);
        }

        private static int GetCellIndex(
            RectInt bounds,
            Vector2Int coordinates)
        {
            int localX = coordinates.x - bounds.xMin;
            int localY = coordinates.y - bounds.yMin;
            return localY * bounds.width + localX;
        }
    }
}
