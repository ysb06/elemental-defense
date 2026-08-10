using System;
using ElementalDef.Gameplay.Flow;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalDef.Gameplay.Economy
{
    [DisallowMultipleComponent]
    public sealed class TowerEnergyManager : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxEnergy = 100f;
        [SerializeField, Min(0f)] private float currentEnergy;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField, Min(0f)] private float energyPerSecondDuringWave;
        [SerializeField, Min(0f)] private float energyOnWaveCompleted;

        public float MaxEnergy => maxEnergy;
        public float CurrentEnergy => currentEnergy;
        public float EnergyPerSecondDuringWave => energyPerSecondDuringWave;
        public float EnergyOnWaveCompleted => energyOnWaveCompleted;

        public TowerEnergyEvent OnTowerEnergyConsumed = new();
        public event Action<float> EnergyChanged;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerEnergyManager)} requires an {nameof(EnemySpawner)} reference.");
            }
            EnergyChanged?.Invoke(currentEnergy);
        }

        private void OnEnable()
        {
            enemySpawner.OnWaveCompleted.AddListener(HandleWaveCompleted);
        }

        private void OnDisable()
        {
            if (enemySpawner != null)
            {
                enemySpawner.OnWaveCompleted.RemoveListener(HandleWaveCompleted);
            }
        }

        private void Update()
        {
            if (!enemySpawner.IsWaveRunning || energyPerSecondDuringWave <= 0f)
            {
                return;
            }

            AddEnergy(energyPerSecondDuringWave * Time.deltaTime);
        }

        public bool CanAfford(float amount)
        {
            return IsValidCost(amount) && currentEnergy >= amount;
        }

        public TowerEnergyEventArgs TryConsumeEnergy(GameObject tower, float amount)
        {
            if (tower == null)
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

            if (!IsValidCost(amount))
            {
                TowerEnergyEventArgs invalidCostResult = new(
                    tower,
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
                    tower,
                    currentEnergy,
                    0f,
                    currentEnergy,
                    TowerEnergyResult.InsufficientEnergy);
                OnTowerEnergyConsumed?.Invoke(gameObject, insufficientEnergyResult);
                return insufficientEnergyResult;
            }

            float previousEnergy = currentEnergy;
            currentEnergy -= amount;
            EnergyChanged?.Invoke(currentEnergy);

            TowerEnergyEventArgs successResult = new(
                tower,
                previousEnergy,
                amount,
                currentEnergy,
                TowerEnergyResult.Success);
            OnTowerEnergyConsumed?.Invoke(gameObject, successResult);
            return successResult;
        }

        public void AddEnergy(float amount)
        {
            if (amount == 0f)
            {
                return;
            }

            currentEnergy += amount;
            EnergyChanged?.Invoke(currentEnergy);
        }

        private void HandleWaveCompleted(GameObject sender)
        {
            if (sender != enemySpawner.gameObject)
            {
                return;
            }

            AddEnergy(energyOnWaveCompleted);
        }

        private static bool IsValidCost(float amount)
        {
            return !float.IsNaN(amount) &&
                   !float.IsInfinity(amount) &&
                   amount >= 0f;
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
