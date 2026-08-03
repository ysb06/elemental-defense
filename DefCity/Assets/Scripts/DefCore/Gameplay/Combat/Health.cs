using UnityEngine;
using UnityEngine.Events;
using System;

namespace DefCore.Gameplay.Combat
{
    public struct DamageEventArgs
    {
        public GameObject Instigator;
        public GameObject Victim;
        public float RequestedDamage;
        public float DamageAmount;
        public float RemainingHealth;
        public readonly bool IsFatal => RemainingHealth <= 0f;
    }

    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float health = 100f;
        [SerializeField] private Collider damageCollider;
        [SerializeField] private bool destroyOnDeath;
        [SerializeField] private float deathDestroyDelay = 3f;
        public Collider DamageCollider => damageCollider;
        public float CurrentHealth => health;
        public float MaxHealth => maxHealth;
        public bool IsAlive => health > 0f;
        public bool DestroyOnDeath => destroyOnDeath;
        public float DeathDestroyDelay => deathDestroyDelay;

        public DamageEvent OnDamaged = new();
        public DamageEvent OnDeath = new();

        public void Initialize(float maxHealth)
        {
            this.maxHealth = maxHealth;
            health = maxHealth;
        }

        public DamageEventArgs TakeDamage(GameObject instigator, float damageAmount)
        {
            if (float.IsNaN(damageAmount) || float.IsInfinity(damageAmount) || damageAmount < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damageAmount),
                    damageAmount,
                    "Damage must be a finite, non-negative value. Use a dedicated healing API to restore health.");
            }

            DamageEventArgs damageEventArgs = new DamageEventArgs
            {
                Instigator = instigator,
                Victim = gameObject,
                RequestedDamage = damageAmount,
                DamageAmount = 0,
                RemainingHealth = health
            };

            if (IsAlive == false)
            {
                return damageEventArgs;
            }

            float previousHealth = health;
            health -= damageAmount;
            health = Mathf.Clamp(health, 0f, maxHealth);

            damageEventArgs.DamageAmount = previousHealth - health;
            damageEventArgs.RemainingHealth = health;

            OnDamaged.Invoke(instigator, damageEventArgs);
            if (IsAlive == false)
            {
                OnDeath.Invoke(instigator, damageEventArgs);
                damageCollider.enabled = false;
                if (destroyOnDeath)
                {
                    Destroy(gameObject, deathDestroyDelay);
                }
            }
            return damageEventArgs;
        }

        public Vector3 GetClosestPoint(Vector3 origin)
        {
            return damageCollider.ClosestPoint(origin);
        }

        public float GetDistanceTo(Vector3 origin)
        {
            return Vector3.Distance(origin, GetClosestPoint(origin));
        }

        public float GetDistanceSqrTo(Vector3 origin)
        {
            return (GetClosestPoint(origin) - origin).sqrMagnitude;
        }
    }

    [Serializable]
    public class DamageEvent : UnityEvent<GameObject, DamageEventArgs> { }
}
