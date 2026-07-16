using System;
using UnityEngine;
using UnityEngine.Events;
using DefCity.Gameplay.City.Buildings;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Entities;
using DefCity.Gameplay.Economy;
using DefCity.Gameplay.Interaction;
using DefCity.Gameplay.Navigation;
using DefCity.Gameplay.World;

namespace DefCity.Gameplay.City.Construction
{
    public struct BuilderEventArgs
    {
        public Entity BuildingTarget { get; private set; }
        public bool IsBuildModeActive { get; private set; }

        public BuilderEventArgs(Entity buildingTarget, bool isBuildModeActive)
        {
            BuildingTarget = buildingTarget;
            IsBuildModeActive = isBuildModeActive;
        }
    }

    // Todo: 추후 Builder에서 생성과 배치 로직을 분리를 검토할 것. 또한 생성 로직은 EnemySpawner의 생성 로직과 통합을 검토할 것.ㄴ
    /// <summary>
    /// Handles the building placement and construction logic in the game. 
    /// This component allows players to place buildings on terrain cells, ensuring that placement is valid according to the game's rules. 
    /// It also manages the build mode state and notifies listeners when the build mode changes.
    /// </summary>
    [RequireComponent(typeof(PlacementValidator))]
    public class Builder : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private PlacementValidator placementValidator;
        [SerializeField] private TerrainMouseEventManager terrainMouseEventManager;
        [SerializeField] private CreditManager creditManager;
        [SerializeField] private Entity entityTarget;

        // References for built target only
        [SerializeField] private Team team;
        [SerializeField] private TerrainCellManager terrainCellManager;
        

        private bool isBuildModeActive;

        public BuilderEvent OnBuildModeChanged = new();
        public bool IsBuildModeActive => isBuildModeActive;

        public Entity EntityTarget
        {
            get => entityTarget;
            set => entityTarget = value;
        }

        private void OnEnable()
        {
            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseClick.AddListener(OnTerrainCellMouseClick);
            }
        }

        private void OnDisable()
        {
            if (terrainMouseEventManager != null)
            {
                terrainMouseEventManager.OnTerrainCellMouseClick.RemoveListener(OnTerrainCellMouseClick);
            }
        }

        public void OnTerrainCellMouseClick(GameObject sender, TerrainCellEventArgs eventArgs)
        {
            if (Build(eventArgs.Cell, entityTarget))
            {
                EndBuild();
            }
        }

        public void BeginBuild(Entity entity)
        {
            entityTarget = entity;
            enabled = true;

            if (isBuildModeActive)
            {
                return;
            }

            isBuildModeActive = true;
            OnBuildModeChanged?.Invoke(gameObject, new BuilderEventArgs(entityTarget, isBuildModeActive));
        }

        public void EndBuild()
        {
            BuilderEventArgs eventArgs = new(entityTarget, false);

            entityTarget = null;
            bool wasBuildModeActive = isBuildModeActive;
            isBuildModeActive = false;
            enabled = false;

            if (wasBuildModeActive)
            {
                OnBuildModeChanged?.Invoke(gameObject, eventArgs);
            }
        }

        public bool Build(TerrainCell cell, Entity entity)
        {
            if (!CanBuild(cell, entity, out string failureReason))
            {
                Debug.LogWarning($"Cannot build on cell {cell.RefPosition}: {failureReason}", this);
                return false;
            }

            Vector3 entityPosition = GetBuildPosition(cell);
            Quaternion entityRotation = Quaternion.identity;

            GameObject createdEntity = Instantiate(entity.gameObject, entityPosition, entityRotation);
            if (createdEntity.TryGetComponent(out Entity createdEntityComponent) && team != null)
            {
                createdEntityComponent.Team = team;
            }

            InjectPlayerCreditManager(createdEntity);

            if (createdEntity.TryGetComponent(out Movable movable) && terrainCellManager != null)
            {
                movable.TerrainCellManager = terrainCellManager;
            }

            if (createdEntity.TryGetComponent(out BaseCombatController combatController) && terrainCellManager != null)
            {
                combatController.TerrainCellManager = terrainCellManager;
            }
            
            if (createdEntity.TryGetComponent(out Building building))
            {
                BuildingNavigationModifier navigationModifier = createdEntity.GetComponentInChildren<BuildingNavigationModifier>(true);
                if (navigationModifier != null)
                {
                    navigationModifier.Apply();
                }

                if (buildingManager != null)
                {
                    buildingManager.RegisterBuilding(building);
                }
            }

            createdEntity.SetActive(true);
            return true;
        }

        public bool CanBuild(TerrainCell cell, Entity entity, out string failureReason)
        {
            if (entity == null)
            {
                failureReason = "No entity is selected.";
                return false;
            }

            if (placementValidator == null)
            {
                failureReason = $"{nameof(Builder)} requires a {nameof(PlacementValidator)} reference.";
                return false;
            }

            Vector3 entityPosition = GetBuildPosition(cell);
            Quaternion entityRotation = Quaternion.identity;

            if (!placementValidator.CanPlace(entity.gameObject, entityPosition, entityRotation, out failureReason))
            {
                return false;
            }

            int totalConstructionCost = GetTotalConstructionCost(entity);
            if (totalConstructionCost <= 0)
            {
                failureReason = string.Empty;
                return true;
            }

            if (creditManager == null)
            {
                failureReason = $"{entity.name} requires {totalConstructionCost} credits, but no CreditManager is assigned.";
                return false;
            }

            if (!creditManager.CanAfford(totalConstructionCost))
            {
                failureReason = $"Not enough credits to build {entity.name}. Required: {totalConstructionCost}, Available: {creditManager.Credits}.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static Vector3 GetBuildPosition(TerrainCell cell)
        {
            Vector3 entityPosition = cell.Center;
            entityPosition.y = cell.AverageWorldHeight;
            return entityPosition;
        }

        private static int GetTotalConstructionCost(Entity entity)
        {
            int totalConstructionCost = 0;
            ConstructionCreditCost[] creditCosts = entity.GetComponentsInChildren<ConstructionCreditCost>(true);
            foreach (ConstructionCreditCost creditCost in creditCosts)
            {
                totalConstructionCost = checked(totalConstructionCost + creditCost.ConstructionCost);
            }

            return totalConstructionCost;
        }

        private void InjectPlayerCreditManager(GameObject createdEntity)
        {
            if (creditManager == null)
            {
                return;
            }

            TurnCreditProvider[] providers = createdEntity.GetComponentsInChildren<TurnCreditProvider>(true);
            foreach (TurnCreditProvider provider in providers)
            {
                provider.SetCreditManager(creditManager);
            }

            ConstructionCreditCost[] creditCosts = createdEntity.GetComponentsInChildren<ConstructionCreditCost>(true);
            foreach (ConstructionCreditCost creditCost in creditCosts)
            {
                creditCost.SetCreditManager(creditManager);
            }
        }
    }

    [Serializable]
    public class BuilderEvent : UnityEvent<GameObject, BuilderEventArgs> { }
}
