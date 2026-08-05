using System;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Flow;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    [DisallowMultipleComponent]
    public sealed class TowerSkillCoordinator : MonoBehaviour
    {
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private TowerRegistry towerRegistry;

        private bool isSubscribed;
        private bool isBattleActive;

        private void Awake()
        {
            if (waveBundleController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerSkillCoordinator)} requires a {nameof(WaveBundleController)} reference.");
            }

            if (towerRegistry == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TowerSkillCoordinator)} requires a {nameof(TowerRegistry)} reference.");
            }
        }

        private void OnEnable()
        {
            Subscribe();
            isBattleActive = waveBundleController.IsBattleActive;
            ApplyBattleStateToRegisteredTowers();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ApplyBattleStateToRegisteredTowers(false);
        }

        private void HandleBundleStarted(GameObject sender)
        {
            if (sender != waveBundleController.gameObject || isBattleActive)
            {
                return;
            }

            isBattleActive = true;
            ApplyBattleStateToRegisteredTowers();
        }

        private void HandleTowerRegistered(GameObject sender)
        {
            SetTowerBattleState(sender, isBattleActive);
        }

        private void HandleTowerUnregistered(GameObject sender)
        {
            if (sender != null && sender.TryGetComponent(out TowerUnit tower) && tower.SkillController != null)
            {
                tower.SkillController.Shutdown();
            }
        }

        private void ApplyBattleStateToRegisteredTowers()
        {
            ApplyBattleStateToRegisteredTowers(isBattleActive);
        }

        private void ApplyBattleStateToRegisteredTowers(bool active)
        {
            if (towerRegistry == null)
            {
                return;
            }

            foreach (TowerUnit tower in towerRegistry.Towers)
            {
                if (tower != null && tower.SkillController != null)
                {
                    tower.SkillController.SetBattleActive(active);
                }
            }
        }

        private static void SetTowerBattleState(GameObject sender, bool isActive)
        {
            if (sender != null && sender.TryGetComponent(out TowerUnit tower) && tower.SkillController != null)
            {
                tower.SkillController.SetBattleActive(isActive);
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            waveBundleController.OnBundleStarted.AddListener(HandleBundleStarted);
            towerRegistry.OnTowerRegistered.AddListener(HandleTowerRegistered);
            towerRegistry.OnTowerUnregistered.AddListener(HandleTowerUnregistered);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (waveBundleController != null)
            {
                waveBundleController.OnBundleStarted.RemoveListener(HandleBundleStarted);
            }

            if (towerRegistry != null)
            {
                towerRegistry.OnTowerRegistered.RemoveListener(HandleTowerRegistered);
                towerRegistry.OnTowerUnregistered.RemoveListener(HandleTowerUnregistered);
            }

            isSubscribed = false;
        }
    }
}
