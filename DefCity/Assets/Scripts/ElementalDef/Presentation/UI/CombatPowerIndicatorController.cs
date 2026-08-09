using System;
using System.Globalization;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Entities.Settings;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class CombatPowerIndicatorController : MonoBehaviour
    {
        [SerializeField] private TowerUnitSpec waterTowerSpec;
        [SerializeField] private TowerUnitSpec fireTowerSpec;
        [SerializeField] private TowerUnitSpec earthTowerSpec;
        [SerializeField] private TMP_Text combatPowerText;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                ClearDisplay();
                Debug.LogError($"[{name}] {errorMessage}", this);
                enabled = false;
                return;
            }

            try
            {
                TowerUnitSpec[] towerSpecs =
                {
                    waterTowerSpec,
                    fireTowerSpec,
                    earthTowerSpec
                };

                int combatPower = CombatPowerCalculator.CalculateTotal(towerSpecs);
                combatPowerText.text = combatPower.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                ClearDisplay();
                Debug.LogException(
                    new InvalidOperationException(
                        "The ElementalDef team combat power could not be displayed.",
                        exception),
                    this);
                enabled = false;
            }
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (waterTowerSpec == null ||
                fireTowerSpec == null ||
                earthTowerSpec == null)
            {
                errorMessage = "Water, fire, and earth tower spec references are required.";
                return false;
            }

            if (combatPowerText == null)
            {
                errorMessage = "A combat-power text reference is required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ClearDisplay()
        {
            if (combatPowerText != null)
            {
                combatPowerText.text = string.Empty;
            }
        }
    }
}
