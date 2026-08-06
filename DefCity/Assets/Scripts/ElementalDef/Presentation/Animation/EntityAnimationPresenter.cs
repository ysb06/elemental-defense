using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Combat.Weapons;
using DefCore.Presentation.UI;
using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Presentation.Animation
{
    public sealed class EntityAnimationPresenter : MonoBehaviour
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField] private Transform rotationPivot;
        [SerializeField] private float modelYawOffset;
        [SerializeField] private GameObject liveObject;
        [SerializeField] private GameObject deadObject;
        [SerializeField] private Animator liveAnimator;
        [SerializeField] private Attacker attacker;
        [SerializeField] private Health targetHealth;

        private void OnEnable()
        {
            attacker.OnAttackCommitted.AddListener(OnAttackCommitted);
            targetHealth.OnDeath.AddListener(OnDeath);
        }

        private void OnDisable()
        {
            attacker.OnAttackCommitted.RemoveListener(OnAttackCommitted);
            targetHealth.OnDeath.RemoveListener(OnDeath);
        }

        public void OnAttackCommitted(GameObject sender, AttackInfoArgs args)
        {
            if (liveAnimator != null)
            {
                liveAnimator.SetTrigger(AttackHash);
            }
            
            if (rotationPivot != null)
            {
                FaceTarget(args.Target);
            }
        }

        private void FaceTarget(Health target)
        {
            Collider targetCollider = target.DamageCollider;
            Vector3 targetPosition = targetCollider != null ? targetCollider.bounds.center : target.transform.position;

            Vector3 direction = targetPosition - rotationPivot.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            rotationPivot.rotation = lookRotation * Quaternion.Euler(0f, modelYawOffset, 0f);
        }


        public void OnDeath(GameObject sender, DamageEventArgs args)
        {
            liveObject.SetActive(false);
            deadObject.SetActive(true);
        }

    }
}