using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public enum StageMapGenerationFailureReason
    {
        None = 0,
        RouteGenerationFailed = 1,
        InsufficientGroundCells = 2,
        InvalidElementPlacement = 3,
        BlockedCellTargetNotReached = 4,
        InvalidBlockedCellPlacement = 5,
        ValidationFailed = 6,
    }

    public sealed class StageMapGenerationResult
    {
        public bool Succeeded =>
            FailureReason == StageMapGenerationFailureReason.None;
        public GeneratedStageMap Map { get; }
        public StageRouteGenerationResult RouteResult { get; }
        public StageMapValidationReport ValidationReport { get; }
        public StageMapGenerationFailureReason FailureReason { get; }
        public string Message { get; }
        public int GroundCellCount { get; }
        public int RequestedBlockedCellCount { get; }
        public int BlockedCellCount { get; }

        private StageMapGenerationResult(
            GeneratedStageMap map,
            StageRouteGenerationResult routeResult,
            StageMapValidationReport validationReport,
            StageMapGenerationFailureReason failureReason,
            string message,
            int groundCellCount,
            int requestedBlockedCellCount,
            int blockedCellCount)
        {
            RouteResult = routeResult ??
                throw new ArgumentNullException(nameof(routeResult));

            if (!Enum.IsDefined(
                    typeof(StageMapGenerationFailureReason),
                    failureReason))
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            if (groundCellCount < 0 ||
                requestedBlockedCellCount < 0 ||
                blockedCellCount < 0 ||
                requestedBlockedCellCount > groundCellCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(groundCellCount),
                    "Generation cell counts are inconsistent.");
            }

            if (failureReason == StageMapGenerationFailureReason.None)
            {
                Map = map ?? throw new ArgumentNullException(nameof(map));
                ValidationReport = validationReport ??
                    throw new ArgumentNullException(nameof(validationReport));
                if (!validationReport.IsValid)
                {
                    throw new ArgumentException(
                        "A successful map result requires a valid report.",
                        nameof(validationReport));
                }

                if (blockedCellCount > groundCellCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(blockedCellCount),
                        "A successful result cannot block more cells than exist.");
                }
            }
            else
            {
                if (map != null)
                {
                    throw new ArgumentException(
                        "A failed map result cannot contain a map.",
                        nameof(map));
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentException(
                        "A failed map result requires a message.",
                        nameof(message));
                }

                ValidationReport = validationReport;
            }

            FailureReason = failureReason;
            Message = string.IsNullOrWhiteSpace(message)
                ? "The stage map was generated successfully."
                : message;
            GroundCellCount = groundCellCount;
            RequestedBlockedCellCount = requestedBlockedCellCount;
            BlockedCellCount = blockedCellCount;
        }

        internal static StageMapGenerationResult Success(
            GeneratedStageMap map,
            StageRouteGenerationResult routeResult,
            StageMapValidationReport validationReport,
            int groundCellCount,
            int requestedBlockedCellCount,
            int blockedCellCount)
        {
            return new StageMapGenerationResult(
                map,
                routeResult,
                validationReport,
                StageMapGenerationFailureReason.None,
                "The stage map was generated successfully.",
                groundCellCount,
                requestedBlockedCellCount,
                blockedCellCount);
        }

        internal static StageMapGenerationResult Failure(
            StageRouteGenerationResult routeResult,
            StageMapGenerationFailureReason failureReason,
            string message,
            int groundCellCount = 0,
            int requestedBlockedCellCount = 0,
            int blockedCellCount = 0,
            StageMapValidationReport validationReport = null)
        {
            if (failureReason == StageMapGenerationFailureReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            return new StageMapGenerationResult(
                null,
                routeResult,
                validationReport,
                failureReason,
                message,
                groundCellCount,
                requestedBlockedCellCount,
                blockedCellCount);
        }
    }
}
