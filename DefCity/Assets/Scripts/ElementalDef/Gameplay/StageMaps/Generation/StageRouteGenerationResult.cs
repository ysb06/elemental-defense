using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public enum StageRouteGenerationFailureReason
    {
        None = 0,
        NoFeasiblePatternLayout = 1,
        NoValidPassageOrder = 2,
        PathNotFound = 3,
        SearchBudgetExceeded = 4,
        TotalSearchBudgetExceeded = 5,
    }

    public sealed class StageRouteGenerationResult
    {
        public bool Succeeded => FailureReason == StageRouteGenerationFailureReason.None;
        public GeneratedStageRoute Route { get; }
        public StageRouteGenerationFailureReason FailureReason { get; }
        public string Message { get; }
        public int AttemptsUsed { get; }
        public int PatternDrawCount { get; }
        public int SearchNodesVisited { get; }
        public StageRouteGenerationDiagnostics Diagnostics { get; }

        private StageRouteGenerationResult(
            GeneratedStageRoute route,
            StageRouteGenerationFailureReason failureReason,
            string message,
            int attemptsUsed,
            int patternDrawCount,
            int searchNodesVisited,
            StageRouteGenerationDiagnostics diagnostics)
        {
            if (attemptsUsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(attemptsUsed));
            }

            if (patternDrawCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patternDrawCount));
            }

            if (searchNodesVisited < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(searchNodesVisited));
            }

            if (failureReason == StageRouteGenerationFailureReason.None)
            {
                Route = route ?? throw new ArgumentNullException(nameof(route));
                Message = string.IsNullOrWhiteSpace(message)
                    ? "The stage route was generated successfully."
                    : message;
            }
            else
            {
                if (!Enum.IsDefined(typeof(StageRouteGenerationFailureReason), failureReason))
                {
                    throw new ArgumentOutOfRangeException(nameof(failureReason));
                }

                if (route != null)
                {
                    throw new ArgumentException(
                        "A failed route generation result cannot contain a route.",
                        nameof(route));
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentException(
                        "A failed route generation result requires a message.",
                        nameof(message));
                }

                Message = message;
            }

            FailureReason = failureReason;
            AttemptsUsed = attemptsUsed;
            PatternDrawCount = patternDrawCount;
            SearchNodesVisited = searchNodesVisited;
            Diagnostics = diagnostics ??
                throw new ArgumentNullException(nameof(diagnostics));
        }

        internal static StageRouteGenerationResult Success(
            GeneratedStageRoute route,
            int attemptsUsed,
            int patternDrawCount,
            int searchNodesVisited)
        {
            return new StageRouteGenerationResult(
                route,
                StageRouteGenerationFailureReason.None,
                "The stage route was generated successfully.",
                attemptsUsed,
                patternDrawCount,
                searchNodesVisited,
                StageRouteGenerationDiagnostics.CreateLegacy(
                    succeeded: true,
                    StageRouteGenerationFailureReason.None,
                    attemptsUsed,
                    patternDrawCount,
                    searchNodesVisited));
        }

        internal static StageRouteGenerationResult Success(
            GeneratedStageRoute route,
            StageRouteGenerationDiagnostics diagnostics)
        {
            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (diagnostics.SelectedCandidateIndex < 0)
            {
                throw new ArgumentException(
                    "Successful route diagnostics require a selected candidate.",
                    nameof(diagnostics));
            }

            return new StageRouteGenerationResult(
                route,
                StageRouteGenerationFailureReason.None,
                "The stage route was generated successfully.",
                diagnostics.CandidatesAttempted,
                diagnostics.PhysicalLayoutDrawCount,
                diagnostics.AStarNodeExpansionCount,
                diagnostics);
        }

        internal static StageRouteGenerationResult Failure(
            StageRouteGenerationFailureReason failureReason,
            string message,
            int attemptsUsed,
            int patternDrawCount,
            int searchNodesVisited)
        {
            if (failureReason == StageRouteGenerationFailureReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            return new StageRouteGenerationResult(
                null,
                failureReason,
                message,
                attemptsUsed,
                patternDrawCount,
                searchNodesVisited,
                StageRouteGenerationDiagnostics.CreateLegacy(
                    succeeded: false,
                    failureReason,
                    attemptsUsed,
                    patternDrawCount,
                    searchNodesVisited));
        }

        internal static StageRouteGenerationResult Failure(
            StageRouteGenerationFailureReason failureReason,
            string message,
            StageRouteGenerationDiagnostics diagnostics)
        {
            if (failureReason == StageRouteGenerationFailureReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (diagnostics.SelectedCandidateIndex >= 0)
            {
                throw new ArgumentException(
                    "Failed route diagnostics cannot contain a selected candidate.",
                    nameof(diagnostics));
            }

            return new StageRouteGenerationResult(
                null,
                failureReason,
                message,
                diagnostics.CandidatesAttempted,
                diagnostics.PhysicalLayoutDrawCount,
                diagnostics.AStarNodeExpansionCount,
                diagnostics);
        }
    }
}
