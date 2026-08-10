using ElementalDef.Gameplay.StageMaps.Generation;
using NUnit.Framework;
using UnityEngine;

namespace ElementalDef.Tests.Editor
{
    public sealed class StageTerrainCompositionEstimatorTests
    {
        private StageMapGenerationProfile profile;

        [SetUp]
        public void SetUp()
        {
            profile = ScriptableObject.CreateInstance<StageMapGenerationProfile>();
            profile.name = "Stage Terrain Composition Test Profile";
        }

        [TearDown]
        public void TearDown()
        {
            if (profile != null)
            {
                Object.DestroyImmediate(profile);
                profile = null;
            }
        }

        [Test]
        public void Estimate_SameProfileAndSeed_ReturnsSameComposition()
        {
            StageTerrainCompositionResult first =
                StageTerrainCompositionEstimator.Estimate(profile, seed: 1023);
            StageTerrainCompositionResult second =
                StageTerrainCompositionEstimator.Estimate(profile, seed: 1023);

            Assert.That(second.TotalCellCount, Is.EqualTo(first.TotalCellCount));
            Assert.That(second.NeutralCellCount, Is.EqualTo(first.NeutralCellCount));
            Assert.That(second.WaterCellCount, Is.EqualTo(first.WaterCellCount));
            Assert.That(second.FireCellCount, Is.EqualTo(first.FireCellCount));
            Assert.That(second.EarthCellCount, Is.EqualTo(first.EarthCellCount));
            Assert.That(second.NeutralRatio, Is.EqualTo(first.NeutralRatio));
            Assert.That(second.WaterRatio, Is.EqualTo(first.WaterRatio));
            Assert.That(second.FireRatio, Is.EqualTo(first.FireRatio));
            Assert.That(second.EarthRatio, Is.EqualTo(first.EarthRatio));
        }

        [Test]
        public void Estimate_UsesEntireBoundsAndIncludesNeutralInRatioDenominator()
        {
            StageTerrainCompositionResult result =
                StageTerrainCompositionEstimator.Estimate(profile, seed: 1001);
            int expectedCellCount = checked(
                profile.Bounds.width * profile.Bounds.height);

            Assert.That(result.TotalCellCount, Is.EqualTo(expectedCellCount));
            Assert.That(
                result.NeutralCellCount +
                result.WaterCellCount +
                result.FireCellCount +
                result.EarthCellCount,
                Is.EqualTo(expectedCellCount));
            Assert.That(result.NeutralCellCount, Is.GreaterThan(0));
            Assert.That(
                result.NeutralRatio,
                Is.EqualTo(result.NeutralCellCount / (double)expectedCellCount)
                    .Within(1e-12d));
            Assert.That(
                result.WaterRatio,
                Is.EqualTo(result.WaterCellCount / (double)expectedCellCount)
                    .Within(1e-12d));
            Assert.That(
                result.FireRatio,
                Is.EqualTo(result.FireCellCount / (double)expectedCellCount)
                    .Within(1e-12d));
            Assert.That(
                result.EarthRatio,
                Is.EqualTo(result.EarthCellCount / (double)expectedCellCount)
                    .Within(1e-12d));
            Assert.That(
                result.NeutralRatio +
                result.WaterRatio +
                result.FireRatio +
                result.EarthRatio,
                Is.EqualTo(1d).Within(1e-12d));
        }
    }
}
