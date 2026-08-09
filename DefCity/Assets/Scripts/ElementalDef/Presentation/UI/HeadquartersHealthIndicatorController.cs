using System.Globalization;
using DefCore.Gameplay.Combat;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class HeadquartersHealthIndicatorController : MonoBehaviour
    {
        private const string HealthNumberFormat = "0.##";

        [SerializeField] private Health headquartersHealth;
        [SerializeField] private TMP_Text currentHealthText;
        [SerializeField] private TMP_Text maximumHealthText;

        private bool isSubscribed;

        private void OnEnable()
        {
            if (!TryValidateConfiguration(out string errorMessage))
            {
                DisableWithError(errorMessage);
                return;
            }

            Subscribe();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleDamaged(GameObject sender, DamageEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (!TryValidateHealthValues(out string errorMessage))
            {
                DisableWithError(errorMessage);
                return;
            }

            currentHealthText.text = headquartersHealth.CurrentHealth.ToString(
                HealthNumberFormat,
                CultureInfo.InvariantCulture);
            maximumHealthText.text = headquartersHealth.MaxHealth.ToString(
                HealthNumberFormat,
                CultureInfo.InvariantCulture);
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (headquartersHealth == null)
            {
                errorMessage = "A headquarters health reference is required.";
                return false;
            }

            if (currentHealthText == null || maximumHealthText == null)
            {
                errorMessage = "Current-health and max-health text references are required.";
                return false;
            }

            if (currentHealthText == maximumHealthText)
            {
                errorMessage = "Current-health and max-health texts must be unique.";
                return false;
            }

            return TryValidateHealthValues(out errorMessage);
        }

        private bool TryValidateHealthValues(out string errorMessage)
        {
            if (!IsFinitePositive(headquartersHealth.MaxHealth))
            {
                errorMessage = $"{nameof(Health)}.{nameof(Health.MaxHealth)} must be a finite positive value.";
                return false;
            }

            if (!IsFinite(headquartersHealth.CurrentHealth))
            {
                errorMessage = $"{nameof(Health)}.{nameof(Health.CurrentHealth)} must be a finite value.";
                return false;
            }

            if (headquartersHealth.CurrentHealth < 0f ||
                headquartersHealth.CurrentHealth > headquartersHealth.MaxHealth)
            {
                errorMessage =
                    $"{nameof(Health)}.{nameof(Health.CurrentHealth)} must be between 0 and " +
                    $"{nameof(Health)}.{nameof(Health.MaxHealth)}.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            headquartersHealth.OnDamaged.AddListener(HandleDamaged);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (headquartersHealth != null)
            {
                headquartersHealth.OnDamaged.RemoveListener(HandleDamaged);
            }

            isSubscribed = false;
        }

        private void DisableWithError(string errorMessage)
        {
            ClearDisplay();
            Debug.LogError(
                $"[{name}] {nameof(HeadquartersHealthIndicatorController)}: {errorMessage}",
                this);
            enabled = false;
        }

        private void ClearDisplay()
        {
            if (currentHealthText != null)
            {
                currentHealthText.text = string.Empty;
            }

            if (maximumHealthText != null)
            {
                maximumHealthText.text = string.Empty;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && IsFinite(value);
        }
    }
}
