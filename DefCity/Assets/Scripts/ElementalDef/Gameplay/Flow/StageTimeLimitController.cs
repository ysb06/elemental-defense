using System;
using ElementalDef.Gameplay.Flow.Settings;
using UnityEngine;

namespace ElementalDef.Gameplay.Flow
{
    [DisallowMultipleComponent]
    public sealed class StageTimeLimitController : MonoBehaviour
    {
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private GameFlowController gameFlowController;

        private bool isSubscribed;
        private bool isTimeoutPending;
        private int timeoutRequestedFrame = -1;

        public float TimeLimitSeconds { get; private set; }
        public float RemainingSeconds { get; private set; }
        public int RemainingWholeSeconds { get; private set; }
        public bool HasStarted { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<int> RemainingWholeSecondsChanged;

        private void Awake()
        {
            EnsureConfigured();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Update()
        {
            if (isTimeoutPending)
            {
                if (Time.frameCount <= timeoutRequestedFrame)
                {
                    return;
                }

                isTimeoutPending = false;
                timeoutRequestedFrame = -1;
                gameFlowController.TryCompleteDefeat();
                return;
            }

            if (!IsRunning)
            {
                return;
            }

            SetRemainingSeconds(RemainingSeconds - Time.deltaTime);
            if (RemainingSeconds > 0f)
            {
                return;
            }

            IsRunning = false;
            isTimeoutPending = true;
            timeoutRequestedFrame = Time.frameCount;
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopTimer();
        }

        private void HandleBundleStarted(GameObject sender)
        {
            if (sender != waveBundleController.gameObject || HasStarted || gameFlowController.IsCompleted)
            {
                return;
            }

            WaveBundle activeBundle = waveBundleController.ActiveBundle;
            if (activeBundle == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTimeLimitController)} requires an active {nameof(WaveBundle)} when the bundle starts.");
            }

            float timeLimitSeconds = activeBundle.TimeLimitSeconds;
            if (float.IsNaN(timeLimitSeconds) ||
                float.IsInfinity(timeLimitSeconds) ||
                timeLimitSeconds <= 0f)
            {
                throw new InvalidOperationException(
                    $"[{activeBundle.name}] Time limit must be finite and greater than 0 seconds.");
            }

            TimeLimitSeconds = timeLimitSeconds;
            HasStarted = true;
            IsRunning = true;
            isTimeoutPending = false;
            timeoutRequestedFrame = -1;
            SetRemainingSeconds(timeLimitSeconds, forceNotification: true);
        }

        private void HandleBundleCompleted(GameObject sender)
        {
            if (sender != waveBundleController.gameObject)
            {
                return;
            }

            if (isTimeoutPending && Time.frameCount != timeoutRequestedFrame)
            {
                return;
            }

            StopTimer();
        }

        private void HandleGameCompleted(GameObject sender)
        {
            if (sender == gameFlowController.gameObject)
            {
                StopTimer();
            }
        }

        private void SetRemainingSeconds(float remainingSeconds, bool forceNotification = false)
        {
            RemainingSeconds = Mathf.Clamp(remainingSeconds, 0f, TimeLimitSeconds);
            int wholeSeconds = ToWholeSeconds(RemainingSeconds);
            if (!forceNotification && wholeSeconds == RemainingWholeSeconds)
            {
                return;
            }

            RemainingWholeSeconds = wholeSeconds;
            Action<int> handlers = RemainingWholeSecondsChanged;
            if (handlers == null)
            {
                return;
            }

            foreach (Action<int> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(wholeSeconds);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void StopTimer()
        {
            IsRunning = false;
            isTimeoutPending = false;
            timeoutRequestedFrame = -1;
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            waveBundleController.OnBundleStarted.AddListener(HandleBundleStarted);
            waveBundleController.OnBundleCompleted.AddListener(HandleBundleCompleted);
            gameFlowController.OnVictory.AddListener(HandleGameCompleted);
            gameFlowController.OnDefeat.AddListener(HandleGameCompleted);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            waveBundleController?.OnBundleStarted.RemoveListener(HandleBundleStarted);
            waveBundleController?.OnBundleCompleted.RemoveListener(HandleBundleCompleted);
            gameFlowController?.OnVictory.RemoveListener(HandleGameCompleted);
            gameFlowController?.OnDefeat.RemoveListener(HandleGameCompleted);
            isSubscribed = false;
        }

        private void EnsureConfigured()
        {
            if (waveBundleController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTimeLimitController)} requires a {nameof(WaveBundleController)} reference.");
            }

            if (gameFlowController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(StageTimeLimitController)} requires a {nameof(GameFlowController)} reference.");
            }
        }

        private static int ToWholeSeconds(float seconds)
        {
            if (seconds >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Mathf.CeilToInt(seconds);
        }
    }
}
