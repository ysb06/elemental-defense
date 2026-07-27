using System.Collections.Generic;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Navigation;
using UnityEngine;

namespace ElementalDef.Gameplay.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Attacker), typeof(HostileTargetScanner))]
    public class AutoCombatController : MonoBehaviour
    {
        private enum AutoCombatState
        {
            Inactive,
            Searching,
            Attacking,
        }

        private enum PreCombatAction
        {
            Waiting,
            Moving,
        }

        [SerializeField] private HostileTargetScanner scanner;
        [SerializeField] private Attacker attacker;
        [SerializeField] private UnitMovement movement;
        [SerializeField] private bool attackWhileMoving;

        public bool AttackWhileMoving => attackWhileMoving;

        private AutoCombatState currentState = AutoCombatState.Inactive;
        private PreCombatAction preCombatAction = PreCombatAction.Waiting;
        private Health currentTarget;
        private bool ownsMovementPause;
        private int lastAttackAttemptFrame = -1;

        private void Awake()
        {
            if (scanner == null)
            {
                scanner = GetComponent<HostileTargetScanner>();
            }

            if (attacker == null)
            {
                attacker = GetComponent<Attacker>();
            }

            if (movement == null)
            {
                movement = GetComponent<UnitMovement>();
            }
        }

        private void OnEnable()
        {
            currentState = AutoCombatState.Searching;
            preCombatAction = PreCombatAction.Waiting;
            currentTarget = null;
            ownsMovementPause = false;
            lastAttackAttemptFrame = -1;

            scanner.OnScanCompleted.AddListener(HandleScanCompleted);
            attacker.OnAttackRejected.AddListener(HandleAttackRejected);
        }

        private void OnDisable()
        {
            scanner.OnScanCompleted.RemoveListener(HandleScanCompleted);
            attacker.OnAttackRejected.RemoveListener(HandleAttackRejected);

            bool shouldRestoreMovement =
                ownsMovementPause && preCombatAction == PreCombatAction.Moving;

            ClearCurrentTarget();
            currentState = AutoCombatState.Inactive;
            preCombatAction = PreCombatAction.Waiting;
            ownsMovementPause = false;
            lastAttackAttemptFrame = -1;

            if (shouldRestoreMovement && gameObject.activeInHierarchy)
            {
                if (movement == null)
                {
                    Debug.LogError($"[{name}] Owned movement pause cannot be released because UnitMovement is missing.", this);
                    return;
                }

                if (!movement.TryResume())
                {
                    movement.Stop();
                }
            }
        }

        public void Shutdown()
        {
            ClearCurrentTarget();
            currentState = AutoCombatState.Inactive;
            preCombatAction = PreCombatAction.Waiting;
            ownsMovementPause = false;
            lastAttackAttemptFrame = -1;

            enabled = false;
        }

        private void Update()
        {
            if (currentState != AutoCombatState.Attacking)
            {
                return;
            }

            // Cooldown is intentionally checked after target validity and range.
            // GetAttackStartRejectReason reports cooldown before range, which would
            // otherwise keep a moving unit paused after its target left attack range.
            if (!attacker.IsAttackAvailable(currentTarget))
            {
                EndEngagement();
                return;
            }

            if (attacker.IsOnCooldown)
            {
                return;
            }

            TryAttackCurrentTarget();
        }

        private void HandleScanCompleted(GameObject sender, HostileTargetScanEventArgs args)
        {
            switch (currentState)
            {
                case AutoCombatState.Inactive:
                    return;

                case AutoCombatState.Attacking:
                    return;

                case AutoCombatState.Searching:
                    TryEngageClosestAttackableTarget(args.Targets);
                    return;
            }
        }

        private void TryEngageClosestAttackableTarget(IReadOnlyList<Health> targets)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Health target = targets[i];
                if (!attacker.IsAttackAvailable(target))
                {
                    continue;
                }

                if (!TryBeginEngagement(target))
                {
                    return;
                }

                // Instant-hit attacks can defeat the selected target inside
                // TryBeginEngagement. In that case, continue through the same
                // scan result so another valid target can be acquired without
                // waiting for the next scan interval.
                if (currentState != AutoCombatState.Searching)
                {
                    return;
                }
            }
        }

        private bool TryBeginEngagement(Health target)
        {
            if (currentState != AutoCombatState.Searching ||
                !attacker.IsAttackAvailable(target))
            {
                return false;
            }

            PreCombatAction previousAction = PreCombatAction.Waiting;
            bool acquiredMovementPause = false;

            if (!attackWhileMoving && movement != null && movement.HasActiveMovement)
            {
                previousAction = PreCombatAction.Moving;
                if (!movement.TryPause())
                {
                    Debug.LogWarning(
                        $"[{name}] Cannot start automatic combat because active movement could not be paused.",
                        this);
                    return false;
                }

                acquiredMovementPause = true;
            }

            currentTarget = target;
            currentTarget.OnDeath.AddListener(HandleTargetDeath);
            preCombatAction = previousAction;
            ownsMovementPause = acquiredMovementPause;
            currentState = AutoCombatState.Attacking;

            // Target acquisition is independent from attack readiness. Keeping
            // the target while the weapon is cooling down prevents a unit from
            // remaining targetless or resuming movement between engagements.
            if (!attacker.IsOnCooldown)
            {
                TryAttackCurrentTarget();
            }

            return true;
        }

        private void TryAttackCurrentTarget()
        {
            if (currentState != AutoCombatState.Attacking ||
                currentTarget == null ||
                lastAttackAttemptFrame == Time.frameCount)
            {
                return;
            }

            Health attackTarget = currentTarget;
            lastAttackAttemptFrame = Time.frameCount;

            try
            {
                attacker.TryAttack(attackTarget);
            }
            catch
            {
                if (currentState == AutoCombatState.Attacking && currentTarget == attackTarget)
                {
                    EndEngagement();
                }

                throw;
            }
        }

        private void HandleAttackRejected(GameObject sender, AttackRejectedEventArgs args)
        {
            if (currentState != AutoCombatState.Attacking || args.Info.Attacker != attacker)
            {
                return;
            }

            if (args.Info.Target != null && args.Info.Target != currentTarget)
            {
                return;
            }

            switch (args.RejectReason)
            {
                case AttackRejectReason.OnCooldown:
                case AttackRejectReason.AttackLocked:
                    return;

                default:
                    EndEngagement();
                    return;
            }
        }

        private void HandleTargetDeath(GameObject sender, DamageEventArgs args)
        {
            if (currentState != AutoCombatState.Attacking ||
                currentTarget == null ||
                args.Victim != currentTarget.gameObject)
            {
                return;
            }

            EndEngagement();
        }

        private void EndEngagement()
        {
            if (currentState != AutoCombatState.Attacking)
            {
                return;
            }

            bool shouldRestoreMovement =
                ownsMovementPause && preCombatAction == PreCombatAction.Moving;

            ClearCurrentTarget();
            currentState = AutoCombatState.Searching;
            preCombatAction = PreCombatAction.Waiting;
            ownsMovementPause = false;

            if (!shouldRestoreMovement)
            {
                return;
            }

            if (movement != null && movement.TryResume())
            {
                return;
            }

            currentState = AutoCombatState.Inactive;
            if (movement == null)
            {
                Debug.LogError($"[{name}] Owned movement pause cannot be released because UnitMovement is missing.", this);
                return;
            }

            movement.Stop();
        }

        private void ClearCurrentTarget()
        {
            Health previousTarget = currentTarget;
            currentTarget = null;

            if (previousTarget != null)
            {
                previousTarget.OnDeath.RemoveListener(HandleTargetDeath);
            }
        }
    }
}
