using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefCity.Gameplay.World
{
    public class TerrainCellManager : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private Grid grid;

        public TerrainCell GetTerrainCell(Vector3 worldPosition)
        {
            Vector3Int cellPosition = grid.WorldToCell(worldPosition);
            return new TerrainCell(terrain, grid, cellPosition);
        }

        public TerrainCell GetTerrainCell(Vector2Int cellPosition)
        {
            Vector3Int cellPos3D = new(cellPosition.x, cellPosition.y, 0);
            return new TerrainCell(terrain, grid, cellPos3D);
        }

        public IEnumerable<TerrainCell> EnumerateTerrainCellsInRange(Vector3 center, float range)
        {
            EnsureReferences();

            if (range < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be non-negative.");
            }

            Vector3 minWorldPosition = new(center.x - range, center.y, center.z - range);
            Vector3 maxWorldPosition = new(center.x + range, center.y, center.z + range);
            Vector3Int minCellPosition = grid.WorldToCell(minWorldPosition);
            Vector3Int maxCellPosition = grid.WorldToCell(maxWorldPosition);

            int minX = Mathf.Min(minCellPosition.x, maxCellPosition.x);
            int maxX = Mathf.Max(minCellPosition.x, maxCellPosition.x);
            int minY = Mathf.Min(minCellPosition.y, maxCellPosition.y);
            int maxY = Mathf.Max(minCellPosition.y, maxCellPosition.y);
            float sqrRange = range * range;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    TerrainCell cell = new(terrain, grid, new Vector3Int(x, y, 0));
                    if (GetHorizontalSqrDistance(cell.Center, center) <= sqrRange + Mathf.Epsilon)
                    {
                        yield return cell;
                    }
                }
            }
        }

        public IEnumerable<TerrainCell> EnumerateTerrainCellsOverlappingBounds(Bounds worldBounds)
        {
            EnsureReferences();

            Vector3Int minCellPosition = grid.WorldToCell(worldBounds.min);
            Vector3Int maxCellPosition = grid.WorldToCell(worldBounds.max);

            int minX = Mathf.Min(minCellPosition.x, maxCellPosition.x);
            int maxX = Mathf.Max(minCellPosition.x, maxCellPosition.x);
            int minY = Mathf.Min(minCellPosition.y, maxCellPosition.y);
            int maxY = Mathf.Max(minCellPosition.y, maxCellPosition.y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    TerrainCell cell = new(terrain, grid, new Vector3Int(x, y, 0));
                    if (OverlapsXz(cell, worldBounds))
                    {
                        yield return cell;
                    }
                }
            }
        }

        private void EnsureReferences()
        {
            if (terrain == null)
            {
                throw new InvalidOperationException($"{name} requires a Terrain.");
            }

            if (grid == null)
            {
                throw new InvalidOperationException($"{name} requires a Grid.");
            }
        }

        private static bool OverlapsXz(TerrainCell cell, Bounds worldBounds)
        {
            Vector3[] corners = cell.CornerWorldPositions;
            float cellMinX = corners[0].x;
            float cellMaxX = corners[0].x;
            float cellMinZ = corners[0].z;
            float cellMaxZ = corners[0].z;

            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 corner = corners[i];
                cellMinX = Mathf.Min(cellMinX, corner.x);
                cellMaxX = Mathf.Max(cellMaxX, corner.x);
                cellMinZ = Mathf.Min(cellMinZ, corner.z);
                cellMaxZ = Mathf.Max(cellMaxZ, corner.z);
            }

            return cellMinX < worldBounds.max.x
                && cellMaxX > worldBounds.min.x
                && cellMinZ < worldBounds.max.z
                && cellMaxZ > worldBounds.min.z;
        }

        private static float GetHorizontalSqrDistance(Vector3 a, Vector3 b)
        {
            float deltaX = a.x - b.x;
            float deltaZ = a.z - b.z;
            return (deltaX * deltaX) + (deltaZ * deltaZ);
        }
    }
}
