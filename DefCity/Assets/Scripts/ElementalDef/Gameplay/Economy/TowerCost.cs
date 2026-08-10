using System;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Entities.Settings;
using UnityEngine;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerUnit))]
    [Obsolete("Use TowerUnitSpec.Cost as the tower cost source.")]
    public sealed class TowerCost : MonoBehaviour
    {
        private TowerUnit tower;

        public float Cost
        {
            get
            {
                tower = tower != null ? tower : GetComponent<TowerUnit>();
                if (tower.Spec == null)
                {
                    throw new InvalidOperationException(
                        $"[{name}] A {nameof(TowerUnitSpec)} reference is required to resolve tower cost.");
                }

                return tower.Spec.Cost;
            }
        }
    }
}
