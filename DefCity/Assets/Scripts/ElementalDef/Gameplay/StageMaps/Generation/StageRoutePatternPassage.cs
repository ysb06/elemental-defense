using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageRoutePatternPassage
    {
        private readonly IReadOnlyList<Vector2Int> cells;

        public string PlacementId { get; }
        public string PassageId { get; }
        public int PassageIndex { get; }
        public StageRoutePatternSlot Slot { get; }
        public StageRoutePassageAxis Axis { get; }
        public IReadOnlyList<Vector2Int> Cells => cells;
        public Vector2Int EntryCell => cells[0];
        public Vector2Int ExitCell => cells[cells.Count - 1];

        public StageRoutePatternPassage(
            string placementId,
            string passageId,
            int passageIndex,
            StageRoutePatternSlot slot,
            StageRoutePassageAxis axis,
            IReadOnlyList<Vector2Int> sourceCells)
        {
            if (string.IsNullOrWhiteSpace(placementId))
            {
                throw new ArgumentException(
                    "A pattern placement ID is required.",
                    nameof(placementId));
            }

            if (string.IsNullOrWhiteSpace(passageId))
            {
                throw new ArgumentException(
                    "A pattern passage ID is required.",
                    nameof(passageId));
            }

            if (passageIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(passageIndex),
                    passageIndex,
                    "A passage index cannot be negative.");
            }

            EnsureDefinedEnum(slot, nameof(slot));
            EnsureDefinedEnum(axis, nameof(axis));

            if (sourceCells == null)
            {
                throw new ArgumentNullException(nameof(sourceCells));
            }

            if (sourceCells.Count == 0)
            {
                throw new ArgumentException(
                    "A pattern passage requires at least one cell.",
                    nameof(sourceCells));
            }

            Vector2Int[] copies = new Vector2Int[sourceCells.Count];
            HashSet<Vector2Int> uniqueCells = new();
            for (int index = 0; index < sourceCells.Count; index++)
            {
                Vector2Int cell = sourceCells[index];
                if (!uniqueCells.Add(cell))
                {
                    throw new ArgumentException(
                        $"Pattern passage '{passageId}' repeats cell {cell}.",
                        nameof(sourceCells));
                }

                if (index > 0 && GetManhattanDistance(copies[index - 1], cell) != 1)
                {
                    throw new ArgumentException(
                        $"Pattern passage '{passageId}' contains non-adjacent cells.",
                        nameof(sourceCells));
                }

                copies[index] = cell;
            }

            ValidateAxis(axis, copies, nameof(sourceCells));

            PlacementId = placementId;
            PassageId = passageId;
            PassageIndex = passageIndex;
            Slot = slot;
            Axis = axis;
            cells = Array.AsReadOnly(copies);
        }

        private static void ValidateAxis(
            StageRoutePassageAxis axis,
            IReadOnlyList<Vector2Int> sourceCells,
            string parameterName)
        {
            bool usedHorizontal = false;
            bool usedVertical = false;

            for (int index = 1; index < sourceCells.Count; index++)
            {
                Vector2Int delta = sourceCells[index] - sourceCells[index - 1];
                usedHorizontal |= delta.x != 0;
                usedVertical |= delta.y != 0;
            }

            if (axis == StageRoutePassageAxis.Horizontal && usedVertical)
            {
                throw new ArgumentException(
                    "A horizontal passage cannot contain vertical steps.",
                    parameterName);
            }

            if (axis == StageRoutePassageAxis.Vertical && usedHorizontal)
            {
                throw new ArgumentException(
                    "A vertical passage cannot contain horizontal steps.",
                    parameterName);
            }

            if (axis == StageRoutePassageAxis.Turn &&
                (!usedHorizontal || !usedVertical))
            {
                throw new ArgumentException(
                    "A turn passage must contain horizontal and vertical steps.",
                    parameterName);
            }
        }

        private static int GetManhattanDistance(Vector2Int first, Vector2Int second)
        {
            return Math.Abs(first.x - second.x) + Math.Abs(first.y - second.y);
        }

        private static void EnsureDefinedEnum<TEnum>(TEnum value, string parameterName)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"{typeof(TEnum).Name} must be a defined value.");
            }
        }
    }
}
