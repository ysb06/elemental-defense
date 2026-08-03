using System;
using System.Collections.Generic;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public enum StageRouteCandidateOutcome
    {
        NotAttempted = 0,
        RejectedByPrevalidation = 1,
        PathNotFound = 2,
        SearchBudgetExceeded = 3,
        Succeeded = 4,
    }

    public enum StageRouteCandidateRejectionReason
    {
        None = 0,
        EntryPortUnavailable = 1,
        ExitPortUnavailable = 2,
        FixedPassageConflict = 3,
        ResidualConnectivityUnavailable = 4,
        InsufficientResidualCells = 5,
    }

    public enum StageRouteSearchLimitKind
    {
        None = 0,
        PerCandidateWork = 1,
        TotalWork = 2,
        OpenSetCapacity = 3,
        ConnectorAlternativeCount = 4,
    }

    public sealed class StageRouteCandidateDiagnostic
    {
        public int CandidateIndex { get; }
        public string LayoutId { get; }
        public int PhysicalLayoutIndex { get; }
        public int VariantIndex { get; }
        public int PhysicalDrawIndex { get; }
        public int PassageOrderDrawIndex { get; }
        public StageRouteCandidateOutcome Outcome { get; }
        public StageRouteCandidateRejectionReason RejectionReason { get; }
        public StageRouteSearchLimitKind SearchLimitKind { get; }
        public int WorkUnits { get; }
        public int AStarNodeExpansionCount { get; }
        public int ConnectorAlternativeCount { get; }
        public int BacktrackCount { get; }
        public int ReachabilityCheckCount { get; }
        public int ReachabilityVisitedCellCount { get; }

        internal StageRouteCandidateDiagnostic(
            int candidateIndex,
            string layoutId,
            int physicalLayoutIndex,
            int variantIndex,
            int physicalDrawIndex,
            int passageOrderDrawIndex,
            StageRouteCandidateOutcome outcome,
            StageRouteCandidateRejectionReason rejectionReason,
            StageRouteSearchLimitKind searchLimitKind,
            int workUnits,
            int aStarNodeExpansionCount,
            int connectorAlternativeCount,
            int backtrackCount,
            int reachabilityCheckCount,
            int reachabilityVisitedCellCount)
        {
            if (candidateIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(candidateIndex));
            }

            if (string.IsNullOrWhiteSpace(layoutId))
            {
                throw new ArgumentException(
                    "A diagnostic layout ID is required.",
                    nameof(layoutId));
            }

            EnsureNonNegative(physicalLayoutIndex, nameof(physicalLayoutIndex));
            EnsureNonNegative(variantIndex, nameof(variantIndex));
            EnsureNonNegative(physicalDrawIndex, nameof(physicalDrawIndex));
            EnsureNonNegative(
                passageOrderDrawIndex,
                nameof(passageOrderDrawIndex));
            EnsureDefined(outcome, nameof(outcome));
            EnsureDefined(rejectionReason, nameof(rejectionReason));
            EnsureDefined(searchLimitKind, nameof(searchLimitKind));
            EnsureNonNegative(workUnits, nameof(workUnits));
            EnsureNonNegative(
                aStarNodeExpansionCount,
                nameof(aStarNodeExpansionCount));
            EnsureNonNegative(
                connectorAlternativeCount,
                nameof(connectorAlternativeCount));
            EnsureNonNegative(backtrackCount, nameof(backtrackCount));
            EnsureNonNegative(
                reachabilityCheckCount,
                nameof(reachabilityCheckCount));
            EnsureNonNegative(
                reachabilityVisitedCellCount,
                nameof(reachabilityVisitedCellCount));

            bool wasRejected =
                outcome == StageRouteCandidateOutcome.RejectedByPrevalidation;
            if (wasRejected !=
                (rejectionReason != StageRouteCandidateRejectionReason.None))
            {
                throw new ArgumentException(
                    "Only a prevalidation-rejected candidate may carry a rejection reason.",
                    nameof(rejectionReason));
            }

            bool exceededSearchLimit =
                outcome == StageRouteCandidateOutcome.SearchBudgetExceeded;
            if (exceededSearchLimit !=
                (searchLimitKind != StageRouteSearchLimitKind.None))
            {
                throw new ArgumentException(
                    "Only a budget-exceeded candidate may carry a search limit kind.",
                    nameof(searchLimitKind));
            }

            if (outcome == StageRouteCandidateOutcome.NotAttempted &&
                (workUnits != 0 ||
                 aStarNodeExpansionCount != 0 ||
                 connectorAlternativeCount != 0 ||
                 backtrackCount != 0 ||
                 reachabilityCheckCount != 0 ||
                 reachabilityVisitedCellCount != 0))
            {
                throw new ArgumentException(
                    "A candidate that was not attempted cannot contain search work.",
                    nameof(workUnits));
            }

            CandidateIndex = candidateIndex;
            LayoutId = layoutId;
            PhysicalLayoutIndex = physicalLayoutIndex;
            VariantIndex = variantIndex;
            PhysicalDrawIndex = physicalDrawIndex;
            PassageOrderDrawIndex = passageOrderDrawIndex;
            Outcome = outcome;
            RejectionReason = rejectionReason;
            SearchLimitKind = searchLimitKind;
            WorkUnits = workUnits;
            AStarNodeExpansionCount = aStarNodeExpansionCount;
            ConnectorAlternativeCount = connectorAlternativeCount;
            BacktrackCount = backtrackCount;
            ReachabilityCheckCount = reachabilityCheckCount;
            ReachabilityVisitedCellCount = reachabilityVisitedCellCount;
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
            where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    public sealed class StageRouteGenerationDiagnostics
    {
        private readonly IReadOnlyList<StageRouteCandidateDiagnostic> candidates;

        public int PhysicalLayoutDrawCount { get; }
        public int PhysicalPlacementRejectedCount { get; }
        public int DuplicatePhysicalLayoutCount { get; }
        public int UnselectedPhysicalLayoutCount { get; }
        public int AcceptedPhysicalLayoutCount { get; }
        public int PassageOrderDrawCount { get; }
        public int DuplicatePassageOrderVariantCount { get; }
        public int AcceptedPassageOrderVariantCount { get; }
        public int LayoutsWithoutValidOrderCount { get; }
        public int GeneratedCandidateCount => candidates.Count;
        public int CandidatesAttempted { get; }
        public int CandidatesSearched { get; }
        public int CandidatesNotAttempted { get; }
        public int PrevalidationRejectedCandidateCount { get; }
        public int PathNotFoundCandidateCount { get; }
        public int PerCandidateBudgetExceededCount { get; }
        public bool TotalSearchBudgetExceeded { get; }
        public int AStarNodeExpansionCount { get; }
        public int ConnectorAlternativeCount { get; }
        public int BacktrackCount { get; }
        public int ReachabilityCheckCount { get; }
        public int ReachabilityVisitedCellCount { get; }
        public int TotalWorkUnits { get; }
        public int MaximumCandidateWorkUnits { get; }
        public int SelectedCandidateIndex { get; }
        public int SelectedPhysicalLayoutIndex { get; }
        public int SelectedVariantIndex { get; }
        public bool HasPreferredPatternComposition { get; }
        public int PreferredStraightPatternCount { get; }
        public int PreferredCornerPatternCount { get; }
        public int PreferredCrossPatternCount { get; }
        public IReadOnlyList<StageRouteCandidateDiagnostic> Candidates =>
            candidates;

        internal StageRouteGenerationDiagnostics(
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int acceptedPhysicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderVariantCount,
            int acceptedPassageOrderVariantCount,
            int layoutsWithoutValidOrderCount,
            int aStarNodeExpansionCount,
            int connectorAlternativeCount,
            int backtrackCount,
            int reachabilityCheckCount,
            int reachabilityVisitedCellCount,
            int totalWorkUnits,
            bool totalSearchBudgetExceeded,
            IReadOnlyList<StageRouteCandidateDiagnostic> sourceCandidates)
            : this(
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount,
                duplicatePhysicalLayoutCount,
                unselectedPhysicalLayoutCount: checked(
                    physicalLayoutDrawCount -
                    physicalPlacementRejectedCount -
                    duplicatePhysicalLayoutCount -
                    acceptedPhysicalLayoutCount),
                acceptedPhysicalLayoutCount,
                passageOrderDrawCount,
                duplicatePassageOrderVariantCount,
                acceptedPassageOrderVariantCount,
                layoutsWithoutValidOrderCount,
                aStarNodeExpansionCount,
                connectorAlternativeCount,
                backtrackCount,
                reachabilityCheckCount,
                reachabilityVisitedCellCount,
                totalWorkUnits,
                totalSearchBudgetExceeded,
                sourceCandidates)
        {
        }

        internal StageRouteGenerationDiagnostics(
            int physicalLayoutDrawCount,
            int physicalPlacementRejectedCount,
            int duplicatePhysicalLayoutCount,
            int unselectedPhysicalLayoutCount,
            int acceptedPhysicalLayoutCount,
            int passageOrderDrawCount,
            int duplicatePassageOrderVariantCount,
            int acceptedPassageOrderVariantCount,
            int layoutsWithoutValidOrderCount,
            int aStarNodeExpansionCount,
            int connectorAlternativeCount,
            int backtrackCount,
            int reachabilityCheckCount,
            int reachabilityVisitedCellCount,
            int totalWorkUnits,
            bool totalSearchBudgetExceeded,
            IReadOnlyList<StageRouteCandidateDiagnostic> sourceCandidates,
            bool hasPreferredPatternComposition = false,
            int preferredStraightPatternCount = 0,
            int preferredCornerPatternCount = 0,
            int preferredCrossPatternCount = 0)
        {
            EnsureNonNegative(
                physicalLayoutDrawCount,
                nameof(physicalLayoutDrawCount));
            EnsureNonNegative(
                physicalPlacementRejectedCount,
                nameof(physicalPlacementRejectedCount));
            EnsureNonNegative(
                duplicatePhysicalLayoutCount,
                nameof(duplicatePhysicalLayoutCount));
            EnsureNonNegative(
                unselectedPhysicalLayoutCount,
                nameof(unselectedPhysicalLayoutCount));
            EnsureNonNegative(
                acceptedPhysicalLayoutCount,
                nameof(acceptedPhysicalLayoutCount));
            EnsureNonNegative(
                passageOrderDrawCount,
                nameof(passageOrderDrawCount));
            EnsureNonNegative(
                duplicatePassageOrderVariantCount,
                nameof(duplicatePassageOrderVariantCount));
            EnsureNonNegative(
                acceptedPassageOrderVariantCount,
                nameof(acceptedPassageOrderVariantCount));
            EnsureNonNegative(
                layoutsWithoutValidOrderCount,
                nameof(layoutsWithoutValidOrderCount));
            EnsureNonNegative(
                aStarNodeExpansionCount,
                nameof(aStarNodeExpansionCount));
            EnsureNonNegative(
                connectorAlternativeCount,
                nameof(connectorAlternativeCount));
            EnsureNonNegative(backtrackCount, nameof(backtrackCount));
            EnsureNonNegative(
                reachabilityCheckCount,
                nameof(reachabilityCheckCount));
            EnsureNonNegative(
                reachabilityVisitedCellCount,
                nameof(reachabilityVisitedCellCount));
            EnsureNonNegative(totalWorkUnits, nameof(totalWorkUnits));
            EnsureNonNegative(
                preferredStraightPatternCount,
                nameof(preferredStraightPatternCount));
            EnsureNonNegative(
                preferredCornerPatternCount,
                nameof(preferredCornerPatternCount));
            EnsureNonNegative(
                preferredCrossPatternCount,
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
                acceptedPhysicalLayoutCount;
            if (accountedPhysicalDrawCount != physicalLayoutDrawCount)
            {
                throw new ArgumentException(
                    "Physical draws must equal rejected, duplicate, " +
                    "unselected, and accepted physical layout counts.");
            }

            if ((long)duplicatePassageOrderVariantCount +
                acceptedPassageOrderVariantCount > passageOrderDrawCount)
            {
                throw new ArgumentException(
                    "Passage-order counters exceed the recorded draw count.");
            }

            if (layoutsWithoutValidOrderCount > acceptedPhysicalLayoutCount)
            {
                throw new ArgumentException(
                    "Layouts without an order cannot exceed accepted physical layouts.");
            }

            if (sourceCandidates == null)
            {
                throw new ArgumentNullException(nameof(sourceCandidates));
            }

            StageRouteCandidateDiagnostic[] copies =
                new StageRouteCandidateDiagnostic[sourceCandidates.Count];
            int attemptedCount = 0;
            int searchedCount = 0;
            int notAttemptedCount = 0;
            int rejectedCount = 0;
            int pathNotFoundCount = 0;
            int candidateBudgetCount = 0;
            bool candidateTotalBudgetExceeded = false;
            int maximumWorkUnits = 0;
            int calculatedAStarNodeExpansionCount = 0;
            int calculatedConnectorAlternativeCount = 0;
            int calculatedBacktrackCount = 0;
            int calculatedReachabilityCheckCount = 0;
            int calculatedReachabilityVisitedCellCount = 0;
            int calculatedTotalWorkUnits = 0;
            StageRouteCandidateDiagnostic selectedCandidate = null;
            HashSet<string> layoutIds = new(StringComparer.Ordinal);
            HashSet<PhysicalVariantKey> physicalVariants = new();
            Dictionary<int, int> drawIndexByPhysicalLayout = new();

            for (int index = 0; index < sourceCandidates.Count; index++)
            {
                StageRouteCandidateDiagnostic candidate =
                    sourceCandidates[index] ?? throw new ArgumentException(
                        $"Candidate diagnostic index {index} is null.",
                        nameof(sourceCandidates));
                if (candidate.CandidateIndex != index)
                {
                    throw new ArgumentException(
                        $"Candidate diagnostic index {candidate.CandidateIndex} " +
                        $"must match its collection index {index}.",
                        nameof(sourceCandidates));
                }

                if (candidate.PhysicalLayoutIndex >=
                    acceptedPhysicalLayoutCount)
                {
                    throw new ArgumentException(
                        $"Candidate physical layout index " +
                        $"{candidate.PhysicalLayoutIndex} is outside the " +
                        $"accepted physical layout count " +
                        $"{acceptedPhysicalLayoutCount}.",
                        nameof(sourceCandidates));
                }

                if (candidate.PhysicalDrawIndex >= physicalLayoutDrawCount)
                {
                    throw new ArgumentException(
                        $"Candidate physical draw index " +
                        $"{candidate.PhysicalDrawIndex} is outside the draw " +
                        $"count {physicalLayoutDrawCount}.",
                        nameof(sourceCandidates));
                }

                if (candidate.VariantIndex >=
                    acceptedPassageOrderVariantCount)
                {
                    throw new ArgumentException(
                        $"Candidate variant index {candidate.VariantIndex} is " +
                        $"outside the accepted variant count " +
                        $"{acceptedPassageOrderVariantCount}.",
                        nameof(sourceCandidates));
                }

                if (candidate.PassageOrderDrawIndex >= passageOrderDrawCount)
                {
                    throw new ArgumentException(
                        $"Candidate passage-order draw index " +
                        $"{candidate.PassageOrderDrawIndex} is outside the " +
                        $"draw count {passageOrderDrawCount}.",
                        nameof(sourceCandidates));
                }

                if (!layoutIds.Add(candidate.LayoutId))
                {
                    throw new ArgumentException(
                        $"Candidate layout ID '{candidate.LayoutId}' is duplicated.",
                        nameof(sourceCandidates));
                }

                PhysicalVariantKey physicalVariant = new(
                    candidate.PhysicalLayoutIndex,
                    candidate.VariantIndex);
                if (!physicalVariants.Add(physicalVariant))
                {
                    throw new ArgumentException(
                        $"Physical layout {candidate.PhysicalLayoutIndex} " +
                        $"contains more than one diagnostic for variant " +
                        $"{candidate.VariantIndex}.",
                        nameof(sourceCandidates));
                }

                if (drawIndexByPhysicalLayout.TryGetValue(
                        candidate.PhysicalLayoutIndex,
                        out int existingDrawIndex) &&
                    existingDrawIndex != candidate.PhysicalDrawIndex)
                {
                    throw new ArgumentException(
                        $"Physical layout {candidate.PhysicalLayoutIndex} maps " +
                        "to more than one physical draw index.",
                        nameof(sourceCandidates));
                }

                drawIndexByPhysicalLayout[candidate.PhysicalLayoutIndex] =
                    candidate.PhysicalDrawIndex;

                copies[index] = candidate;
                maximumWorkUnits = Math.Max(
                    maximumWorkUnits,
                    candidate.WorkUnits);
                calculatedAStarNodeExpansionCount = checked(
                    calculatedAStarNodeExpansionCount +
                    candidate.AStarNodeExpansionCount);
                calculatedConnectorAlternativeCount = checked(
                    calculatedConnectorAlternativeCount +
                    candidate.ConnectorAlternativeCount);
                calculatedBacktrackCount = checked(
                    calculatedBacktrackCount + candidate.BacktrackCount);
                calculatedReachabilityCheckCount = checked(
                    calculatedReachabilityCheckCount +
                    candidate.ReachabilityCheckCount);
                calculatedReachabilityVisitedCellCount = checked(
                    calculatedReachabilityVisitedCellCount +
                    candidate.ReachabilityVisitedCellCount);
                calculatedTotalWorkUnits = checked(
                    calculatedTotalWorkUnits + candidate.WorkUnits);
                switch (candidate.Outcome)
                {
                    case StageRouteCandidateOutcome.NotAttempted:
                        notAttemptedCount++;
                        break;
                    case StageRouteCandidateOutcome.RejectedByPrevalidation:
                        attemptedCount++;
                        rejectedCount++;
                        break;
                    case StageRouteCandidateOutcome.PathNotFound:
                        attemptedCount++;
                        searchedCount++;
                        pathNotFoundCount++;
                        break;
                    case StageRouteCandidateOutcome.SearchBudgetExceeded:
                        attemptedCount++;
                        searchedCount++;
                        if (candidate.SearchLimitKind ==
                            StageRouteSearchLimitKind.TotalWork)
                        {
                            candidateTotalBudgetExceeded = true;
                        }
                        else
                        {
                            candidateBudgetCount++;
                        }

                        break;
                    case StageRouteCandidateOutcome.Succeeded:
                        attemptedCount++;
                        searchedCount++;
                        if (selectedCandidate != null)
                        {
                            throw new ArgumentException(
                                "Diagnostics cannot contain more than one successful candidate.",
                                nameof(sourceCandidates));
                        }

                        selectedCandidate = candidate;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(sourceCandidates),
                            candidate.Outcome,
                            "Candidate outcome is not defined.");
                }
            }

            if (sourceCandidates.Count > acceptedPassageOrderVariantCount)
            {
                throw new ArgumentException(
                    "Candidate diagnostics cannot exceed accepted passage-order variants.",
                    nameof(sourceCandidates));
            }

            if (aStarNodeExpansionCount != calculatedAStarNodeExpansionCount ||
                connectorAlternativeCount !=
                calculatedConnectorAlternativeCount ||
                backtrackCount != calculatedBacktrackCount ||
                reachabilityCheckCount != calculatedReachabilityCheckCount ||
                reachabilityVisitedCellCount !=
                calculatedReachabilityVisitedCellCount ||
                totalWorkUnits != calculatedTotalWorkUnits)
            {
                throw new ArgumentException(
                    "Aggregate route diagnostics must equal the sum of their " +
                    "candidate diagnostics.",
                    nameof(sourceCandidates));
            }

            PhysicalLayoutDrawCount = physicalLayoutDrawCount;
            PhysicalPlacementRejectedCount = physicalPlacementRejectedCount;
            DuplicatePhysicalLayoutCount = duplicatePhysicalLayoutCount;
            UnselectedPhysicalLayoutCount = unselectedPhysicalLayoutCount;
            AcceptedPhysicalLayoutCount = acceptedPhysicalLayoutCount;
            PassageOrderDrawCount = passageOrderDrawCount;
            DuplicatePassageOrderVariantCount =
                duplicatePassageOrderVariantCount;
            AcceptedPassageOrderVariantCount =
                acceptedPassageOrderVariantCount;
            LayoutsWithoutValidOrderCount = layoutsWithoutValidOrderCount;
            AStarNodeExpansionCount = aStarNodeExpansionCount;
            ConnectorAlternativeCount = connectorAlternativeCount;
            BacktrackCount = backtrackCount;
            ReachabilityCheckCount = reachabilityCheckCount;
            ReachabilityVisitedCellCount = reachabilityVisitedCellCount;
            TotalWorkUnits = totalWorkUnits;
            MaximumCandidateWorkUnits = maximumWorkUnits;
            CandidatesAttempted = attemptedCount;
            CandidatesSearched = searchedCount;
            CandidatesNotAttempted = notAttemptedCount;
            PrevalidationRejectedCandidateCount = rejectedCount;
            PathNotFoundCandidateCount = pathNotFoundCount;
            PerCandidateBudgetExceededCount = candidateBudgetCount;
            TotalSearchBudgetExceeded =
                totalSearchBudgetExceeded || candidateTotalBudgetExceeded;
            SelectedCandidateIndex = selectedCandidate?.CandidateIndex ?? -1;
            SelectedPhysicalLayoutIndex =
                selectedCandidate?.PhysicalLayoutIndex ?? -1;
            SelectedVariantIndex = selectedCandidate?.VariantIndex ?? -1;
            HasPreferredPatternComposition = hasPreferredPatternComposition;
            PreferredStraightPatternCount = preferredStraightPatternCount;
            PreferredCornerPatternCount = preferredCornerPatternCount;
            PreferredCrossPatternCount = preferredCrossPatternCount;
            candidates = Array.AsReadOnly(copies);
        }

        public int GetPrevalidationRejectionCount(
            StageRouteCandidateRejectionReason reason)
        {
            if (reason == StageRouteCandidateRejectionReason.None ||
                !Enum.IsDefined(
                    typeof(StageRouteCandidateRejectionReason),
                    reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            int count = 0;
            foreach (StageRouteCandidateDiagnostic candidate in candidates)
            {
                if (candidate.RejectionReason == reason)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetSearchLimitCount(StageRouteSearchLimitKind limitKind)
        {
            if (limitKind == StageRouteSearchLimitKind.None ||
                !Enum.IsDefined(typeof(StageRouteSearchLimitKind), limitKind))
            {
                throw new ArgumentOutOfRangeException(nameof(limitKind));
            }

            int count = 0;
            foreach (StageRouteCandidateDiagnostic candidate in candidates)
            {
                if (candidate.SearchLimitKind == limitKind)
                {
                    count++;
                }
            }

            return count;
        }

        internal static StageRouteGenerationDiagnostics CreateLegacy(
            bool succeeded,
            StageRouteGenerationFailureReason failureReason,
            int attemptsUsed,
            int patternDrawCount,
            int searchNodesVisited)
        {
            StageRouteCandidateDiagnostic[] legacyCandidates =
                new StageRouteCandidateDiagnostic[attemptsUsed];
            for (int index = 0; index < attemptsUsed; index++)
            {
                bool isLastCandidate = index == attemptsUsed - 1;
                StageRouteCandidateOutcome outcome =
                    succeeded && isLastCandidate
                        ? StageRouteCandidateOutcome.Succeeded
                        : failureReason ==
                          StageRouteGenerationFailureReason.SearchBudgetExceeded ||
                          failureReason ==
                          StageRouteGenerationFailureReason
                              .TotalSearchBudgetExceeded
                            ? StageRouteCandidateOutcome.SearchBudgetExceeded
                            : StageRouteCandidateOutcome.PathNotFound;
                StageRouteSearchLimitKind limitKind =
                    outcome == StageRouteCandidateOutcome.SearchBudgetExceeded
                        ? failureReason ==
                          StageRouteGenerationFailureReason
                              .TotalSearchBudgetExceeded
                            ? StageRouteSearchLimitKind.TotalWork
                            : StageRouteSearchLimitKind.PerCandidateWork
                        : StageRouteSearchLimitKind.None;
                int candidateWork = isLastCandidate ? searchNodesVisited : 0;
                legacyCandidates[index] = new StageRouteCandidateDiagnostic(
                    index,
                    $"legacy-{index}",
                    index,
                    variantIndex: 0,
                    physicalDrawIndex: index,
                    passageOrderDrawIndex: index,
                    outcome,
                    StageRouteCandidateRejectionReason.None,
                    limitKind,
                    candidateWork,
                    candidateWork,
                    connectorAlternativeCount: 0,
                    backtrackCount: 0,
                    reachabilityCheckCount: 0,
                    reachabilityVisitedCellCount: 0);
            }

            return new StageRouteGenerationDiagnostics(
                patternDrawCount,
                physicalPlacementRejectedCount: checked(
                    patternDrawCount - attemptsUsed),
                duplicatePhysicalLayoutCount: 0,
                unselectedPhysicalLayoutCount: 0,
                acceptedPhysicalLayoutCount: attemptsUsed,
                passageOrderDrawCount: attemptsUsed,
                duplicatePassageOrderVariantCount: 0,
                acceptedPassageOrderVariantCount: attemptsUsed,
                layoutsWithoutValidOrderCount: 0,
                aStarNodeExpansionCount: searchNodesVisited,
                connectorAlternativeCount: 0,
                backtrackCount: 0,
                reachabilityCheckCount: 0,
                reachabilityVisitedCellCount: 0,
                totalWorkUnits: searchNodesVisited,
                totalSearchBudgetExceeded:
                    failureReason == StageRouteGenerationFailureReason
                        .TotalSearchBudgetExceeded,
                sourceCandidates: legacyCandidates);
        }

        internal static StageRouteGenerationDiagnostics Empty(
            int physicalLayoutDrawCount = 0)
        {
            return new StageRouteGenerationDiagnostics(
                physicalLayoutDrawCount,
                physicalPlacementRejectedCount: 0,
                duplicatePhysicalLayoutCount: 0,
                acceptedPhysicalLayoutCount: 0,
                passageOrderDrawCount: 0,
                duplicatePassageOrderVariantCount: 0,
                acceptedPassageOrderVariantCount: 0,
                layoutsWithoutValidOrderCount: 0,
                aStarNodeExpansionCount: 0,
                connectorAlternativeCount: 0,
                backtrackCount: 0,
                reachabilityCheckCount: 0,
                reachabilityVisitedCellCount: 0,
                totalWorkUnits: 0,
                totalSearchBudgetExceeded: false,
                sourceCandidates: Array.Empty<StageRouteCandidateDiagnostic>());
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
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
