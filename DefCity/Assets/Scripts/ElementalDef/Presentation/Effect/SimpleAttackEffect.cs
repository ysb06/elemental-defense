using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using UnityEngine;

namespace ElementalDef.Presentation.Effect
{
    public class SimpleAttackEffect : MonoBehaviour
    {
        [SerializeField] private Attacker attacker;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactAudioClip;
        [SerializeField] private float impactEffectLifetime = 6f;
        [SerializeField] private float effectScale = 0.1f;
        [SerializeField] private float yOffset = 0.5f;
        [SerializeField] private float impactAudioVolume = 1f;

        private void OnEnable()
        {
            if (attacker == null)
            {
                attacker = GetComponentInParent<Attacker>();
            }

            if (attacker != null)
            {
                attacker.OnAttackResolved.AddListener(OnAttackResolved);
            }
        }

        private void OnDisable()
        {
            if (attacker != null)
            {
                attacker.OnAttackResolved.RemoveListener(OnAttackResolved);
            }
        }

        public void OnAttackResolved(GameObject sender, AttackResolvedEventArgs args)
        {
            if (impactEffectPrefab != null)
            {
                Vector3 effectPosition = GetEffectPosition(args);
                GameObject effectInstance = Instantiate(impactEffectPrefab, effectPosition, Quaternion.identity);
                effectInstance.transform.localScale *= effectScale;
                if (impactEffectLifetime > 0f)
                {
                    Destroy(effectInstance, impactEffectLifetime);
                }
            }

            if (impactAudioClip != null)
            {
                AudioSource.PlayClipAtPoint(impactAudioClip, args.ImpactPoint, impactAudioVolume);
            }
        }

        private Vector3 GetEffectPosition(AttackResolvedEventArgs args)
        {
            if (args.Hits == null || args.Hits.Count == 0)
            {
                return args.ImpactPoint + Vector3.up * yOffset;
            }

            Health target = args.Hits[0].Target;
            Collider targetCollider = target != null ? target.DamageCollider : null;

            if (targetCollider == null)
            {
                return args.Hits[0].ImpactPoint + Vector3.up * yOffset;
            }

            Bounds bounds = targetCollider.bounds;

            return new Vector3(bounds.center.x, Mathf.Lerp(bounds.min.y, bounds.max.y, 0.65f) + yOffset, bounds.center.z);
        }
    }
}