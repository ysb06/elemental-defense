using DefCore.Gameplay.Combat;
using UnityEngine;

namespace DefCore.Presentation.Audio
{
    [DisallowMultipleComponent]
    public sealed class EntityAudioPresenter : MonoBehaviour
    {
        [SerializeField] private Attacker attacker;
        [SerializeField] private Health health;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip attackClip;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private AudioClip spawnClip;

        private bool isSubscribed;
        private bool warnedMissingAttackClip;
        private bool warnedMissingDeathClip;
        private bool warnedMissingSpawnClip;

        private void Awake()
        {
            ResolveComponentReferences();

            if (audioSource == null)
            {
                Debug.LogError($"[{name}] Entity audio requires an AudioSource.", this);
                enabled = false;
                return;
            }

            audioSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            ResolveComponentReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void PlaySpawn()
        {
            PlayReplacing(spawnClip, "spawn", ref warnedMissingSpawnClip);
        }

        private void HandleAttackCommitted(GameObject sender, AttackInfoArgs args)
        {
            PlayReplacing(attackClip, "attack", ref warnedMissingAttackClip);
        }

        private void HandleDeath(GameObject sender, DamageEventArgs args)
        {
            PlayReplacing(deathClip, "death", ref warnedMissingDeathClip);
        }

        private void PlayReplacing(
            AudioClip clip,
            string audioRole,
            ref bool hasWarnedMissingClip)
        {
            if (clip == null)
            {
                if (!hasWarnedMissingClip)
                {
                    hasWarnedMissingClip = true;
                    Debug.LogError(
                        $"[{name}] Entity {audioRole} audio is not assigned.",
                        this);
                }

                return;
            }

            if (audioSource == null || !audioSource.isActiveAndEnabled)
            {
                return;
            }

            try
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(new System.InvalidOperationException(
                    $"[{name}] Entity audio could not be played.",
                    exception), this);
            }
        }

        private void ResolveComponentReferences()
        {
            if (attacker == null)
            {
                attacker = GetComponent<Attacker>();
            }

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            if (attacker != null)
            {
                attacker.OnAttackCommitted.AddListener(HandleAttackCommitted);
            }

            if (health != null)
            {
                health.OnDeath.AddListener(HandleDeath);
            }

            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (attacker != null)
            {
                attacker.OnAttackCommitted.RemoveListener(HandleAttackCommitted);
            }

            if (health != null)
            {
                health.OnDeath.RemoveListener(HandleDeath);
            }

            isSubscribed = false;
        }
    }
}
