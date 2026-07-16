using UnityEngine;
using UnityEngine.Events;
using System;

namespace DefCity.Gameplay.Combat
{
    public struct DamageEventArgs
    {
        public GameObject Instigator;
        public GameObject Victim;
        public float DamageAmount;
    }

    public class Damageable : MonoBehaviour
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

        public void TakeDamage(GameObject instigator, float damageAmount)
        {
            if (IsAlive == false)
            {
                return;
            }

            health -= damageAmount;
            health = Mathf.Clamp(health, 0f, maxHealth);

            OnDamaged.Invoke(instigator, new DamageEventArgs
            {
                Instigator = instigator,
                Victim = gameObject,
                DamageAmount = damageAmount
            });
            if (IsAlive == false)
            {
                OnDeathController(instigator, gameObject, damageAmount);
            }
        }

        private void OnDeathController(GameObject instigator, GameObject victim, float damageAmount)
        {
            OnDeath.Invoke(instigator, new DamageEventArgs
            {
                Instigator = instigator,
                Victim = gameObject,
                DamageAmount = damageAmount
            });
            damageCollider.enabled = false;

            if (destroyOnDeath)
            {
                Destroy(gameObject, deathDestroyDelay);
            }
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
