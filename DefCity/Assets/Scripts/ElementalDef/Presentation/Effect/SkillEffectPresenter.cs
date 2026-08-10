using System.Collections;
using ElementalDef.Presentation.Audio;
using ElementalDef.Runtime;
using UnityEngine;

namespace ElementalDef.Presentation.Effect
{
    [DisallowMultipleComponent]
    public sealed class SkillEffectPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject effectRoot;
        [SerializeField, Min(0.01f)] private float activeDurationSeconds = 2f;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private string audioKey;

        private Coroutine deactivateCoroutine;

        public bool IsPlaying => effectRoot != null && effectRoot.activeSelf;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            effectRoot.SetActive(false);
        }

        private void OnDisable()
        {
            StopPlayback();
        }

        public void Play()
        {
            if (!isActiveAndEnabled || effectRoot == null)
            {
                return;
            }

            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }

            effectRoot.SetActive(false);
            effectRoot.SetActive(true);
            PlayAudio();
            deactivateCoroutine = StartCoroutine(DeactivateAfterDuration());
        }

        private IEnumerator DeactivateAfterDuration()
        {
            yield return new WaitForSeconds(activeDurationSeconds);

            effectRoot.SetActive(false);
            deactivateCoroutine = null;
        }

        private void StopPlayback()
        {
            if (deactivateCoroutine != null)
            {
                StopCoroutine(deactivateCoroutine);
                deactivateCoroutine = null;
            }

            if (effectRoot != null && effectRoot != gameObject)
            {
                effectRoot.SetActive(false);
            }

            StopAudio();
        }

        private void PlayAudio()
        {
            if (audioClip == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(SkillEffectPresenter)} has no ultimate audio clip.",
                    this);
                return;
            }

            if (string.IsNullOrWhiteSpace(audioKey))
            {
                Debug.LogError(
                    $"[{name}] {nameof(SkillEffectPresenter)} requires an audio key.",
                    this);
                return;
            }

            ElementalDefAudioService audioService =
                ElementalDefApplicationRoot.Instance?.Audio;
            if (audioService == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(ElementalDefAudioService)} is unavailable; " +
                    "the skill effect will continue without audio.",
                    this);
                return;
            }

            if (!audioService.PlayExclusive2D(audioKey, audioClip))
            {
                Debug.LogError(
                    $"[{name}] Ultimate audio '{audioKey}' could not be played; " +
                    "the skill effect will continue without audio.",
                    this);
            }
        }

        private void StopAudio()
        {
            if (string.IsNullOrWhiteSpace(audioKey))
            {
                return;
            }

            ElementalDefApplicationRoot.Instance?.Audio?.StopExclusive2D(audioKey);
        }

        private bool ValidateConfiguration()
        {
            if (effectRoot == null)
            {
                Debug.LogError(
                    $"[{name}] {nameof(SkillEffectPresenter)} requires an effect root.",
                    this);
                return false;
            }

            if (effectRoot == gameObject)
            {
                Debug.LogError(
                    $"[{name}] {nameof(SkillEffectPresenter)} requires an effect root other than its own GameObject.",
                    this);
                return false;
            }

            if (float.IsNaN(activeDurationSeconds) ||
                float.IsInfinity(activeDurationSeconds) ||
                activeDurationSeconds <= 0f)
            {
                Debug.LogError(
                    $"[{name}] {nameof(SkillEffectPresenter)} requires a finite, positive active duration.",
                    this);
                return false;
            }

            return true;
        }
    }
}
