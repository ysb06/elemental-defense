using System;
using DefCore.Gameplay.Interaction;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Placement;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalDef.Gameplay.Flow
{
    [DisallowMultipleComponent]
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private HeadquartersBuilding headquartersBuilding;
        [SerializeField] private TowerInteractionController towerInteractionController;
        [SerializeField] private TowerRegistry towerRegistry;
        [SerializeField] private EntitySelectionManager entitySelectionManager;

        public GameResult Result { get; private set; } = GameResult.InProgress;
        public bool IsCompleted => Result != GameResult.InProgress;

        public GameFlowControllerEvent OnVictory = new();
        public GameFlowControllerEvent OnDefeat = new();

        private bool isSubscribed;
        private bool isVictoryPending;
        private bool isGameplayStopped;
        private int victoryRequestedFrame = -1;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            if (!IsCompleted)
            {
                if (isSubscribed)
                {
                    return;
                }

                waveBundleController.OnBundleCompleted.AddListener(HandleBundleCompleted);
                headquartersBuilding.OnDestroyed.AddListener(HandleHeadquartersDestroyed);
                isSubscribed = true;
            }
        }

        private void Update()
        {
            if (!isVictoryPending ||
                IsCompleted ||
                Time.frameCount <= victoryRequestedFrame)
            {
                return;
            }

            CompleteGame(GameResult.Victory);
        }

        private void OnDisable()
        {
            if (!isSubscribed) return;

            waveBundleController?.OnBundleCompleted.RemoveListener(HandleBundleCompleted);
            headquartersBuilding?.OnDestroyed.RemoveListener(HandleHeadquartersDestroyed);

            isSubscribed = false;
        }

        private void HandleBundleCompleted(GameObject sender)
        {
            if (sender != waveBundleController.gameObject || IsCompleted || isVictoryPending)
            {
                return;
            }

            // Victory is confirmed on the next frame so Headquarters destruction
            // in the same frame can deterministically take priority.
            isVictoryPending = true;
            victoryRequestedFrame = Time.frameCount;
            StopGameplay();
        }

        private void HandleHeadquartersDestroyed(GameObject sender)
        {
            if (sender != headquartersBuilding.gameObject)
            {
                return;
            }

            TryCompleteDefeat();
        }

        public bool TryCompleteDefeat()
        {
            if (IsCompleted)
            {
                return false;
            }

            CompleteGame(GameResult.Defeat);
            return true;
        }

        private void CompleteGame(GameResult result)
        {
            if (result == GameResult.InProgress)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(result),
                    result,
                    "A completed game requires a terminal result.");
            }

            if (IsCompleted)
            {
                return;
            }

            Result = result;
            isVictoryPending = false;
            victoryRequestedFrame = -1;

            if (isSubscribed)
            {
                waveBundleController?.OnBundleCompleted.RemoveListener(HandleBundleCompleted);
                headquartersBuilding?.OnDestroyed.RemoveListener(HandleHeadquartersDestroyed);

                isSubscribed = false;
            }

            try
            {
                StopGameplay();
            }
            finally
            {
                if (result == GameResult.Victory)
                {
                    OnVictory?.Invoke(gameObject);
                }
                else
                {
                    OnDefeat?.Invoke(gameObject);
                }
            }
        }

        private void StopGameplay()
        {
            if (isGameplayStopped)
            {
                return;
            }

            isGameplayStopped = true;
            try
            {
                waveBundleController.Shutdown();
            }
            finally
            {
                try
                {
                    towerInteractionController.Shutdown();
                }
                finally
                {
                    try
                    {
                        towerRegistry.Shutdown();
                    }
                    finally
                    {
                        entitySelectionManager.Shutdown();
                    }
                }
            }
        }

        private void EnsureConfigured()
        {
            if (waveBundleController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameFlowController)} requires a {nameof(WaveBundleController)} reference.");
            }

            if (headquartersBuilding == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameFlowController)} requires a {nameof(HeadquartersBuilding)} reference.");
            }

            if (towerRegistry == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameFlowController)} requires a {nameof(TowerRegistry)} reference.");
            }

            if (towerInteractionController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameFlowController)} requires a {nameof(TowerInteractionController)} reference.");
            }

            if (entitySelectionManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameFlowController)} requires an {nameof(EntitySelectionManager)} reference.");
            }
        }
    }

    public enum GameResult
    {
        InProgress = 0,
        Victory = 1,
        Defeat = 2,
    }

    [Serializable]
    public class GameFlowControllerEvent : UnityEvent<GameObject> { }
}
