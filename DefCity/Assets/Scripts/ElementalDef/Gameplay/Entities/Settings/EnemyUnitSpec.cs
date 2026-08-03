using UnityEngine;

namespace ElementalDef.Gameplay.Entities.Settings
{
    [CreateAssetMenu(menuName = "ElementalDef/Units/Enemy Spec")]
    public sealed class EnemyUnitSpec : UnitSpec
    {
        [SerializeField] private MovementStats movement;

        public MovementStats Movement => movement;
    }
}