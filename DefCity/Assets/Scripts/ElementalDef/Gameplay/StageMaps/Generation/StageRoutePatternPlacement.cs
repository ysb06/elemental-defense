using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageRoutePatternPlacement
    {
        private readonly IReadOnlyList<StageRoutePatternPassage> passages;
        private readonly IReadOnlyList<Vector2Int> roadCells;

        public string Id { get; }
        public StageRoutePatternSlot Slot { get; }
        public StageRoutePatternKind Kind { get; }
        public Vector2Int AnchorCell { get; }
        public int QuarterTurnsClockwise { get; }
        public IReadOnlyList<StageRoutePatternPassage> Passages => passages;
        public IReadOnlyList<Vector2Int> RoadCells => roadCells;

        public StageRoutePatternPlacement(
            string id,
            StageRoutePatternSlot slot,
            StageRoutePatternKind kind,
            Vector2Int anchorCell,
            int quarterTurnsClockwise,
            IReadOnlyList<StageRoutePatternPassage> sourcePassages)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "A pattern placement ID is required.",
                    nameof(id));
            }

            EnsureDefinedEnum(slot, nameof(slot));
            EnsureDefinedEnum(kind, nameof(kind));

            if (quarterTurnsClockwise < 0 || quarterTurnsClockwise > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quarterTurnsClockwise),
                    quarterTurnsClockwise,
                    "Pattern rotation must be between zero and three quarter turns.");
            }

            if (sourcePassages == null)
            {
                throw new ArgumentNullException(nameof(sourcePassages));
            }

            int expectedPassageCount =
                kind == StageRoutePatternKind.DisconnectedCross ? 2 : 1;
            if (sourcePassages.Count != expectedPassageCount)
            {
                throw new ArgumentException(
                    $"Pattern kind {kind} requires {expectedPassageCount} passage(s).",
                    nameof(sourcePassages));
            }

            StageRoutePatternPassage[] passageCopies =
                new StageRoutePatternPassage[sourcePassages.Count];
            HashSet<string> passageIds = new(StringComparer.Ordinal);
            HashSet<Vector2Int> uniqueRoadCells = new();

            for (int sourceIndex = 0; sourceIndex < sourcePassages.Count; sourceIndex++)
            {
                StageRoutePatternPassage passage = sourcePassages[sourceIndex]
                    ?? throw new ArgumentException(
                        $"Pattern passage index {sourceIndex} is null.",
                        nameof(sourcePassages));

                if (!string.Equals(passage.PlacementId, id, StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        $"Passage '{passage.PassageId}' belongs to another placement.",
                        nameof(sourcePassages));
                }

                if (passage.Slot != slot)
                {
                    throw new ArgumentException(
                        $"Passage '{passage.PassageId}' uses another pattern slot.",
                        nameof(sourcePassages));
                }

                if (passage.PassageIndex >= sourcePassages.Count ||
                    passageCopies[passage.PassageIndex] != null)
                {
                    throw new ArgumentException(
                        "Passage indices must be contiguous and unique.",
                        nameof(sourcePassages));
                }

                if (!passageIds.Add(passage.PassageId))
                {
                    throw new ArgumentException(
                        $"Passage ID '{passage.PassageId}' is duplicated.",
                        nameof(sourcePassages));
                }

                bool containsAnchor = false;
                for (int cellIndex = 0; cellIndex < passage.Cells.Count; cellIndex++)
                {
                    Vector2Int cell = passage.Cells[cellIndex];
                    containsAnchor |= cell == anchorCell;
                    uniqueRoadCells.Add(cell);
                }

                if (!containsAnchor)
                {
                    throw new ArgumentException(
                        $"Passage '{passage.PassageId}' does not contain the pattern anchor.",
                        nameof(sourcePassages));
                }

                passageCopies[passage.PassageIndex] = passage;
            }

            ValidatePassageKinds(kind, passageCopies, nameof(sourcePassages));

            Vector2Int[] roadCellCopies = new Vector2Int[uniqueRoadCells.Count];
            uniqueRoadCells.CopyTo(roadCellCopies);
            Array.Sort(roadCellCopies, CompareCellsRowMajor);

            Id = id;
            Slot = slot;
            Kind = kind;
            AnchorCell = anchorCell;
            QuarterTurnsClockwise = quarterTurnsClockwise;
            passages = Array.AsReadOnly(passageCopies);
            roadCells = Array.AsReadOnly(roadCellCopies);
        }

        private static void ValidatePassageKinds(
            StageRoutePatternKind kind,
            IReadOnlyList<StageRoutePatternPassage> sourcePassages,
            string parameterName)
        {
            switch (kind)
            {
                case StageRoutePatternKind.Straight:
                    if (sourcePassages[0].Axis == StageRoutePassageAxis.Turn)
                    {
                        throw new ArgumentException(
                            "A straight pattern passage must use a straight axis.",
                            parameterName);
                    }

                    break;

                case StageRoutePatternKind.Corner:
                    if (sourcePassages[0].Axis != StageRoutePassageAxis.Turn)
                    {
                        throw new ArgumentException(
                            "A corner pattern requires a turn passage.",
                            parameterName);
                    }

                    break;

                case StageRoutePatternKind.DisconnectedCross:
                    if (sourcePassages[0].Axis != StageRoutePassageAxis.Horizontal ||
                        sourcePassages[1].Axis != StageRoutePassageAxis.Vertical)
                    {
                        throw new ArgumentException(
                            "A disconnected cross requires horizontal index 0 and vertical index 1.",
                            parameterName);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static int CompareCellsRowMajor(Vector2Int first, Vector2Int second)
        {
            int yComparison = first.y.CompareTo(second.y);
            return yComparison != 0 ? yComparison : first.x.CompareTo(second.x);
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
