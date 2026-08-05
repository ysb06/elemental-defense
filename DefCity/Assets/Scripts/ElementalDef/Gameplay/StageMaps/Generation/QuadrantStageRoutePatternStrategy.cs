using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    /// <summary>
    /// Places one compact 3x3 pattern in each selected quadrant/center slot.
    /// This strategy owns only physical pattern layouts and logical passage
    /// order; connector search and route graph construction remain separate.
    /// </summary>
    public sealed class QuadrantStageRoutePatternStrategy : IStageRoutePatternStrategy
    {
        public const string DefaultStrategyId = "quadrant-compact-patterns";
        public const string CurrentVersion = "5";

        private const int MaxPassageOrderDrawsPerPhysicalLayout = 64;
        private const int PreferredCompositionProbePhysicalLayoutCount = 4;
        private const int PreferredCompositionRandomStream =
            unchecked((int)0xC2B2AE35);
        private const int FallbackCompositionOrderDomain =
            unchecked((int)0x85EBCA77);

        private static readonly StageRoutePatternSlot[] AllSlots =
        {
            StageRoutePatternSlot.Quadrant1,
            StageRoutePatternSlot.Quadrant2,
            StageRoutePatternSlot.Quadrant3,
            StageRoutePatternSlot.Quadrant4,
            StageRoutePatternSlot.Center,
        };

        private static readonly Vector2Int[] HorizontalCells =
        {
            Vector2Int.left,
            Vector2Int.zero,
            Vector2Int.right,
        };

        private static readonly Vector2Int[] VerticalCells =
        {
            Vector2Int.down,
            Vector2Int.zero,
            Vector2Int.up,
        };

        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
        };

        private static readonly Vector2Int[][] CornerCells =
        {
            new[] { Vector2Int.up, Vector2Int.zero, Vector2Int.right },
            new[] { Vector2Int.right, Vector2Int.zero, Vector2Int.down },
            new[] { Vector2Int.down, Vector2Int.zero, Vector2Int.left },
            new[] { Vector2Int.left, Vector2Int.zero, Vector2Int.up },
        };

        public string StrategyId => DefaultStrategyId;
        public string Version => CurrentVersion;

        public StageRoutePatternCandidateSet CreateCandidates(
            StageRouteGenerationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            PhysicalPlacementCatalog placementCatalog = new(settings);
            List<PhysicalLayoutDraw> physicalLayoutPool =
                new(settings.MaxPhysicalLayoutDraws);
            List<PhysicalLayoutCandidate> physicalLayouts =
                new(settings.MaxPhysicalLayoutCount);
            HashSet<string> physicalSignatures = new(StringComparer.Ordinal);
            int physicalLayoutDrawCount = 0;
            int duplicatePhysicalLayoutCount = 0;
            int passageOrderDrawCount = 0;
            int duplicatePassageOrderCount = 0;
            int passageOrderVariantCount = 0;
            int layoutsWithoutValidOrderCount = 0;
            PatternComposition preferredComposition =
                CreatePreferredPatternComposition(settings);

            while (physicalLayoutDrawCount < settings.MaxPhysicalLayoutDraws)
            {
                int physicalDrawIndex = physicalLayoutDrawCount;
                physicalLayoutDrawCount++;
                StageRouteDeterministicRandom placementRandom =
                    new(settings.Seed, physicalDrawIndex);

                if (!TryCreatePlacements(
                        settings,
                        placementCatalog,
                        ref placementRandom,
                        out List<StageRoutePatternPlacement> placements))
                {
                    continue;
                }

                string physicalSignature =
                    CreatePhysicalLayoutSignature(placements);
                if (!physicalSignatures.Add(physicalSignature))
                {
                    duplicatePhysicalLayoutCount++;
                    continue;
                }

                if (!TryEvaluatePhysicalLayoutQuality(
                        settings,
                        placements,
                        out int qualityScore))
                {
                    continue;
                }

                physicalLayoutPool.Add(new PhysicalLayoutDraw(
                    physicalDrawIndex,
                    physicalSignature,
                    qualityScore,
                    CreatePatternCompositionKey(placements),
                    CreatePhysicalSelectionKey(
                        settings.Seed,
                        physicalSignature),
                    placements));
            }

            int feasibleUniquePhysicalLayoutCount = physicalLayoutPool.Count;
            physicalLayoutPool = CreatePreferredCompositionPhysicalLayoutOrder(
                settings.Seed,
                settings.MaxPhysicalLayoutCount,
                preferredComposition.Key,
                physicalLayoutPool);
            int preferredProbePhysicalLayoutCount = 0;
            for (int poolIndex = 0;
                 poolIndex < physicalLayoutPool.Count &&
                 physicalLayouts.Count < settings.MaxPhysicalLayoutCount;
                 poolIndex++)
            {
                PhysicalLayoutDraw physicalDraw = physicalLayoutPool[poolIndex];
                List<PassageOrderVariant> variants = CreatePassageOrderVariants(
                    settings,
                    physicalDraw.Placements,
                    physicalDraw.PhysicalSignature,
                    ref passageOrderDrawCount,
                    ref duplicatePassageOrderCount);
                passageOrderVariantCount += variants.Count;
                if (variants.Count == 0)
                {
                    layoutsWithoutValidOrderCount++;
                }

                int physicalLayoutIndex = physicalLayouts.Count;
                bool isPreferredCompositionProbe =
                    variants.Count > 0 &&
                    preferredProbePhysicalLayoutCount <
                    PreferredCompositionProbePhysicalLayoutCount &&
                    physicalDraw.PatternCompositionKey ==
                    preferredComposition.Key;
                if (isPreferredCompositionProbe)
                {
                    preferredProbePhysicalLayoutCount++;
                }

                physicalLayouts.Add(new PhysicalLayoutCandidate(
                    physicalLayoutIndex,
                    physicalDraw.PhysicalLayoutDrawIndex,
                    isPreferredCompositionProbe,
                    variants));
            }

            int physicalPlacementRejectedCount = checked(
                physicalLayoutDrawCount -
                duplicatePhysicalLayoutCount -
                feasibleUniquePhysicalLayoutCount);
            int unselectedPhysicalLayoutCount = checked(
                feasibleUniquePhysicalLayoutCount -
                physicalLayouts.Count);

            List<StageRoutePatternCandidateRecord> candidateRecords =
                CreateVariantMajorCandidateRecords(settings, physicalLayouts);
            if (candidateRecords.Count > 0)
            {
                return StageRoutePatternCandidateSet.CreateSuccess(
                    candidateRecords,
                    physicalLayoutDrawCount,
                    physicalPlacementRejectedCount,
                    duplicatePhysicalLayoutCount,
                    unselectedPhysicalLayoutCount,
                    physicalLayouts.Count,
                    passageOrderDrawCount,
                    duplicatePassageOrderCount,
                    passageOrderVariantCount,
                    layoutsWithoutValidOrderCount,
                    preferredComposition.StraightCount,
                    preferredComposition.CornerCount,
                    preferredComposition.CrossCount);
            }

            StageRoutePatternCandidateFailureReason failureReason =
                physicalLayoutPool.Count > 0
                    ? StageRoutePatternCandidateFailureReason.NoValidPassageOrder
                    : StageRoutePatternCandidateFailureReason.NoFeasiblePatternLayout;
            return StageRoutePatternCandidateSet.CreateFailure(
                failureReason,
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount,
                physicalLayouts.Count,
                passageOrderDrawCount,
                duplicatePassageOrderCount,
                layoutsWithoutValidOrderCount,
                preferredComposition.StraightCount,
                preferredComposition.CornerCount,
                preferredComposition.CrossCount);
        }

        private List<PassageOrderVariant> CreatePassageOrderVariants(
            StageRouteGenerationSettings settings,
            IReadOnlyList<StageRoutePatternPlacement> placements,
            string physicalSignature,
            ref int passageOrderDrawCount,
            ref int duplicatePassageOrderCount)
        {
            if (!TryCreatePassageOrderContext(
                    settings,
                    placements,
                    out PassageOrderContext context))
            {
                return new List<PassageOrderVariant>();
            }

            List<PassageOrderVariant> variants =
                new(settings.OrderVariantsPerPhysicalLayout);
            HashSet<string> variantSignatures = new(StringComparer.Ordinal);
            for (int orderDrawIndex = 0;
                 orderDrawIndex < MaxPassageOrderDrawsPerPhysicalLayout &&
                 variants.Count < settings.OrderVariantsPerPhysicalLayout;
                 orderDrawIndex++)
            {
                passageOrderDrawCount++;
                int stream = CreatePassageOrderStream(
                    physicalSignature,
                    orderDrawIndex);
                StageRouteDeterministicRandom orderRandom =
                    new(settings.Seed, stream);
                if (!TryCreatePassageOrder(
                        context,
                        placements,
                        ref orderRandom,
                        out List<StageRoutePatternPlacement> orientedPlacements,
                        out List<StageRoutePatternPassage> orderedPassages) ||
                    HasInvalidEndpointContact(
                        settings,
                        orientedPlacements,
                        orderedPassages))
                {
                    continue;
                }

                string signature = CreateLayoutSignature(
                    orientedPlacements,
                    orderedPassages);
                if (!variantSignatures.Add(signature))
                {
                    duplicatePassageOrderCount++;
                    continue;
                }

                variants.Add(new PassageOrderVariant(
                    orderDrawIndex,
                    signature,
                    orientedPlacements,
                    orderedPassages));
            }

            return variants;
        }

        private List<StageRoutePatternCandidateRecord>
            CreateVariantMajorCandidateRecords(
                StageRouteGenerationSettings settings,
                IReadOnlyList<PhysicalLayoutCandidate> physicalLayouts)
        {
            List<StageRoutePatternCandidateRecord> records =
                new(settings.MaxRouteCandidateCount);
            for (int variantIndex = 0;
                 variantIndex < settings.OrderVariantsPerPhysicalLayout &&
                 records.Count < settings.MaxRouteCandidateCount;
                 variantIndex++)
            {
                for (int preferencePass = 0;
                     preferencePass < 2 &&
                     records.Count < settings.MaxRouteCandidateCount;
                     preferencePass++)
                {
                    bool requirePreferredProbe = preferencePass == 0;
                    for (int physicalIndex = 0;
                         physicalIndex < physicalLayouts.Count &&
                         records.Count < settings.MaxRouteCandidateCount;
                         physicalIndex++)
                    {
                        PhysicalLayoutCandidate physical =
                            physicalLayouts[physicalIndex];
                        if (physical.IsPreferredCompositionProbe !=
                                requirePreferredProbe ||
                            variantIndex >= physical.Variants.Count)
                        {
                            continue;
                        }

                        PassageOrderVariant variant =
                            physical.Variants[variantIndex];
                        ulong stableHash =
                            StageRouteStableHash.Fnv1A64(variant.Signature);
                        string layoutId = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}:{1}:P{2:D2}:V{3:D2}:{4:X16}",
                            StrategyId,
                            Version,
                            physical.PhysicalLayoutIndex,
                            variantIndex,
                            stableHash);
                        StageRoutePatternLayout layout = new(
                            layoutId,
                            variant.Placements,
                            variant.OrderedPassages);
                        records.Add(new StageRoutePatternCandidateRecord(
                            layout,
                            physical.PhysicalLayoutIndex,
                            physical.PhysicalLayoutDrawIndex,
                            variantIndex,
                            variant.PassageOrderDrawIndex));
                    }
                }
            }

            return records;
        }

        /// <summary>
        /// Returns the anchor area for a slot. A pattern belongs to a slot by
        /// its anchor only; its other road cells may cross a slot boundary.
        /// </summary>
        public static RectInt GetAnchorArea(
            RectInt bounds,
            StageRoutePatternSlot slot,
            int centerBandRadius)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds));
            }

            if (!Enum.IsDefined(typeof(StageRoutePatternSlot), slot))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }

            if (centerBandRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(centerBandRadius));
            }

            int splitX = bounds.xMin + bounds.width / 2;
            int splitY = bounds.yMin + bounds.height / 2;

            switch (slot)
            {
                case StageRoutePatternSlot.Quadrant1:
                    return CreateRectFromMinMax(
                        bounds.xMin,
                        bounds.yMin,
                        splitX,
                        splitY);

                case StageRoutePatternSlot.Quadrant2:
                    return CreateRectFromMinMax(
                        splitX,
                        bounds.yMin,
                        bounds.xMax,
                        splitY);

                case StageRoutePatternSlot.Quadrant3:
                    return CreateRectFromMinMax(
                        splitX,
                        splitY,
                        bounds.xMax,
                        bounds.yMax);

                case StageRoutePatternSlot.Quadrant4:
                    return CreateRectFromMinMax(
                        bounds.xMin,
                        splitY,
                        splitX,
                        bounds.yMax);

                case StageRoutePatternSlot.Center:
                    int coreXMin = bounds.xMin + (bounds.width - 1) / 2;
                    int coreXMaxInclusive = bounds.xMin + bounds.width / 2;
                    int coreYMin = bounds.yMin + (bounds.height - 1) / 2;
                    int coreYMaxInclusive = bounds.yMin + bounds.height / 2;

                    int centerXMin = ClampToRange(
                        (long)coreXMin - centerBandRadius,
                        bounds.xMin,
                        bounds.xMax - 1);
                    int centerXMaxInclusive = ClampToRange(
                        (long)coreXMaxInclusive + centerBandRadius,
                        bounds.xMin,
                        bounds.xMax - 1);
                    int centerYMin = ClampToRange(
                        (long)coreYMin - centerBandRadius,
                        bounds.yMin,
                        bounds.yMax - 1);
                    int centerYMaxInclusive = ClampToRange(
                        (long)coreYMaxInclusive + centerBandRadius,
                        bounds.yMin,
                        bounds.yMax - 1);

                    return CreateRectFromMinMax(
                        centerXMin,
                        centerYMin,
                        centerXMaxInclusive + 1,
                        centerYMaxInclusive + 1);

                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        public static bool CanConnectSlots(
            StageRoutePatternSlot first,
            StageRoutePatternSlot second)
        {
            if (!Enum.IsDefined(typeof(StageRoutePatternSlot), first))
            {
                throw new ArgumentOutOfRangeException(nameof(first), first, null);
            }

            if (!Enum.IsDefined(typeof(StageRoutePatternSlot), second))
            {
                throw new ArgumentOutOfRangeException(nameof(second), second, null);
            }

            return !((first == StageRoutePatternSlot.Quadrant1 &&
                       second == StageRoutePatternSlot.Quadrant3) ||
                      (first == StageRoutePatternSlot.Quadrant3 &&
                       second == StageRoutePatternSlot.Quadrant1) ||
                      (first == StageRoutePatternSlot.Quadrant2 &&
                       second == StageRoutePatternSlot.Quadrant4) ||
                      (first == StageRoutePatternSlot.Quadrant4 &&
                       second == StageRoutePatternSlot.Quadrant2));
        }

        private static bool TryEvaluatePhysicalLayoutQuality(
            StageRouteGenerationSettings settings,
            IReadOnlyList<StageRoutePatternPlacement> placements,
            out int qualityScore)
        {
            HashSet<Vector2Int> reservedRoadCells = new();
            HashSet<Vector2Int> passageEndpointCells = new();
            List<Vector2Int> passageEndpoints = new();
            for (int placementIndex = 0;
                 placementIndex < placements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = placements[placementIndex];
                AddCells(placement.RoadCells, reservedRoadCells);
                for (int passageIndex = 0;
                     passageIndex < placement.Passages.Count;
                     passageIndex++)
                {
                    StageRoutePatternPassage passage =
                        placement.Passages[passageIndex];
                    passageEndpointCells.Add(passage.EntryCell);
                    passageEndpointCells.Add(passage.ExitCell);
                    passageEndpoints.Add(passage.EntryCell);
                    passageEndpoints.Add(passage.ExitCell);
                }
            }

            if (!HasUsableEndpointContact(
                    placements,
                    settings.SpawnCell,
                    passageEndpointCells) ||
                !HasUsableEndpointContact(
                    placements,
                    settings.RouteGoalCell,
                    passageEndpointCells))
            {
                qualityScore = 0;
                return false;
            }

            Dictionary<Vector2Int, int> residualComponents = new();
            List<int> componentSizes = new() { 0 };
            Queue<Vector2Int> queue = new();
            for (int y = settings.Bounds.yMin; y < settings.Bounds.yMax; y++)
            {
                for (int x = settings.Bounds.xMin;
                     x < settings.Bounds.xMax;
                     x++)
                {
                    Vector2Int seed = new(x, y);
                    if (residualComponents.ContainsKey(seed) ||
                        !IsPhysicalResidualCell(
                            settings,
                            seed,
                            reservedRoadCells,
                            passageEndpointCells))
                    {
                        continue;
                    }

                    int componentId = componentSizes.Count;
                    int componentSize = 0;
                    residualComponents.Add(seed, componentId);
                    queue.Enqueue(seed);
                    while (queue.Count > 0)
                    {
                        Vector2Int cell = queue.Dequeue();
                        componentSize++;
                        for (int directionIndex = 0;
                             directionIndex < CardinalDirections.Length;
                             directionIndex++)
                        {
                            Vector2Int neighbor =
                                cell + CardinalDirections[directionIndex];
                            if (residualComponents.ContainsKey(neighbor) ||
                                !IsPhysicalResidualCell(
                                    settings,
                                    neighbor,
                                    reservedRoadCells,
                                    passageEndpointCells))
                            {
                                continue;
                            }

                            residualComponents.Add(neighbor, componentId);
                            queue.Enqueue(neighbor);
                        }
                    }

                    componentSizes.Add(componentSize);
                }
            }

            int largestComponentId = 0;
            for (int componentId = 1;
                 componentId < componentSizes.Count;
                 componentId++)
            {
                if (largestComponentId == 0 ||
                    componentSizes[componentId] >
                    componentSizes[largestComponentId])
                {
                    largestComponentId = componentId;
                }
            }

            int minimumPortCount = int.MaxValue;
            int totalPortCount = 0;
            int endpointsReachingLargestComponent = 0;
            for (int endpointIndex = 0;
                 endpointIndex < passageEndpoints.Count;
                 endpointIndex++)
            {
                Vector2Int endpoint = passageEndpoints[endpointIndex];
                int portCount = 0;
                bool reachesLargestComponent = false;
                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    Vector2Int port =
                        endpoint + CardinalDirections[directionIndex];
                    if (!residualComponents.TryGetValue(
                            port,
                            out int componentId) ||
                        !IsDedicatedEndpointPort(
                            port,
                            endpoint,
                            reservedRoadCells))
                    {
                        continue;
                    }

                    portCount++;
                    reachesLargestComponent |=
                        componentId == largestComponentId;
                }

                if (portCount == 0)
                {
                    qualityScore = 0;
                    return false;
                }

                minimumPortCount = Math.Min(minimumPortCount, portCount);
                totalPortCount += portCount;
                if (reachesLargestComponent)
                {
                    endpointsReachingLargestComponent++;
                }
            }

            int largestComponentSize = largestComponentId == 0
                ? 0
                : componentSizes[largestComponentId];
            qualityScore = checked(
                minimumPortCount * 1_000_000 +
                endpointsReachingLargestComponent * 10_000 +
                largestComponentSize * 100 +
                totalPortCount);
            return true;
        }

        private static bool HasUsableEndpointContact(
            IReadOnlyList<StageRoutePatternPlacement> placements,
            Vector2Int endpoint,
            HashSet<Vector2Int> passageEndpointCells)
        {
            return TryGetSingleEndpointContact(
                       placements,
                       endpoint,
                       out Vector2Int? touchingRoadCell) &&
                   (!touchingRoadCell.HasValue ||
                    passageEndpointCells.Contains(touchingRoadCell.Value));
        }

        private static bool IsPhysicalResidualCell(
            StageRouteGenerationSettings settings,
            Vector2Int cell,
            HashSet<Vector2Int> reservedRoadCells,
            HashSet<Vector2Int> passageEndpointCells)
        {
            if (!settings.Bounds.Contains(cell) ||
                settings.HeadquartersFootprint.Contains(cell) ||
                reservedRoadCells.Contains(cell))
            {
                return false;
            }

            Vector2Int touchingRoadCell = default;
            int touchingRoadCellCount = 0;
            for (int directionIndex = 0;
                 directionIndex < CardinalDirections.Length;
                 directionIndex++)
            {
                Vector2Int neighbor =
                    cell + CardinalDirections[directionIndex];
                if (!reservedRoadCells.Contains(neighbor))
                {
                    continue;
                }

                touchingRoadCell = neighbor;
                touchingRoadCellCount++;
            }

            return touchingRoadCellCount == 0 ||
                (touchingRoadCellCount == 1 &&
                 passageEndpointCells.Contains(touchingRoadCell));
        }

        private static bool IsDedicatedEndpointPort(
            Vector2Int port,
            Vector2Int endpoint,
            HashSet<Vector2Int> reservedRoadCells)
        {
            for (int directionIndex = 0;
                 directionIndex < CardinalDirections.Length;
                 directionIndex++)
            {
                Vector2Int touchingRoadCell =
                    port + CardinalDirections[directionIndex];
                if (reservedRoadCells.Contains(touchingRoadCell) &&
                    touchingRoadCell != endpoint)
                {
                    return false;
                }
            }

            return true;
        }

        private static ulong CreatePhysicalSelectionKey(
            int seed,
            string physicalSignature)
        {
            StringBuilder builder = new(physicalSignature.Length + 32);
            builder.AppendInvariant(seed)
                .Append("#physical-quality:")
                .Append(physicalSignature);
            return StageRouteStableHash.Fnv1A64(builder.ToString());
        }

        private static int CreatePatternCompositionKey(
            IReadOnlyList<StageRoutePatternPlacement> placements)
        {
            int straightCount = 0;
            int cornerCount = 0;
            int crossCount = 0;
            for (int index = 0; index < placements.Count; index++)
            {
                switch (placements[index].Kind)
                {
                    case StageRoutePatternKind.Straight:
                        straightCount++;
                        break;
                    case StageRoutePatternKind.Corner:
                        cornerCount++;
                        break;
                    case StageRoutePatternKind.DisconnectedCross:
                        crossCount++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(placements));
                }
            }

            return checked(straightCount + cornerCount * 8 + crossCount * 64);
        }

        private static PatternComposition CreatePreferredPatternComposition(
            StageRouteGenerationSettings settings)
        {
            StageRoutePatternKind[] allowedKinds =
                GetAllowedKinds(settings.AllowedPatternKinds);
            StageRouteDeterministicRandom random = new(
                settings.Seed,
                PreferredCompositionRandomStream);
            int straightCount = 0;
            int cornerCount = 0;
            int crossCount = 0;
            for (int index = 0; index < settings.PatternCount; index++)
            {
                switch (allowedKinds[random.NextInt(allowedKinds.Length)])
                {
                    case StageRoutePatternKind.Straight:
                        straightCount++;
                        break;
                    case StageRoutePatternKind.Corner:
                        cornerCount++;
                        break;
                    case StageRoutePatternKind.DisconnectedCross:
                        crossCount++;
                        break;
                    default:
                        throw new InvalidOperationException(
                            "The preferred composition selected an unknown pattern kind.");
                }
            }

            return new PatternComposition(
                straightCount,
                cornerCount,
                crossCount);
        }

        private static List<PhysicalLayoutDraw>
            CreatePreferredCompositionPhysicalLayoutOrder(
                int seed,
                int maximumPhysicalLayoutCount,
                int preferredCompositionKey,
                IReadOnlyList<PhysicalLayoutDraw> physicalLayoutPool)
        {
            // Pattern kinds are drawn with equal probability into one preferred
            // composition. Probe several distinct physical layouts of that
            // composition before using the composition-balanced fallback pool.
            List<PhysicalLayoutDraw> preferredDraws = new();
            List<PhysicalLayoutDraw> fallbackDraws = new();
            for (int index = 0; index < physicalLayoutPool.Count; index++)
            {
                PhysicalLayoutDraw draw = physicalLayoutPool[index];
                if (draw.PatternCompositionKey == preferredCompositionKey)
                {
                    preferredDraws.Add(draw);
                }
                else
                {
                    fallbackDraws.Add(draw);
                }
            }

            preferredDraws.Sort(ComparePhysicalLayoutDraws);
            int preferredProbeCount = Math.Min(
                Math.Min(
                    PreferredCompositionProbePhysicalLayoutCount,
                    maximumPhysicalLayoutCount),
                preferredDraws.Count);
            List<PhysicalLayoutDraw> selected = new(
                Math.Min(maximumPhysicalLayoutCount, physicalLayoutPool.Count));
            for (int index = 0; index < preferredProbeCount; index++)
            {
                selected.Add(preferredDraws[index]);
            }

            List<PhysicalLayoutDraw> remaining = new(
                physicalLayoutPool.Count - preferredProbeCount);
            for (int index = preferredProbeCount;
                 index < preferredDraws.Count;
                 index++)
            {
                remaining.Add(preferredDraws[index]);
            }

            remaining.AddRange(fallbackDraws);
            List<PhysicalLayoutDraw> orderedFallback =
                CreateCompositionBalancedOrder(
                    unchecked(seed ^ FallbackCompositionOrderDomain),
                    remaining);
            for (int index = 0;
                 index < orderedFallback.Count &&
                 selected.Count < maximumPhysicalLayoutCount;
                 index++)
            {
                selected.Add(orderedFallback[index]);
            }

            return selected;
        }

        private static List<PhysicalLayoutDraw>
            CreateCompositionBalancedOrder(
                int seed,
                IReadOnlyList<PhysicalLayoutDraw> physicalLayoutPool)
        {
            Dictionary<int, List<PhysicalLayoutDraw>> drawsByComposition = new();
            for (int index = 0; index < physicalLayoutPool.Count; index++)
            {
                PhysicalLayoutDraw draw = physicalLayoutPool[index];
                if (!drawsByComposition.TryGetValue(
                        draw.PatternCompositionKey,
                        out List<PhysicalLayoutDraw> compositionDraws))
                {
                    compositionDraws = new List<PhysicalLayoutDraw>();
                    drawsByComposition.Add(
                        draw.PatternCompositionKey,
                        compositionDraws);
                }

                compositionDraws.Add(draw);
            }

            List<int> compositionKeys = new(drawsByComposition.Keys);
            compositionKeys.Sort((first, second) =>
            {
                ulong firstKey = CreateCompositionSelectionKey(seed, first);
                ulong secondKey = CreateCompositionSelectionKey(seed, second);
                int comparison = firstKey.CompareTo(secondKey);
                return comparison != 0
                    ? comparison
                    : first.CompareTo(second);
            });
            int maximumCompositionSize = 0;
            for (int index = 0; index < compositionKeys.Count; index++)
            {
                List<PhysicalLayoutDraw> compositionDraws =
                    drawsByComposition[compositionKeys[index]];
                compositionDraws.Sort(ComparePhysicalLayoutDraws);
                maximumCompositionSize = Math.Max(
                    maximumCompositionSize,
                    compositionDraws.Count);
            }

            List<PhysicalLayoutDraw> ordered = new(physicalLayoutPool.Count);
            for (int rank = 0; rank < maximumCompositionSize; rank++)
            {
                for (int compositionIndex = 0;
                     compositionIndex < compositionKeys.Count;
                     compositionIndex++)
                {
                    List<PhysicalLayoutDraw> compositionDraws =
                        drawsByComposition[compositionKeys[compositionIndex]];
                    if (rank < compositionDraws.Count)
                    {
                        ordered.Add(compositionDraws[rank]);
                    }
                }
            }

            return ordered;
        }

        private static ulong CreateCompositionSelectionKey(
            int seed,
            int compositionKey)
        {
            StringBuilder builder = new(48);
            builder.AppendInvariant(seed)
                .Append("#pattern-composition:")
                .AppendInvariant(compositionKey);
            return StageRouteStableHash.Fnv1A64(builder.ToString());
        }

        private static int ComparePhysicalLayoutDraws(
            PhysicalLayoutDraw first,
            PhysicalLayoutDraw second)
        {
            int qualityComparison = second.QualityScore.CompareTo(
                first.QualityScore);
            if (qualityComparison != 0)
            {
                return qualityComparison;
            }

            int selectionComparison = first.SelectionKey.CompareTo(
                second.SelectionKey);
            return selectionComparison != 0
                ? selectionComparison
                : first.PhysicalLayoutDrawIndex.CompareTo(
                    second.PhysicalLayoutDrawIndex);
        }

        private static bool TryCreatePlacements(
            StageRouteGenerationSettings settings,
            PhysicalPlacementCatalog placementCatalog,
            ref StageRouteDeterministicRandom random,
            out List<StageRoutePatternPlacement> placements)
        {
            List<StageRoutePatternSlot> shuffledSlots = new(AllSlots);
            random.Shuffle(shuffledSlots);

            List<PlacementRequest> requests = new(settings.PatternCount);
            StageRoutePatternKind[] allowedKinds =
                GetAllowedKinds(settings.AllowedPatternKinds);
            for (int index = 0; index < settings.PatternCount; index++)
            {
                StageRoutePatternKind kind =
                    allowedKinds[random.NextInt(allowedKinds.Length)];
                IReadOnlyList<StageRoutePatternPlacement> catalogPlacements =
                    placementCatalog.GetPlacements(shuffledSlots[index], kind);
                List<StageRoutePatternPlacement> shuffledPlacements =
                    new(catalogPlacements);
                random.Shuffle(shuffledPlacements);
                requests.Add(new PlacementRequest(
                    shuffledSlots[index],
                    kind,
                    index,
                    shuffledPlacements));
            }

            requests.Sort(ComparePlacementRequests);

            placements = new List<StageRoutePatternPlacement>(settings.PatternCount);
            HashSet<Vector2Int> occupiedRoadCells = new();
            if (!TryPlaceRequest(
                    requests,
                    0,
                    placements,
                    occupiedRoadCells))
            {
                placements = null;
                return false;
            }

            placements.Sort((first, second) => first.Slot.CompareTo(second.Slot));
            return true;
        }

        private static bool TryPlaceRequest(
            IReadOnlyList<PlacementRequest> requests,
            int requestIndex,
            List<StageRoutePatternPlacement> placements,
            HashSet<Vector2Int> occupiedRoadCells)
        {
            if (requestIndex == requests.Count)
            {
                return true;
            }

            PlacementRequest request = requests[requestIndex];
            for (int candidateIndex = 0;
                 candidateIndex < request.Placements.Count;
                 candidateIndex++)
            {
                StageRoutePatternPlacement placement =
                    request.Placements[candidateIndex];

                if (OverlapsOrTouchesOtherPlacement(
                        placement.RoadCells,
                        occupiedRoadCells))
                {
                    continue;
                }

                AddCells(placement.RoadCells, occupiedRoadCells);
                placements.Add(placement);

                if (RemainingRequestsHavePlacement(
                        requests,
                        requestIndex + 1,
                        occupiedRoadCells) &&
                    TryPlaceRequest(
                        requests,
                        requestIndex + 1,
                        placements,
                        occupiedRoadCells))
                {
                    return true;
                }

                placements.RemoveAt(placements.Count - 1);
                RemoveCells(placement.RoadCells, occupiedRoadCells);
            }

            return false;
        }

        private static bool RemainingRequestsHavePlacement(
            IReadOnlyList<PlacementRequest> requests,
            int firstRequestIndex,
            HashSet<Vector2Int> occupiedRoadCells)
        {
            for (int requestIndex = firstRequestIndex;
                 requestIndex < requests.Count;
                 requestIndex++)
            {
                IReadOnlyList<StageRoutePatternPlacement> candidates =
                    requests[requestIndex].Placements;
                bool foundPlacement = false;
                for (int candidateIndex = 0;
                     candidateIndex < candidates.Count;
                     candidateIndex++)
                {
                    if (!OverlapsOrTouchesOtherPlacement(
                            candidates[candidateIndex].RoadCells,
                            occupiedRoadCells))
                    {
                        foundPlacement = true;
                        break;
                    }
                }

                if (!foundPlacement)
                {
                    return false;
                }
            }

            return true;
        }

        private static int ComparePlacementRequests(
            PlacementRequest first,
            PlacementRequest second)
        {
            int countComparison = first.Placements.Count.CompareTo(
                second.Placements.Count);
            return countComparison != 0
                ? countComparison
                : first.RandomOrderIndex.CompareTo(second.RandomOrderIndex);
        }

        private static List<StageRoutePatternPlacement>
            CreatePlacementCandidates(
            StageRouteGenerationSettings settings,
            StageRoutePatternSlot slot,
            StageRoutePatternKind kind)
        {
            RectInt anchorArea = GetAnchorArea(
                settings.Bounds,
                slot,
                settings.CenterBandRadius);
            int orientationCount = GetOrientationCount(kind);
            List<StageRoutePatternPlacement> candidates = new();

            for (int y = anchorArea.yMin; y < anchorArea.yMax; y++)
            {
                for (int x = anchorArea.xMin; x < anchorArea.xMax; x++)
                {
                    Vector2Int anchor = new(x, y);
                    for (int quarterTurns = 0;
                         quarterTurns < orientationCount;
                         quarterTurns++)
                    {
                        if (CanPlacePattern(
                                settings,
                                kind,
                                anchor,
                                quarterTurns))
                        {
                            candidates.Add(CreatePlacement(
                                slot,
                                kind,
                                anchor,
                                quarterTurns));
                        }
                    }
                }
            }

            return candidates;
        }

        private static bool CanPlacePattern(
            StageRouteGenerationSettings settings,
            StageRoutePatternKind kind,
            Vector2Int anchor,
            int quarterTurnsClockwise)
        {
            Vector2Int[][] passageCells = CreatePassageCells(
                kind,
                anchor,
                quarterTurnsClockwise);
            for (int passageIndex = 0;
                 passageIndex < passageCells.Length;
                 passageIndex++)
            {
                Vector2Int[] cells = passageCells[passageIndex];
                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    Vector2Int cell = cells[cellIndex];
                    if (!settings.Bounds.Contains(cell) ||
                        cell == settings.SpawnCell ||
                        cell == settings.RouteGoalCell ||
                        settings.HeadquartersFootprint.Contains(cell))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static StageRoutePatternPlacement CreatePlacement(
            StageRoutePatternSlot slot,
            StageRoutePatternKind kind,
            Vector2Int anchor,
            int quarterTurnsClockwise)
        {
            string placementId = GetPlacementId(slot);
            Vector2Int[][] passageCells = CreatePassageCells(
                kind,
                anchor,
                quarterTurnsClockwise);
            StageRoutePatternPassage[] passages =
                new StageRoutePatternPassage[passageCells.Length];

            for (int index = 0; index < passageCells.Length; index++)
            {
                Vector2Int[] cells = passageCells[index];
                StageRoutePassageAxis axis = GetPassageAxis(
                    kind,
                    quarterTurnsClockwise,
                    index);
                string passageId = kind == StageRoutePatternKind.DisconnectedCross
                    ? $"{placementId}:{(index == 0 ? "H" : "V")}"
                    : $"{placementId}:0";
                passages[index] = new StageRoutePatternPassage(
                    placementId,
                    passageId,
                    index,
                    slot,
                    axis,
                    cells);
            }

            return new StageRoutePatternPlacement(
                placementId,
                slot,
                kind,
                anchor,
                quarterTurnsClockwise,
                passages);
        }

        private static Vector2Int[][] CreatePassageCells(
            StageRoutePatternKind kind,
            Vector2Int anchor,
            int quarterTurnsClockwise)
        {
            switch (kind)
            {
                case StageRoutePatternKind.Straight:
                    Vector2Int[] straightOffsets =
                        quarterTurnsClockwise == 0
                            ? HorizontalCells
                            : VerticalCells;
                    return new[] { AddAnchor(anchor, straightOffsets) };

                case StageRoutePatternKind.Corner:
                    return new[]
                    {
                        AddAnchor(anchor, CornerCells[quarterTurnsClockwise]),
                    };

                case StageRoutePatternKind.DisconnectedCross:
                    return new[]
                    {
                        AddAnchor(anchor, HorizontalCells),
                        AddAnchor(anchor, VerticalCells),
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static Vector2Int[] AddAnchor(
            Vector2Int anchor,
            IReadOnlyList<Vector2Int> offsets)
        {
            Vector2Int[] cells = new Vector2Int[offsets.Count];
            for (int index = 0; index < offsets.Count; index++)
            {
                cells[index] = anchor + offsets[index];
            }

            return cells;
        }

        private static StageRoutePassageAxis GetPassageAxis(
            StageRoutePatternKind kind,
            int quarterTurnsClockwise,
            int passageIndex)
        {
            switch (kind)
            {
                case StageRoutePatternKind.Straight:
                    return quarterTurnsClockwise == 0
                        ? StageRoutePassageAxis.Horizontal
                        : StageRoutePassageAxis.Vertical;

                case StageRoutePatternKind.Corner:
                    return StageRoutePassageAxis.Turn;

                case StageRoutePatternKind.DisconnectedCross:
                    return passageIndex == 0
                        ? StageRoutePassageAxis.Horizontal
                        : StageRoutePassageAxis.Vertical;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static bool TryCreatePassageOrderContext(
            StageRouteGenerationSettings settings,
            IReadOnlyList<StageRoutePatternPlacement> sourcePlacements,
            out PassageOrderContext context)
        {
            List<StageRoutePatternPassage> passages = new();
            for (int placementIndex = 0;
                 placementIndex < sourcePlacements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement =
                    sourcePlacements[placementIndex];
                for (int passageIndex = 0;
                     passageIndex < placement.Passages.Count;
                     passageIndex++)
                {
                    passages.Add(placement.Passages[passageIndex]);
                }
            }

            passages.Sort((first, second) =>
                StringComparer.Ordinal.Compare(
                    first.PassageId,
                    second.PassageId));

            if (!TryGetSingleEndpointContact(
                    sourcePlacements,
                    settings.SpawnCell,
                    out Vector2Int? requiredFirstEntry) ||
                !TryGetSingleEndpointContact(
                    sourcePlacements,
                    settings.RouteGoalCell,
                    out Vector2Int? requiredLastExit))
            {
                context = null;
                return false;
            }

            int passageCount = passages.Count;
            int optionCount = checked(passageCount * 2);
            int maskCount = 1 << passageCount;
            int stateCount = checked(maskCount * optionCount);
            int fullMask = maskCount - 1;
            sbyte[] completionStates = new sbyte[stateCount];
            List<PassageChoice> feasibleFirstChoices = new(optionCount);

            for (int passageIndex = 0;
                 passageIndex < passageCount;
                 passageIndex++)
            {
                for (int orientation = 0; orientation < 2; orientation++)
                {
                    Vector2Int entry = GetEntryCell(
                        passages[passageIndex],
                        orientation);
                    if (requiredFirstEntry.HasValue &&
                        entry != requiredFirstEntry.Value)
                    {
                        continue;
                    }

                    int option = GetOptionIndex(passageIndex, orientation);
                    int mask = 1 << passageIndex;
                    if (CanCompletePassageOrder(
                            passages,
                            mask,
                            option,
                            fullMask,
                            optionCount,
                            requiredLastExit,
                            completionStates))
                    {
                        feasibleFirstChoices.Add(new PassageChoice(
                            passageIndex,
                            orientation));
                    }
                }
            }

            if (feasibleFirstChoices.Count == 0)
            {
                context = null;
                return false;
            }

            context = new PassageOrderContext(
                passages,
                feasibleFirstChoices,
                requiredLastExit,
                fullMask,
                optionCount,
                completionStates);
            return true;
        }

        private static bool TryCreatePassageOrder(
            PassageOrderContext context,
            IReadOnlyList<StageRoutePatternPlacement> sourcePlacements,
            ref StageRouteDeterministicRandom random,
            out List<StageRoutePatternPlacement> orientedPlacements,
            out List<StageRoutePatternPassage> orderedPassages)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            List<PassageChoice> feasibleChoices =
                new(context.OptionCount);
            for (int index = 0;
                 index < context.FeasibleFirstChoices.Count;
                 index++)
            {
                feasibleChoices.Add(context.FeasibleFirstChoices[index]);
            }

            List<PassageChoice> orderedChoices =
                new(context.Passages.Count);
            PassageChoice selectedChoice = SelectRandomChoice(
                feasibleChoices,
                ref random);
            orderedChoices.Add(selectedChoice);

            int usedMask = 1 << selectedChoice.PassageIndex;

            while (usedMask != context.FullMask)
            {
                feasibleChoices.Clear();
                StageRoutePatternPassage lastPassage =
                    context.Passages[selectedChoice.PassageIndex];

                for (int nextPassageIndex = 0;
                     nextPassageIndex < context.Passages.Count;
                     nextPassageIndex++)
                {
                    int nextBit = 1 << nextPassageIndex;
                    if ((usedMask & nextBit) != 0 ||
                        !CanConnectSlots(
                            lastPassage.Slot,
                            context.Passages[nextPassageIndex].Slot))
                    {
                        continue;
                    }

                    for (int nextOrientation = 0;
                         nextOrientation < 2;
                         nextOrientation++)
                    {
                        int nextOption = GetOptionIndex(
                            nextPassageIndex,
                            nextOrientation);
                        if (CanCompletePassageOrder(
                                context.Passages,
                                usedMask | nextBit,
                                nextOption,
                                context.FullMask,
                                context.OptionCount,
                                context.RequiredLastExit,
                                context.CompletionStates))
                        {
                            feasibleChoices.Add(new PassageChoice(
                                nextPassageIndex,
                                nextOrientation));
                        }
                    }
                }

                if (feasibleChoices.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Passage feasibility changed while reconstructing a random order.");
                }

                selectedChoice = SelectRandomChoice(
                    feasibleChoices,
                    ref random);
                orderedChoices.Add(selectedChoice);
                usedMask |= 1 << selectedChoice.PassageIndex;
            }

            return TryOrientPlacements(
                sourcePlacements,
                context.Passages,
                orderedChoices,
                out orientedPlacements,
                out orderedPassages);
        }

        private static bool CanCompletePassageOrder(
            IReadOnlyList<StageRoutePatternPassage> passages,
            int usedMask,
            int lastOption,
            int fullMask,
            int optionCount,
            Vector2Int? requiredLastExit,
            sbyte[] completionStates)
        {
            int stateIndex = GetStateIndex(
                usedMask,
                lastOption,
                optionCount);
            if (completionStates[stateIndex] != 0)
            {
                return completionStates[stateIndex] > 0;
            }

            int lastPassageIndex = lastOption / 2;
            int lastOrientation = lastOption % 2;
            bool canComplete;

            if (usedMask == fullMask)
            {
                canComplete = !requiredLastExit.HasValue ||
                    GetExitCell(
                        passages[lastPassageIndex],
                        lastOrientation) == requiredLastExit.Value;
            }
            else
            {
                canComplete = false;
                StageRoutePatternPassage lastPassage =
                    passages[lastPassageIndex];
                for (int nextPassageIndex = 0;
                     nextPassageIndex < passages.Count && !canComplete;
                     nextPassageIndex++)
                {
                    int nextBit = 1 << nextPassageIndex;
                    if ((usedMask & nextBit) != 0 ||
                        !CanConnectSlots(
                            lastPassage.Slot,
                            passages[nextPassageIndex].Slot))
                    {
                        continue;
                    }

                    for (int nextOrientation = 0;
                         nextOrientation < 2;
                         nextOrientation++)
                    {
                        int nextOption = GetOptionIndex(
                            nextPassageIndex,
                            nextOrientation);
                        if (CanCompletePassageOrder(
                                passages,
                                usedMask | nextBit,
                                nextOption,
                                fullMask,
                                optionCount,
                                requiredLastExit,
                                completionStates))
                        {
                            canComplete = true;
                            break;
                        }
                    }
                }
            }

            completionStates[stateIndex] = canComplete ? (sbyte)1 : (sbyte)-1;
            return canComplete;
        }

        private static PassageChoice SelectRandomChoice(
            IReadOnlyList<PassageChoice> stableChoices,
            ref StageRouteDeterministicRandom random)
        {
            if (stableChoices == null || stableChoices.Count == 0)
            {
                throw new ArgumentException(
                    "At least one passage choice is required.",
                    nameof(stableChoices));
            }

            return stableChoices[random.NextInt(stableChoices.Count)];
        }

        private static bool TryOrientPlacements(
            IReadOnlyList<StageRoutePatternPlacement> sourcePlacements,
            IReadOnlyList<StageRoutePatternPassage> sourcePassages,
            IReadOnlyList<PassageChoice> orderedChoices,
            out List<StageRoutePatternPlacement> orientedPlacements,
            out List<StageRoutePatternPassage> orderedPassages)
        {
            Dictionary<string, int> orientationsByPassageId =
                new(StringComparer.Ordinal);
            for (int index = 0; index < orderedChoices.Count; index++)
            {
                PassageChoice choice = orderedChoices[index];
                orientationsByPassageId.Add(
                    sourcePassages[choice.PassageIndex].PassageId,
                    choice.Orientation);
            }

            Dictionary<string, StageRoutePatternPassage> orientedById =
                new(StringComparer.Ordinal);
            orientedPlacements = new List<StageRoutePatternPlacement>(
                sourcePlacements.Count);
            for (int placementIndex = 0;
                 placementIndex < sourcePlacements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement sourcePlacement =
                    sourcePlacements[placementIndex];
                StageRoutePatternPassage[] orientedPlacementPassages =
                    new StageRoutePatternPassage[sourcePlacement.Passages.Count];
                for (int passageIndex = 0;
                     passageIndex < sourcePlacement.Passages.Count;
                     passageIndex++)
                {
                    StageRoutePatternPassage sourcePassage =
                        sourcePlacement.Passages[passageIndex];
                    Vector2Int[] cells = new Vector2Int[sourcePassage.Cells.Count];
                    for (int cellIndex = 0;
                         cellIndex < sourcePassage.Cells.Count;
                         cellIndex++)
                    {
                        cells[cellIndex] = sourcePassage.Cells[cellIndex];
                    }

                    if (orientationsByPassageId[sourcePassage.PassageId] == 1)
                    {
                        Array.Reverse(cells);
                    }

                    StageRoutePatternPassage orientedPassage =
                        new StageRoutePatternPassage(
                            sourcePassage.PlacementId,
                            sourcePassage.PassageId,
                            sourcePassage.PassageIndex,
                            sourcePassage.Slot,
                            sourcePassage.Axis,
                            cells);
                    orientedPlacementPassages[passageIndex] = orientedPassage;
                    orientedById.Add(orientedPassage.PassageId, orientedPassage);
                }

                orientedPlacements.Add(new StageRoutePatternPlacement(
                    sourcePlacement.Id,
                    sourcePlacement.Slot,
                    sourcePlacement.Kind,
                    sourcePlacement.AnchorCell,
                    sourcePlacement.QuarterTurnsClockwise,
                    orientedPlacementPassages));
            }

            orderedPassages = new List<StageRoutePatternPassage>(
                orderedChoices.Count);
            for (int index = 0; index < orderedChoices.Count; index++)
            {
                StageRoutePatternPassage sourcePassage =
                    sourcePassages[orderedChoices[index].PassageIndex];
                orderedPassages.Add(orientedById[sourcePassage.PassageId]);
            }

            return true;
        }

        private static bool TryGetSingleEndpointContact(
            IReadOnlyList<StageRoutePatternPlacement> placements,
            Vector2Int endpoint,
            out Vector2Int? touchingRoadCell)
        {
            touchingRoadCell = null;
            for (int placementIndex = 0;
                 placementIndex < placements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = placements[placementIndex];
                for (int cellIndex = 0;
                     cellIndex < placement.RoadCells.Count;
                     cellIndex++)
                {
                    Vector2Int roadCell = placement.RoadCells[cellIndex];
                    if (GetManhattanDistance(roadCell, endpoint) != 1)
                    {
                        continue;
                    }

                    if (touchingRoadCell.HasValue &&
                        touchingRoadCell.Value != roadCell)
                    {
                        return false;
                    }

                    touchingRoadCell = roadCell;
                }
            }

            return true;
        }

        private static int GetOptionIndex(int passageIndex, int orientation)
        {
            return checked(passageIndex * 2 + orientation);
        }

        private static int GetStateIndex(int mask, int option, int optionCount)
        {
            return checked(mask * optionCount + option);
        }

        private static Vector2Int GetEntryCell(
            StageRoutePatternPassage passage,
            int orientation)
        {
            return orientation == 0 ? passage.EntryCell : passage.ExitCell;
        }

        private static Vector2Int GetExitCell(
            StageRoutePatternPassage passage,
            int orientation)
        {
            return orientation == 0 ? passage.ExitCell : passage.EntryCell;
        }

        private static StageRoutePatternKind[] GetAllowedKinds(
            StageRoutePatternKinds allowedKinds)
        {
            List<StageRoutePatternKind> kinds = new(3);
            if ((allowedKinds & StageRoutePatternKinds.Straight) != 0)
            {
                kinds.Add(StageRoutePatternKind.Straight);
            }

            if ((allowedKinds & StageRoutePatternKinds.Corner) != 0)
            {
                kinds.Add(StageRoutePatternKind.Corner);
            }

            if ((allowedKinds & StageRoutePatternKinds.DisconnectedCross) != 0)
            {
                kinds.Add(StageRoutePatternKind.DisconnectedCross);
            }

            return kinds.ToArray();
        }

        private static int GetOrientationCount(StageRoutePatternKind kind)
        {
            switch (kind)
            {
                case StageRoutePatternKind.Straight:
                    return 2;
                case StageRoutePatternKind.Corner:
                    return 4;
                case StageRoutePatternKind.DisconnectedCross:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static string GetPlacementId(StageRoutePatternSlot slot)
        {
            switch (slot)
            {
                case StageRoutePatternSlot.Quadrant1:
                    return "Q1";
                case StageRoutePatternSlot.Quadrant2:
                    return "Q2";
                case StageRoutePatternSlot.Quadrant3:
                    return "Q3";
                case StageRoutePatternSlot.Quadrant4:
                    return "Q4";
                case StageRoutePatternSlot.Center:
                    return "Center";
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        private static string CreateLayoutSignature(
            IReadOnlyList<StageRoutePatternPlacement> placements,
            IReadOnlyList<StageRoutePatternPassage> orderedPassages)
        {
            StringBuilder builder = new();
            for (int placementIndex = 0;
                 placementIndex < placements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = placements[placementIndex];
                builder.Append(placement.Id).Append('|')
                    .Append((int)placement.Kind).Append('|')
                    .AppendInvariant(placement.AnchorCell.x).Append(',')
                    .AppendInvariant(placement.AnchorCell.y).Append('|')
                    .AppendInvariant(placement.QuarterTurnsClockwise).Append(':');

                for (int passageIndex = 0;
                     passageIndex < placement.Passages.Count;
                     passageIndex++)
                {
                    StageRoutePatternPassage passage = placement.Passages[passageIndex];
                    builder.Append(passage.PassageId).Append('=');
                    for (int cellIndex = 0; cellIndex < passage.Cells.Count; cellIndex++)
                    {
                        Vector2Int cell = passage.Cells[cellIndex];
                        builder.AppendInvariant(cell.x).Append(',')
                            .AppendInvariant(cell.y).Append(';');
                    }
                }

                builder.Append('/');
            }

            builder.Append('#');
            for (int index = 0; index < orderedPassages.Count; index++)
            {
                builder.Append(orderedPassages[index].PassageId).Append(';');
            }

            return builder.ToString();
        }

        private static string CreatePhysicalLayoutSignature(
            IReadOnlyList<StageRoutePatternPlacement> placements)
        {
            StringBuilder builder = new();
            for (int placementIndex = 0;
                 placementIndex < placements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = placements[placementIndex];
                builder.Append(placement.Id).Append('|')
                    .Append((int)placement.Kind).Append('|')
                    .AppendInvariant(placement.AnchorCell.x).Append(',')
                    .AppendInvariant(placement.AnchorCell.y).Append('|')
                    .AppendInvariant(placement.QuarterTurnsClockwise).Append(':');

                for (int passageIndex = 0;
                     passageIndex < placement.Passages.Count;
                     passageIndex++)
                {
                    StageRoutePatternPassage passage =
                        placement.Passages[passageIndex];
                    builder.Append(passage.PassageId).Append('=');
                    for (int cellIndex = 0;
                         cellIndex < passage.Cells.Count;
                         cellIndex++)
                    {
                        Vector2Int cell = passage.Cells[cellIndex];
                        builder.AppendInvariant(cell.x).Append(',')
                            .AppendInvariant(cell.y).Append(';');
                    }
                }

                builder.Append('/');
            }

            return builder.ToString();
        }

        private static int CreatePassageOrderStream(
            string physicalSignature,
            int orderDrawIndex)
        {
            StringBuilder builder = new(physicalSignature.Length + 24);
            builder.Append(physicalSignature)
                .Append("#order:")
                .AppendInvariant(orderDrawIndex);
            ulong hash = StageRouteStableHash.Fnv1A64(builder.ToString());
            return unchecked((int)(hash ^ (hash >> 32)));
        }

        private static bool HasInvalidEndpointContact(
            StageRouteGenerationSettings settings,
            IReadOnlyList<StageRoutePatternPlacement> placements,
            IReadOnlyList<StageRoutePatternPassage> orderedPassages)
        {
            Vector2Int allowedSpawnNeighbor = orderedPassages[0].EntryCell;
            Vector2Int allowedGoalNeighbor =
                orderedPassages[orderedPassages.Count - 1].ExitCell;

            for (int placementIndex = 0;
                 placementIndex < placements.Count;
                 placementIndex++)
            {
                StageRoutePatternPlacement placement = placements[placementIndex];
                for (int cellIndex = 0;
                     cellIndex < placement.RoadCells.Count;
                     cellIndex++)
                {
                    Vector2Int roadCell = placement.RoadCells[cellIndex];
                    if (GetManhattanDistance(roadCell, settings.SpawnCell) == 1 &&
                        roadCell != allowedSpawnNeighbor)
                    {
                        return true;
                    }

                    if (GetManhattanDistance(roadCell, settings.RouteGoalCell) == 1 &&
                        roadCell != allowedGoalNeighbor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool OverlapsOrTouchesOtherPlacement(
            IReadOnlyList<Vector2Int> roadCells,
            HashSet<Vector2Int> occupiedRoadCells)
        {
            for (int index = 0; index < roadCells.Count; index++)
            {
                Vector2Int roadCell = roadCells[index];
                if (occupiedRoadCells.Contains(roadCell))
                {
                    return true;
                }

                for (int directionIndex = 0;
                     directionIndex < CardinalDirections.Length;
                     directionIndex++)
                {
                    if (occupiedRoadCells.Contains(
                            roadCell + CardinalDirections[directionIndex]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int GetManhattanDistance(
            Vector2Int first,
            Vector2Int second)
        {
            return Math.Abs(first.x - second.x) + Math.Abs(first.y - second.y);
        }

        private static void AddCells(
            IReadOnlyList<Vector2Int> roadCells,
            HashSet<Vector2Int> destination)
        {
            for (int index = 0; index < roadCells.Count; index++)
            {
                destination.Add(roadCells[index]);
            }
        }

        private static void RemoveCells(
            IReadOnlyList<Vector2Int> roadCells,
            HashSet<Vector2Int> destination)
        {
            for (int index = 0; index < roadCells.Count; index++)
            {
                destination.Remove(roadCells[index]);
            }
        }

        private static int ClampToRange(long value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return (int)value;
        }

        private static RectInt CreateRectFromMinMax(
            int xMinimum,
            int yMinimum,
            int xMaximum,
            int yMaximum)
        {
            return new RectInt(
                xMinimum,
                yMinimum,
                xMaximum - xMinimum,
                yMaximum - yMinimum);
        }

        private readonly struct PlacementRequest
        {
            internal StageRoutePatternSlot Slot { get; }
            internal StageRoutePatternKind Kind { get; }
            internal int RandomOrderIndex { get; }
            internal IReadOnlyList<StageRoutePatternPlacement> Placements
            {
                get;
            }

            internal PlacementRequest(
                StageRoutePatternSlot slot,
                StageRoutePatternKind kind,
                int randomOrderIndex,
                IReadOnlyList<StageRoutePatternPlacement> placements)
            {
                Slot = slot;
                Kind = kind;
                RandomOrderIndex = randomOrderIndex;
                Placements = placements;
            }
        }

        private sealed class PhysicalPlacementCatalog
        {
            private const int PatternKindCount = 3;
            private readonly IReadOnlyList<StageRoutePatternPlacement>[]
                placementsBySlotAndKind;

            internal PhysicalPlacementCatalog(
                StageRouteGenerationSettings settings)
            {
                placementsBySlotAndKind =
                    new IReadOnlyList<StageRoutePatternPlacement>[
                        AllSlots.Length * PatternKindCount];
                for (int slotIndex = 0;
                     slotIndex < AllSlots.Length;
                     slotIndex++)
                {
                    StageRoutePatternSlot slot = AllSlots[slotIndex];
                    for (int kindIndex = 0;
                         kindIndex < PatternKindCount;
                         kindIndex++)
                    {
                        StageRoutePatternKind kind =
                            (StageRoutePatternKind)kindIndex;
                        placementsBySlotAndKind[GetIndex(slot, kind)] =
                            CreatePlacementCandidates(settings, slot, kind)
                                .AsReadOnly();
                    }
                }
            }

            internal IReadOnlyList<StageRoutePatternPlacement> GetPlacements(
                StageRoutePatternSlot slot,
                StageRoutePatternKind kind)
            {
                return placementsBySlotAndKind[GetIndex(slot, kind)];
            }

            private static int GetIndex(
                StageRoutePatternSlot slot,
                StageRoutePatternKind kind)
            {
                return checked((int)slot * PatternKindCount + (int)kind);
            }
        }

        private readonly struct PassageChoice
        {
            internal int PassageIndex { get; }
            internal int Orientation { get; }

            internal PassageChoice(int passageIndex, int orientation)
            {
                PassageIndex = passageIndex;
                Orientation = orientation;
            }
        }

        private sealed class PassageOrderContext
        {
            internal IReadOnlyList<StageRoutePatternPassage> Passages { get; }
            internal IReadOnlyList<PassageChoice> FeasibleFirstChoices { get; }
            internal Vector2Int? RequiredLastExit { get; }
            internal int FullMask { get; }
            internal int OptionCount { get; }
            internal sbyte[] CompletionStates { get; }

            internal PassageOrderContext(
                IReadOnlyList<StageRoutePatternPassage> passages,
                IReadOnlyList<PassageChoice> feasibleFirstChoices,
                Vector2Int? requiredLastExit,
                int fullMask,
                int optionCount,
                sbyte[] completionStates)
            {
                Passages = passages;
                FeasibleFirstChoices = feasibleFirstChoices;
                RequiredLastExit = requiredLastExit;
                FullMask = fullMask;
                OptionCount = optionCount;
                CompletionStates = completionStates;
            }
        }

        private sealed class PassageOrderVariant
        {
            internal int PassageOrderDrawIndex { get; }
            internal string Signature { get; }
            internal IReadOnlyList<StageRoutePatternPlacement> Placements { get; }
            internal IReadOnlyList<StageRoutePatternPassage> OrderedPassages { get; }

            internal PassageOrderVariant(
                int passageOrderDrawIndex,
                string signature,
                IReadOnlyList<StageRoutePatternPlacement> placements,
                IReadOnlyList<StageRoutePatternPassage> orderedPassages)
            {
                PassageOrderDrawIndex = passageOrderDrawIndex;
                Signature = signature;
                Placements = placements;
                OrderedPassages = orderedPassages;
            }
        }

        private sealed class PhysicalLayoutDraw
        {
            internal int PhysicalLayoutDrawIndex { get; }
            internal string PhysicalSignature { get; }
            internal int QualityScore { get; }
            internal int PatternCompositionKey { get; }
            internal ulong SelectionKey { get; }
            internal IReadOnlyList<StageRoutePatternPlacement> Placements { get; }
            internal PhysicalLayoutDraw(
                int physicalLayoutDrawIndex,
                string physicalSignature,
                int qualityScore,
                int patternCompositionKey,
                ulong selectionKey,
                IReadOnlyList<StageRoutePatternPlacement> placements)
            {
                PhysicalLayoutDrawIndex = physicalLayoutDrawIndex;
                PhysicalSignature = physicalSignature;
                QualityScore = qualityScore;
                PatternCompositionKey = patternCompositionKey;
                SelectionKey = selectionKey;
                Placements = placements;
            }
        }

        private sealed class PhysicalLayoutCandidate
        {
            internal int PhysicalLayoutIndex { get; }
            internal int PhysicalLayoutDrawIndex { get; }
            internal bool IsPreferredCompositionProbe { get; }
            internal IReadOnlyList<PassageOrderVariant> Variants { get; }

            internal PhysicalLayoutCandidate(
                int physicalLayoutIndex,
                int physicalLayoutDrawIndex,
                bool isPreferredCompositionProbe,
                IReadOnlyList<PassageOrderVariant> variants)
            {
                PhysicalLayoutIndex = physicalLayoutIndex;
                PhysicalLayoutDrawIndex = physicalLayoutDrawIndex;
                IsPreferredCompositionProbe = isPreferredCompositionProbe;
                Variants = variants;
            }
        }

        private readonly struct PatternComposition
        {
            internal int StraightCount { get; }
            internal int CornerCount { get; }
            internal int CrossCount { get; }
            internal int Key => checked(
                StraightCount + CornerCount * 8 + CrossCount * 64);

            internal PatternComposition(
                int straightCount,
                int cornerCount,
                int crossCount)
            {
                StraightCount = straightCount;
                CornerCount = cornerCount;
                CrossCount = crossCount;
            }
        }

    }

    internal static class StageRouteStringBuilderExtensions
    {
        internal static StringBuilder AppendInvariant(
            this StringBuilder builder,
            int value)
        {
            return builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
