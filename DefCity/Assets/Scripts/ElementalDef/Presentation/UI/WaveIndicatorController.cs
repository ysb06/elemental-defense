using System.Globalization;
using ElementalDef.Gameplay.Flow;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class WaveIndicatorController : MonoBehaviour
    {
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private TMP_Text currentWaveText;
        [SerializeField] private TMP_Text totalWaveText;

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

        private void HandleWaveProgressChanged(int currentWaveNumber, int totalWaveCount)
        {
            if (!IsValidWaveProgress(currentWaveNumber, totalWaveCount))
            {
                DisableWithError(
                    $"Received invalid wave progress {currentWaveNumber}/{totalWaveCount}.");
                return;
            }

            DisplayWaveProgress(currentWaveNumber, totalWaveCount);
        }

        private void RefreshDisplay()
        {
            if (waveBundleController.ActiveBundle == null ||
                waveBundleController.CurrentWaveIndex < 0)
            {
                ClearDisplay();
                return;
            }

            int totalWaveCount = waveBundleController.TotalWaveCount;
            int currentWaveNumber = waveBundleController.CurrentWaveIndex + 1;
            if (!IsValidWaveProgress(currentWaveNumber, totalWaveCount))
            {
                DisableWithError(
                    $"The current wave progress {currentWaveNumber}/{totalWaveCount} is invalid.");
                return;
            }

            DisplayWaveProgress(currentWaveNumber, totalWaveCount);
        }

        private void DisplayWaveProgress(int currentWaveNumber, int totalWaveCount)
        {
            currentWaveText.text = currentWaveNumber.ToString(CultureInfo.InvariantCulture);
            totalWaveText.text = totalWaveCount.ToString(CultureInfo.InvariantCulture);
        }

        private bool TryValidateConfiguration(out string errorMessage)
        {
            if (waveBundleController == null)
            {
                errorMessage = $"A {nameof(WaveBundleController)} reference is required.";
                return false;
            }

            if (currentWaveText == null || totalWaveText == null)
            {
                errorMessage = "Current-wave and total-wave text references are required.";
                return false;
            }

            if (currentWaveText == totalWaveText)
            {
                errorMessage = "Current-wave and total-wave texts must be unique.";
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

            waveBundleController.WaveProgressChanged += HandleWaveProgressChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (waveBundleController != null)
            {
                waveBundleController.WaveProgressChanged -= HandleWaveProgressChanged;
            }

            isSubscribed = false;
        }

        private void DisableWithError(string errorMessage)
        {
            ClearDisplay();
            Debug.LogError($"[{name}] {nameof(WaveIndicatorController)}: {errorMessage}", this);
            enabled = false;
        }

        private void ClearDisplay()
        {
            if (currentWaveText != null)
            {
                currentWaveText.text = string.Empty;
            }

            if (totalWaveText != null)
            {
                totalWaveText.text = string.Empty;
            }
        }

        private static bool IsValidWaveProgress(int currentWaveNumber, int totalWaveCount)
        {
            return totalWaveCount > 0 &&
                   currentWaveNumber > 0 &&
                   currentWaveNumber <= totalWaveCount;
        }
    }
}
