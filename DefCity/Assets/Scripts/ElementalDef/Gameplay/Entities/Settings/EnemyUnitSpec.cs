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
            scaled.Power *= GetStrengthMultiplier(difficultyMultiplier, attackPower, nameof(attackPower));
            scaled.Range *= GetStrengthMultiplier(difficultyMultiplier, attackRange, nameof(attackRange));
            scaled.Cooldown /= GetStrengthMultiplier(difficultyMultiplier, attackCooldown, nameof(attackCooldown));
            return scaled;
        }

        internal DefenseStats Apply(DefenseStats baseStats, float difficultyMultiplier)
        {
            DefenseStats scaled = baseStats;
            scaled.MaxHealth *= GetStrengthMultiplier(difficultyMultiplier, maxHealth, nameof(maxHealth));
            scaled.Defense *= GetStrengthMultiplier(difficultyMultiplier, defense, nameof(defense));
            return scaled;
        }

        internal ScannerStats Apply(ScannerStats baseStats, float difficultyMultiplier)
        {
            ScannerStats scaled = baseStats;
            scaled.AcquisitionPadding *= GetStrengthMultiplier(
                difficultyMultiplier,
                acquisitionPadding,
                nameof(acquisitionPadding));
            scaled.Interval /= GetStrengthMultiplier(
                difficultyMultiplier,
                scannerInterval,
                nameof(scannerInterval));
            return scaled;
        }

        internal MovementStats Apply(MovementStats baseStats, float difficultyMultiplier)
        {
            MovementStats scaled = baseStats;
            scaled.Speed *= GetStrengthMultiplier(difficultyMultiplier, movementSpeed, nameof(movementSpeed));
            scaled.Acceleration *= GetStrengthMultiplier(
                difficultyMultiplier,
                movementAcceleration,
                nameof(movementAcceleration));
            scaled.AngularSpeed *= GetStrengthMultiplier(
                difficultyMultiplier,
                movementAngularSpeed,
                nameof(movementAngularSpeed));
            scaled.StoppingDistance /= GetStrengthMultiplier(
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
                difficultyMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(difficultyMultiplier));
            }

            if (float.IsNaN(application) ||
                float.IsInfinity(application) ||
                application < 0f)
            {
                throw new InvalidOperationException(
                    $"Difficulty application '{applicationName}' must be finite and non-negative.");
            }

            float multiplier = 1f + ((difficultyMultiplier - 1f) * application);
            if (float.IsInfinity(multiplier))
            {
                throw new OverflowException(
                    $"Difficulty application '{applicationName}' produced an infinite multiplier.");
            }

            return multiplier;
        }
    }

    [CreateAssetMenu(menuName = "ElementalDef/Units/Enemy Spec")]
    public sealed class EnemyUnitSpec : UnitSpec
    {
        [SerializeField] private MovementStats movement;
        [SerializeField] private EnemyDifficultyStatApplication difficultyScaling =
            EnemyDifficultyStatApplication.Full;

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
