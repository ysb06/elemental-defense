using System;
using DefCore.Gameplay.World;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.Combat.Settings;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Entities.Settings;
using ElementalDef.Gameplay.World;
using UnityEngine;

namespace ElementalDef.Presentation.Effect
{
    [DisallowMultipleComponent]
    public sealed class TowerTerrainEffectPresenter : MonoBehaviour
    {
        [SerializeField] private TowerUnit towerUnit;
        [SerializeField] private TerrainModifier terrainModifier;
        [SerializeField] private GameObject synergyEffectRoot;
        [SerializeField] private GameObject disadvantageEffectRoot;

        private bool isSubscribed;

        public TerrainRelationship CurrentRelationship { get; private set; } = TerrainRelationship.Neutral;

        private void Awake()
        {
            towerUnit = towerUnit != null ? towerUnit : GetComponent<TowerUnit>();
            ApplyRelationship(TerrainRelationship.Neutral);
        }

        private void OnEnable()
        {
            ApplyRelationship(TerrainRelationship.Neutral);
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ApplyRelationship(TerrainRelationship.Neutral);
        }

        public void Refresh(CellRef cell)
        {
            ApplyRelationship(TerrainRelationship.Neutral);

            if (!isActiveAndEnabled)
            {
                return;
            }

            if (synergyEffectRoot != null && synergyEffectRoot == disadvantageEffectRoot)
            {
                LogWarning(
                    "The synergy and disadvantage effect roots reference the same GameObject. " +
                    "Terrain effects will remain disabled.");
                return;
            }

            if (towerUnit == null)
            {
                LogWarning($"A {nameof(TowerUnit)} reference is required to resolve the tower element.");
                return;
            }

            TowerUnitSpec spec = towerUnit.Spec;
            if (spec == null)
            {
                LogWarning($"Tower '{towerUnit.name}' has no {nameof(TowerUnitSpec)} assigned.");
                return;
            }

            ElementType attackElement = spec.Attack.Element;
            ElementType defenseElement = spec.Defense.Element;
            if (!Enum.IsDefined(typeof(ElementType), attackElement) ||
                !Enum.IsDefined(typeof(ElementType), defenseElement))
            {
                LogWarning(
                    $"Tower '{towerUnit.name}' has an undefined attack or defense {nameof(ElementType)} value.");
                return;
            }

            if (attackElement != defenseElement)
            {
                LogWarning(
                    $"Tower '{towerUnit.name}' has mismatched attack ({attackElement}) and defense " +
                    $"({defenseElement}) elements, so its terrain relationship cannot be displayed.");
                return;
            }

            if (terrainModifier == null)
            {
                LogWarning($"A {nameof(TerrainModifier)} reference is required to resolve the terrain relationship.");
                return;
            }

            if (!cell.IsValid)
            {
                LogWarning("Cannot resolve the terrain relationship from an invalid cell reference.");
                return;
            }

            if (cell.Space is not Tile3DCellManager tileManager)
            {
                LogWarning(
                    $"Cell {cell.Coordinates} belongs to '{cell.Space.name}', which is not a " +
                    $"{nameof(Tile3DCellManager)}.");
                return;
            }

            GameObject tileInstance;
            try
            {
                tileInstance = tileManager.GetTileInstance(cell.RefCoordinates);
            }
            catch (InvalidOperationException exception)
            {
                LogWarning(
                    $"Cannot resolve the tile instance for cell {cell.Coordinates}: {exception.Message}");
                return;
            }

            if (tileInstance == null)
            {
                LogWarning($"Cell {cell.Coordinates} has no instantiated tile GameObject.");
                return;
            }

            if (!tileInstance.TryGetComponent(out ElementalTile elementalTile))
            {
                LogWarning(
                    $"Tile '{tileInstance.name}' at cell {cell.Coordinates} has no " +
                    $"{nameof(ElementalTile)} component.");
                return;
            }

            ElementType terrainElement = elementalTile.ElementType;
            if (!Enum.IsDefined(typeof(ElementType), terrainElement))
            {
                LogWarning(
                    $"Tile '{tileInstance.name}' at cell {cell.Coordinates} has an undefined " +
                    $"{nameof(ElementType)} value: {(int)terrainElement}.");
                return;
            }

            ApplyRelationship(terrainModifier.GetRelationship(attackElement, terrainElement));
        }

        private void HandleTowerDestroyed(GameObject destroyedTower)
        {
            if (towerUnit != null && destroyedTower == towerUnit.gameObject)
            {
                ApplyRelationship(TerrainRelationship.Neutral);
            }
        }

        private void Subscribe()
        {
            if (isSubscribed || towerUnit == null)
            {
                return;
            }

            towerUnit.OnDestroyed.AddListener(HandleTowerDestroyed);
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (towerUnit != null)
            {
                towerUnit.OnDestroyed.RemoveListener(HandleTowerDestroyed);
            }

            isSubscribed = false;
        }

        private void ApplyRelationship(TerrainRelationship relationship)
        {
            if (synergyEffectRoot != null && synergyEffectRoot == disadvantageEffectRoot)
            {
                CurrentRelationship = TerrainRelationship.Neutral;
                SetActiveIfNeeded(synergyEffectRoot, false);
                return;
            }

            CurrentRelationship = relationship;
            SetActiveIfNeeded(
                synergyEffectRoot,
                relationship == TerrainRelationship.Synergy);
            SetActiveIfNeeded(
                disadvantageEffectRoot,
                relationship == TerrainRelationship.Disadvantage);
        }

        private static void SetActiveIfNeeded(GameObject effectRoot, bool active)
        {
            if (effectRoot != null && effectRoot.activeSelf != active)
            {
                effectRoot.SetActive(active);
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[{name}] {nameof(TowerTerrainEffectPresenter)}: {message}", this);
        }
    }
}
