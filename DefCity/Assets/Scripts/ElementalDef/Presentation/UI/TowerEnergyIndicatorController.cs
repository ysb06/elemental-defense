using System;
using ElementalDef.Gameplay.Economy;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class TowerEnergyIndicatorController : MonoBehaviour
    {
        [SerializeField] private TowerEnergyManager towerEnergyManager;
        [SerializeField] private TMP_Text energyText;

        private int displayedEnergy = int.MinValue;

        private void OnEnable()
        {
            towerEnergyManager.EnergyChanged += HandleEnergyChanged;
            RefreshEnergyText(towerEnergyManager.CurrentEnergy);
        }

        private void OnDisable()
        {
            if (towerEnergyManager != null)
            {
                towerEnergyManager.EnergyChanged -= HandleEnergyChanged;
            }
        }

        private void HandleEnergyChanged(float currentEnergy)
        {
            RefreshEnergyText(currentEnergy);
        }

        private void RefreshEnergyText(float currentEnergy)
        {
            int nextDisplayedEnergy = Mathf.FloorToInt(currentEnergy);
            if (displayedEnergy == nextDisplayedEnergy)
            {
                return;
            }

            displayedEnergy = nextDisplayedEnergy;
            energyText.text = displayedEnergy.ToString();
        }
    }
}
