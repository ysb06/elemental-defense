using System;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    public abstract class SkillExecutorBase : MonoBehaviour
    {
        private SkillExecutorInitializationContext initializationContext;

        public bool IsInitialized => initializationContext != null;
        protected SkillExecutorInitializationContext InitializationContext => initializationContext ?? throw new InvalidOperationException($"[{name}] {GetType().Name} has not been initialized.");

        public void Initialize(SkillExecutorInitializationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (initializationContext != null)
            {
                if (initializationContext.Owner == context.Owner &&
                    initializationContext.Attacker == context.Attacker &&
                    initializationContext.DamageCalculator == context.DamageCalculator)
                {
                    return;
                }

                throw new InvalidOperationException(
                    $"[{name}] {GetType().Name} is already initialized with different services.");
            }

            initializationContext = context;
            try
            {
                OnInitialized(context);
            }
            catch
            {
                initializationContext = null;
                throw;
            }
        }

        public abstract bool CanExecute(SkillExecutionContext context);
        public abstract void BeginExecute(SkillExecutionContext context, Action<SkillExecutionResult> onResolved);
        public virtual void Cancel(SkillExecutionContext context) { }
        protected virtual void OnInitialized(SkillExecutorInitializationContext context){ }
        
        protected void EnsureOwnedContext(SkillExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Owner != InitializationContext.Owner)
            {
                throw new ArgumentException(
                    "The execution context belongs to a different tower.",
                    nameof(context));
            }
        }
    }
}
