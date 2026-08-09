using UnityEngine;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class SkillIndicatorController : MonoBehaviour
    {
        [Header("Tower Buttons")]
        [SerializeField] private Button waterTowerButton;
        [SerializeField] private Button fireTowerButton;
        [SerializeField] private Button earthTowerButton;

        [Header("Skill Images")]
        [SerializeField] private Image skillIconImage;
        [SerializeField] private Image skillDetailImage;

        [Header("Water Skill")]
        [SerializeField] private Sprite waterSkillIcon;
        [SerializeField] private Sprite waterSkillDetail;

        [Header("Fire Skill")]
        [SerializeField] private Sprite fireSkillIcon;
        [SerializeField] private Sprite fireSkillDetail;

        [Header("Earth Skill")]
        [SerializeField] private Sprite earthSkillIcon;
        [SerializeField] private Sprite earthSkillDetail;

        private bool listenersBound;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                ClearDisplay();
                Debug.LogError($"[{name}] {errorMessage}", this);
                enabled = false;
                return;
            }

            BindButtonListeners();
            DisplaySkill(waterSkillIcon, waterSkillDetail);
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

            waterTowerButton.onClick.AddListener(ShowWaterSkill);
            fireTowerButton.onClick.AddListener(ShowFireSkill);
            earthTowerButton.onClick.AddListener(ShowEarthSkill);
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
                waterTowerButton.onClick.RemoveListener(ShowWaterSkill);
            }

            if (fireTowerButton != null)
            {
                fireTowerButton.onClick.RemoveListener(ShowFireSkill);
            }

            if (earthTowerButton != null)
            {
                earthTowerButton.onClick.RemoveListener(ShowEarthSkill);
            }

            listenersBound = false;
        }

        private void ShowWaterSkill()
        {
            DisplaySkill(waterSkillIcon, waterSkillDetail);
        }

        private void ShowFireSkill()
        {
            DisplaySkill(fireSkillIcon, fireSkillDetail);
        }

        private void ShowEarthSkill()
        {
            DisplaySkill(earthSkillIcon, earthSkillDetail);
        }

        private void DisplaySkill(Sprite icon, Sprite detail)
        {
            skillIconImage.sprite = icon;
            skillDetailImage.sprite = detail;
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (waterTowerButton == null ||
                fireTowerButton == null ||
                earthTowerButton == null)
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

            if (skillIconImage == null || skillDetailImage == null)
            {
                errorMessage = "Skill icon and detail image references are required.";
                return false;
            }

            if (skillIconImage == skillDetailImage)
            {
                errorMessage = "Skill icon and detail images must be unique.";
                return false;
            }

            if (waterSkillIcon == null ||
                waterSkillDetail == null ||
                fireSkillIcon == null ||
                fireSkillDetail == null ||
                earthSkillIcon == null ||
                earthSkillDetail == null)
            {
                errorMessage = "Water, fire, and earth skill sprite references are required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ClearDisplay()
        {
            if (skillIconImage != null)
            {
                skillIconImage.sprite = null;
            }

            if (skillDetailImage != null)
            {
                skillDetailImage.sprite = null;
            }
        }
    }
}
