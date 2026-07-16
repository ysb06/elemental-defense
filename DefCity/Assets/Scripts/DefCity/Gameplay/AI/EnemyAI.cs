using UnityEngine;
using DefCity.Gameplay.City;
using DefCity.Gameplay.Combat;
using DefCity.Gameplay.Navigation;
using DefCity.Gameplay.World;

namespace DefCity.Gameplay.AI
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private Movable movable;
        [SerializeField] private TerrainCellManager terrainCellManager;
        [SerializeField] private CityCenter cityCenter;
        [SerializeField] private BaseCombatController combatController;

        private CityCenter subscribedCityCenter;

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToCityCenter();
        }

        private void OnDisable()
        {
            UnsubscribeFromCityCenter();
        }

        public void SetCityCenter(CityCenter targetCityCenter, TerrainCellManager cellManager)
        {
            UnsubscribeFromCityCenter();
            cityCenter = targetCityCenter;
            terrainCellManager = cellManager;
            ResolveReferences();
            SubscribeToCityCenter();
            RefreshTargetFromCityCenter();
        }

        public void RefreshTargetFromCityCenter()
        {
            ResolveReferences();

            if (movable == null)
            {
                Debug.LogError($"{name} requires a Movable to refresh its city center target.", this);
                return;
            }

            if (terrainCellManager == null)
            {
                Debug.LogError($"{name} requires a TerrainCellManager to refresh its city center target.", this);
                return;
            }

            if (cityCenter == null)
            {
                Debug.LogError($"{name} requires a CityCenter to refresh its target.", this);
                return;
            }

            TerrainCell targetCell = terrainCellManager.GetTerrainCell(cityCenter.CurrentPosition);
            movable.TargetCellCoordinates = new Vector2Int(targetCell.RefPosition.x, targetCell.RefPosition.y);
        }

        public void MoveToConfiguredTarget()
        {
            if (cityCenter != null)
            {
                RefreshTargetFromCityCenter();
            }

            if (movable == null)
            {
                Debug.LogError($"{name} requires a Movable to run EnemyAI.", this);
                return;
            }

            if (!movable.HasTargetCellCoordinates)
            {
                Debug.LogError($"{name} requires target cell coordinates to run EnemyAI.", this);
                return;
            }

            movable.MoveToCell();
        }

        private void OnCityCenterChanged(CityCenter changedCityCenter)
        {
            RefreshTargetFromCityCenter();

            if (Application.isPlaying && !IsAttacking())
            {
                MoveToConfiguredTarget();
            }
        }

        private bool IsAttacking()
        {
            ResolveReferences();
            return combatController != null && combatController.CurrentState == CombatState.Attacking;
        }

        private void ResolveReferences()
        {
            if (movable == null)
            {
                movable = GetComponent<Movable>();
            }

            if (combatController == null)
            {
                combatController = GetComponent<BaseCombatController>();
            }
        }

        private void SubscribeToCityCenter()
        {
            if (!isActiveAndEnabled || cityCenter == null || subscribedCityCenter == cityCenter)
            {
                return;
            }

            if (subscribedCityCenter != null)
            {
                UnsubscribeFromCityCenter();
            }

            cityCenter.CenterChanged += OnCityCenterChanged;
            subscribedCityCenter = cityCenter;
        }

        private void UnsubscribeFromCityCenter()
        {
            if (subscribedCityCenter == null)
            {
                return;
            }

            subscribedCityCenter.CenterChanged -= OnCityCenterChanged;
            subscribedCityCenter = null;
        }
    }
}
