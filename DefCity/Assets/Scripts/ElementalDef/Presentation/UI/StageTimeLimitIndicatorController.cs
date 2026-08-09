using System;
using ElementalDef.Gameplay.Flow;
using TMPro;
using UnityEngine;

namespace ElementalDef.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class StageTimeLimitIndicatorController : MonoBehaviour
    {
        [SerializeField] private StageTimeLimitController timeLimitController;
        [SerializeField] private TMP_Text countdownText;

        private bool isSubscribed;
        private int displayedWholeSeconds = -1;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Refresh()
        {
            if (!timeLimitController.HasStarted)
            {
                displayedWholeSeconds = -1;
                countdownText.enabled = false;
                return;
            }

            countdownText.enabled = true;
            DisplayRemainingTime(timeLimitController.RemainingWholeSeconds);
        }

        private void HandleRemainingWholeSecondsChanged(int remainingWholeSeconds)
        {
            countdownText.enabled = true;
            DisplayRemainingTime(remainingWholeSeconds);
        }

        private void DisplayRemainingTime(int remainingWholeSeconds)
        {
            int clampedSeconds = Mathf.Max(0, remainingWholeSeconds);
            if (displayedWholeSeconds == clampedSeconds)
            {
                return;
            }

            displayedWholeSeconds = clampedSeconds;
            int minutes = clampedSeconds / 60;
            int seconds = clampedSeconds % 60;
            countdownText.text = $"{minutes:00}:{seconds:00}";
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            timeLimitController.RemainingWholeSecondsChanged +=
                HandleRemainingWholeSecondsChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            timeLimitController.RemainingWholeSecondsChanged -=
                HandleRemainingWholeSecondsChanged;
            isSubscribed = false;
        }

        private void EnsureConfigured()
        {
            if (timeLimitController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTimeLimitIndicatorController)} requires a " +
                    $"{nameof(StageTimeLimitController)} reference.");
            }

            if (countdownText == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTimeLimitIndicatorController)} requires a countdown text reference.");
            }
        }
    }
}
