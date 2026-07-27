using UnityEngine;

namespace ElementalDef.Gameplay.Combat
{
    public class ElementalTile : MonoBehaviour
    {
        [SerializeField] private ElementType elementType = ElementType.Neutral;
        public ElementType ElementType => elementType;
    }
}