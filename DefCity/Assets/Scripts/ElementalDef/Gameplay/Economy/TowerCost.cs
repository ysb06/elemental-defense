using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public sealed class TowerCost : MonoBehaviour
    {
        [SerializeField, Min(0)] private float cost = 100;
        public float Cost => cost;

        public void Initialize(float towerCost)
        {
            cost = towerCost;
        }
    }
}
