using DefCore.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.AI
{
    [DisallowMultipleComponent]
    public abstract class AutoCombatTargetPolicy : MonoBehaviour
    {
        public abstract bool CanTarget(Health target);
    }
}
