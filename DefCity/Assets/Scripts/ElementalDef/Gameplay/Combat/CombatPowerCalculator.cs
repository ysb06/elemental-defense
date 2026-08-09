using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Entities.Settings;

namespace ElementalDef.Gameplay.Combat
{
    public static class CombatPowerCalculator
    {
        private const double DamagePerSecondWeight = 10d;
        private const double AttackRangeBonusPerUnit = 0.1d;
        private const double MaxHealthWeight = 0.5d;
        private const double DefenseWeight = 15d;

        public static int Calculate(TowerUnitSpec towerSpec)
        {
            if (towerSpec == null)
            {
                throw new ArgumentNullException(nameof(towerSpec));
            }

            return RoundToCombatPower(CalculateRaw(towerSpec.Attack, towerSpec.Defense));
        }

        public static int Calculate(AttackStats attack, DefenseStats defense)
        {
            return RoundToCombatPower(CalculateRaw(attack, defense));
        }

        public static int CalculateTotal(IReadOnlyList<TowerUnitSpec> towerSpecs)
        {
            if (towerSpecs == null)
            {
                throw new ArgumentNullException(nameof(towerSpecs));
            }

            double rawTotal = 0d;
            for (int index = 0; index < towerSpecs.Count; index++)
            {
                TowerUnitSpec towerSpec = towerSpecs[index];
                if (towerSpec == null)
                {
                    throw new ArgumentException(
                        $"Tower spec at index {index} cannot be null.",
                        nameof(towerSpecs));
                }

                rawTotal += CalculateRaw(towerSpec.Attack, towerSpec.Defense);
                EnsureFinite(rawTotal, nameof(towerSpecs));
            }

            return RoundToCombatPower(rawTotal);
        }

        private static double CalculateRaw(AttackStats attack, DefenseStats defense)
        {
            EnsureNonNegativeFinite(attack.Power, $"{nameof(attack)}.{nameof(AttackStats.Power)}");
            EnsurePositiveFinite(attack.Cooldown, $"{nameof(attack)}.{nameof(AttackStats.Cooldown)}");
            EnsureNonNegativeFinite(attack.Range, $"{nameof(attack)}.{nameof(AttackStats.Range)}");
            EnsureNonNegativeFinite(defense.MaxHealth, $"{nameof(defense)}.{nameof(DefenseStats.MaxHealth)}");
            EnsureNonNegativeFinite(defense.Defense, $"{nameof(defense)}.{nameof(DefenseStats.Defense)}");

            double damagePerSecond = attack.Power / (double)attack.Cooldown;
            double rawPower =
                (damagePerSecond * (1d + (attack.Range * AttackRangeBonusPerUnit)) * DamagePerSecondWeight) +
                (defense.MaxHealth * MaxHealthWeight) +
                (defense.Defense * DefenseWeight);

            EnsureFinite(rawPower, "combatPower");
            return rawPower;
        }

        private static int RoundToCombatPower(double rawPower)
        {
            EnsureFinite(rawPower, nameof(rawPower));
            double roundedPower = Math.Round(rawPower, MidpointRounding.AwayFromZero);
            if (roundedPower < 0d || roundedPower > int.MaxValue)
            {
                throw new OverflowException(
                    $"Combat power {roundedPower} is outside the supported Int32 range.");
            }

            return (int)roundedPower;
        }

        private static void EnsureNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Combat power inputs must be finite and non-negative.");
            }
        }

        private static void EnsurePositiveFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Attack cooldown must be finite and greater than zero.");
            }
        }

        private static void EnsureFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new OverflowException(
                    $"Combat power calculation for '{parameterName}' produced a non-finite value.");
            }
        }
    }
}
