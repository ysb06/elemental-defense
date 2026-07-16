using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DefCity.Gameplay.Combat.Weapons;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.Combat
{
    [RequireComponent(typeof(Entity))]
    public class AttackCapable : MonoBehaviour
    {
        // Inspector에서 노출되는 속성
        [SerializeField] private WeaponBase equippedWeapon;
        [SerializeField] private Entity entity;

        // 기본 발생 이벤트. Inspector에서도 확인 가능하도록 public으로 노출
        public AttackStartedEvent OnAttackStarted = new();
        public AttackRejectedEvent OnAttackRejected = new();
        public AttackResolvedEvent OnAttackResolved = new();
        public AttackFinishedEvent OnAttackFinished = new();
        public AttackCooldownEndEvent OnAttackCooldownEnd = new();

        // Inspector에서 노출되지 않지만 공개되는 속성
        public WeaponBase EquippedWeapon => equippedWeapon;
        public Team Team => entity.Team;
        public bool IsOnCooldown => isOnCooldown;

        // 내부 상태
        private bool isOnCooldown = false;
        private bool isAttackStartReserved = false;
        private readonly Dictionary<int, AttackInfoArgs> activeAttacks = new();
        private int nextAttackId = 0;

        private void Awake()
        {
            entity = GetComponent<Entity>();
        }

        public bool IsReferenceValid(Damageable target)
        {
            if (equippedWeapon == null)
            {
                Debug.LogWarning($"[{name}] Weapon is not assigned.");
                return false;
            }

            if (target == null)
            {
                Debug.LogWarning($"Unknown target.");
                return false;
            }

            return true;
        }

        public static AttackRejectReason GetTargetRejectReason(
            Team attackerTeam,
            Damageable target,
            bool requireActiveTarget,
            out Entity targetEntity,
            out Team targetTeam)
        {
            targetEntity = null;
            targetTeam = null;

            if (target == null)
            {
                return AttackRejectReason.ReferenceNotValid;
            }

            targetEntity = target.GetComponent<Entity>();
            targetTeam = targetEntity != null ? targetEntity.Team : null;

            if (!target.IsAlive)
            {
                return AttackRejectReason.TargetDead;
            }

            if (requireActiveTarget && (!target.isActiveAndEnabled || !target.gameObject.activeInHierarchy))
            {
                return AttackRejectReason.TargetInactive;
            }

            if (targetEntity == null || targetTeam == null)
            {
                return AttackRejectReason.ReferenceNotValid;
            }

            if (attackerTeam == null)
            {
                return AttackRejectReason.ReferenceNotValid;
            }

            if (targetTeam.IsAlliedWith(attackerTeam))
            {
                return AttackRejectReason.AlliedTarget;
            }

            return AttackRejectReason.None;
        }

        public bool IsAttackAvailable(Damageable target)
        {
            if (equippedWeapon == null)
            {
                return false;
            }

            if (GetTargetRejectReason(Team, target, true, out _, out _) != AttackRejectReason.None)
            {
                return false;
            }

            return target.GetDistanceTo(transform.position) <= equippedWeapon.AttackRange;
        }

        public AttackRejectReason GetAttackStartRejectReason(Damageable target)
        {
            if (equippedWeapon == null)
            {
                return AttackRejectReason.ReferenceNotValid;
            }

            AttackRejectReason targetRejectReason = GetTargetRejectReason(Team, target, true, out _, out _);
            if (targetRejectReason != AttackRejectReason.None)
            {
                return targetRejectReason;
            }

            if (isOnCooldown)
            {
                return AttackRejectReason.OnCooldown;
            }

            return target.GetDistanceTo(transform.position) > equippedWeapon.AttackRange
                ? AttackRejectReason.TargetOutOfRange
                : AttackRejectReason.None;
        }

        public bool CanStartAttack(Damageable target)
        {
            return GetAttackStartRejectReason(target) == AttackRejectReason.None;
        }

        /// <summary>
        /// 공격을 시도한다. 조건이 충족되지 않으면 공격이 실패할 수 있음.
        /// </summary>
        public void TryAttack(Damageable target)
        {
            int attackId = nextAttackId++;
            AttackInfoArgs initialAttackInfoArgs = new(attackId, this);

            if (isAttackStartReserved)
            {
                OnAttackStarted.Invoke(gameObject, initialAttackInfoArgs);
                OnAttackRejected.Invoke(gameObject, new AttackRejectedEventArgs(initialAttackInfoArgs, AttackRejectReason.AttackLocked));
                return;
            }

            AttackRejectedEventArgs pendingReject = default;
            AttackInfoArgs attackInfoArgs = default;
            bool attackAccepted = false;
            bool hasReject = false;
            bool addedActiveAttack = false;
            WeaponBase acceptedWeapon = null;

            isAttackStartReserved = true;
            try
            {
                OnAttackStarted.Invoke(gameObject, initialAttackInfoArgs);

                if (!TryBuildValidatedAttackInfo(attackId, target, initialAttackInfoArgs, out attackInfoArgs, out pendingReject))
                {
                    hasReject = true;
                    return;
                }

                activeAttacks.Add(attackId, attackInfoArgs);
                addedActiveAttack = true;

                isOnCooldown = true;
                acceptedWeapon = attackInfoArgs.Weapon;
                attackAccepted = acceptedWeapon.TryStartAttack(attackInfoArgs, ResolveAttack);

                if (!attackAccepted)
                {
                    activeAttacks.Remove(attackId);
                    addedActiveAttack = false;
                    isOnCooldown = false;
                    pendingReject = new AttackRejectedEventArgs(attackInfoArgs, AttackRejectReason.WeaponRejected);
                    hasReject = true;
                }
            }
            catch
            {
                if (addedActiveAttack)
                {
                    activeAttacks.Remove(attackId);
                }

                isOnCooldown = false;
                throw;
            }
            finally
            {
                isAttackStartReserved = false;
            }

            if (attackAccepted)
            {
                StartCoroutine(AttackCooldownRoutine(acceptedWeapon));
            }

            if (hasReject)
            {
                OnAttackRejected.Invoke(gameObject, pendingReject);
            }
        }

        private bool TryBuildValidatedAttackInfo(
            int attackId,
            Damageable target,
            AttackInfoArgs initialAttackInfoArgs,
            out AttackInfoArgs attackInfoArgs,
            out AttackRejectedEventArgs rejectArgs)
        {
            attackInfoArgs = initialAttackInfoArgs;
            rejectArgs = default;

            if (equippedWeapon == null)
            {
                Debug.LogWarning($"[{name}] Weapon is not assigned.");
                rejectArgs = new AttackRejectedEventArgs(initialAttackInfoArgs, AttackRejectReason.ReferenceNotValid);
                return false;
            }

            AttackRejectReason targetRejectReason = GetTargetRejectReason(Team, target, true, out Entity targetEntity, out Team targetTeam);
            if (target != null && targetEntity != null && targetTeam != null)
            {
                attackInfoArgs = new AttackInfoArgs(
                    attackId: attackId,
                    attacker: this,
                    weapon: equippedWeapon,
                    target: target,
                    attackerTeam: Team,
                    targetTeam: targetTeam
                );
            }

            if (targetRejectReason != AttackRejectReason.None)
            {
                rejectArgs = new AttackRejectedEventArgs(attackInfoArgs, targetRejectReason);
                return false;
            }

            if (isOnCooldown)
            {
                rejectArgs = new AttackRejectedEventArgs(attackInfoArgs, AttackRejectReason.OnCooldown);
                return false;
            }

            float distance = target.GetDistanceTo(transform.position);
            if (distance > equippedWeapon.AttackRange)
            {
                rejectArgs = new AttackRejectedEventArgs(attackInfoArgs, AttackRejectReason.TargetOutOfRange);
                return false;
            }

            return true;
        }

        private void ResolveAttack(WeaponBase weapon, AttackResolvedEventArgs args)
        {
            if (args.Info.AttackId.HasValue && !activeAttacks.Remove(args.Info.AttackId.Value))
            {
                Debug.LogWarning($"[{name}] Attack ID {args.Info.AttackId} not found in active attacks. This may indicate a duplicate resolve callback.");
                return;
            }

            OnAttackResolved.Invoke(gameObject, args);

            // 추후 Resolve 후 대기 관련 코드를 이곳에 추가할 수 있음
            // 다만, Cooldown을 어떻게 함께 처리할지 고민 필요.

            OnAttackFinished.Invoke(gameObject, args);
        }

        private IEnumerator AttackCooldownRoutine(WeaponBase acceptedWeapon)
        {
            if (acceptedWeapon.AttackCooldown > 0f)
            {
                yield return new WaitForSeconds(acceptedWeapon.AttackCooldown);
            }

            isOnCooldown = false;
            AttackCooldownEndEventArgs args = new(this, acceptedWeapon);
            OnAttackCooldownEnd.Invoke(gameObject, args);
        }
    }

    public enum AttackRejectReason
    {
        None = 0,
        ReferenceNotValid = 1,
        OnCooldown = 2,
        TargetDead = 3,
        TargetOutOfRange = 4,
        AlliedTarget = 5,
        WeaponRejected = 6,
        AttackLocked = 7,
        TargetInactive = 8
    }

    [Serializable]
    public readonly struct AttackInfoArgs
    {
        public int? AttackId { get; }
        public AttackCapable Attacker { get; }
        public WeaponBase Weapon { get; }
        public IWeapon WeaponSnapshot { get; }
        public Damageable Target { get; }
        public Team AttackerTeam { get; }
        public Team TargetTeam { get; }

        public AttackInfoArgs(
            int? attackId,
            AttackCapable attacker,
            WeaponBase weapon,
            Damageable target,
            Team attackerTeam,
            Team targetTeam
        )
        {
            AttackId = attackId;
            Attacker = attacker;
            Weapon = weapon;
            WeaponSnapshot = new WeaponSnapshot(weapon);
            Target = target;
            AttackerTeam = attackerTeam;
            TargetTeam = targetTeam;
        }

        public AttackInfoArgs(int? attackId, AttackCapable attacker)
        {
            AttackId = attackId;
            Attacker = attacker;
            Weapon = null;
            WeaponSnapshot = null;
            Target = null;
            AttackerTeam = attacker.Team;
            TargetTeam = null;
        }
    }

    [Serializable]
    public readonly struct AttackRejectedEventArgs
    {
        public AttackInfoArgs Info { get; }
        public AttackRejectReason RejectReason { get; }

        public AttackRejectedEventArgs(AttackInfoArgs info, AttackRejectReason rejectReason)
        {
            Info = info;
            RejectReason = rejectReason;
        }
    }

    [Serializable]
    public readonly struct AttackCooldownEndEventArgs
    {
        public AttackCapable Attacker { get; }
        public WeaponBase Weapon { get; }
        public float CooldownDuration { get; }

        public AttackCooldownEndEventArgs(AttackCapable attacker, WeaponBase weapon) : this()
        {
            Attacker = attacker;
            Weapon = weapon;
            CooldownDuration = weapon.AttackCooldown;
        }
    }

    [Serializable]
    public class AttackStartedEvent : UnityEvent<GameObject, AttackInfoArgs> { }

    [Serializable]
    public class AttackRejectedEvent : UnityEvent<GameObject, AttackRejectedEventArgs> { }

    [Serializable]
    public class AttackResolvedEvent : UnityEvent<GameObject, AttackResolvedEventArgs> { }

    [Serializable]
    public class AttackFinishedEvent : UnityEvent<GameObject, AttackResolvedEventArgs> { }

    [Serializable]
    public class AttackCooldownEndEvent : UnityEvent<GameObject, AttackCooldownEndEventArgs> { }
}
