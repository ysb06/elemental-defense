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
            float requestedDamage = InitializationContext.DamageCalculator.CalculateDamage(context.AttackPower, damageMultiplier, context.AttackElement, context.CastPosition);
            float totalDamage = 0;
            int defeatedTargetCount = 0;
            IReadOnlyList<EnemyUnit> activeEnemies = enemySpawner.ActiveEnemies;
            foreach (var enemy in activeEnemies)
            {
                Health enemyHealth = enemy.GetComponent<Health>();
                DamageEventArgs result = enemyHealth.TakeDamage(context.CasterRoot, requestedDamage);
                totalDamage += result.DamageAmount;
                defeatedTargetCount += result.IsFatal ? 1 : 0;
            }

            SkillExecutionResult args = new(context, SkillExecutionStatus.Succeeded, activeEnemies.Count, totalDamage, defeatedTargetCount);
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