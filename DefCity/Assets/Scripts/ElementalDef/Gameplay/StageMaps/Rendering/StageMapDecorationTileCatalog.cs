using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using ElementalDef.Gameplay.StageMaps.Decoration;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    [CreateAssetMenu(
        fileName = "ElementalDef Stage Decoration Tile Catalog",
        menuName = "ElementalDef/Stage Maps/Decoration Tile Catalog")]
    public sealed class StageMapDecorationTileCatalog : ScriptableObject
    {
        public const int ElementVariantCount = 4;

        [Header("Ground Decoration Tiles")]
        [SerializeField]
        private RuleTile[] neutralDecorationTiles =
            new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] waterDecorationTiles =
            new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] fireDecorationTiles =
            new RuleTile[ElementVariantCount];

        [SerializeField]
        private RuleTile[] earthDecorationTiles =
            new RuleTile[ElementVariantCount];

        [Header("Boundary Wall")]
        [SerializeField]
        private RuleTile boundaryWallTile;

        private enum TileGroup
        {
            NeutralDecoration,
            WaterDecoration,
            FireDecoration,
            EarthDecoration,
            BoundaryWall,
        }

        public int GetVariantCount(StageDecorationCellEntry entry)
        {
            return GetTileGroup(entry) == TileGroup.BoundaryWall
                ? 1
                : ElementVariantCount;
        }

        public TileBase GetTileVariant(
            StageDecorationCellEntry entry,
            int variantIndex)
        {
            TileGroup group = GetTileGroup(entry);
            int variantCount = group == TileGroup.BoundaryWall
                ? 1
                : ElementVariantCount;

            if (variantIndex < 0 || variantIndex >= variantCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(variantIndex),
                    variantIndex,
                    $"{group} requires a variant index from 0 through " +
                    $"{variantCount - 1}.");
            }

            if (group == TileGroup.BoundaryWall)
            {
                return EnsureTileAssigned(boundaryWallTile, "Boundary Wall");
            }

            RuleTile[] variants = GetRequiredVariantGroup(group);
            RuleTile tile = variants[variantIndex];
            if (tile == null)
            {
                throw new InvalidOperationException(
                    $"{name} has no tile assigned for {group} variant " +
                    $"{variantIndex}.");
            }

            return tile;
        }

        public IReadOnlyList<string> GetValidationErrors()
        {
            List<string> errors = new();

            ValidateVariantGroup(
                "Neutral Decoration",
                neutralDecorationTiles,
                ElementType.Neutral,
                errors);
            ValidateVariantGroup(
                "Water Decoration",
                waterDecorationTiles,
                ElementType.Water,
                errors);
            ValidateVariantGroup(
                "Fire Decoration",
                fireDecorationTiles,
                ElementType.Fire,
                errors);
            ValidateVariantGroup(
                "Earth Decoration",
                earthDecorationTiles,
                ElementType.Earth,
                errors);
            ValidateRuleTile(
                "Boundary Wall",
                boundaryWallTile,
                expectedElement: null,
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

            throw new InvalidOperationException(
                $"{name} has {errors.Count} decoration tile catalog error(s):" +
                Environment.NewLine + "- " +
                string.Join(Environment.NewLine + "- ", errors));
        }

        private static TileGroup GetTileGroup(
            StageDecorationCellEntry entry)
        {
            switch (entry.Kind)
            {
                case StageDecorationCellKind.ElementalGround:
                    switch (entry.Element)
                    {
                        case ElementType.Neutral:
                            return TileGroup.NeutralDecoration;

                        case ElementType.Water:
                            return TileGroup.WaterDecoration;

                        case ElementType.Fire:
                            return TileGroup.FireDecoration;

                        case ElementType.Earth:
                            return TileGroup.EarthDecoration;

                        default:
                            throw CreateUnsupportedCellException(entry);
                    }

                case StageDecorationCellKind.BoundaryWall:
                    if (entry.Element == ElementType.Neutral)
                    {
                        return TileGroup.BoundaryWall;
                    }

                    throw CreateUnsupportedCellException(entry);

                default:
                    throw CreateUnsupportedCellException(entry);
            }
        }

        private RuleTile[] GetRequiredVariantGroup(TileGroup group)
        {
            RuleTile[] variants;
            switch (group)
            {
                case TileGroup.NeutralDecoration:
                    variants = neutralDecorationTiles;
                    break;

                case TileGroup.WaterDecoration:
                    variants = waterDecorationTiles;
                    break;

                case TileGroup.FireDecoration:
                    variants = fireDecorationTiles;
                    break;

                case TileGroup.EarthDecoration:
                    variants = earthDecorationTiles;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(group),
                        group,
                        "The requested decoration tile group has no variants.");
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

        private static RuleTile EnsureTileAssigned(
            RuleTile tile,
            string slotName)
        {
            if (tile == null)
            {
                throw new InvalidOperationException(
                    $"No RuleTile is assigned to the {slotName} slot.");
            }

            return tile;
        }

        private static ArgumentException CreateUnsupportedCellException(
            StageDecorationCellEntry entry)
        {
            return new ArgumentException(
                $"No decoration tile group supports kind {entry.Kind} and " +
                $"element {entry.Element}.",
                nameof(entry));
        }

        private static void ValidateVariantGroup(
            string groupName,
            RuleTile[] tiles,
            ElementType expectedElement,
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
                ValidateRuleTile(
                    $"{groupName} variant {index}",
                    tiles[index],
                    expectedElement,
                    errors);
            }
        }

        private static void ValidateRuleTile(
            string slotName,
            RuleTile tile,
            ElementType? expectedElement,
            ICollection<string> errors)
        {
            if (tile == null)
            {
                errors.Add($"{slotName} has no RuleTile assigned.");
                return;
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
                errors.Add(
                    $"{slotName} RuleTile '{tile.name}' has a null rule list.");
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
                        $"{slotName} RuleTile '{tile.name}' contains a null " +
                        $"rule at index {ruleIndex}.");
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
            ElementType? expectedElement,
            ISet<GameObject> validatedObjects,
            ICollection<string> errors)
        {
            if (tileObject == null)
            {
                errors.Add(
                    $"{slotName} has no GameObject for its {outputName}.");
                return;
            }

            if (!validatedObjects.Add(tileObject))
            {
                return;
            }

            if (expectedElement.HasValue)
            {
                ElementalTile elementalTile =
                    tileObject.GetComponent<ElementalTile>();
                if (elementalTile == null)
                {
                    errors.Add(
                        $"{slotName} GameObject '{tileObject.name}' has no " +
                        $"{nameof(ElementalTile)} on its root.");
                }
                else if (elementalTile.ElementType != expectedElement.Value)
                {
                    errors.Add(
                        $"{slotName} GameObject '{tileObject.name}' uses " +
                        $"element {elementalTile.ElementType}; " +
                        $"{expectedElement.Value} is required.");
                }
            }

            Collider[] colliders = tileObject.GetComponentsInChildren<Collider>(
                includeInactive: true);
            bool hasEnabledCollider = false;
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider != null &&
                    collider.enabled &&
                    IsActiveInPrefabHierarchy(
                        collider.transform,
                        tileObject.transform))
                {
                    hasEnabledCollider = true;
                    break;
                }
            }

            if (!hasEnabledCollider)
            {
                errors.Add(
                    $"{slotName} GameObject '{tileObject.name}' has no " +
                    "enabled Collider on an active object in its hierarchy.");
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
