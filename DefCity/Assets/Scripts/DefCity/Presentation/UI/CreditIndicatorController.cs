using System;
using UnityEngine;
using DefCity.Gameplay.Economy;

namespace DefCity.Presentation.UI
{
    public class CreditIndicatorController : MonoBehaviour
    {
        [SerializeField] private CreditManager creditManager;
        [SerializeField] private TMPro.TextMeshProUGUI creditText;

        private void OnEnable()
        {
            creditManager.OnCreditsChanged.AddListener(OnCreditsChanged);
            creditManager.OnTurnIncomeChanged.AddListener(OnTurnIncomeChanged);
            RefreshCreditText();
        }

        private void OnDisable()
        {
            creditManager.OnCreditsChanged.RemoveListener(OnCreditsChanged);
            creditManager.OnTurnIncomeChanged.RemoveListener(OnTurnIncomeChanged);
        }

        private void OnCreditsChanged(GameObject sender, CreditChangedEventArgs args)
        {
            RefreshCreditText();
        }

        private void OnTurnIncomeChanged(GameObject sender, TurnIncomeChangedEventArgs args)
        {
            RefreshCreditText();
        }

        private void RefreshCreditText()
        {
            creditText.text = $"{creditManager.Credits} (+{creditManager.TurnIncome})";
        }
    }
}
