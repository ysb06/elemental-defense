using System.Collections.Generic;
using System.Globalization;
using ElementalDef.Gameplay.Entities.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class StatIndicatorController : MonoBehaviour
    {
        private const string StatNumberFormat = "0.##";

        [Header("Tower Buttons")]
        [SerializeField] private Button waterTowerButton;
        [SerializeField] private Button fireTowerButton;
        [SerializeField] private Button earthTowerButton;

        [Header("Tower Specs")]
        [SerializeField] private TowerUnitSpec waterTowerSpec;
        [SerializeField] private TowerUnitSpec fireTowerSpec;
        [SerializeField] private TowerUnitSpec earthTowerSpec;

        [Header("Profile Text")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text storyText;

        [Header("Stat Text")]
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text attackRangeText;

        private bool listenersBound;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                ClearDisplay();
                SetButtonsInteractable(false);
                Debug.LogError($"[{name}] {errorMessage}", this);
                enabled = false;
                return;
            }

            BindButtonListeners();
            SetButtonsInteractable(true);
            DisplayTower(waterTowerSpec);
        }

        private void OnDisable()
        {
            UnbindButtonListeners();
        }

        private void BindButtonListeners()
        {
            if (listenersBound)
            {
                return;
            }

            waterTowerButton.onClick.AddListener(ShowWaterTower);
            fireTowerButton.onClick.AddListener(ShowFireTower);
            earthTowerButton.onClick.AddListener(ShowEarthTower);
            listenersBound = true;
        }

        private void UnbindButtonListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            if (waterTowerButton != null)
            {
                waterTowerButton.onClick.RemoveListener(ShowWaterTower);
            }

            if (fireTowerButton != null)
            {
                fireTowerButton.onClick.RemoveListener(ShowFireTower);
            }

            if (earthTowerButton != null)
            {
                earthTowerButton.onClick.RemoveListener(ShowEarthTower);
            }

            listenersBound = false;
        }

        private void ShowWaterTower()
        {
            DisplayTower(waterTowerSpec);
        }

        private void ShowFireTower()
        {
            DisplayTower(fireTowerSpec);
        }

        private void ShowEarthTower()
        {
            DisplayTower(earthTowerSpec);
        }

        private void DisplayTower(TowerUnitSpec towerSpec)
        {
            nameText.text = towerSpec.DisplayName;
            storyText.text = towerSpec.Story;
            healthText.text = FormatStat(towerSpec.Defense.MaxHealth);
            attackText.text = FormatStat(towerSpec.Attack.Power);
            defenseText.text = FormatStat(towerSpec.Defense.Defense);
            attackSpeedText.text = FormatStat(towerSpec.Attack.Cooldown);
            attackRangeText.text = FormatStat(towerSpec.Attack.Range);
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (waterTowerButton == null || fireTowerButton == null || earthTowerButton == null)
            {
                errorMessage = "Water, fire, and earth tower button references are required.";
                return false;
            }

            if (waterTowerButton == fireTowerButton ||
                waterTowerButton == earthTowerButton ||
                fireTowerButton == earthTowerButton)
            {
                errorMessage = "Water, fire, and earth tower buttons must be unique.";
                return false;
            }

            if (waterTowerSpec == null || fireTowerSpec == null || earthTowerSpec == null)
            {
                errorMessage = "Water, fire, and earth tower spec references are required.";
                return false;
            }

            if (waterTowerSpec == fireTowerSpec ||
                waterTowerSpec == earthTowerSpec ||
                fireTowerSpec == earthTowerSpec)
            {
                errorMessage = "Water, fire, and earth tower specs must be unique.";
                return false;
            }

            TMP_Text[] textReferences =
            {
                nameText,
                storyText,
                healthText,
                attackText,
                defenseText,
                attackSpeedText,
                attackRangeText
            };

            HashSet<TMP_Text> uniqueTexts = new();
            foreach (TMP_Text textReference in textReferences)
            {
                if (textReference == null)
                {
                    errorMessage = "Name, story, and all stat text references are required.";
                    return false;
                }

                if (!uniqueTexts.Add(textReference))
                {
                    errorMessage = "Name, story, and stat text references must be unique.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (waterTowerButton != null)
            {
                waterTowerButton.interactable = interactable;
            }

            if (fireTowerButton != null)
            {
                fireTowerButton.interactable = interactable;
            }

            if (earthTowerButton != null)
            {
                earthTowerButton.interactable = interactable;
            }
        }

        private void ClearDisplay()
        {
            SetText(nameText, string.Empty);
            SetText(storyText, string.Empty);
            SetText(healthText, string.Empty);
            SetText(attackText, string.Empty);
            SetText(defenseText, string.Empty);
            SetText(attackSpeedText, string.Empty);
            SetText(attackRangeText, string.Empty);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static string FormatStat(float value)
        {
            return value.ToString(StatNumberFormat, CultureInfo.InvariantCulture);
        }
    }
}
