using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Settings;
using NUnit.Framework;
using UnityEngine;

namespace ElementalDef.Tests.Editor
{
    public sealed class TerrainModifierTests
    {
        private const float Tolerance = 0.0001f;

        private TerrainModifier terrainModifier;

        [SetUp]
        public void SetUp()
        {
            terrainModifier = ScriptableObject.CreateInstance<TerrainModifier>();
        }

        [TearDown]
        public void TearDown()
        {
            if (terrainModifier != null)
            {
                Object.DestroyImmediate(terrainModifier);
                terrainModifier = null;
            }
        }

        [TestCase(ElementType.Neutral, ElementType.Neutral, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Neutral, ElementType.Water, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Neutral, ElementType.Fire, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Neutral, ElementType.Earth, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Water, ElementType.Neutral, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Water, ElementType.Water, TerrainRelationship.Synergy)]
        [TestCase(ElementType.Water, ElementType.Fire, TerrainRelationship.Synergy)]
        [TestCase(ElementType.Water, ElementType.Earth, TerrainRelationship.Disadvantage)]
        [TestCase(ElementType.Fire, ElementType.Neutral, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Fire, ElementType.Water, TerrainRelationship.Disadvantage)]
        [TestCase(ElementType.Fire, ElementType.Fire, TerrainRelationship.Synergy)]
        [TestCase(ElementType.Fire, ElementType.Earth, TerrainRelationship.Synergy)]
        [TestCase(ElementType.Earth, ElementType.Neutral, TerrainRelationship.Neutral)]
        [TestCase(ElementType.Earth, ElementType.Water, TerrainRelationship.Synergy)]
        [TestCase(ElementType.Earth, ElementType.Fire, TerrainRelationship.Disadvantage)]
        [TestCase(ElementType.Earth, ElementType.Earth, TerrainRelationship.Synergy)]
        public void GetRelationship_ReturnsApprovedMatrix(
            ElementType combatantElement,
            ElementType terrainElement,
            TerrainRelationship expectedRelationship)
        {
            TerrainRelationship actualRelationship =
                terrainModifier.GetRelationship(combatantElement, terrainElement);

            Assert.That(actualRelationship, Is.EqualTo(expectedRelationship));
        }

        [Test]
        public void DefaultMultipliers_MatchApprovedSettings()
        {
            Assert.That(
                terrainModifier.SameElementAttackMultiplier,
                Is.EqualTo(1.15f).Within(Tolerance));
            Assert.That(
                terrainModifier.SameElementDefenseMultiplier,
                Is.EqualTo(1.10f).Within(Tolerance));
            Assert.That(
                terrainModifier.NeutralAttackMultiplier,
                Is.EqualTo(1.00f).Within(Tolerance));
            Assert.That(
                terrainModifier.NeutralDefenseMultiplier,
                Is.EqualTo(1.00f).Within(Tolerance));
            Assert.That(
                terrainModifier.DisadvantageAttackMultiplier,
                Is.EqualTo(0.85f).Within(Tolerance));
            Assert.That(
                terrainModifier.DisadvantageDefenseMultiplier,
                Is.EqualTo(0.90f).Within(Tolerance));
        }

        [TestCase(TerrainRelationship.Neutral, 1.00f, 1.00f)]
        [TestCase(TerrainRelationship.Synergy, 1.15f, 1.10f)]
        [TestCase(TerrainRelationship.Disadvantage, 0.85f, 0.90f)]
        public void GetMultipliers_ReturnValuesForResolvedRelationship(
            TerrainRelationship relationship,
            float expectedAttackMultiplier,
            float expectedDefenseMultiplier)
        {
            GetElementsForRelationship(
                relationship,
                out ElementType combatantElement,
                out ElementType terrainElement);

            Assert.That(
                terrainModifier.GetAttackMultiplier(combatantElement, terrainElement),
                Is.EqualTo(expectedAttackMultiplier).Within(Tolerance));
            Assert.That(
                terrainModifier.GetDefenseMultiplier(combatantElement, terrainElement),
                Is.EqualTo(expectedDefenseMultiplier).Within(Tolerance));
        }

        private static void GetElementsForRelationship(
            TerrainRelationship relationship,
            out ElementType combatantElement,
            out ElementType terrainElement)
        {
            switch (relationship)
            {
                case TerrainRelationship.Synergy:
                    combatantElement = ElementType.Water;
                    terrainElement = ElementType.Fire;
                    break;
                case TerrainRelationship.Disadvantage:
                    combatantElement = ElementType.Water;
                    terrainElement = ElementType.Earth;
                    break;
                default:
                    combatantElement = ElementType.Neutral;
                    terrainElement = ElementType.Water;
                    break;
            }
        }
    }
}
