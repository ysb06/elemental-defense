using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using DefCity.Gameplay.Flow;

namespace DefCity.Presentation.UI
{
    public class TurnIndicatorController : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private WaveSchedule waveSchedule;
        [SerializeField] private GameStateManager gameStateManager;
        [FormerlySerializedAs("waveCountdownText")]
        [SerializeField] private TMP_Text countdownText;
        [SerializeField, Min(1)] private int startYear = 3246;
        [SerializeField, Range(1, 12)] private int startMonth = 5;
        [SerializeField, Range(1, 31)] private int startDay = 17;

        private int displayedTurn = int.MinValue;

        private void OnValidate()
        {
            startYear = Math.Max(1, startYear);
            startMonth = Mathf.Clamp(startMonth, 1, 12);
            startDay = Mathf.Clamp(startDay, 1, DateTime.DaysInMonth(startYear, startMonth));
        }

        private void OnEnable()
        {
            displayedTurn = int.MinValue;
        }

        private void Update()
        {
            if (turnManager == null || turnText == null || waveSchedule == null ||
                gameStateManager == null || countdownText == null)
            {
                return;
            }

            int currentTurn = turnManager.CurrentTurn;
            if (currentTurn == displayedTurn)
            {
                return;
            }

            Refresh(currentTurn);
            displayedTurn = currentTurn;
        }

        private void Refresh(int currentTurn)
        {
            DateTime currentDate = new DateTime(startYear, startMonth, startDay).AddDays(currentTurn);
            turnText.text = currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (waveSchedule.TryGetNextWaveTurn(currentTurn, out int nextWaveTurn))
            {
                ShowWaveCountdown(nextWaveTurn - currentTurn);
                return;
            }

            ShowVictoryCountdown(gameStateManager.VictoryTurn - currentTurn);
        }

        private void ShowWaveCountdown(int remainingTurns)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = remainingTurns == 0
                ? "D-DAY"
                : $"D-{remainingTurns.ToString(CultureInfo.InvariantCulture)}";
        }

        private void ShowVictoryCountdown(int remainingTurns)
        {
            if (remainingTurns <= 0)
            {
                countdownText.gameObject.SetActive(false);
                return;
            }

            countdownText.gameObject.SetActive(true);
            countdownText.text = $"VICTORY D-{remainingTurns.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
