using System;
using ElementalDef.Data;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Runtime;
using UnityEngine;

namespace ElementalDef.Presentation.Audio
{
    [DisallowMultipleComponent]
    public sealed class ResultAudioPresenter : MonoBehaviour
    {
        private const string ResultAudioKey = "result";

        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip defeatClip;

        private ElementalDefAudioService audioService;
        private bool hasPlayed;

        private void Awake()
        {
            if (victoryClip == null || defeatClip == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(ResultAudioPresenter)} requires victory and defeat clips.",
                    this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!enabled || hasPlayed)
            {
                return;
            }

            TryPlayResultAudio();
        }

        private void OnDisable()
        {
            audioService?.StopExclusive2D(ResultAudioKey);
            audioService = null;
        }

        private void TryPlayResultAudio()
        {
            ElementalDefApplicationRoot applicationRoot =
                ElementalDefApplicationRoot.Instance;
            StageRunContext context = applicationRoot?.StageLaunch?.Current;
            audioService = applicationRoot?.Audio;
            if (applicationRoot == null || context == null ||
                applicationRoot.RunStore == null || audioService == null)
            {
                Debug.LogError(
                    $"[{name}] Current stage, run-store, and audio services are required " +
                    "to play result audio.",
                    this);
                return;
            }

            CompletedStageRunRecord completedRun;
            try
            {
                if (!applicationRoot.RunStore.TryGetRun(
                        context.RunId,
                        out completedRun))
                {
                    Debug.LogError(
                        $"[{name}] No completed run was found for RunId " +
                        $"'{context.RunId}'.",
                        this);
                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(new InvalidOperationException(
                    "The completed ElementalDef stage run could not be loaded for result audio.",
                    exception), this);
                return;
            }

            if (!string.Equals(
                    completedRun.StageId,
                    context.StageId,
                    StringComparison.Ordinal) ||
                completedRun.StageDisplayOrder != context.DisplayOrder)
            {
                Debug.LogError(
                    $"[{name}] The completed run does not match the current stage context.",
                    this);
                return;
            }

            AudioClip clip = completedRun.Outcome switch
            {
                StageRunOutcome.Victory => victoryClip,
                StageRunOutcome.Defeat => defeatClip,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(completedRun.Outcome),
                    completedRun.Outcome,
                    null),
            };

            hasPlayed = audioService.PlayExclusive2D(ResultAudioKey, clip);
            if (!hasPlayed)
            {
                Debug.LogError(
                    $"[{name}] Result audio could not be played.",
                    this);
            }
        }
    }
}
