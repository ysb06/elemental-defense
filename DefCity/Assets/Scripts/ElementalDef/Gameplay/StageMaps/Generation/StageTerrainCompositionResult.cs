using System;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class StageTerrainCompositionResult
    {
        public int TotalCellCount { get; }
        public int NeutralCellCount { get; }
        public int WaterCellCount { get; }
        public int FireCellCount { get; }
        public int EarthCellCount { get; }

        public double NeutralRatio => GetRatio(NeutralCellCount);
        public double WaterRatio => GetRatio(WaterCellCount);
        public double FireRatio => GetRatio(FireCellCount);
        public double EarthRatio => GetRatio(EarthCellCount);

        internal StageTerrainCompositionResult(
            int totalCellCount,
            int neutralCellCount,
            int waterCellCount,
            int fireCellCount,
            int earthCellCount)
        {
            if (totalCellCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCellCount));
            }

            EnsureNonNegative(neutralCellCount, nameof(neutralCellCount));
            EnsureNonNegative(waterCellCount, nameof(waterCellCount));
            EnsureNonNegative(fireCellCount, nameof(fireCellCount));
            EnsureNonNegative(earthCellCount, nameof(earthCellCount));

            int countedCellCount = checked(
                neutralCellCount +
                waterCellCount +
                fireCellCount +
                earthCellCount);
            if (countedCellCount != totalCellCount)
            {
                throw new ArgumentException(
                    $"The terrain counts total {countedCellCount}, but " +
                    $"the expected cell count is {totalCellCount}.");
            }

            TotalCellCount = totalCellCount;
            NeutralCellCount = neutralCellCount;
            WaterCellCount = waterCellCount;
            FireCellCount = fireCellCount;
            EarthCellCount = earthCellCount;
        }

        private double GetRatio(int cellCount)
        {
            return cellCount / (double)TotalCellCount;
        }

        private static void EnsureNonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
