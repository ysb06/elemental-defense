using UnityEngine;
using ElementalDef.Gameplay.Economy;
using ElementalDef.Gameplay.Combat.Skills;

namespace ElementalDef.Gameplay.Entities.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Units/Tower Spec")]
    public sealed class TowerUnitSpec : UnitSpec
    {
        [SerializeField] private int cost = 1;
        [SerializeField] private SkillDefinition skill;

        public int Cost => cost;
        public SkillDefinition Skill => skill;
    }
}
