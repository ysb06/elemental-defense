using System;
using DefCore.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity))]
    public sealed class ElementalCombatant : MonoBehaviour
    {
        [SerializeField] private ElementType defenseElement = ElementType.Neutral;
        [SerializeField, Min(0f)] private float defense;

        public ElementType DefenseElement => defenseElement;
        public float Defense => defense;

        private void Awake()
        {
            if (!Enum.IsDefined(typeof(ElementType), defenseElement))
            {
                throw new InvalidOperationException($"[{name}] {nameof(ElementalCombatant)} has an undefined {nameof(DefenseElement)} value: {(int)defenseElement}.");
            }

            if (float.IsNaN(defense) || float.IsInfinity(defense) || defense < 0f)
            {
                throw new InvalidOperationException(
                    $"[{name}] {nameof(ElementalCombatant)} requires a finite, non-negative {nameof(Defense)} value.");
            }
        }

        public void Initialize(ElementType element, float defenseValue)
        {
            defenseElement = element;
            defense = defenseValue;
        }
    }
}
