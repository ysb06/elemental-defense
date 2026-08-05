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

            var snapshot = new CompletedStageRunSnapshot(
                stageRunContext.RunId,
                stageRunContext.StageId,
                stageDurationMilliseconds,
                headquartersHealth.CurrentHealth,
                defeatedEnemyCount,
                attackCount,
                outcome,
                DateTimeOffset.UtcNow);

            applicationRoot.RunStore.Commit(snapshot);
        }
    }
}
