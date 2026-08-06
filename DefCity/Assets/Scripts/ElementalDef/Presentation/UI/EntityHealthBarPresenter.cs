using System;
using DefCore.Gameplay.Combat;
using DefCore.Presentation.UI;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    public sealed class EntityHealthBarPresenter : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private ProgressBarController progressBar;

        private void OnEnable()
        {
            health.OnDamaged.AddListener(HandleDamaged);
            health.OnDeath.AddListener(HandleDeath);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            health.OnDamaged.RemoveListener(HandleDamaged);
            health.OnDeath.RemoveListener(HandleDeath);
        }
        

        private void HandleDamaged(GameObject sender, DamageEventArgs eventArgs)
        {
            Refresh();
        }

        private void HandleDeath(GameObject sender, DamageEventArgs eventArgs)
        {
            Refresh();
        }

        private void Refresh()
        {
            float value = health.CurrentHealth / health.MaxHealth * 100f;
            progressBar.SetValue(value);
        }
    }
}
