using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DefCore.Gameplay.Flow;

namespace DefCity.Gameplay.Economy
{
    [Serializable]
    public struct CreditChangedEventArgs
    {
        public int PreviousCredits;
        public int CurrentCredits;
        public int Delta;

        public CreditChangedEventArgs(int previousCredits, int currentCredits)
        {
            PreviousCredits = previousCredits;
            CurrentCredits = currentCredits;
            Delta = currentCredits - previousCredits;
        }
    }

    [Serializable]
    public struct TurnIncomeChangedEventArgs
    {
        public int PreviousTurnIncome;
        public int CurrentTurnIncome;
        public int Delta;

        public TurnIncomeChangedEventArgs(int previousTurnIncome, int currentTurnIncome)
        {
            PreviousTurnIncome = previousTurnIncome;
            CurrentTurnIncome = currentTurnIncome;
            Delta = currentTurnIncome - previousTurnIncome;
        }
    }

    public class CreditManager : MonoBehaviour
    {
        private readonly List<TurnCreditProvider> turnCreditProviders = new();

        [SerializeField] private TurnManager turnManager;
        [SerializeField] private int credits = 0;
        [SerializeField] private CreditChangedEvent onCreditsChanged = new();
        [SerializeField] private TurnIncomeChangedEvent onTurnIncomeChanged = new();

        public int Credits => credits;
        public int TurnIncome => CalculateTurnIncome();
        public IReadOnlyList<TurnCreditProvider> TurnCreditProviders => turnCreditProviders.AsReadOnly();
        public CreditChangedEvent OnCreditsChanged => onCreditsChanged ??= new CreditChangedEvent();
        public TurnIncomeChangedEvent OnTurnIncomeChanged => onTurnIncomeChanged ??= new TurnIncomeChangedEvent();

        private void OnEnable()
        {
            if (turnManager == null)
            {
                throw new InvalidOperationException($"{nameof(CreditManager)} on {name} requires a {nameof(TurnManager)} reference.");
            }

            turnManager.OnTurnChanged.AddListener(OnTurnChanged);
        }

        private void OnDisable()
        {
            if (turnManager != null)
            {
                turnManager.OnTurnChanged.RemoveListener(OnTurnChanged);
            }
        }

        public void AddCredits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Credit amount must be non-negative.");
            }

            SetCredits(checked(credits + amount));
        }

        public void SubtractCredits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Credit amount must be non-negative.");
            }

            SetCredits(checked(credits - amount));
        }

        public bool CanAfford(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Credit amount must be non-negative.");
            }

            return credits >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            SetCredits(credits - amount);
            return true;
        }

        public void RegisterTurnCreditProvider(TurnCreditProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (turnCreditProviders.Contains(provider))
            {
                throw new InvalidOperationException($"{provider.name} is already registered.");
            }

            int previousTurnIncome = TurnIncome;
            turnCreditProviders.Add(provider);
            RaiseTurnIncomeChanged(previousTurnIncome);
        }

        public bool UnregisterTurnCreditProvider(TurnCreditProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            int previousTurnIncome = TurnIncome;
            if (!turnCreditProviders.Remove(provider))
            {
                return false;
            }

            RaiseTurnIncomeChanged(previousTurnIncome);
            return true;
        }

        private void OnTurnChanged(GameObject sender, TurnChangedEventArgs args)
        {
            int turnsElapsed = args.CurrentTurn - args.PreviousTurn;
            if (turnsElapsed > 0)
            {
                AddCredits(checked(turnsElapsed * TurnIncome));
            }
        }

        private int CalculateTurnIncome()
        {
            int totalIncome = 0;
            foreach (TurnCreditProvider provider in turnCreditProviders)
            {
                if (provider != null && provider.isActiveAndEnabled)
                {
                    totalIncome = checked(totalIncome + provider.TurnCredit);
                }
            }

            return totalIncome;
        }

        private void SetCredits(int value)
        {
            int previousCredits = credits;
            credits = value;

            if (credits != previousCredits)
            {
                OnCreditsChanged.Invoke(
                    gameObject,
                    new CreditChangedEventArgs(previousCredits, credits));
            }
        }

        private void RaiseTurnIncomeChanged(int previousTurnIncome)
        {
            int currentTurnIncome = TurnIncome;
            if (currentTurnIncome == previousTurnIncome)
            {
                return;
            }

            OnTurnIncomeChanged.Invoke(
                gameObject,
                new TurnIncomeChangedEventArgs(previousTurnIncome, currentTurnIncome));
        }
    }

    [Serializable]
    public class CreditChangedEvent : UnityEvent<GameObject, CreditChangedEventArgs> { }

    [Serializable]
    public class TurnIncomeChangedEvent : UnityEvent<GameObject, TurnIncomeChangedEventArgs> { }
}
