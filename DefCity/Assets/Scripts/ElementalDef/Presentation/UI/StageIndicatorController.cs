using System;
using System.Globalization;
using ElementalDef.Data;
using ElementalDef.Runtime;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class StageIndicatorController : MonoBehaviour
    {
        private const long StagesPerLoop = 10L;

        [SerializeField] private TMP_Text stageText;

        private void OnEnable()
        {
            if (stageText == null)
            {
                Debug.LogError($"[{name}] A stage text reference is required.", this);
                enabled = false;
                return;
            }

            RefreshStageText();
        }

        private void RefreshStageText()
        {
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            if (applicationRoot == null || applicationRoot.PlayerProgress == null)
            {
                stageText.text = string.Empty;
                Debug.LogError(
                    $"[{name}] ElementalDef player-progress services are unavailable.",
                    this);
                return;
            }

            try
            {
                PlayerProgressSnapshot progress = applicationRoot.PlayerProgress.GetProgress();
                long nextStageNumber;
                checked
                {
                    nextStageNumber =
                        progress.Loop * StagesPerLoop + progress.MaxStageProgress + 1L;
                }

                stageText.text = nextStageNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                stageText.text = string.Empty;
                Debug.LogException(new InvalidOperationException(
                    "The next ElementalDef stage number could not be displayed.",
                    exception),
                    this);
            }
        }
    }
}
