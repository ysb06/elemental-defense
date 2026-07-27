using System;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Combat/Elemental Affinity Settings")]
    public sealed class ElementalAffinitySettings : ScriptableObject
    {
        [SerializeField, Min(0f)] private float advantageMultiplier = 1.30f;
        [SerializeField, Min(0f)] private float neutralMultiplier = 1.00f;
        [SerializeField, Min(0f)] private float disadvantageMultiplier = 0.80f;

        public float AdvantageMultiplier => advantageMultiplier;
        public float NeutralMultiplier => neutralMultiplier;
        public float DisadvantageMultiplier => disadvantageMultiplier;

        public float GetCombatMultiplier(ElementType attacker, ElementType defender)
        {
            if (attacker == ElementType.Neutral || defender == ElementType.Neutral || attacker == defender)
            {
                return neutralMultiplier;
            }

            bool hasAdvantage =
                attacker == ElementType.Water && defender == ElementType.Fire ||
                attacker == ElementType.Fire && defender == ElementType.Earth ||
                attacker == ElementType.Earth && defender == ElementType.Water;

            return hasAdvantage ? advantageMultiplier : disadvantageMultiplier;
        }
    }
}
