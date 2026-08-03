using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public sealed class TowerCost : MonoBehaviour
    {
        [SerializeField, Min(0)] private int cost = 100;
        [SerializeField] private TowerUnit targetTower;
        public int Cost => cost;

        private void Awake()
        {
            if (targetTower == null)
            {
                Debug.LogWarning($"[{name}] {nameof(TowerCost)} has no target tower assigned. This component will not function correctly without a valid target tower.");
            }
        }

        public void Initialize(int towerCost)
        {
            if (towerCost < 0)
            {
                Debug.LogWarning($"[{name}] {nameof(TowerCost)} has an invalid cost value: {towerCost}. Cost must be non-negative.");
                return;
            }

            cost = towerCost;
        }
    }
}
