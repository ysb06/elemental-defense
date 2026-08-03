using System;
using System.Collections;
using UnityEngine;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Entities;
using DefCity.Gameplay.Navigation;
using DefCore.Gameplay.Combat;

namespace DefCity.Gameplay.City.Buildings
{
    [RequireComponent(typeof(Entity))]
    public class Building : MonoBehaviour
    {
        private BuildingManager buildingManager;
        [SerializeField] private GameObject buildingModel;
        [SerializeField] private Entity entity;
        [SerializeField] private BuildingNavigationModifier navigationModifier;
        [SerializeField] private Health damageable;
        [SerializeField] private Collider buildingCollider;

        [SerializeField] private float demolishDuration = 3f;
        public float Height => buildingCollider != null ? buildingCollider.bounds.size.y : 0f;
        public bool IsAlive => damageable == null || damageable.IsAlive;
        public bool IsRegistered => buildingManager != null;
        public Collider BuildingCollider => buildingCollider;
        public Team Team => Entity != null ? Entity.Team : null;
        private Coroutine demolishCoroutine;

        private Entity Entity
        {
            get
            {
                ResolveReferences();
                return entity;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (damageable != null)
            {
                damageable.OnDeath.AddListener(OnDeath);
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDeath.RemoveListener(OnDeath);
            }
        }

        public void OnDeath(GameObject sender, DamageEventArgs args)
        {
            float demolishDistance = Height;

            if (navigationModifier != null)
            {
                navigationModifier.SetNavigationEnabled(false);
            }

            if (buildingCollider != null)
            {
                buildingCollider.enabled = false;
            }

            if (buildingManager != null)
            {
                buildingManager.UnregisterBuilding(this);
            }

            if (demolishCoroutine != null)
            {
                StopCoroutine(demolishCoroutine);
            }

            if (buildingModel != null)
            {
                demolishCoroutine = StartCoroutine(Demolish(demolishDistance));
            }
        }

        public IEnumerator Demolish(float demolishDistance)
        {
            Vector3 startPosition = buildingModel.transform.position;
            Vector3 targetPosition = startPosition + Vector3.down * demolishDistance;

            if (demolishDuration <= 0f)
            {
                buildingModel.transform.position = targetPosition;
                demolishCoroutine = null;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < demolishDuration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / demolishDuration);
                buildingModel.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            buildingModel.transform.position = targetPosition;
            demolishCoroutine = null;
        }

        internal void AssignBuildingManager(BuildingManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (buildingManager != null && buildingManager != manager)
            {
                throw new InvalidOperationException($"{name} is already assigned to another BuildingManager.");
            }

            buildingManager = manager;
        }

        internal void ClearBuildingManager(BuildingManager manager)
        {
            if (buildingManager != manager)
            {
                throw new InvalidOperationException($"{name} is not assigned to this BuildingManager.");
            }

            buildingManager = null;
        }

        private void ResolveReferences()
        {
            if (entity == null)
            {
                entity = GetComponent<Entity>();
            }

            if (navigationModifier == null)
            {
                navigationModifier = GetComponentInChildren<BuildingNavigationModifier>(true);
            }

            if (damageable == null)
            {
                damageable = GetComponent<Health>();
            }

            if (buildingCollider == null)
            {
                buildingCollider = GetComponent<Collider>();
            }

            if (buildingModel == null)
            {
                buildingModel = gameObject;
            }
        }
    }
}
