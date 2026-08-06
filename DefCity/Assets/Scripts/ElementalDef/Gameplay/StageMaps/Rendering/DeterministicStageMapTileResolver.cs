using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    public sealed class DeterministicStageMapTileResolver
    {
        private const ulong HashDomain = 0xC6BC279692B5C323UL;

        private readonly StageMapTileCatalog catalog;

        public DeterministicStageMapTileResolver(StageMapTileCatalog catalog)
        {
            this.catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            catalog.ValidateOrThrow();
        }

        public int GetVariantIndex(int seed, Vector2Int coordinates, StageMapCell cell)
        {
            EnsureCatalogAvailable();

            int variantCount = catalog.GetVariantCount(cell);
            if (variantCount == 1)
            {
                return 0;
            }

            ulong hash = HashDomain;
            hash = Combine(hash, unchecked((uint)seed));
            hash = Combine(hash, unchecked((uint)coordinates.x));
            hash = Combine(hash, unchecked((uint)coordinates.y));
            hash = Combine(hash, unchecked((uint)cell.Terrain));
            hash = Combine(hash, unchecked((uint)cell.Element));
            hash = Combine(hash, unchecked((uint)cell.Marker));

            return (int)(hash % (uint)variantCount);
        }

        public TileBase ResolveTile(int seed, Vector2Int coordinates, StageMapCell cell)
        {
            EnsureCatalogAvailable();

            int variantIndex = GetVariantIndex(seed, coordinates, cell);
            return catalog.GetTileVariant(cell, variantIndex);
        }

        private void EnsureCatalogAvailable()
        {
            if (catalog == null)
            {
                throw new InvalidOperationException("The stage map tile catalog is no longer available.");
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
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }
    }
}
