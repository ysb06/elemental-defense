using System;
using ElementalDef.Gameplay.StageMaps.Decoration;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Rendering
{
    public sealed class StageMapDecorationTilemapRenderer
    {
        private readonly Tilemap targetTilemap;
        private readonly StageMapDecorationTileCatalog tileCatalog;

        public StageMapDecorationTilemapRenderer(
            Tilemap targetTilemap,
            StageMapDecorationTileCatalog tileCatalog)
        {
            this.targetTilemap = targetTilemap != null
                ? targetTilemap
                : throw new ArgumentNullException(nameof(targetTilemap));
            this.tileCatalog = tileCatalog != null
                ? tileCatalog
                : throw new ArgumentNullException(nameof(tileCatalog));
        }

        public void Render(GeneratedStageDecoration decoration)
        {
            EnsureTargetTilemapAvailable();

            if (decoration == null)
            {
                throw new ArgumentNullException(nameof(decoration));
            }

            DeterministicStageDecorationTileResolver resolver =
                new(EnsureTileCatalogAvailable());
            TileBase[] tiles = ResolveAllTiles(decoration, resolver);
            BoundsInt tileBounds = CreateTileBounds(decoration.Bounds);

            targetTilemap.ClearAllTiles();
            targetTilemap.SetTilesBlock(tileBounds, tiles);
            targetTilemap.RefreshAllTiles();
            targetTilemap.CompressBounds();
        }

        internal void ValidateRender(
            GeneratedStageDecoration decoration)
        {
            EnsureTargetTilemapAvailable();

            if (decoration == null)
            {
                throw new ArgumentNullException(nameof(decoration));
            }

            DeterministicStageDecorationTileResolver resolver =
                new(EnsureTileCatalogAvailable());
            _ = ResolveAllTiles(decoration, resolver);
        }

        public void Clear()
        {
            EnsureTargetTilemapAvailable();

            targetTilemap.ClearAllTiles();
            targetTilemap.RefreshAllTiles();
            targetTilemap.CompressBounds();
        }

        private static TileBase[] ResolveAllTiles(
            GeneratedStageDecoration decoration,
            DeterministicStageDecorationTileResolver resolver)
        {
            RectInt bounds = decoration.Bounds;
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new InvalidOperationException(
                    $"The generated stage decoration has invalid bounds {bounds}.");
            }

            int tileCount = checked(bounds.width * bounds.height);
            TileBase[] tiles = new TileBase[tileCount];
            bool[] assignedCells = new bool[tileCount];
            int resolvedCellCount = 0;

            foreach (StageDecorationCellEntry entry in decoration.EnumerateCells())
            {
                Vector2Int coordinates = entry.Coordinates;
                if (!bounds.Contains(coordinates))
                {
                    throw new InvalidOperationException(
                        $"The generated stage decoration enumerated cell {coordinates} " +
                        $"outside its declared bounds {bounds}.");
                }

                int tileIndex = GetCellIndex(bounds, coordinates);
                if (assignedCells[tileIndex])
                {
                    throw new InvalidOperationException(
                        $"The generated stage decoration enumerated cell {coordinates} more than once.");
                }

                TileBase tile;
                try
                {
                    tile = resolver.ResolveTile(
                        decoration.Seed,
                        entry);
                }
                catch (Exception exception) when (
                    exception is ArgumentException ||
                    exception is InvalidOperationException)
                {
                    throw new InvalidOperationException(
                        $"Failed to resolve the stage decoration tile at " +
                        $"{coordinates}: {exception.Message}",
                        exception);
                }

                if (tile == null)
                {
                    throw new InvalidOperationException(
                        $"The stage decoration tile resolver returned no tile for " +
                        $"decoration cell {coordinates}.");
                }

                assignedCells[tileIndex] = true;
                tiles[tileIndex] = tile;
                resolvedCellCount++;
            }

            if (resolvedCellCount != decoration.CellCount)
            {
                throw new InvalidOperationException(
                    $"The generated stage decoration declared " +
                    $"{decoration.CellCount} cells but enumerated " +
                    $"{resolvedCellCount}.");
            }

            return tiles;
        }

        private static BoundsInt CreateTileBounds(RectInt bounds)
        {
            return new BoundsInt(
                bounds.xMin,
                bounds.yMin,
                0,
                bounds.width,
                bounds.height,
                1);
        }

        private static int GetCellIndex(RectInt bounds, Vector2Int coordinates)
        {
            int localX = coordinates.x - bounds.xMin;
            int localY = coordinates.y - bounds.yMin;
            return checked(localY * bounds.width + localX);
        }

        private void EnsureTargetTilemapAvailable()
        {
            if (targetTilemap == null)
            {
                throw new InvalidOperationException(
                    "The target stage decoration Tilemap is no longer available.");
            }
        }

        private StageMapDecorationTileCatalog EnsureTileCatalogAvailable()
        {
            if (tileCatalog == null)
            {
                throw new InvalidOperationException(
                    "The stage decoration tile catalog is no longer available.");
            }

            return tileCatalog;
        }
    }
}
