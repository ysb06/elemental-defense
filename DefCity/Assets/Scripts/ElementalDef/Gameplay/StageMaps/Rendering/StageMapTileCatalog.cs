using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    [CreateAssetMenu(
        fileName = "ElementalDef Stage Map Tile Catalog",
        menuName = "ElementalDef/Stage Maps/Tile Catalog")]
    public sealed class StageMapTileCatalog : ScriptableObject
    {
        public const int ElementVariantCount = 4;

        [Header("Neutral Tiles")]
        [SerializeField]
        private RuleTile roadTile;

        [SerializeField]
        private RuleTile headquartersTile;

        [Header("Deployable Element Tiles")]
        [SerializeField]
        private RuleTile[] waterDeployableTiles = new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] fireDeployableTiles = new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] earthDeployableTiles = new RuleTile[ElementVariantCount];

        [Header("Blocked Element Tiles")]
        [SerializeField]
        private RuleTile[] waterBlockedTiles = new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] fireBlockedTiles = new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] earthBlockedTiles = new RuleTile[ElementVariantCount];

        private enum TileGroup
        {
            Road,
            Headquarters,
            WaterDeployable,
            FireDeployable,
            EarthDeployable,
            WaterBlocked,
            FireBlocked,
            EarthBlocked,
        }

        public int GetVariantCount(StageMapCell cell)
        {
            TileGroup group = GetTileGroup(cell);
            switch (group)
            {
                case TileGroup.Road:
                case TileGroup.Headquarters:
                    return 1;
                default:
                    return ElementVariantCount;
            }
        }

        public TileBase GetTileVariant(StageMapCell cell, int variantIndex)
        {
            TileGroup group = GetTileGroup(cell);
            int variantCount = group == TileGroup.Road || group == TileGroup.Headquarters ? 1 : ElementVariantCount;
            if (variantIndex < 0 || variantIndex >= variantCount)
            {
                throw new ArgumentOutOfRangeException(nameof(variantIndex), variantIndex, $"{group} requires a variant index from 0 through {variantCount - 1}.");
            }

            switch (group)
            {
                case TileGroup.Road:
                    return EnsureSingleTileAssigned(roadTile, "Road");
                case TileGroup.Headquarters:
                    return EnsureSingleTileAssigned(headquartersTile, "Headquarters");
                default:
                    RuleTile[] variants = GetRequiredVariantGroup(group);
                    RuleTile tile = variants[variantIndex];
                    if (tile == null)
                    {
                        throw new InvalidOperationException($"{name} has no tile assigned for {group} variant {variantIndex}.");
                    }

                    return tile;
            }
        }

        public IReadOnlyList<string> GetValidationErrors()
        {
            List<string> errors = new();
            Dictionary<RuleTile, string> assignedTiles = new();

            ValidateSingleTile(
                "Road",
                roadTile,
                ElementType.Neutral,
                assignedTiles,
                errors);
            ValidateSingleTile(
                "Headquarters",
                headquartersTile,
                ElementType.Neutral,
                assignedTiles,
                errors);

            ValidateVariantGroup(
                "Water Deployable",
                waterDeployableTiles,
                ElementType.Water,
                assignedTiles,
                errors);
            ValidateVariantGroup(
                "Fire Deployable",
                fireDeployableTiles,
                ElementType.Fire,
                assignedTiles,
                errors);
            ValidateVariantGroup(
                "Earth Deployable",
                earthDeployableTiles,
                ElementType.Earth,
                assignedTiles,
                errors);
            ValidateVariantGroup(
                "Water Blocked",
                waterBlockedTiles,
                ElementType.Water,
                assignedTiles,
                errors);
            ValidateVariantGroup(
                "Fire Blocked",
                fireBlockedTiles,
                ElementType.Fire,
                assignedTiles,
                errors);
            ValidateVariantGroup(
                "Earth Blocked",
                earthBlockedTiles,
                ElementType.Earth,
                assignedTiles,
                errors);

            return Array.AsReadOnly(errors.ToArray());
        }

        public void ValidateOrThrow()
        {
            IReadOnlyList<string> errors = GetValidationErrors();
            if (errors.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException($"{name} has {errors.Count} tile catalog error(s):" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", errors));
        }

        private TileGroup GetTileGroup(StageMapCell cell)
        {
            if (!cell.IsDefined)
            {
                throw new ArgumentException("A tile can only be resolved for a defined stage map cell.", nameof(cell));
            }

            switch (cell.Terrain)
            {
                case StageTerrainKind.Road:
                    return TileGroup.Road;
                case StageTerrainKind.Deployable:
                    return GetElementGroup(cell, TileGroup.WaterDeployable, TileGroup.FireDeployable, TileGroup.EarthDeployable);
                case StageTerrainKind.Object:
                    if (cell.Marker == StageCellMarker.Headquarters && cell.Element == ElementType.Neutral)
                    {
                        return TileGroup.Headquarters;
                    }

                    if (cell.Marker == StageCellMarker.None)
                    {
                        return GetElementGroup(cell, TileGroup.WaterBlocked, TileGroup.FireBlocked, TileGroup.EarthBlocked);
                    }

                    break;
            }

            throw CreateUnsupportedCellException(cell);
        }

        private static TileGroup GetElementGroup(
            StageMapCell cell,
            TileGroup waterGroup,
            TileGroup fireGroup,
            TileGroup earthGroup)
        {
            switch (cell.Element)
            {
                case ElementType.Water:
                    return waterGroup;

                case ElementType.Fire:
                    return fireGroup;

                case ElementType.Earth:
                    return earthGroup;

                default:
                    throw CreateUnsupportedCellException(cell);
            }
        }

        private RuleTile[] GetRequiredVariantGroup(TileGroup group)
        {
            RuleTile[] variants;
            switch (group)
            {
                case TileGroup.WaterDeployable:
                    variants = waterDeployableTiles;
                    break;

                case TileGroup.FireDeployable:
                    variants = fireDeployableTiles;
                    break;

                case TileGroup.EarthDeployable:
                    variants = earthDeployableTiles;
                    break;

                case TileGroup.WaterBlocked:
                    variants = waterBlockedTiles;
                    break;

                case TileGroup.FireBlocked:
                    variants = fireBlockedTiles;
                    break;

                case TileGroup.EarthBlocked:
                    variants = earthBlockedTiles;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(group),
                        group,
                        "The requested tile group does not contain variants.");
            }

            if (variants == null || variants.Length != ElementVariantCount)
            {
                int actualCount = variants?.Length ?? 0;
                throw new InvalidOperationException(
                    $"{name} requires exactly {ElementVariantCount} {group} " +
                    $"variants, but {actualCount} are configured.");
            }

            return variants;
        }

        private static RuleTile EnsureSingleTileAssigned(RuleTile tile, string slotName)
        {
            if (tile == null)
            {
                throw new InvalidOperationException($"No RuleTile is assigned to the {slotName} slot.");
            }

            return tile;
        }

        private static ArgumentException CreateUnsupportedCellException(StageMapCell cell)
        {
            return new ArgumentException($"No tile group supports terrain {cell.Terrain}, element {cell.Element}, and marker {cell.Marker}.", nameof(cell));
        }

        private static void ValidateVariantGroup(
            string groupName,
            RuleTile[] tiles,
            ElementType expectedElement,
            IDictionary<RuleTile, string> assignedTiles,
            ICollection<string> errors)
        {
            if (tiles == null)
            {
                errors.Add($"{groupName} variants are not assigned.");
                return;
            }

            if (tiles.Length != ElementVariantCount)
            {
                errors.Add(
                    $"{groupName} requires exactly {ElementVariantCount} " +
                    $"variants, but {tiles.Length} are configured.");
            }

            for (int index = 0; index < tiles.Length; index++)
            {
                ValidateSingleTile(
                    $"{groupName} variant {index}",
                    tiles[index],
                    expectedElement,
                    assignedTiles,
                    errors);
            }
        }

        private static void ValidateSingleTile(
            string slotName,
            RuleTile tile,
            ElementType expectedElement,
            IDictionary<RuleTile, string> assignedTiles,
            ICollection<string> errors)
        {
            if (tile == null)
            {
                errors.Add($"{slotName} has no RuleTile assigned.");
                return;
            }

            if (assignedTiles.TryGetValue(tile, out string existingSlot))
            {
                errors.Add(
                    $"{slotName} reuses RuleTile '{tile.name}' already assigned " +
                    $"to {existingSlot}.");
            }
            else
            {
                assignedTiles.Add(tile, slotName);
            }

            HashSet<GameObject> validatedObjects = new();
            ValidateTileGameObject(
                slotName,
                "default output",
                tile.m_DefaultGameObject,
                expectedElement,
                validatedObjects,
                errors);

            if (tile.m_TilingRules == null)
            {
                errors.Add($"{slotName} RuleTile '{tile.name}' has a null rule list.");
                return;
            }

            for (int ruleIndex = 0;
                 ruleIndex < tile.m_TilingRules.Count;
                 ruleIndex++)
            {
                RuleTile.TilingRule rule = tile.m_TilingRules[ruleIndex];
                if (rule == null)
                {
                    errors.Add(
                        $"{slotName} RuleTile '{tile.name}' contains a null rule " +
                        $"at index {ruleIndex}.");
                    continue;
                }

                ValidateTileGameObject(
                    slotName,
                    $"rule {ruleIndex} output",
                    rule.m_GameObject,
                    expectedElement,
                    validatedObjects,
                    errors);
            }
        }

        private static void ValidateTileGameObject(
            string slotName,
            string outputName,
            GameObject tileObject,
            ElementType expectedElement,
            ISet<GameObject> validatedObjects,
            ICollection<string> errors)
        {
            if (tileObject == null)
            {
                errors.Add($"{slotName} has no GameObject for its {outputName}.");
                return;
            }

            if (!validatedObjects.Add(tileObject))
            {
                return;
            }

            ElementalTile elementalTile = tileObject.GetComponent<ElementalTile>();
            if (elementalTile == null)
            {
                errors.Add(
                    $"{slotName} GameObject '{tileObject.name}' has no " +
                    $"{nameof(ElementalTile)} on its root.");
            }
            else if (elementalTile.ElementType != expectedElement)
            {
                errors.Add(
                    $"{slotName} GameObject '{tileObject.name}' uses element " +
                    $"{elementalTile.ElementType}; {expectedElement} is required.");
            }

            Collider[] colliders = tileObject.GetComponentsInChildren<Collider>(includeInactive: true);
            bool hasEnabledCollider = false;
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider != null && collider.enabled && IsActiveInPrefabHierarchy(collider.transform, tileObject.transform))
                {
                    hasEnabledCollider = true;
                    break;
                }
            }

            if (!hasEnabledCollider)
            {
                errors.Add($"{slotName} GameObject '{tileObject.name}' has no enabled Collider on an active object in its hierarchy.");
            }
        }

        private static bool IsActiveInPrefabHierarchy(
            Transform candidate,
            Transform prefabRoot)
        {
            Transform current = candidate;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    return false;
                }

                if (current == prefabRoot)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}
