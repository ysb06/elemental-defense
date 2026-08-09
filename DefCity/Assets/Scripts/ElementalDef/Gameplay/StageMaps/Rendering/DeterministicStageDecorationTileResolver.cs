using System;
using ElementalDef.Gameplay.StageMaps.Decoration;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    public sealed class DeterministicStageDecorationTileResolver
    {
        private const ulong HashDomain = 0xA0761D6478BD642FUL;

        private readonly StageMapDecorationTileCatalog catalog;

        public DeterministicStageDecorationTileResolver(
            StageMapDecorationTileCatalog catalog)
        {
            this.catalog = catalog != null
                ? catalog
                : throw new ArgumentNullException(nameof(catalog));
            catalog.ValidateOrThrow();
        }

        public int GetVariantIndex(
            int seed,
            StageDecorationCellEntry entry)
        {
            EnsureCatalogAvailable();

            int variantCount = catalog.GetVariantCount(entry);
            if (variantCount == 1)
            {
                return 0;
            }

            Vector2Int coordinates = entry.Coordinates;
            ulong hash = HashDomain;
            hash = Combine(hash, unchecked((uint)seed));
            hash = Combine(hash, unchecked((uint)coordinates.x));
            hash = Combine(hash, unchecked((uint)coordinates.y));
            hash = Combine(hash, unchecked((uint)entry.Kind));
            hash = Combine(hash, unchecked((uint)entry.Element));

            return (int)(hash % (uint)variantCount);
        }

        public TileBase ResolveTile(
            int seed,
            StageDecorationCellEntry entry)
        {
            EnsureCatalogAvailable();

            int variantIndex = GetVariantIndex(seed, entry);
            return catalog.GetTileVariant(entry, variantIndex);
        }

        private void EnsureCatalogAvailable()
        {
            if (catalog == null)
            {
                throw new InvalidOperationException(
                    "The stage decoration tile catalog is no longer available.");
            }
        }

        private static ulong Combine(ulong hash, uint value)
        {
            return Mix(hash ^ value);
        }

        private static ulong Mix(ulong value)
        {
            unchecked
            {
                value = (value ^ (value >> 30)) *
                    0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) *
                    0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }
    }
}
