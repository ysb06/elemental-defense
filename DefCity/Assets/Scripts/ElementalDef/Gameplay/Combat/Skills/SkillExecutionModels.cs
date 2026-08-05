using System;
using DefCore.Gameplay.Combat;
using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    public enum SkillExecutionStatus
    {
        Succeeded,
        NoTargets,
        Cancelled,
        Faulted,
    }

    public sealed class SkillExecutorInitializationContext
    {
        public TowerUnit Owner { get; }
        public Attacker Attacker { get; }
        public ElementalDamageCalculator DamageCalculator { get; }

        public SkillExecutorInitializationContext(
            TowerUnit owner,
            Attacker attacker,
            ElementalDamageCalculator damageCalculator)
        {
            Owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            Attacker = attacker != null ? attacker : throw new ArgumentNullException(nameof(attacker));
            DamageCalculator = damageCalculator != null
                ? damageCalculator
                : throw new ArgumentNullException(nameof(damageCalculator));

            if (attacker.gameObject != owner.gameObject)
            {
                throw new ArgumentException(
                    "The skill attacker must belong to the owner tower root.",
                    nameof(attacker));
            }
        }
    }

    public sealed class SkillExecutionContext
    {
        public string ExecutionId { get; }
        public SkillDefinition Definition { get; }
        public TowerUnit Owner { get; }
        public GameObject CasterRoot { get; }
        public string TowerInstanceId { get; }
        public Vector3 CastPosition { get; }
        public float AttackPower { get; }
        public ElementType AttackElement { get; }
        public DateTimeOffset StartedAtUtc { get; }

        public SkillExecutionContext(
            string executionId,
            SkillDefinition definition,
            TowerUnit owner,
            Vector3 castPosition,
            float attackPower,
            ElementType attackElement,
            DateTimeOffset startedAtUtc)
        {
            if (!Guid.TryParseExact(executionId, "N", out Guid parsedExecutionId))
            {
                throw new ArgumentException(
                    "A 32-character GUID in N format is required.",
                    nameof(executionId));
            }

            Definition = definition != null
                ? definition
                : throw new ArgumentNullException(nameof(definition));
            Definition.ValidateOrThrow();
            Owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));

            if (!Guid.TryParseExact(owner.InstanceId, "N", out Guid parsedTowerInstanceId))
            {
                throw new InvalidOperationException(
                    "The owner tower must have a runtime instance ID in GUID N format.");
            }

            EnsureFinite(castPosition, nameof(castPosition));
            EnsureNonNegativeFinite(attackPower, nameof(attackPower));
            if (!Enum.IsDefined(typeof(ElementType), attackElement))
            {
                throw new ArgumentOutOfRangeException(nameof(attackElement));
            }

            ExecutionId = parsedExecutionId.ToString("N");
            CasterRoot = owner.gameObject;
            TowerInstanceId = parsedTowerInstanceId.ToString("N");
            CastPosition = castPosition;
            AttackPower = attackPower;
            AttackElement = attackElement;
            StartedAtUtc = startedAtUtc.ToUniversalTime();
        }

        private static void EnsureFinite(Vector3 value, string parameterName)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || !IsFinite(value.z))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "The cast position must contain only finite values.");
            }
        }

        private static void EnsureNonNegativeFinite(float value, string parameterName)
        {
            if (!IsFinite(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "The attack power must be a finite, non-negative value.");
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    public sealed class SkillExecutionResult
    {
        public SkillExecutionContext Context { get; }
        public string ExecutionId => Context.ExecutionId;
        public SkillExecutionStatus Status { get; }
        public int AffectedTargetCount { get; }
        public float TotalDamage { get; }
        public int DefeatedTargetCount { get; }
        public Exception Exception { get; }

        public SkillExecutionResult(
            SkillExecutionContext context,
            SkillExecutionStatus status,
            int affectedTargetCount,
            float totalDamage,
            int defeatedTargetCount,
            Exception exception = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            if (!Enum.IsDefined(typeof(SkillExecutionStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (affectedTargetCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(affectedTargetCount));
            }

            if (float.IsNaN(totalDamage) || float.IsInfinity(totalDamage) || totalDamage < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(totalDamage));
            }

            if (defeatedTargetCount < 0 || defeatedTargetCount > affectedTargetCount)
            {
                throw new ArgumentOutOfRangeException(nameof(defeatedTargetCount));
            }

            Status = status;
            AffectedTargetCount = affectedTargetCount;
            TotalDamage = totalDamage;
            DefeatedTargetCount = defeatedTargetCount;
            Exception = exception;
        }
    }
}
