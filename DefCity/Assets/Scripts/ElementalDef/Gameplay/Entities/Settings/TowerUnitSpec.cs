using UnityEngine;
using ElementalDef.Gameplay.Economy;

namespace ElementalDef.Gameplay.Entities.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Units/Tower Spec")]
    public sealed class TowerUnitSpec : UnitSpec
    {
        [SerializeField] private int cost = 1;

        public int Cost => cost;
    }
}