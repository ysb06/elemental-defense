using System;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Combat/Terrain Modifier")]
    public sealed class TerrainModifier : ScriptableObject
    {
        [SerializeField, Min(0f)]
        private float sameElementAttackMultiplier = 1.15f;
        [SerializeField, Min(0f)]
        private float sameElementDefenseMultiplier = 1.10f;
        [SerializeField, Min(0f)]
        private float neutralAttackMultiplier = 1.00f;
        [SerializeField, Min(0f)]
        private float neutralDefenseMultiplier = 1.00f;
        [SerializeField, Min(0f)]
        private float disadvantageAttackMultiplier = 0.85f;
        [SerializeField, Min(0f)]
        private float disadvantageDefenseMultiplier = 0.90f;

        public float SameElementAttackMultiplier => sameElementAttackMultiplier;
        public float SameElementDefenseMultiplier => sameElementDefenseMultiplier;
        public float NeutralAttackMultiplier => neutralAttackMultiplier;
        public float NeutralDefenseMultiplier => neutralDefenseMultiplier;
        public float DisadvantageAttackMultiplier => disadvantageAttackMultiplier;
        public float DisadvantageDefenseMultiplier => disadvantageDefenseMultiplier;

        public float GetAttackMultiplier(
            ElementType combatantElement,
            ElementType terrainElement)
        {
            return GetMultiplier(
                combatantElement,
                terrainElement,
                sameElementAttackMultiplier,
                neutralAttackMultiplier,
                disadvantageAttackMultiplier);
        }

        public float GetDefenseMultiplier(
            ElementType combatantElement,
            ElementType terrainElement)
        {
            return GetMultiplier(
                combatantElement,
                terrainElement,
                sameElementDefenseMultiplier,
                neutralDefenseMultiplier,
                disadvantageDefenseMultiplier);
        }

        private float GetMultiplier(
            ElementType combatantElement,
            ElementType terrainElement,
            float sameElementMultiplier,
            float neutralMultiplier,
            float disadvantageMultiplier)
        {
            if (combatantElement == ElementType.Neutral ||
                terrainElement == ElementType.Neutral)
            {
                return neutralMultiplier;
            }

            if (combatantElement == terrainElement)
            {
                return sameElementMultiplier;
            }

            bool terrainHasAdvantage =
                (terrainElement == ElementType.Water && combatantElement == ElementType.Fire) ||
                (terrainElement == ElementType.Fire && combatantElement == ElementType.Earth) ||
                (terrainElement == ElementType.Earth && combatantElement == ElementType.Water);

            return terrainHasAdvantage ? disadvantageMultiplier : neutralMultiplier;
        }
    }
}
