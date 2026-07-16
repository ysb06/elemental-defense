using UnityEngine;

namespace DefCity.Gameplay.World
{
    public readonly struct TerrainCell
    {
        private Terrain TargetTerrain { get; }
        private Grid TargetGrid { get; }
        public Vector3Int RefPosition { get; }
        public Vector2 Position2D { get { return new Vector2(RefPosition.x, RefPosition.y); } }
        public Vector3 Center { get { return TargetGrid.GetCellCenterWorld(RefPosition); } }
        public float AverageHeightmapValue
        {
            get
            {
                if (!TryGetHeightmapSampleRegion(out TerrainData terrainData, out int startX, out int startY, out int width, out int height))
                {
                    Debug.LogWarning($"Failed to get heightmap sample region for TerrainCell at RefPosition {RefPosition}");
                    return -1f;
                }

                float[,] heights = terrainData.GetHeights(startX, startY, width, height);
                float sum = 0f;
                int sampleCount = width * height;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        sum += heights[y, x];
                    }
                }

                return sampleCount > 0 ? sum / sampleCount : -2f;
            }
        }
        public float AverageWorldHeight
        {
            get
            {
                if (TargetTerrain == null)
                {
                    return 0f;
                }

                TerrainData terrainData = TargetTerrain.terrainData;
                if (terrainData == null)
                {
                    return 0f;
                }

                return TargetTerrain.GetPosition().y + (AverageHeightmapValue * terrainData.size.y);
            }
        }
        public Vector3[] CornerWorldPositions
        {
            get
            {
                Vector3Int cell = RefPosition;
                // Todo: 성능 병목 지점, 문제 될 시 수정 필요.
                return new Vector3[]
                {
                    TargetGrid.CellToWorld(cell),
                    TargetGrid.CellToWorld(cell + new Vector3Int(1, 0, 0)),
                    TargetGrid.CellToWorld(cell + new Vector3Int(1, 1, 0)),
                    TargetGrid.CellToWorld(cell + new Vector3Int(0, 1, 0)),
                };
            }
        }

        public TerrainCell(Terrain targetTerrain, Grid targetGrid, Vector3Int cellRefPosition)
        {
            TargetTerrain = targetTerrain;
            TargetGrid = targetGrid;
            RefPosition = cellRefPosition;
        }

        private bool TryGetHeightmapSampleRegion(out TerrainData terrainData, out int startX, out int startY, out int width, out int height)
        {
            terrainData = null;
            startX = 0;
            startY = 0;
            width = 0;
            height = 0;

            if (TargetTerrain == null || TargetGrid == null)
            {
                return false;
            }

            terrainData = TargetTerrain.terrainData;
            if (terrainData == null)
            {
                return false;
            }

            Vector3 terrainPosition = TargetTerrain.GetPosition();
            Vector3 terrainSize = terrainData.size;
            if (terrainSize.x <= Mathf.Epsilon || terrainSize.z <= Mathf.Epsilon)
            {
                return false;
            }

            Vector3Int cell = RefPosition;
            Vector3 corner0 = TargetGrid.CellToWorld(cell);
            Vector3 corner1 = TargetGrid.CellToWorld(cell + new Vector3Int(1, 0, 0));
            Vector3 corner2 = TargetGrid.CellToWorld(cell + new Vector3Int(1, 1, 0));
            Vector3 corner3 = TargetGrid.CellToWorld(cell + new Vector3Int(0, 1, 0));

            float minX = Mathf.Min(Mathf.Min(corner0.x, corner1.x), Mathf.Min(corner2.x, corner3.x));
            float maxX = Mathf.Max(Mathf.Max(corner0.x, corner1.x), Mathf.Max(corner2.x, corner3.x));
            float minZ = Mathf.Min(Mathf.Min(corner0.z, corner1.z), Mathf.Min(corner2.z, corner3.z));
            float maxZ = Mathf.Max(Mathf.Max(corner0.z, corner1.z), Mathf.Max(corner2.z, corner3.z));

            float terrainMinX = terrainPosition.x;
            float terrainMaxX = terrainPosition.x + terrainSize.x;
            float terrainMinZ = terrainPosition.z;
            float terrainMaxZ = terrainPosition.z + terrainSize.z;
            if (maxX < terrainMinX || minX > terrainMaxX || maxZ < terrainMinZ || minZ > terrainMaxZ)
            {
                return false;
            }

            float normalizedMinX = Mathf.Clamp01((minX - terrainMinX) / terrainSize.x);
            float normalizedMaxX = Mathf.Clamp01((maxX - terrainMinX) / terrainSize.x);
            float normalizedMinZ = Mathf.Clamp01((minZ - terrainMinZ) / terrainSize.z);
            float normalizedMaxZ = Mathf.Clamp01((maxZ - terrainMinZ) / terrainSize.z);

            int resolution = terrainData.heightmapResolution;
            if (resolution <= 0)
            {
                return false;
            }

            int maxSampleIndex = resolution - 1;
            int minSampleX = Mathf.Clamp(Mathf.FloorToInt(normalizedMinX * maxSampleIndex), 0, maxSampleIndex);
            int maxSampleX = Mathf.Clamp(Mathf.CeilToInt(normalizedMaxX * maxSampleIndex), 0, maxSampleIndex);
            int minSampleY = Mathf.Clamp(Mathf.FloorToInt(normalizedMinZ * maxSampleIndex), 0, maxSampleIndex);
            int maxSampleY = Mathf.Clamp(Mathf.CeilToInt(normalizedMaxZ * maxSampleIndex), 0, maxSampleIndex);

            startX = minSampleX;
            startY = minSampleY;
            width = maxSampleX - minSampleX + 1;
            height = maxSampleY - minSampleY + 1;
            return width > 0 && height > 0;
        }
    }
}
