using System;
using DefCore.Gameplay.Combat;
using DefCore.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace ElementalDef.Gameplay.Entities
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Entity), typeof(Health))]
    public class HeadquartersBuilding : MonoBehaviour
    {
        private Entity entity;
        private Health health;

        public HeadquartersBuildingEvent OnDestroyed = new();

        private void Awake()
        {
            entity = GetComponent<Entity>();
            health = GetComponent<Health>();

            health.OnDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(HandleDeath);
            }
        }

        private void HandleDeath(GameObject sender, DamageEventArgs args)
        {
            if (args.Victim != gameObject || !entity.TryMarkDead())
            {
                return;
            }

            health.OnDeath.RemoveListener(HandleDeath);

            try
            {
                OnDestroyed?.Invoke(gameObject);
            }
            finally
            {
                Destroy(gameObject);
            }
        }
    }

    [Serializable]
    public class HeadquartersBuildingEvent : UnityEvent<GameObject> { }
}
