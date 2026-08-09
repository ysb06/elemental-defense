using System;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class LobbyCreditIndicatorController : MonoBehaviour
    {
        [SerializeField] private TMP_Text creditText;

        private void OnEnable()
        {
            if (creditText == null)
            {
                Debug.LogError($"[{name}] A credit text reference is required.", this);
                enabled = false;
                return;
            }

            RefreshCreditText();
        }

        private void RefreshCreditText()
        {
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot == null || applicationRoot.PlayerProgress == null)
            {
                creditText.text = string.Empty;
                Debug.LogError(
                    $"[{name}] ElementalDef player-progress services are unavailable.",
                    this);
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                creditText.text = progress.TotalCredits.ToString(
                    "N0",
                    CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                creditText.text = string.Empty;
                Debug.LogException(new InvalidOperationException(
                    "The current ElementalDef credit balance could not be displayed.",
                    exception),
                    this);
            }
        }
    }
}
