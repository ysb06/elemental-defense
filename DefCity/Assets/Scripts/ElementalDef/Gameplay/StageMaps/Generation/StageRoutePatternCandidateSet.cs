using System;
using System.Collections.Generic;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    /// <summary>
    /// Identifies the physical layout and passage-order draw that produced a
    /// route candidate. All indices are zero-based and deterministic.
    /// </summary>
    public sealed class StageRoutePatternCandidateRecord
    {
        public StageRoutePatternLayout Layout { get; }
        public int PhysicalLayoutIndex { get; }
        public int PhysicalLayoutDrawIndex { get; }
        public int VariantIndex { get; }
        public int PassageOrderDrawIndex { get; }

        public StageRoutePatternCandidateRecord(
            StageRoutePatternLayout layout,
            int physicalLayoutIndex,
            int physicalLayoutDrawIndex,
            int variantIndex,
            int passageOrderDrawIndex)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            EnsureNonNegative(physicalLayoutIndex, nameof(physicalLayoutIndex));
            EnsureNonNegative(
                physicalLayoutDrawIndex,
                nameof(physicalLayoutDrawIndex));
            EnsureNonNegative(variantIndex, nameof(variantIndex));
            EnsureNonNegative(
                passageOrderDrawIndex,
                nameof(passageOrderDrawIndex));

            PhysicalLayoutIndex = physicalLayoutIndex;
            PhysicalLayoutDrawIndex = physicalLayoutDrawIndex;
            VariantIndex = variantIndex;
            PassageOrderDrawIndex = passageOrderDrawIndex;
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Candidate metadata indices cannot be negative.");
            }
        }
    }

    public sealed class StageRoutePatternCandidateSet
    {
        private readonly IReadOnlyList<StageRoutePatternCandidateRecord>
            candidateRecords;
        private readonly IReadOnlyList<StageRoutePatternLayout> candidates;

        public bool Succeeded => candidateRecords.Count > 0;
        public IReadOnlyList<StageRoutePatternCandidateRecord> CandidateRecords =>
            candidateRecords;
        public IReadOnlyList<StageRoutePatternLayout> Candidates => candidates;
        public StageRoutePatternCandidateFailureReason FailureReason { get; }

        public int PhysicalLayoutDrawCount { get; }
        public int PhysicalPlacementRejectedCount { get; }
        public int DuplicatePhysicalLayoutCount { get; }
        public int UnselectedPhysicalLayoutCount { get; }
        public int PhysicalLayoutCount { get; }
        public int PassageOrderDrawCount { get; }
        public int DuplicatePassageOrderCount { get; }
        public int PassageOrderVariantCount { get; }
        public int LayoutsWithoutValidOrderCount { get; }
        public int GeneratedCandidateCount => candidateRecords.Count;
        public bool HasPreferredPatternComposition { get; }
        public int PreferredStraightPatternCount { get; }
        public int PreferredCornerPatternCount { get; }
        public int PreferredCrossPatternCount { get; }

        [Obsolete("Use PhysicalLayoutDrawCount instead.")]
        public int DrawCount => PhysicalLayoutDrawCount;

        private StageRoutePatternCandidateSet(
            IReadOnlyList<StageRoutePatternCandidateRecord> sourceRecords,
            StageRoutePatternCandidateFailureReason failureReason,
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int unselectedPhysicalLayoutCount,
            int physicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderCount,
            int passageOrderVariantCount,
            int layoutsWithoutValidOrderCount,
            bool hasPreferredPatternComposition = false,
            int preferredStraightPatternCount = 0,
            int preferredCornerPatternCount = 0,
            int preferredCrossPatternCount = 0)
        {
            if (sourceRecords == null)
            {
                throw new ArgumentNullException(nameof(sourceRecords));
            }

            EnsureNonNegative(physicalLayoutDrawCount,
                nameof(physicalLayoutDrawCount));
            EnsureNonNegative(physicalPlacementRejectedCount,
                nameof(physicalPlacementRejectedCount));
            EnsureNonNegative(duplicatePhysicalLayoutCount,
                nameof(duplicatePhysicalLayoutCount));
            EnsureNonNegative(unselectedPhysicalLayoutCount,
                nameof(unselectedPhysicalLayoutCount));
            EnsureNonNegative(physicalLayoutCount, nameof(physicalLayoutCount));
            EnsureNonNegative(passageOrderDrawCount,
                nameof(passageOrderDrawCount));
            EnsureNonNegative(duplicatePassageOrderCount,
                nameof(duplicatePassageOrderCount));
            EnsureNonNegative(passageOrderVariantCount,
                nameof(passageOrderVariantCount));
            EnsureNonNegative(layoutsWithoutValidOrderCount,
                nameof(layoutsWithoutValidOrderCount));
            EnsureNonNegative(preferredStraightPatternCount,
                nameof(preferredStraightPatternCount));
            EnsureNonNegative(preferredCornerPatternCount,
                nameof(preferredCornerPatternCount));
            EnsureNonNegative(preferredCrossPatternCount,
                nameof(preferredCrossPatternCount));

            int preferredPatternCount = checked(
                preferredStraightPatternCount +
                preferredCornerPatternCount +
                preferredCrossPatternCount);
            if (hasPreferredPatternComposition != (preferredPatternCount > 0))
            {
                throw new ArgumentException(
                    "A preferred pattern composition must contain at least one " +
                    "pattern, and absent preference counts must all be zero.");
            }

            long accountedPhysicalDrawCount =
                (long)physicalPlacementRejectedCount +
                duplicatePhysicalLayoutCount +
                unselectedPhysicalLayoutCount +
                physicalLayoutCount;
            if (accountedPhysicalDrawCount != physicalLayoutDrawCount)
            {
                throw new ArgumentException(
                    "Physical draws must equal rejected, duplicate, " +
                    "unselected, and accepted physical layout counts.");
            }

            StageRoutePatternCandidateRecord[] recordCopies =
                new StageRoutePatternCandidateRecord[sourceRecords.Count];
            StageRoutePatternLayout[] layoutCopies =
                new StageRoutePatternLayout[sourceRecords.Count];
            HashSet<string> layoutIds = new(StringComparer.Ordinal);
            HashSet<PhysicalVariantKey> physicalVariants = new();
            Dictionary<int, int> drawIndexByPhysicalLayout = new();
            for (int index = 0; index < sourceRecords.Count; index++)
            {
                StageRoutePatternCandidateRecord record = sourceRecords[index]
                    ?? throw new ArgumentException(
                        $"Pattern candidate record index {index} is null.",
                        nameof(sourceRecords));
                StageRoutePatternLayout candidate = record.Layout;

                if (record.PhysicalLayoutIndex >= physicalLayoutCount)
                {
                    throw new ArgumentException(
                        $"Candidate physical layout index " +
                        $"{record.PhysicalLayoutIndex} is outside the accepted " +
                        $"physical layout count {physicalLayoutCount}.",
                        nameof(sourceRecords));
                }

                if (record.PhysicalLayoutDrawIndex >= physicalLayoutDrawCount)
                {
                    throw new ArgumentException(
                        $"Candidate physical draw index " +
                        $"{record.PhysicalLayoutDrawIndex} is outside the draw " +
                        $"count {physicalLayoutDrawCount}.",
                        nameof(sourceRecords));
                }

                if (record.VariantIndex >= passageOrderVariantCount)
                {
                    throw new ArgumentException(
                        $"Candidate variant index {record.VariantIndex} is " +
                        $"outside the accepted variant count " +
                        $"{passageOrderVariantCount}.",
                        nameof(sourceRecords));
                }

                if (record.PassageOrderDrawIndex >= passageOrderDrawCount)
                {
                    throw new ArgumentException(
                        $"Candidate passage-order draw index " +
                        $"{record.PassageOrderDrawIndex} is outside the draw " +
                        $"count {passageOrderDrawCount}.",
                        nameof(sourceRecords));
                }

                PhysicalVariantKey physicalVariant = new(
                    record.PhysicalLayoutIndex,
                    record.VariantIndex);
                if (!physicalVariants.Add(physicalVariant))
                {
                    throw new ArgumentException(
                        $"Physical layout {record.PhysicalLayoutIndex} contains " +
                        $"more than one candidate for variant " +
                        $"{record.VariantIndex}.",
                        nameof(sourceRecords));
                }

                if (drawIndexByPhysicalLayout.TryGetValue(
                        record.PhysicalLayoutIndex,
                        out int existingDrawIndex) &&
                    existingDrawIndex != record.PhysicalLayoutDrawIndex)
                {
                    throw new ArgumentException(
                        $"Physical layout {record.PhysicalLayoutIndex} maps to " +
                        "more than one physical draw index.",
                        nameof(sourceRecords));
                }

                drawIndexByPhysicalLayout[record.PhysicalLayoutIndex] =
                    record.PhysicalLayoutDrawIndex;

                if (!layoutIds.Add(candidate.LayoutId))
                {
                    throw new ArgumentException(
                        $"Pattern layout ID '{candidate.LayoutId}' is duplicated.",
                        nameof(sourceRecords));
                }

                recordCopies[index] = record;
                layoutCopies[index] = candidate;
            }

            if (recordCopies.Length == 0 &&
                failureReason == StageRoutePatternCandidateFailureReason.None)
            {
                throw new ArgumentException(
                    "An empty candidate set requires a failure reason.",
                    nameof(failureReason));
            }

            if (recordCopies.Length > 0 &&
                failureReason != StageRoutePatternCandidateFailureReason.None)
            {
                throw new ArgumentException(
                    "A non-empty candidate set cannot carry a failure reason.",
                    nameof(failureReason));
            }

            if ((long)duplicatePassageOrderCount + passageOrderVariantCount >
                passageOrderDrawCount)
            {
                throw new ArgumentException(
                    "Passage-order counters exceed the recorded draw count.");
            }

            if (layoutsWithoutValidOrderCount > physicalLayoutCount)
            {
                throw new ArgumentException(
                    "Layouts without an order cannot exceed physical layouts.");
            }

            if (recordCopies.Length > passageOrderVariantCount)
            {
                throw new ArgumentException(
                    "Candidate records cannot exceed generated order variants.");
            }

            if (hasPreferredPatternComposition)
            {
                for (int index = 0; index < recordCopies.Length; index++)
                {
                    if (recordCopies[index].Layout.Placements.Count !=
                        preferredPatternCount)
                    {
                        throw new ArgumentException(
                            "Preferred pattern counts must equal each candidate " +
                            "layout's placement count.",
                            nameof(sourceRecords));
                    }
                }
            }

            candidateRecords = Array.AsReadOnly(recordCopies);
            candidates = Array.AsReadOnly(layoutCopies);
            FailureReason = failureReason;
            PhysicalLayoutDrawCount = physicalLayoutDrawCount;
            PhysicalPlacementRejectedCount = physicalPlacementRejectedCount;
            DuplicatePhysicalLayoutCount = duplicatePhysicalLayoutCount;
            UnselectedPhysicalLayoutCount = unselectedPhysicalLayoutCount;
            PhysicalLayoutCount = physicalLayoutCount;
            PassageOrderDrawCount = passageOrderDrawCount;
            DuplicatePassageOrderCount = duplicatePassageOrderCount;
            PassageOrderVariantCount = passageOrderVariantCount;
            LayoutsWithoutValidOrderCount = layoutsWithoutValidOrderCount;
            HasPreferredPatternComposition = hasPreferredPatternComposition;
            PreferredStraightPatternCount = preferredStraightPatternCount;
            PreferredCornerPatternCount = preferredCornerPatternCount;
            PreferredCrossPatternCount = preferredCrossPatternCount;
        }

        public static StageRoutePatternCandidateSet Success(
            IReadOnlyList<StageRoutePatternLayout> candidates,
            int drawCount)
        {
            if (candidates == null || candidates.Count == 0)
            {
                throw new ArgumentException(
                    "A successful candidate set cannot be empty.",
                    nameof(candidates));
            }

            if (drawCount < candidates.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(drawCount),
                    drawCount,
                    "A compatibility candidate set cannot accept more " +
                    "physical layouts than were drawn.");
            }

            StageRoutePatternCandidateRecord[] records =
                new StageRoutePatternCandidateRecord[candidates.Count];
            for (int index = 0; index < candidates.Count; index++)
            {
                records[index] = new StageRoutePatternCandidateRecord(
                    candidates[index],
                    index,
                    index,
                    0,
                    index);
            }

            return new StageRoutePatternCandidateSet(
                records,
                StageRoutePatternCandidateFailureReason.None,
                drawCount,
                physicalPlacementRejectedCount: drawCount - candidates.Count,
                duplicatePhysicalLayoutCount: 0,
                unselectedPhysicalLayoutCount: 0,
                physicalLayoutCount: candidates.Count,
                passageOrderDrawCount: candidates.Count,
                duplicatePassageOrderCount: 0,
                passageOrderVariantCount: candidates.Count,
                layoutsWithoutValidOrderCount: 0);
        }

        public static StageRoutePatternCandidateSet Failure(
            StageRoutePatternCandidateFailureReason failureReason,
            int drawCount)
        {
            ValidateFailureReason(failureReason);

            return new StageRoutePatternCandidateSet(
                Array.Empty<StageRoutePatternCandidateRecord>(),
                failureReason,
                drawCount,
                physicalPlacementRejectedCount: drawCount,
                duplicatePhysicalLayoutCount: 0,
                unselectedPhysicalLayoutCount: 0,
                physicalLayoutCount: 0,
                passageOrderDrawCount: 0,
                duplicatePassageOrderCount: 0,
                passageOrderVariantCount: 0,
                layoutsWithoutValidOrderCount: 0);
        }

        internal static StageRoutePatternCandidateSet CreateSuccess(
            IReadOnlyList<StageRoutePatternCandidateRecord> candidateRecords,
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int unselectedPhysicalLayoutCount,
            int physicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderCount,
            int passageOrderVariantCount,
            int layoutsWithoutValidOrderCount,
            int preferredStraightPatternCount = 0,
            int preferredCornerPatternCount = 0,
            int preferredCrossPatternCount = 0)
        {
            if (candidateRecords == null || candidateRecords.Count == 0)
            {
                throw new ArgumentException(
                    "A successful candidate set cannot be empty.",
                    nameof(candidateRecords));
            }

            return new StageRoutePatternCandidateSet(
                candidateRecords,
                StageRoutePatternCandidateFailureReason.None,
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount,
                physicalLayoutCount,
                passageOrderDrawCount,
                duplicatePassageOrderCount,
                passageOrderVariantCount,
                layoutsWithoutValidOrderCount,
                hasPreferredPatternComposition: HasAnyPreferredPattern(
                preferredStraightPatternCount,
                preferredCornerPatternCount,
                preferredCrossPatternCount),
                preferredStraightPatternCount: preferredStraightPatternCount,
                preferredCornerPatternCount: preferredCornerPatternCount,
                preferredCrossPatternCount: preferredCrossPatternCount);
        }

        internal static StageRoutePatternCandidateSet CreateSuccess(
            IReadOnlyList<StageRoutePatternCandidateRecord> candidateRecords,
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int physicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderCount,
            int passageOrderVariantCount,
            int layoutsWithoutValidOrderCount)
        {
            return CreateSuccess(
                candidateRecords,
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount: 0,
                physicalLayoutCount,
                passageOrderDrawCount,
                duplicatePassageOrderCount,
                passageOrderVariantCount,
                layoutsWithoutValidOrderCount);
        }

        internal static StageRoutePatternCandidateSet CreateFailure(
            StageRoutePatternCandidateFailureReason failureReason,
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int unselectedPhysicalLayoutCount,
            int physicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderCount,
            int layoutsWithoutValidOrderCount,
            int preferredStraightPatternCount = 0,
            int preferredCornerPatternCount = 0,
            int preferredCrossPatternCount = 0)
        {
            ValidateFailureReason(failureReason);

            return new StageRoutePatternCandidateSet(
                Array.Empty<StageRoutePatternCandidateRecord>(),
                failureReason,
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount,
                physicalLayoutCount,
                passageOrderDrawCount,
                duplicatePassageOrderCount,
                passageOrderVariantCount: 0,
                layoutsWithoutValidOrderCount: layoutsWithoutValidOrderCount,
                hasPreferredPatternComposition: HasAnyPreferredPattern(
                    preferredStraightPatternCount,
                    preferredCornerPatternCount,
                    preferredCrossPatternCount),
                preferredStraightPatternCount: preferredStraightPatternCount,
                preferredCornerPatternCount: preferredCornerPatternCount,
                preferredCrossPatternCount: preferredCrossPatternCount);
        }

        internal static StageRoutePatternCandidateSet CreateFailure(
            StageRoutePatternCandidateFailureReason failureReason,
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int physicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderCount,
            int layoutsWithoutValidOrderCount)
        {
            return CreateFailure(
                failureReason,
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount: 0,
                physicalLayoutCount,
                passageOrderDrawCount,
                duplicatePassageOrderCount,
                layoutsWithoutValidOrderCount);
        }

        private static void ValidateFailureReason(
            StageRoutePatternCandidateFailureReason failureReason)
        {
            if (failureReason == StageRoutePatternCandidateFailureReason.None ||
                !Enum.IsDefined(
                    typeof(StageRoutePatternCandidateFailureReason),
                    failureReason))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(failureReason),
                    failureReason,
                    "A defined pattern candidate failure reason is required.");
            }
        }

        private static bool HasAnyPreferredPattern(
            int straightCount,
            int cornerCount,
            int crossCount)
        {
            return checked(straightCount + cornerCount + crossCount) > 0;
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                "Candidate-set counters cannot be negative.");
            }
        }

        private readonly struct PhysicalVariantKey : IEquatable<PhysicalVariantKey>
        {
            private readonly int physicalLayoutIndex;
            private readonly int variantIndex;

            internal PhysicalVariantKey(int physicalLayoutIndex, int variantIndex)
            {
                this.physicalLayoutIndex = physicalLayoutIndex;
                this.variantIndex = variantIndex;
            }

            public bool Equals(PhysicalVariantKey other)
            {
                return physicalLayoutIndex == other.physicalLayoutIndex &&
                       variantIndex == other.variantIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is PhysicalVariantKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return physicalLayoutIndex * 397 ^ variantIndex;
                }
            }
        }
    }
}
