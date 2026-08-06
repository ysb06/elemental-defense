using DefCore.Gameplay.Combat;
using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyRouteFollower))]
    public sealed class EnemyRouteTargetPolicy : AutoCombatTargetPolicy
    {
        [SerializeField] private EnemyRouteFollower routeFollower;

        private void Awake()
        {
            if (routeFollower == null)
            {
                routeFollower = GetComponent<EnemyRouteFollower>();
            }
        }

        public override bool CanTarget(Health target)
        {
            if (target == null || routeFollower == null)
            {
                return false;
            }

            bool isHeadquarters = target.TryGetComponent(out HeadquartersBuilding _);
            return routeFollower.HasCompletedRoute ? isHeadquarters : !isHeadquarters;
        }
    }
}
