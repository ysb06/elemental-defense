using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    public sealed class StageMapTilemapRenderer
    {
        private readonly Tilemap targetTilemap;
        private readonly StageMapTileCatalog tileCatalog;

        public StageMapTilemapRenderer(Tilemap targetTilemap, StageMapTileCatalog tileCatalog)
        {
            this.targetTilemap = targetTilemap != null ? targetTilemap : throw new ArgumentNullException(nameof(targetTilemap));
            this.tileCatalog = tileCatalog != null ? tileCatalog : throw new ArgumentNullException(nameof(tileCatalog));
        }

        public void Render(GeneratedStageMap map)
        {
            EnsureTargetTilemapAvailable();

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            DeterministicStageMapTileResolver resolver = new(EnsureTileCatalogAvailable());
            TileBase[] tiles = ResolveAllTiles(map, resolver);
            BoundsInt tileBounds = new(
                map.Bounds.xMin,
                map.Bounds.yMin,
                0,
                map.Bounds.width,
                map.Bounds.height,
                1);

            targetTilemap.ClearAllTiles();
            targetTilemap.SetTilesBlock(tileBounds, tiles);
            targetTilemap.RefreshAllTiles();
            targetTilemap.CompressBounds();
        }

        internal void ValidateRender(GeneratedStageMap map)
        {
            EnsureTargetTilemapAvailable();

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            DeterministicStageMapTileResolver resolver =
                new(EnsureTileCatalogAvailable());
            _ = ResolveAllTiles(map, resolver);
        }

        public void Clear()
        {
            EnsureTargetTilemapAvailable();

            targetTilemap.ClearAllTiles();
            targetTilemap.RefreshAllTiles();
            targetTilemap.CompressBounds();
        }

        private static TileBase[] ResolveAllTiles(
            GeneratedStageMap map,
            DeterministicStageMapTileResolver resolver)
        {
            TileBase[] tiles = new TileBase[map.CellCount];
            int tileIndex = 0;

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (tileIndex >= tiles.Length)
                {
                    throw new InvalidOperationException(
                        "The generated stage map enumerated more cells than its declared cell count.");
                }

                TileBase tile;
                try
                {
                    tile = resolver.ResolveTile(
                        map.Seed,
                        entry.Coordinates,
                        entry.Cell);
                }
                catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException)
                {
                    throw new InvalidOperationException($"Failed to resolve the stage map tile at {entry.Coordinates}: {exception.Message}", exception);
                }

                if (tile == null)
                {
                    throw new InvalidOperationException($"The stage map tile resolver returned no tile for cell {entry.Coordinates}.");
                }

                tiles[tileIndex] = tile;
                tileIndex++;
            }

            if (tileIndex != tiles.Length)
            {
                throw new InvalidOperationException($"The generated stage map declared {tiles.Length} cells but enumerated {tileIndex}.");
            }

            return tiles;
        }

        private void EnsureTargetTilemapAvailable()
        {
            if (targetTilemap == null)
            {
                throw new InvalidOperationException($"The target stage map Tilemap is no longer available.");
            }
        }

        private StageMapTileCatalog EnsureTileCatalogAvailable()
        {
            if (tileCatalog == null)
            {
                throw new InvalidOperationException($"The stage map tile catalog is no longer available.");
            }

            return tileCatalog;
        }
    }
}
