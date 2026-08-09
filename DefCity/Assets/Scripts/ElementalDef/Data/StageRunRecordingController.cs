using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Runtime;
using UnityEngine;

namespace ElementalDef.Data
{
    [DisallowMultipleComponent]
    public sealed class StageRunRecordingController : MonoBehaviour
    {
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private GameFlowController gameFlowController;
        [SerializeField] private TowerRegistry towerRegistry;
        [SerializeField] private Health headquartersHealth;

        private double stageStartTime;
        private long defeatedEnemyCount;
        private long attackCount;

        private void OnEnable()
        {
            waveBundleController.OnBundleStarted.AddListener(HandleWaveBundleStarted);
            enemySpawner.OnEnemyDefeated.AddListener(HandleEnemyDefeated);
            gameFlowController.OnVictory.AddListener(HandleVictory);
            gameFlowController.OnDefeat.AddListener(HandleDefeat);
            towerRegistry.OnTowerRegistered.AddListener(HandleTowerRegistered);
            towerRegistry.OnTowerUnregistered.AddListener(HandleTowerUnregistered);

            foreach (TowerUnit tower in towerRegistry.Towers)
            {
                tower.GetComponent<Attacker>().OnAttackFinished.AddListener(HandleAttackFinished);
            }
        }

        private void OnDisable()
        {
            waveBundleController.OnBundleStarted.RemoveListener(HandleWaveBundleStarted);
            enemySpawner.OnEnemyDefeated.RemoveListener(HandleEnemyDefeated);
            gameFlowController.OnVictory.RemoveListener(HandleVictory);
            gameFlowController.OnDefeat.RemoveListener(HandleDefeat);
            towerRegistry.OnTowerRegistered.RemoveListener(HandleTowerRegistered);
            towerRegistry.OnTowerUnregistered.RemoveListener(HandleTowerUnregistered);

            foreach (TowerUnit tower in towerRegistry.Towers)
            {
                tower.GetComponent<Attacker>().OnAttackFinished.RemoveListener(HandleAttackFinished);
            }
        }

        private void HandleWaveBundleStarted(GameObject sender)
        {
            stageStartTime = Time.realtimeSinceStartupAsDouble;
            defeatedEnemyCount = 0;
            attackCount = 0;
        }

        private void HandleEnemyDefeated(GameObject sender)
        {
            defeatedEnemyCount++;
        }

        private void HandleAttackFinished(GameObject sender, AttackResolvedEventArgs args)
        {
            if (args.ResolveStatus == AttackResolveStatus.Succeeded)
            {
                attackCount++;
            }
        }

        private void HandleVictory(GameObject sender)
        {
            RecordStageRunData(StageRunOutcome.Victory);
        }

        private void HandleDefeat(GameObject sender)
        {
            RecordStageRunData(StageRunOutcome.Defeat);
        }

        private void HandleTowerRegistered(GameObject sender)
        {
            TowerUnit tower = sender.GetComponent<TowerUnit>();
            tower.GetComponent<Attacker>().OnAttackFinished.AddListener(HandleAttackFinished);
        }

        private void HandleTowerUnregistered(GameObject sender)
        {
            TowerUnit tower = sender.GetComponent<TowerUnit>();
            tower.GetComponent<Attacker>().OnAttackFinished.RemoveListener(HandleAttackFinished);
        }

        private void RecordStageRunData(StageRunOutcome outcome)
        {
            double stageDurationSeconds = Time.realtimeSinceStartupAsDouble - stageStartTime;
            long stageDurationMilliseconds = (long)Math.Round(stageDurationSeconds * 1000d);

            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            StageRunContext stageRunContext = applicationRoot.StageLaunch.Current;
            long earnedCredits = 0;
            long earnedExperience = 0;

            if (outcome == StageRunOutcome.Victory)
            {
                earnedCredits = CalculateReward(
                    stageRunContext.BaseCreditReward,
                    stageRunContext.CreditRewardMultiplier,
                    nameof(stageRunContext.CreditRewardMultiplier));
                earnedExperience = CalculateReward(
                    stageRunContext.BaseExperienceReward,
                    stageRunContext.ExperienceRewardMultiplier,
                    nameof(stageRunContext.ExperienceRewardMultiplier));
            }

            var snapshot = new CompletedStageRunSnapshot(
                stageRunContext.RunId,
                stageRunContext.StageId,
                stageRunContext.DisplayOrder,
                stageDurationMilliseconds,
                headquartersHealth.CurrentHealth,
                headquartersHealth.MaxHealth,
                defeatedEnemyCount,
                attackCount,
                earnedCredits,
                earnedExperience,
                outcome,
                DateTimeOffset.UtcNow);

            applicationRoot.RunStore.Commit(snapshot);
        }

        private static long CalculateReward(
            int baseReward,
            float rewardMultiplier,
            string multiplierName)
        {
            if (baseReward < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseReward),
                    baseReward,
                    "A base reward cannot be negative.");
            }

            if (float.IsNaN(rewardMultiplier) ||
                float.IsInfinity(rewardMultiplier) ||
                rewardMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    multiplierName,
                    rewardMultiplier,
                    "A reward multiplier must be finite and non-negative.");
            }

            double scaledReward = baseReward * (double)rewardMultiplier;
            if (scaledReward > long.MaxValue)
            {
                throw new OverflowException(
                    $"The calculated reward {scaledReward} exceeds {long.MaxValue}.");
            }

            return (long)Math.Round(
                scaledReward,
                MidpointRounding.AwayFromZero);
        }
    }
}
