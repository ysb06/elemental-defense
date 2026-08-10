using System;
using System.Collections.Generic;
using DefCore.Gameplay.Combat;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Flow;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    [DisallowMultipleComponent]
    public sealed class SimpleAreaSkill : SkillExecutorBase
    {
        private readonly struct PendingTargetDamage
        {
            public Health Health { get; }
            public float RequestedDamage { get; }

            public PendingTargetDamage(Health health, float requestedDamage)
            {
                Health = health;
                RequestedDamage = requestedDamage;
            }
        }

        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField, Min(0f)] private float damageMultiplier = 3f;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
                // 임시 참조 해결 코드
                // 추후 EnemySpawner를 명시적으로 주입하거나 적을 찾는 방법을 변경
            }
        }

        public override void BeginExecute(SkillExecutionContext context, Action<SkillExecutionResult> onResolved)
        {
            IReadOnlyList<EnemyUnit> activeEnemies = enemySpawner.ActiveEnemies;
            List<PendingTargetDamage> pendingTargetDamages = new(activeEnemies.Count);
            foreach (EnemyUnit enemy in activeEnemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!enemy.TryGetComponent(out Health enemyHealth))
                {
                    throw new InvalidOperationException(
                        $"[{enemy.name}] An active area-skill target requires a {nameof(Health)} component.");
                }

                if (!enemyHealth.IsAlive)
                {
                    continue;
                }

                if (!enemy.TryGetComponent(out ElementalCombatant enemyCombatant))
                {
                    throw new InvalidOperationException(
                        $"[{enemy.name}] An active area-skill target requires an {nameof(ElementalCombatant)} component.");
                }

                float requestedDamage = InitializationContext.DamageCalculator.CalculateDamage(
                    context.AttackPower,
                    damageMultiplier,
                    context.AttackElement,
                    context.CastPosition,
                    enemyCombatant);
                pendingTargetDamages.Add(new PendingTargetDamage(enemyHealth, requestedDamage));
            }

            float totalDamage = 0f;
            int affectedTargetCount = 0;
            int defeatedTargetCount = 0;
            foreach (PendingTargetDamage pendingTargetDamage in pendingTargetDamages)
            {
                Health enemyHealth = pendingTargetDamage.Health;
                if (enemyHealth == null ||
                    !enemyHealth.gameObject.activeInHierarchy ||
                    !enemyHealth.IsAlive)
                {
                    continue;
                }

                DamageEventArgs result = enemyHealth.TakeDamage(
                    context.CasterRoot,
                    pendingTargetDamage.RequestedDamage);
                affectedTargetCount++;
                totalDamage += result.DamageAmount;
                defeatedTargetCount += result.IsFatal ? 1 : 0;
            }

            SkillExecutionStatus status = affectedTargetCount > 0
                ? SkillExecutionStatus.Succeeded
                : SkillExecutionStatus.NoTargets;
            SkillExecutionResult args = new(
                context,
                status,
                affectedTargetCount,
                totalDamage,
                defeatedTargetCount);
            if (onResolved != null)
            {
                onResolved(args);
            }
            else
            {
                Debug.LogError($"No callback provided for skill execution result. Result: {args}");
            }
        }

        public override bool CanExecute(SkillExecutionContext context)
        {
            if (enemySpawner.CurrentActiveEnemyCount <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
