using System;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public enum StageDecorationGenerationFailureReason
    {
        None = 0,
        NoElementalSourceCells = 1,
        InvalidExpansionResult = 2,
    }

    public sealed class StageDecorationGenerationResult
    {
        public bool Succeeded =>
            FailureReason == StageDecorationGenerationFailureReason.None;
        public GeneratedStageDecoration Decoration { get; }
        public StageDecorationGenerationFailureReason FailureReason { get; }
        public string Message { get; }

        private StageDecorationGenerationResult(
            GeneratedStageDecoration decoration,
            StageDecorationGenerationFailureReason failureReason,
            string message)
        {
            if (!Enum.IsDefined(
                    typeof(StageDecorationGenerationFailureReason),
                    failureReason))
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            if (failureReason == StageDecorationGenerationFailureReason.None)
            {
                Decoration = decoration ??
                    throw new ArgumentNullException(nameof(decoration));
            }
            else
            {
                if (decoration != null)
                {
                    throw new ArgumentException(
                        "A failed decoration result cannot contain decoration data.",
                        nameof(decoration));
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentException(
                        "A failed decoration result requires a message.",
                        nameof(message));
                }
            }

            FailureReason = failureReason;
            Message = string.IsNullOrWhiteSpace(message)
                ? "The stage decoration was generated successfully."
                : message;
        }

        internal static StageDecorationGenerationResult Success(
            GeneratedStageDecoration decoration)
        {
            return new StageDecorationGenerationResult(
                decoration,
                StageDecorationGenerationFailureReason.None,
                "The stage decoration was generated successfully.");
        }

        internal static StageDecorationGenerationResult Failure(
            StageDecorationGenerationFailureReason failureReason,
            string message)
        {
            if (failureReason == StageDecorationGenerationFailureReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failureReason));
            }

            return new StageDecorationGenerationResult(
                null,
                failureReason,
                message);
        }
    }
}
