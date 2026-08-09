using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElementalDef.Presentation.Effect
{
    public class SimpleAttackEffect : MonoBehaviour
    {
        private const float TargetHeightRatio = 0.65f;

        [SerializeField] private Attacker attacker;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactAudioClip;
        [SerializeField] private float impactEffectLifetime = 6f;
        [SerializeField] private float effectScale = 0.1f;
        [FormerlySerializedAs("yOffset")]
        [SerializeField] private float fallbackYOffset = 0.5f;
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
                return ApplyFallbackOffset(args.ImpactPoint);
            }

            AttackHitEntry hit = args.Hits[0];
            Health target = hit.Target;
            Collider targetCollider = target != null ? target.DamageCollider : null;

            if (targetCollider == null ||
                !targetCollider.enabled ||
                !targetCollider.gameObject.activeInHierarchy)
            {
                return ApplyFallbackOffset(hit.ImpactPoint);
            }

            Bounds bounds = targetCollider.bounds;
            if (bounds.size.sqrMagnitude <= Mathf.Epsilon)
            {
                return ApplyFallbackOffset(hit.ImpactPoint);
            }

            return new Vector3(
                bounds.center.x,
                Mathf.Lerp(bounds.min.y, bounds.max.y, TargetHeightRatio),
                bounds.center.z);
        }

        private Vector3 ApplyFallbackOffset(Vector3 impactPoint)
        {
            return impactPoint + Vector3.up * fallbackYOffset;
        }
    }
}
