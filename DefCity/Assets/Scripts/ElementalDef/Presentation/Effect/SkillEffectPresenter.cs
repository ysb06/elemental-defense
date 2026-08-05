using System;
using ElementalDef.Gameplay.Combat.Skills;
using UnityEngine;

namespace ElementalDef.Presentation.Effect
{
    [DisallowMultipleComponent]
    public sealed class SkillEffectPresenter : MonoBehaviour
    {
        [SerializeField] private TowerSkillController skillController;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform effectAnchor;
        [Tooltip("Zero lets the spawned effect prefab manage its own lifetime.")]
        [SerializeField, Min(0f)] private float effectLifetimeSeconds;

        private bool isSubscribed;

        private void Awake()
        {
            skillController = skillController != null ? skillController : GetComponent<TowerSkillController>();

            if (skillController == null)
            {
                throw new InvalidOperationException($"{nameof(SkillEffectPresenter)} requires a {nameof(TowerSkillController)} reference.");
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void HandleSkillUseStarted(
            TowerSkillController sender,
            SkillExecutionContext context)
        {
            if (sender != skillController)
            {
                return;
            }

            SkillDefinition definition = context.Definition;
            if (animator != null && !string.IsNullOrWhiteSpace(definition.AnimatorTrigger))
            {
                animator.SetTrigger(definition.AnimatorTrigger);
            }

            if (definition.CastVfxPrefab == null)
            {
                return;
            }

            Vector3 position = effectAnchor != null ? effectAnchor.position : context.CastPosition;
            Quaternion rotation = effectAnchor != null ? effectAnchor.rotation : sender.transform.rotation;
            GameObject effectInstance = Instantiate(definition.CastVfxPrefab, position, rotation);
            if (effectLifetimeSeconds > 0f)
            {
                Destroy(effectInstance, effectLifetimeSeconds);
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            skillController.OnSkillUseStarted += HandleSkillUseStarted;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            skillController.OnSkillUseStarted -= HandleSkillUseStarted;
            isSubscribed = false;
        }
    }
}
