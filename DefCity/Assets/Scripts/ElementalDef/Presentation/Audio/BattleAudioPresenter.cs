using DefCore.Presentation.Audio;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Placement;
using ElementalDef.Runtime;
using UnityEngine;

namespace ElementalDef.Presentation.Audio
{
    [DisallowMultipleComponent]
    public sealed class BattleAudioPresenter : MonoBehaviour
    {
        private const string HeadquartersCollapseAudioKey = "headquarters-collapse";

        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private GameFlowController gameFlowController;
        [SerializeField] private TowerInteractionController towerInteractionController;
        [SerializeField] private HeadquartersBuilding headquartersBuilding;
        [SerializeField] private AudioClip headquartersCollapseClip;

        private ElementalDefAudioService audioService;
        private bool isSubscribed;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!enabled || isSubscribed)
            {
                return;
            }

            audioService = ElementalDefApplicationRoot.Instance?.Audio;
            if (audioService == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(BattleAudioPresenter)} requires an active " +
                    $"{nameof(ElementalDefAudioService)}.",
                    this);
                enabled = false;
                return;
            }

            waveBundleController.OnBundleStarted.AddListener(HandleBundleStarted);
            gameFlowController.OnVictory.AddListener(HandleGameCompleted);
            gameFlowController.OnDefeat.AddListener(HandleGameCompleted);
            towerInteractionController.OnTowerPlacementCompleted +=
                HandleTowerPlacementCompleted;
            headquartersBuilding.OnDestroyed.AddListener(
                HandleHeadquartersDestroyed);
            isSubscribed = true;
        }

        private void OnDisable()
        {
            Unsubscribe();
            audioService?.StopBattleMusic();
            audioService = null;
        }

        private void HandleBundleStarted(GameObject sender)
        {
            if (sender != waveBundleController.gameObject)
            {
                return;
            }

            if (!audioService.StartBattleMusic())
            {
                Debug.LogError(
                    $"[{name}] Battle music could not be started.",
                    this);
            }
        }

        private void HandleGameCompleted(GameObject sender)
        {
            if (sender == gameFlowController.gameObject)
            {
                audioService.StopBattleMusic();
            }
        }

        private void HandleTowerPlacementCompleted(TowerUnit tower)
        {
            if (tower == null)
            {
                return;
            }

            if (tower.TryGetComponent(out EntityAudioPresenter presenter))
            {
                presenter.PlaySpawn();
                return;
            }

            Debug.LogError(
                $"[{name}] Newly placed tower '{tower.name}' has no " +
                $"{nameof(EntityAudioPresenter)}; its spawn audio cannot be played.",
                tower);
        }

        private void HandleHeadquartersDestroyed(GameObject sender)
        {
            if (sender != headquartersBuilding.gameObject)
            {
                return;
            }

            if (headquartersCollapseClip == null)
            {
                return;
            }

            if (!audioService.PlayExclusive2D(
                    HeadquartersCollapseAudioKey,
                    headquartersCollapseClip))
            {
                Debug.LogError(
                    $"[{name}] Headquarters-collapse audio could not be played.",
                    this);
            }
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            waveBundleController?.OnBundleStarted.RemoveListener(
                HandleBundleStarted);
            gameFlowController?.OnVictory.RemoveListener(HandleGameCompleted);
            gameFlowController?.OnDefeat.RemoveListener(HandleGameCompleted);
            towerInteractionController.OnTowerPlacementCompleted -=
                HandleTowerPlacementCompleted;
            headquartersBuilding?.OnDestroyed.RemoveListener(
                HandleHeadquartersDestroyed);
            isSubscribed = false;
        }

        private bool ValidateConfiguration()
        {
            if (waveBundleController == null || gameFlowController == null ||
                towerInteractionController == null || headquartersBuilding == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(BattleAudioPresenter)} requires wave, game-flow, " +
                    "tower-interaction, and headquarters references.",
                    this);
                return false;
            }

            if (headquartersCollapseClip == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(BattleAudioPresenter)} requires a headquarters-collapse clip.",
                    this);
            }

            return true;
        }
    }
}
