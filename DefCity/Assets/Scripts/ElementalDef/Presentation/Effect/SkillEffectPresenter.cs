using System.Collections;
using UnityEngine;

namespace ElementalDef.Presentation.Effect
{
    [DisallowMultipleComponent]
    public sealed class SkillEffectPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject effectRoot;
        [SerializeField, Min(0.01f)] private float activeDurationSeconds = 1f;

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
