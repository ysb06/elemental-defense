using System;
using ElementalDef.Gameplay.Flow;
using UnityEngine;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public sealed class StageTowerEnergyInitializer : MonoBehaviour
    {
        [SerializeField] private TowerEnergyManager towerEnergyManager;

        private bool isInitialized;

        public void Initialize(StageRunContext stageRunContext)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTowerEnergyInitializer)} has already initialized the stage energy.");
            }

            EnsureConfigured();

            if (stageRunContext == null)
            {
                throw new ArgumentNullException(nameof(stageRunContext));
            }

            int targetEnergy = stageRunContext.StartingTowerEnergy;
            if (targetEnergy > towerEnergyManager.MaxEnergy)
            {
                throw new InvalidOperationException(
                    $"Stage '{stageRunContext.StageId}' requires {targetEnergy} starting tower " +
                    $"energy, but {nameof(TowerEnergyManager)} has a maximum of " +
                    $"{towerEnergyManager.MaxEnergy}.");
            }

            float additionalEnergy = targetEnergy - towerEnergyManager.CurrentEnergy;
            if (additionalEnergy < 0f)
            {
                throw new InvalidOperationException(
                    $"Stage '{stageRunContext.StageId}' requires {targetEnergy} starting tower " +
                    $"energy, but the Scene already contains " +
                    $"{towerEnergyManager.CurrentEnergy}.");
            }

            isInitialized = true;
            if (additionalEnergy > 0f)
            {
                towerEnergyManager.AddEnergy(additionalEnergy);
            }
        }

        private void Awake()
        {
            EnsureConfigured();
        }

        private void EnsureConfigured()
        {
            if (towerEnergyManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTowerEnergyInitializer)} requires a " +
                    $"{nameof(TowerEnergyManager)} reference.");
            }
        }
    }
}
