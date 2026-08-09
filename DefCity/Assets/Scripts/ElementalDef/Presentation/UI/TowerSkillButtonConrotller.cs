using ElementalDef.Gameplay.Combat.Skills;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Entities.Settings;
using ElementalDef.Presentation.Effect;
using UnityEngine;
using UnityEngine.UI;

namespace ElementalDef.Presentation.UI
{
    public class TowerSkillButtonConrotller : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image chargeFill;
        [SerializeField] private TowerRegistry towerRegistry;
        [SerializeField] private TowerUnitSpec targetTowerSpec;
        [SerializeField] private SkillEffectPresenter skillEffectPresenter;

        private void Update()
        {
            RefreshChargeFill();
        }

        public void RequestUseSkill()
        {
            foreach (var tower in towerRegistry.Towers)
            {
                if (IsTargetTower(tower) && tower.SkillController.CanUse)
                {
                    SkillUseRequestResult result = tower.SkillController.RequestUse();
                    if (result == SkillUseRequestResult.Accepted)
                    {
                        skillEffectPresenter?.Play();
                    }

                    break;
                }
            }
        }

        private void RefreshChargeFill()
        {
            if (chargeFill == null)
            {
                return;
            }

            float highestChargePercent = 0f;
            foreach (TowerUnit tower in towerRegistry.Towers)
            {
                if (!IsTargetTower(tower))
                {
                    continue;
                }

                highestChargePercent = Mathf.Max(highestChargePercent, tower.SkillController.NormalizedCharge * 100f);
            }

            RectTransform chargeFillRectTransform = chargeFill.rectTransform;
            if (chargeFillRectTransform.parent is not RectTransform parentRectTransform)
            {
                return;
            }

            Vector2 offsetMin = chargeFillRectTransform.offsetMin;
            offsetMin.y = parentRectTransform.rect.height * highestChargePercent / 100f;
            chargeFillRectTransform.offsetMin = offsetMin;
        }

        private bool IsTargetTower(TowerUnit tower)
        {
            return tower != null && tower.Spec == targetTowerSpec && tower.SkillController != null;
        }
    }
}
