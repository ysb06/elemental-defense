using System;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public sealed class TowerEnergyManager : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxEnergy = 100f;
        [SerializeField, Min(0f)] private float currentEnergy;

        public float MaxEnergy => maxEnergy;
        public float CurrentEnergy => currentEnergy;

        public TowerEnergyEvent OnTowerEnergyConsumed = new();

        private void Awake()
        {
            currentEnergy = maxEnergy;
        }

        public bool CanAfford(TowerCost towerCost)
        {
            if (towerCost == null)
            {
                return false;
            }

            float amount = towerCost.Cost;
            return amount >= 0f && currentEnergy >= amount;
        }

        public TowerEnergyEventArgs TryConsumeEnergy(TowerCost towerCost)
        {
            if (towerCost == null)
            {
                TowerEnergyEventArgs undefinedCostResult = new(
                    null,
                    currentEnergy,
                    0f,
                    currentEnergy,
                    TowerEnergyResult.NotDefinedCost);
                OnTowerEnergyConsumed?.Invoke(gameObject, undefinedCostResult);
                return undefinedCostResult;
            }

            float amount = towerCost.Cost;
            if (amount < 0f)
            {
                TowerEnergyEventArgs invalidCostResult = new(
                    towerCost.gameObject,
                    currentEnergy,
                    0f,
                    currentEnergy,
                    TowerEnergyResult.InvalidCost);
                OnTowerEnergyConsumed?.Invoke(gameObject, invalidCostResult);
                return invalidCostResult;
            }

            if (currentEnergy < amount)
            {
                TowerEnergyEventArgs insufficientEnergyResult = new(
                    towerCost.gameObject,
                    currentEnergy,
                    0f,
                    currentEnergy,
                    TowerEnergyResult.InsufficientEnergy);
                OnTowerEnergyConsumed?.Invoke(gameObject, insufficientEnergyResult);
                return insufficientEnergyResult;
            }

            float previousEnergy = currentEnergy;
            currentEnergy -= amount;

            TowerEnergyEventArgs successResult = new(
                towerCost.gameObject,
                previousEnergy,
                amount,
                currentEnergy,
                TowerEnergyResult.Success);
            OnTowerEnergyConsumed?.Invoke(gameObject, successResult);
            return successResult;
        }
    }

    public enum TowerEnergyResult
    {
        Undefined,
        Success,
        NotDefinedCost,
        InvalidCost,
        InsufficientEnergy,
    }

    public struct TowerEnergyEventArgs
    {
        public GameObject Tower { get; }
        public float PreviousEnergy { get; }
        public float EnergyConsumed { get; }
        public float CurrentEnergy { get; set; }
        public TowerEnergyResult Result { get; set; }

        public TowerEnergyEventArgs(
            GameObject tower,
            float previousEnergy,
            float energyConsumed,
            float currentEnergy,
            TowerEnergyResult result)
        {
            Tower = tower;
            PreviousEnergy = previousEnergy;
            EnergyConsumed = energyConsumed;
            CurrentEnergy = currentEnergy;
            Result = result;
        }
    }

    [Serializable]
    public class TowerEnergyEvent : UnityEvent<GameObject, TowerEnergyEventArgs>
    {
    }
}
