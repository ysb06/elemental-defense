using System;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Interaction;
using UnityEngine;

namespace DefCity.Presentation.UI
{
    [DisallowMultipleComponent]
    public class UnitSelectionPresentationController : MonoBehaviour
    {
        [SerializeField] private UnitSelectEventManager unitSelectEventManager;
        [SerializeField] private UnitHealthBarView unitHealthBarView;
        [SerializeField] private UnitSelectionIndicatorView unitSelectionIndicatorView;
        [SerializeField] private UnitAttackRangeView unitAttackRangeView;

        private void Awake()
        {
            if (unitSelectEventManager == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a UnitSelectEventManager.");
            }

            if (unitHealthBarView == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a UnitHealthBarView.");
            }

            if (unitSelectionIndicatorView == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a UnitSelectionIndicatorView.");
            }

            if (unitAttackRangeView == null)
            {
                throw new InvalidOperationException(
                    $"{name} requires a UnitAttackRangeView.");
            }

            if (unitSelectEventManager.TargetCamera == null)
            {
                throw new InvalidOperationException(
                    $"{unitSelectEventManager.name} requires a target Camera.");
            }
        }

        private void OnEnable()
        {
            unitSelectEventManager.OnUnitSelected.AddListener(OnUnitSelected);
            unitSelectEventManager.OnUnitSelectMiss.AddListener(OnUnitSelectMiss);
        }

        private void OnDisable()
        {
            unitSelectEventManager.OnUnitSelected.RemoveListener(OnUnitSelected);
            unitSelectEventManager.OnUnitSelectMiss.RemoveListener(OnUnitSelectMiss);
            UnbindViews();
        }

        private void OnUnitSelected(GameObject sender, UnitSelectEventArgs args)
        {
            unitHealthBarView.Bind(args.Damageable, unitSelectEventManager.TargetCamera);
            unitSelectionIndicatorView.Bind(args.Damageable);

            if (args.Entity.TryGetComponent(out AttackCapable attackCapable))
            {
                unitAttackRangeView.Bind(attackCapable, args.Damageable);
            }
            else
            {
                unitAttackRangeView.Unbind();
            }
        }

        private void OnUnitSelectMiss(GameObject sender)
        {
            UnbindViews();
        }

        private void UnbindViews()
        {
            unitHealthBarView.Unbind();
            unitSelectionIndicatorView.Unbind();
            unitAttackRangeView.Unbind();
        }
    }
}
