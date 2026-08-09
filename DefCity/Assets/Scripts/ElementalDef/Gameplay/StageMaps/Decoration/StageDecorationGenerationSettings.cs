using System;

namespace ElementalDef.Gameplay.StageMaps.Decoration
{
    public sealed class StageDecorationGenerationSettings
    {
        public const int DefaultOuterPadding = 3;
        public const bool DefaultGenerateGroundDecoration = false;

        public int OuterPadding { get; }
        public bool GenerateGroundDecoration { get; }

        public StageDecorationGenerationSettings(
            int outerPadding = DefaultOuterPadding,
            bool generateGroundDecoration =
                DefaultGenerateGroundDecoration)
        {
            if (outerPadding < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(outerPadding),
                    outerPadding,
                    "The stage decoration outer padding cannot be negative.");
            }

            OuterPadding = outerPadding;
            GenerateGroundDecoration = generateGroundDecoration;
        }
    }
}
