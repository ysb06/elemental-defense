using System;
using UnityEngine;

namespace DefCity.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public class TurnCreditProvider : MonoBehaviour
    {
        [SerializeField] private CreditManager creditManager;
        [SerializeField, Min(0)] private int turnCredit;

        private CreditManager registeredCreditManager;

        public int TurnCredit => turnCredit;
        public CreditManager CreditManager => creditManager;

        private void OnValidate()
        {
            turnCredit = Math.Max(0, turnCredit);
        }

        private void OnEnable()
        {
            RegisterWithCreditManagerIfPossible();
        }

        private void OnDisable()
        {
            UnregisterFromCurrentCreditManager();
        }

        public void SetCreditManager(CreditManager manager)
        {
            if (creditManager == manager)
            {
                return;
            }

            UnregisterFromCurrentCreditManager();
            creditManager = manager;
            RegisterWithCreditManagerIfPossible();
        }

        private void RegisterWithCreditManagerIfPossible()
        {
            if (!isActiveAndEnabled || creditManager == null)
            {
                return;
            }

            if (registeredCreditManager == creditManager)
            {
                return;
            }

            if (registeredCreditManager != null)
            {
                UnregisterFromCurrentCreditManager();
            }

            creditManager.RegisterTurnCreditProvider(this);
            registeredCreditManager = creditManager;
        }

        private void UnregisterFromCurrentCreditManager()
        {
            if (registeredCreditManager == null)
            {
                return;
            }

            registeredCreditManager.UnregisterTurnCreditProvider(this);
            registeredCreditManager = null;
        }
    }
}
