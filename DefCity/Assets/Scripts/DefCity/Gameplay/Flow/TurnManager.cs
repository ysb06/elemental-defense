using System;
using UnityEngine;
using UnityEngine.Events;

namespace DefCity.Gameplay.Flow
{
    [Serializable]
    public struct TurnChangedEventArgs
    {
        public int PreviousTurn;
        public int CurrentTurn;
        public float IntervalSeconds;

        public TurnChangedEventArgs(int previousTurn, int currentTurn, float intervalSeconds)
        {
            PreviousTurn = previousTurn;
            CurrentTurn = currentTurn;
            IntervalSeconds = intervalSeconds;
        }
    }

    public class TurnManager : MonoBehaviour
    {
        private const float MinIntervalSeconds = 0.0001f;

        [SerializeField, Min(MinIntervalSeconds)] private float intervalSeconds = 5f;
        [SerializeField, Min(0)] private int currentTurn;
        [SerializeField, Min(0f)] private float elapsedInCurrentTurn;
        [SerializeField] private bool isRunning = true;

        public int CurrentTurn => currentTurn;
        public float ElapsedInCurrentTurn => elapsedInCurrentTurn;

        public float IntervalSeconds
        {
            get => intervalSeconds;
            set
            {
                if (value <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "IntervalSeconds must be greater than 0.");
                }

                intervalSeconds = Mathf.Max(MinIntervalSeconds, value);
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set => isRunning = value;
        }

        public TurnChangedEvent OnTurnChanged = new();

        private void OnValidate()
        {
            intervalSeconds = Mathf.Max(MinIntervalSeconds, intervalSeconds);
            currentTurn = Mathf.Max(0, currentTurn);
            elapsedInCurrentTurn = Mathf.Max(0f, elapsedInCurrentTurn);
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            AdvanceTime(UnityEngine.Time.deltaTime);
        }

        public void ResetTurn(int turn = 0)
        {
            if (turn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(turn), turn, "Turn must be non-negative.");
            }

            currentTurn = turn;
            elapsedInCurrentTurn = 0f;
        }

        public void AdvanceTime(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time must be non-negative.");
            }

            elapsedInCurrentTurn += deltaTime;

            while (elapsedInCurrentTurn >= intervalSeconds)
            {
                elapsedInCurrentTurn -= intervalSeconds;
                AdvanceTurn();
            }
        }

        private void AdvanceTurn()
        {
            int previousTurn = currentTurn;
            currentTurn++;

            OnTurnChanged.Invoke(
                gameObject,
                new TurnChangedEventArgs(previousTurn, currentTurn, intervalSeconds));
        }
    }

    [Serializable]
    public class TurnChangedEvent : UnityEvent<GameObject, TurnChangedEventArgs> { }
}
