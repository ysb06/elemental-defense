using System;
using DefCore.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Weapons;
using ElementalDef.Gameplay.Entities;
using UnityEngine;

namespace ElementalDef.Gameplay.Combat.Skills
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerUnit))]
    public sealed class TowerSkillController : MonoBehaviour
    {
        [SerializeField] private SkillExecutorBase executor;

        private TowerUnit owner;
        private SkillExecutorInitializationContext initializationContext;
        private SkillExecutionContext activeExecution;
        private float chargeSeconds;
        private bool isInitialized;
        private bool isBattleActive;
        private bool isBeginningExecution;
        private bool isExecuting;
        private bool isShutdown;

        public SkillDefinition Definition { get; private set; }
        public SkillExecutorBase Executor => ResolveExecutor();
        public float NormalizedCharge => Definition == null ? 0f : Mathf.Clamp01(chargeSeconds / Definition.FullChargeDurationSeconds);
        public bool IsReady => isInitialized && NormalizedCharge >= 1f;
        public bool IsExecuting => isExecuting;
        public bool CanUse => isInitialized && !isShutdown && isBattleActive && !isExecuting && IsReady;

        public event Action<TowerSkillController> OnSkillReady;
        public event Action<TowerSkillController, SkillExecutionContext> OnSkillUseStarted;
        public event Action<TowerSkillController, SkillExecutionResult> OnSkillResolved;

        private void Awake()
        {
            ResolveExecutor();
        }

        private void Update()
        {
            if (!isInitialized || isShutdown || !isBattleActive || IsReady)
            {
                return;
            }

            float previousCharge = chargeSeconds;
            chargeSeconds = Mathf.Min(Definition.FullChargeDurationSeconds, chargeSeconds + Time.deltaTime);

            if (previousCharge < Definition.FullChargeDurationSeconds && IsReady)
            {
                PublishReadySafely();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public void Initialize(TowerUnit tower, SkillDefinition definition, ElementalDamageCalculator damageCalculator)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException($"[{name}] {nameof(TowerSkillController)} is already initialized.");
            }

            if (isShutdown)
            {
                throw new InvalidOperationException($"[{name}] A shut down {nameof(TowerSkillController)} cannot be initialized.");
            }

            if (tower == null)
            {
                throw new ArgumentNullException(nameof(tower));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (damageCalculator == null)
            {
                throw new ArgumentNullException(nameof(damageCalculator));
            }

            SkillExecutorBase resolvedExecutor = ResolveExecutor();
            if (resolvedExecutor == null)
            {
                throw new InvalidOperationException($"[{name}] {nameof(TowerSkillController)} requires a {nameof(SkillExecutorBase)}.");
            }

            if (!tower.TryGetComponent(out Attacker attacker))
            {
                throw new InvalidOperationException($"[{tower.name}] A tower skill requires an {nameof(Attacker)} component.");
            }

            definition.ValidateOrThrow();

            SkillExecutorInitializationContext context = new(tower, attacker, damageCalculator);
            resolvedExecutor.Initialize(context);

            owner = tower;
            Definition = definition;
            initializationContext = context;
            chargeSeconds = 0f;
            isBattleActive = false;
            isExecuting = false;
            isInitialized = true;
        }

        public void SetBattleActive(bool active)
        {
            if (isShutdown)
            {
                return;
            }

            isBattleActive = active;
        }

        public SkillUseRequestResult RequestUse()
        {
            if (isShutdown)
            {
                return SkillUseRequestResult.Shutdown;
            }

            if (!isInitialized)
            {
                return SkillUseRequestResult.NotInitialized;
            }

            if (!isBattleActive)
            {
                return SkillUseRequestResult.BattleInactive;
            }

            if (isExecuting)
            {
                return SkillUseRequestResult.AlreadyExecuting;
            }

            if (!IsReady)
            {
                return SkillUseRequestResult.NotReady;
            }

            SkillExecutionContext executionContext;
            try
            {
                executionContext = CreateExecutionContext();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return SkillUseRequestResult.Faulted;
            }

            bool canExecute;
            try
            {
                canExecute = executor.CanExecute(executionContext);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return SkillUseRequestResult.ExecutorRejected;
            }

            if (!canExecute)
            {
                return SkillUseRequestResult.ExecutorRejected;
            }

            AcceptExecution(executionContext);

            // A listener can synchronously shut the tower down while the start
            // event is being published. In that case the accepted execution has
            // already been cancelled and must not be started afterwards.
            if (!IsActiveExecution(executionContext))
            {
                return SkillUseRequestResult.Accepted;
            }

            SkillUseRequestResult requestResult = SkillUseRequestResult.Accepted;
            isBeginningExecution = true;
            try
            {
                executor.BeginExecute(executionContext, result => HandleExecutionResolved(executionContext, result));
            }
            catch (Exception exception)
            {
                if (IsActiveExecution(executionContext))
                {
                    ResolveFault(executionContext, exception);
                    requestResult = SkillUseRequestResult.Faulted;
                }
                else
                {
                    // The executor resolved synchronously before throwing. The accepted
                    // resolution remains authoritative and must not be published twice.
                    Debug.LogException(exception, this);
                }
            }
            finally
            {
                isBeginningExecution = false;

                // A synchronous effect can finish the final enemy and trigger the
                // victory shutdown before BeginExecute returns. Give its completion
                // callback a chance to publish the actual result first; only an
                // unresolved execution is cancelled after control returns here.
                if (isShutdown && IsActiveExecution(executionContext))
                {
                    CancelActiveExecution(executionContext);
                }
            }

            return requestResult;
        }

        public void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }

            isShutdown = true;
            isBattleActive = false;

            SkillExecutionContext executionToCancel = activeExecution;
            if (executionToCancel == null || executor == null)
            {
                activeExecution = null;
                isExecuting = false;
                return;
            }

            if (isBeginningExecution)
            {
                return;
            }

            CancelActiveExecution(executionToCancel);
        }

        private void CancelActiveExecution(SkillExecutionContext executionToCancel)
        {
            if (!IsActiveExecution(executionToCancel))
            {
                return;
            }

            try
            {
                executor.Cancel(executionToCancel);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }

            if (IsActiveExecution(executionToCancel))
            {
                HandleExecutionResolved(executionToCancel, new SkillExecutionResult(executionToCancel, SkillExecutionStatus.Cancelled, 0, 0f, 0));
            }
        }

        private void AcceptExecution(SkillExecutionContext context)
        {
            chargeSeconds = 0f;
            activeExecution = context;
            isExecuting = true;
            PublishUseStartedSafely(context);
        }

        private void HandleExecutionResolved(SkillExecutionContext expectedContext, SkillExecutionResult result)
        {
            if (!IsActiveExecution(expectedContext))
            {
                return;
            }

            if (result == null)
            {
                ResolveFault(
                    expectedContext,
                    new InvalidOperationException(
                        $"[{name}] A skill executor returned a null result."));
                return;
            }

            if (!ReferenceEquals(result.Context, expectedContext))
            {
                ResolveFault(
                    expectedContext,
                    new InvalidOperationException(
                        $"[{name}] A skill executor resolved a different execution context."));
                return;
            }

            activeExecution = null;
            isExecuting = false;
            PublishResolvedSafely(result);
        }

        private void ResolveFault(SkillExecutionContext context, Exception exception)
        {
            SkillExecutionResult faultResult = new(
                context,
                SkillExecutionStatus.Faulted,
                0,
                0f,
                0,
                exception);
            HandleExecutionResolved(context, faultResult);
        }

        private SkillExecutionContext CreateExecutionContext()
        {
            if (initializationContext.Attacker.EquippedWeapon is not ElementalWeaponBase elementalWeapon)
            {
                throw new InvalidOperationException(
                    $"[{name}] A tower skill requires its attacker's equipped weapon to be an " +
                    $"{nameof(ElementalWeaponBase)}.");
            }

            return new SkillExecutionContext(
                Guid.NewGuid().ToString("N"),
                Definition,
                owner,
                owner.transform.position,
                elementalWeapon.AttackPower,
                elementalWeapon.AttackElement,
                DateTimeOffset.UtcNow);
        }

        private bool IsActiveExecution(SkillExecutionContext context)
        {
            return isExecuting &&
                   activeExecution != null &&
                   ReferenceEquals(activeExecution, context);
        }

        private SkillExecutorBase ResolveExecutor()
        {
            executor = executor != null ? executor : GetComponent<SkillExecutorBase>();
            return executor;
        }

        private void PublishReadySafely()
        {
            Action<TowerSkillController> listeners = OnSkillReady;
            if (listeners == null)
            {
                return;
            }

            foreach (Action<TowerSkillController> listener in listeners.GetInvocationList())
            {
                try
                {
                    listener(this);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void PublishUseStartedSafely(SkillExecutionContext context)
        {
            Action<TowerSkillController, SkillExecutionContext> listeners = OnSkillUseStarted;
            if (listeners == null)
            {
                return;
            }

            foreach (Action<TowerSkillController, SkillExecutionContext> listener in listeners.GetInvocationList())
            {
                try
                {
                    listener(this, context);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void PublishResolvedSafely(SkillExecutionResult result)
        {
            Action<TowerSkillController, SkillExecutionResult> listeners = OnSkillResolved;
            if (listeners == null)
            {
                return;
            }

            foreach (Action<TowerSkillController, SkillExecutionResult> listener in listeners.GetInvocationList())
            {
                try
                {
                    listener(this, result);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
    }

    public enum SkillUseRequestResult
    {
        Accepted = 0,
        NotInitialized = 1,
        BattleInactive = 2,
        NotReady = 3,
        AlreadyExecuting = 4,
        ExecutorRejected = 5,
        Faulted = 6,
        Shutdown = 7,
    }
}
