using System;
using UnityEngine;

namespace ElementalDef.Gameplay.Entities.Settings
{
    [Serializable]
    public struct EnemyDifficultyStatApplication
    {
        [SerializeField, Min(0f)] private float attackPower;
        [SerializeField, Min(0f)] private float attackRange;
        [SerializeField, Min(0f)] private float attackCooldown;
        [SerializeField, Min(0f)] private float maxHealth;
        [SerializeField, Min(0f)] private float defense;
        [SerializeField, Min(0f)] private float acquisitionPadding;
        [SerializeField, Min(0f)] private float scannerInterval;
        [SerializeField, Min(0f)] private float movementSpeed;
        [SerializeField, Min(0f)] private float movementAcceleration;
        [SerializeField, Min(0f)] private float movementAngularSpeed;
        [SerializeField, Min(0f)] private float stoppingDistance;

        public float AttackPower => attackPower;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float MaxHealth => maxHealth;
        public float Defense => defense;
        public float AcquisitionPadding => acquisitionPadding;
        public float ScannerInterval => scannerInterval;
        public float MovementSpeed => movementSpeed;
        public float MovementAcceleration => movementAcceleration;
        public float MovementAngularSpeed => movementAngularSpeed;
        public float StoppingDistance => stoppingDistance;

        public static EnemyDifficultyStatApplication Full => new(
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f,
            1f);

        public static EnemyDifficultyStatApplication StandardStageScaling => new(
            1f,
            0f,
            0.5f,
            1f,
            0.5f,
            0f,
            0f,
            0.3f,
            0f,
            0f,
            0f);

        public EnemyDifficultyStatApplication(
            float attackPower,
            float attackRange,
            float attackCooldown,
            float maxHealth,
            float defense,
            float acquisitionPadding,
            float scannerInterval,
            float movementSpeed,
            float movementAcceleration,
            float movementAngularSpeed,
            float stoppingDistance)
        {
            this.attackPower = attackPower;
            this.attackRange = attackRange;
            this.attackCooldown = attackCooldown;
            this.maxHealth = maxHealth;
            this.defense = defense;
            this.acquisitionPadding = acquisitionPadding;
            this.scannerInterval = scannerInterval;
            this.movementSpeed = movementSpeed;
            this.movementAcceleration = movementAcceleration;
            this.movementAngularSpeed = movementAngularSpeed;
            this.stoppingDistance = stoppingDistance;
        }

        internal AttackStats Apply(AttackStats baseStats, float difficultyMultiplier)
        {
            AttackStats scaled = baseStats;
            scaled.Power = GetRoundedScaledStat(
                baseStats.Power,
                difficultyMultiplier,
                attackPower,
                nameof(attackPower));
            scaled.Range = GetMultipliedScaledStat(
                baseStats.Range,
                difficultyMultiplier,
                attackRange,
                nameof(attackRange));
            scaled.Cooldown = GetDividedScaledStat(
                baseStats.Cooldown,
                difficultyMultiplier,
                attackCooldown,
                nameof(attackCooldown));
            return scaled;
        }

        internal DefenseStats Apply(DefenseStats baseStats, float difficultyMultiplier)
        {
            DefenseStats scaled = baseStats;
            scaled.MaxHealth = GetRoundedScaledStat(
                baseStats.MaxHealth,
                difficultyMultiplier,
                maxHealth,
                nameof(maxHealth));
            scaled.Defense = GetRoundedScaledStat(
                baseStats.Defense,
                difficultyMultiplier,
                defense,
                nameof(defense));
            return scaled;
        }

        internal ScannerStats Apply(ScannerStats baseStats, float difficultyMultiplier)
        {
            ScannerStats scaled = baseStats;
            scaled.AcquisitionPadding = GetMultipliedScaledStat(
                baseStats.AcquisitionPadding,
                difficultyMultiplier,
                acquisitionPadding,
                nameof(acquisitionPadding));
            scaled.Interval = GetDividedScaledStat(
                baseStats.Interval,
                difficultyMultiplier,
                scannerInterval,
                nameof(scannerInterval));
            return scaled;
        }

        internal MovementStats Apply(MovementStats baseStats, float difficultyMultiplier)
        {
            MovementStats scaled = baseStats;
            scaled.Speed = GetMultipliedScaledStat(
                baseStats.Speed,
                difficultyMultiplier,
                movementSpeed,
                nameof(movementSpeed));
            scaled.Acceleration = GetMultipliedScaledStat(
                baseStats.Acceleration,
                difficultyMultiplier,
                movementAcceleration,
                nameof(movementAcceleration));
            scaled.AngularSpeed = GetMultipliedScaledStat(
                baseStats.AngularSpeed,
                difficultyMultiplier,
                movementAngularSpeed,
                nameof(movementAngularSpeed));
            scaled.StoppingDistance = GetDividedScaledStat(
                baseStats.StoppingDistance,
                difficultyMultiplier,
                stoppingDistance,
                nameof(stoppingDistance));
            return scaled;
        }

        private static float GetStrengthMultiplier(
            float difficultyMultiplier,
            float application,
            string applicationName)
        {
            if (float.IsNaN(difficultyMultiplier) ||
                float.IsInfinity(difficultyMultiplier) ||
                difficultyMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(difficultyMultiplier),
                    difficultyMultiplier,
                    "A difficulty multiplier must be finite and greater than 0.");
            }

            if (float.IsNaN(application) ||
                float.IsInfinity(application) ||
                application < 0f)
            {
                throw new InvalidOperationException(
                    $"Difficulty application '{applicationName}' must be finite and non-negative.");
            }

            float multiplier = 1f + ((difficultyMultiplier - 1f) * application);
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier))
            {
                throw new OverflowException(
                    $"Difficulty application '{applicationName}' produced a non-finite multiplier.");
            }

            if (multiplier <= 0f)
            {
                throw new InvalidOperationException(
                    $"Difficulty application '{applicationName}' produced a non-positive " +
                    $"strength multiplier {multiplier} from difficulty {difficultyMultiplier} " +
                    $"and application {application}.");
            }

            return multiplier;
        }

        private static float GetMultipliedScaledStat(
            float baseValue,
            float difficultyMultiplier,
            float application,
            string applicationName)
        {
            float multiplier = GetStrengthMultiplier(
                difficultyMultiplier,
                application,
                applicationName);
            float scaledValue = baseValue * multiplier;
            ValidateScaledStat(baseValue, scaledValue, applicationName);
            return scaledValue;
        }

        private static float GetDividedScaledStat(
            float baseValue,
            float difficultyMultiplier,
            float application,
            string applicationName)
        {
            float multiplier = GetStrengthMultiplier(
                difficultyMultiplier,
                application,
                applicationName);
            float scaledValue = baseValue / multiplier;
            ValidateScaledStat(baseValue, scaledValue, applicationName);
            return scaledValue;
        }

        private static void ValidateScaledStat(
            float baseValue,
            float scaledValue,
            string applicationName)
        {
            if (float.IsNaN(scaledValue) || float.IsInfinity(scaledValue))
            {
                throw new OverflowException(
                    $"Difficulty application '{applicationName}' produced a non-finite scaled stat.");
            }

            if (baseValue > 0f && scaledValue <= 0f)
            {
                throw new InvalidOperationException(
                    $"Difficulty application '{applicationName}' reduced a positive stat " +
                    $"to the non-positive value {scaledValue}.");
            }
        }

        private static float GetRoundedScaledStat(
            float baseValue,
            float difficultyMultiplier,
            float application,
            string applicationName)
        {
            float multiplier = GetStrengthMultiplier(
                difficultyMultiplier,
                application,
                applicationName);
            double scaledValue = baseValue * (double)multiplier;
            if (double.IsNaN(scaledValue) ||
                double.IsInfinity(scaledValue) ||
                scaledValue > float.MaxValue ||
                scaledValue < float.MinValue)
            {
                throw new OverflowException(
                    $"Difficulty application '{applicationName}' produced an invalid scaled stat.");
            }

            float roundedValue = Mathf.Round((float)scaledValue);
            if (baseValue > 0f && roundedValue < 1f)
            {
                return 1f;
            }

            return roundedValue;
        }
    }

    [CreateAssetMenu(menuName = "ElementalDef/Units/Enemy Spec")]
    public sealed class EnemyUnitSpec : UnitSpec
    {
        [SerializeField] private MovementStats movement;
        [SerializeField] private EnemyDifficultyStatApplication difficultyScaling =
            EnemyDifficultyStatApplication.StandardStageScaling;

        public MovementStats Movement => movement;
        public EnemyDifficultyStatApplication DifficultyScaling => difficultyScaling;

        public AttackStats GetAttackStats(float difficultyMultiplier)
        {
            return difficultyScaling.Apply(Attack, difficultyMultiplier);
        }

        public DefenseStats GetDefenseStats(float difficultyMultiplier)
        {
            return difficultyScaling.Apply(Defense, difficultyMultiplier);
        }

        public ScannerStats GetScannerStats(float difficultyMultiplier)
        {
            return difficultyScaling.Apply(Scanner, difficultyMultiplier);
        }

        public MovementStats GetMovementStats(float difficultyMultiplier)
        {
            return difficultyScaling.Apply(movement, difficultyMultiplier);
        }
    }
}
