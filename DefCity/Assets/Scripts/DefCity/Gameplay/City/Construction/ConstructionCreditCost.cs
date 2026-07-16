using System;
using UnityEngine;
using DefCity.Gameplay.Economy;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.City.Construction
{
    [RequireComponent(typeof(Entity))]
    public class ConstructionCreditCost : MonoBehaviour
    {
        [SerializeField] private CreditManager creditManager;
        [SerializeField, Min(0)] private int constructionCost;
        [SerializeField, Min(0)] private int demolitionRefund;

        private bool hasChargedConstructionCost;
        private bool hasRefundedDemolitionCredit;

        public CreditManager CreditManager => creditManager;
        public int ConstructionCost => constructionCost;
        public int DemolitionRefund => demolitionRefund;

        private void OnValidate()
        {
            constructionCost = Math.Max(0, constructionCost);
            demolitionRefund = Math.Max(0, demolitionRefund);
        }

        private void Start()
        {
            TryChargeConstructionCost();
        }

        public void SetCreditManager(CreditManager manager)
        {
            creditManager = manager;
        }

        public bool TryChargeConstructionCost()
        {
            if (hasChargedConstructionCost || creditManager == null || constructionCost <= 0)
            {
                return false;
            }

            creditManager.SubtractCredits(constructionCost);
            hasChargedConstructionCost = true;
            return true;
        }

        public bool TryRefundDemolitionCredit()
        {
            if (hasRefundedDemolitionCredit || creditManager == null || demolitionRefund <= 0)
            {
                return false;
            }

            creditManager.AddCredits(demolitionRefund);
            hasRefundedDemolitionCredit = true;
            return true;
        }
    }
}
