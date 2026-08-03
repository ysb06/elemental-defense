using System;
using DefCore.Gameplay.Flow;
using UnityEngine;
using UnityEngine.Events;
using ElementalDef.Gameplay.Flow.Settings;

namespace ElementalDef.Gameplay.Flow
{
    [DisallowMultipleComponent]
    public class WaveBundleController : MonoBehaviour
    {
        private enum WaveBundleRuntimeState
        {
            Idle,
            Running,
            WaitingForNextWave,
            Completed,
            Stopped,
        }

        [SerializeField] private WaveBundle waveBundle;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EnemySpawner enemySpawner;

        private WaveBundleRuntimeState bundleState;
        private int currentWaveIndex = -1;
        private int transitionRequestedFrame = -1;
        private bool isConfigured;
        private bool isSubscribedToEnemySpawner;

        public int TotalWaveCount => waveBundle.Waves.Count;
        public int CurrentWaveIndex => currentWaveIndex;

        public WaveBundleControllerEvent OnBundleStarted = new();
        public WaveBundleControllerEvent OnBundleCompleted = new();

        private void Awake()
        {
            EnsureConfigured();
            isConfigured = true;
        }

        private void Start()
        {
            if (!isConfigured || bundleState != WaveBundleRuntimeState.Idle)
            {
                return;
            }

            SubscribeToEnemySpawner();
            StartWave(0);
            OnBundleStarted?.Invoke(gameObject);
        }

        private void Update()
        {
            if (bundleState != WaveBundleRuntimeState.WaitingForNextWave ||
                Time.frameCount <= transitionRequestedFrame)
            {
                return;
            }

            StartWave(currentWaveIndex + 1);
        }

        private void OnDisable()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            if (bundleState == WaveBundleRuntimeState.Stopped)
            {
                return;
            }

            bundleState = WaveBundleRuntimeState.Stopped;
            transitionRequestedFrame = -1;

            UnsubscribeFromEnemySpawner();

            if (turnManager != null)
            {
                turnManager.IsRunning = false;
            }

            if (enemySpawner != null)
            {
                enemySpawner.Shutdown();
            }
        }

        private void HandleWaveCompleted(GameObject sender)
        {
            if (sender != enemySpawner.gameObject || bundleState != WaveBundleRuntimeState.Running)
            {
                return;
            }

            turnManager.IsRunning = false;

            if (currentWaveIndex + 1 >= waveBundle.Waves.Count)
            {
                bundleState = WaveBundleRuntimeState.Completed;
                transitionRequestedFrame = -1;
                UnsubscribeFromEnemySpawner();
                OnBundleCompleted?.Invoke(gameObject);
                return;
            }

            bundleState = WaveBundleRuntimeState.WaitingForNextWave;
            transitionRequestedFrame = Time.frameCount;
        }

        private void StartWave(int waveIndex)
        {
            turnManager.IsRunning = false;

            try
            {
                turnManager.ResetTurn(0);
                enemySpawner.PrepareWave(waveBundle.Waves[waveIndex]);
            }
            catch
            {
                try
                {
                    Shutdown();
                }
                catch (Exception shutdownException)
                {
                    Debug.LogException(shutdownException, this);
                }

                throw;
            }

            currentWaveIndex = waveIndex;
            transitionRequestedFrame = -1;
            bundleState = WaveBundleRuntimeState.Running;
            turnManager.IsRunning = true;
        }

        private void SubscribeToEnemySpawner()
        {
            if (isSubscribedToEnemySpawner)
            {
                return;
            }

            enemySpawner.OnWaveCompleted.AddListener(HandleWaveCompleted);
            isSubscribedToEnemySpawner = true;
        }

        private void UnsubscribeFromEnemySpawner()
        {
            if (!isSubscribedToEnemySpawner)
            {
                return;
            }

            if (enemySpawner != null)
            {
                enemySpawner.OnWaveCompleted.RemoveListener(HandleWaveCompleted);
            }

            isSubscribedToEnemySpawner = false;
        }

        private void EnsureConfigured()
        {
            if (waveBundle == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveBundleController)} requires a {nameof(WaveBundle)} reference.");
            }

            if (turnManager == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveBundleController)} requires a {nameof(TurnManager)} reference.");
            }

            if (enemySpawner == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveBundleController)} requires an {nameof(EnemySpawner)} reference.");
            }

            if (waveBundle.Waves.Count == 0)
            {
                throw new InvalidOperationException("A wave bundle must contain at least one wave schedule.");
            }

            for (int i = 0; i < waveBundle.Waves.Count; i++)
            {
                WaveSchedule schedule = waveBundle.Waves[i];
                if (schedule == null)
                {
                    throw new InvalidOperationException($"Wave bundle has a missing schedule at index {i}.");
                }

                if (schedule.Entries.Count == 0)
                {
                    throw new InvalidOperationException($"Wave schedule at index {i} must contain at least one entry.");
                }
            }
        }
    }

    [Serializable]
    public class WaveBundleControllerEvent : UnityEvent<GameObject> { }
}
