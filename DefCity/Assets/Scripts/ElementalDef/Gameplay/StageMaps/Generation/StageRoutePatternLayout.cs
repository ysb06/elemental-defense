using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageRoutePatternLayout
    {
        private readonly IReadOnlyList<StageRoutePatternPlacement> placements;
        private readonly IReadOnlyList<StageRoutePatternPassage> orderedPassages;

        public string LayoutId { get; }
        public IReadOnlyList<StageRoutePatternPlacement> Placements => placements;
        public IReadOnlyList<StageRoutePatternPassage> OrderedPassages => orderedPassages;

        public StageRoutePatternLayout(
            string layoutId,
            IReadOnlyList<StageRoutePatternPlacement> sourcePlacements,
            IReadOnlyList<StageRoutePatternPassage> sourceOrderedPassages)
        {
            if (string.IsNullOrWhiteSpace(layoutId))
            {
                throw new ArgumentException("A pattern layout ID is required.", nameof(layoutId));
            }

            if (sourcePlacements == null)
            {
                throw new ArgumentNullException(nameof(sourcePlacements));
            }

            if (sourcePlacements.Count == 0)
            {
                throw new ArgumentException(
                    "A pattern layout requires at least one placement.",
                    nameof(sourcePlacements));
            }

            if (sourceOrderedPassages == null)
            {
                throw new ArgumentNullException(nameof(sourceOrderedPassages));
            }

            StageRoutePatternPlacement[] placementCopies =
                new StageRoutePatternPlacement[sourcePlacements.Count];
            Dictionary<string, StageRoutePatternPassage> passagesById =
                new(StringComparer.Ordinal);
            HashSet<string> placementIds = new(StringComparer.Ordinal);
            HashSet<StageRoutePatternSlot> occupiedSlots = new();
            HashSet<Vector2Int> occupiedRoadCells = new();

            for (int placementIndex = 0;
                 placementIndex < sourcePlacements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = sourcePlacements[placementIndex]
                    ?? throw new ArgumentException(
                        $"Pattern placement index {placementIndex} is null.",
                        nameof(sourcePlacements));

                if (!placementIds.Add(placement.Id))
                {
                    throw new ArgumentException(
                        $"Pattern placement ID '{placement.Id}' is duplicated.",
                        nameof(sourcePlacements));
                }

                if (!occupiedSlots.Add(placement.Slot))
                {
                    throw new ArgumentException(
                        $"Pattern slot {placement.Slot} contains more than one placement.",
                        nameof(sourcePlacements));
                }

                for (int cellIndex = 0; cellIndex < placement.RoadCells.Count; cellIndex++)
                {
                    Vector2Int roadCell = placement.RoadCells[cellIndex];
                    if (!occupiedRoadCells.Add(roadCell))
                    {
                        throw new ArgumentException(
                            $"Physical pattern placements overlap at {roadCell}.",
                            nameof(sourcePlacements));
                    }
                }

                for (int passageIndex = 0;
                     passageIndex < placement.Passages.Count;
                     passageIndex++)
                {
                    StageRoutePatternPassage passage = placement.Passages[passageIndex];
                    if (!passagesById.TryAdd(passage.PassageId, passage))
                    {
                        throw new ArgumentException(
                            $"Pattern passage ID '{passage.PassageId}' is duplicated.",
                            nameof(sourcePlacements));
                    }
                }

                placementCopies[placementIndex] = placement;
            }

            if (sourceOrderedPassages.Count != passagesById.Count)
            {
                throw new ArgumentException(
                    "The logical passage order must contain every physical passage once.",
                    nameof(sourceOrderedPassages));
            }

            StageRoutePatternPassage[] orderedCopies =
                new StageRoutePatternPassage[sourceOrderedPassages.Count];
            HashSet<string> orderedIds = new(StringComparer.Ordinal);
            for (int index = 0; index < sourceOrderedPassages.Count; index++)
            {
                StageRoutePatternPassage passage = sourceOrderedPassages[index]
                    ?? throw new ArgumentException(
                        $"Ordered passage index {index} is null.",
                        nameof(sourceOrderedPassages));

                if (!orderedIds.Add(passage.PassageId) ||
                    !passagesById.TryGetValue(
                        passage.PassageId,
                        out StageRoutePatternPassage canonicalPassage))
                {
                    throw new ArgumentException(
                        $"Ordered passage '{passage.PassageId}' is duplicated or unknown.",
                        nameof(sourceOrderedPassages));
                }

                orderedCopies[index] = canonicalPassage;
            }

            LayoutId = layoutId;
            placements = Array.AsReadOnly(placementCopies);
            orderedPassages = Array.AsReadOnly(orderedCopies);
        }
    }
}
