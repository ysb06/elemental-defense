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

        [SerializeField] private WaveBundle activeBundle;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EnemySpawner enemySpawner;

        private WaveBundleRuntimeState bundleState;
        private int currentWaveIndex = -1;
        private int transitionRequestedFrame = -1;

        public WaveBundle ActiveBundle => activeBundle;
        public int TotalWaveCount => activeBundle.Waves.Count;
        public int CurrentWaveIndex => currentWaveIndex;
        public bool IsBattleActive => bundleState == WaveBundleRuntimeState.Running || bundleState == WaveBundleRuntimeState.WaitingForNextWave;

        public WaveBundleControllerEvent OnBundleStarted = new();
        public WaveBundleControllerEvent OnBundleCompleted = new();

        public void Initialize()
        {
            if (bundleState != WaveBundleRuntimeState.Idle)
            {
                throw new InvalidOperationException($"{nameof(WaveBundleController)} cannot be initialized when it is not in the {nameof(WaveBundleRuntimeState.Idle)} state.");
            }

            enemySpawner.OnWaveCompleted.AddListener(HandleWaveCompleted);
            StartWave(0);
            OnBundleStarted?.Invoke(gameObject);
        }

        public void Initialize(WaveBundle bundle)
        {
            activeBundle = bundle;
            Initialize();
        }

        private void Update()
        {
            if (bundleState != WaveBundleRuntimeState.WaitingForNextWave || Time.frameCount <= transitionRequestedFrame)
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

            enemySpawner.OnWaveCompleted.RemoveListener(HandleWaveCompleted);

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

            if (currentWaveIndex + 1 >= activeBundle.Waves.Count)
            {
                bundleState = WaveBundleRuntimeState.Completed;
                transitionRequestedFrame = -1;
                enemySpawner.OnWaveCompleted.RemoveListener(HandleWaveCompleted);
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
                enemySpawner.PrepareWave(activeBundle.Waves[waveIndex]);
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


    }

    [Serializable]
    public class WaveBundleControllerEvent : UnityEvent<GameObject> { }
}
