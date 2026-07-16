using System;
using Unity.AI.Navigation;
using UnityEngine;
using DefCity.Gameplay.City.Buildings;
using DefCity.Gameplay.Entities;

namespace DefCity.Gameplay.Navigation
{
    [DisallowMultipleComponent]
    public class BuildingNavigationModifier : MonoBehaviour
    {
        private const string ModifierChildName = "Navigation Modifier";

        [SerializeField] private Building building;
        [SerializeField] private BoxCollider sourceCollider;
        [SerializeField] private NavMeshModifierVolume modifierVolume;
        [SerializeField] private bool createModifierVolumeIfMissing = true;
        [SerializeField] private bool syncVolumeFromBoxCollider = true;

        private void Awake()
        {
            ResolveReferences(true);
        }

        private void OnEnable()
        {
            ResolveReferences(true);
            Apply();
        }

        private void OnValidate()
        {
            ResolveReferences(false);
            Apply(logMissingTeam: false);
        }

        public void SetNavigationEnabled(bool isEnabled)
        {
            ResolveReferences(false);

            if (modifierVolume != null)
            {
                modifierVolume.enabled = isEnabled;
            }
        }

        public void Apply()
        {
            Apply(logMissingTeam: true);
        }

        private void Apply(bool logMissingTeam)
        {
            if (!HasRequiredReferences())
            {
                return;
            }
            
            ApplyNavigationModifierLayer();
            ApplyNavigationArea(logMissingTeam);

            if (syncVolumeFromBoxCollider)
            {
                SyncVolumeFromBoxCollider();
            }
        }

        private void ApplyNavigationArea(bool logMissingTeam)
        {
            Team team = building.Team;
            if (team == null)
            {
                if (logMissingTeam)
                {
                    Debug.LogError($"{building.name} has no Team assigned.", this);
                }

                return;
            }

            try
            {
                modifierVolume.area = TeamNavigationPolicy.GetBuildingAreaIndex(team.Kind);
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
            }
        }

        private void ResolveReferences(bool canCreateModifierVolume)
        {
            if (building == null)
            {
                building = GetComponent<Building>();
            }

            if (sourceCollider == null && building != null)
            {
                sourceCollider = building.BuildingCollider as BoxCollider;
            }

            if (modifierVolume == null)
            {
                modifierVolume = GetComponentInChildren<NavMeshModifierVolume>(true);
            }

            if (modifierVolume == null && canCreateModifierVolume && createModifierVolumeIfMissing)
            {
                modifierVolume = CreateModifierVolume();
            }
        }

        private NavMeshModifierVolume CreateModifierVolume()
        {
            GameObject modifierObject = new(ModifierChildName);
            modifierObject.transform.SetParent(transform, false);
            modifierObject.layer = ResolveNavigationModifierLayer();
            return modifierObject.AddComponent<NavMeshModifierVolume>();
        }

        private bool HasRequiredReferences()
        {
            if (building == null)
            {
                Debug.LogError($"{name} requires a Building reference.", this);
                return false;
            }

            if (modifierVolume == null)
            {
                Debug.LogError($"{name} requires a NavMeshModifierVolume.", this);
                return false;
            }

            return true;
        }

        private void ApplyNavigationModifierLayer()
        {
            modifierVolume.gameObject.layer = ResolveNavigationModifierLayer();
        }

        private static int ResolveNavigationModifierLayer()
        {
            int layer = LayerMask.NameToLayer(TeamNavigationPolicy.NavigationModifierLayerName);
            if (layer < 0)
            {
                throw new InvalidOperationException($"Layer '{TeamNavigationPolicy.NavigationModifierLayerName}' is not configured.");
            }

            return layer;
        }

        private void SyncVolumeFromBoxCollider()
        {
            if (sourceCollider == null)
            {
                Debug.LogError($"{name} cannot sync navigation volume because no BoxCollider is assigned.", this);
                return;
            }

            Transform volumeTransform = modifierVolume.transform;
            modifierVolume.center = volumeTransform.InverseTransformPoint(sourceCollider.transform.TransformPoint(sourceCollider.center));
            modifierVolume.size = GetColliderSizeInTargetSpace(sourceCollider, volumeTransform);
        }

        private static Vector3 GetColliderSizeInTargetSpace(BoxCollider collider, Transform targetTransform)
        {
            Vector3 colliderScale = collider.transform.lossyScale;
            Vector3 targetScale = targetTransform.lossyScale;

            return new Vector3(
                Mathf.Abs(collider.size.x * colliderScale.x / targetScale.x),
                Mathf.Abs(collider.size.y * colliderScale.y / targetScale.y),
                Mathf.Abs(collider.size.z * colliderScale.z / targetScale.z));
        }
    }
}
