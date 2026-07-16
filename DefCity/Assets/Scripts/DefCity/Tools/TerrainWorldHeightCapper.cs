using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DefCity.Tools
{
    public class TerrainWorldHeightCapper : MonoBehaviour
    {
        [SerializeField] private Terrain targetTerrain;
        [SerializeField] private float worldHeightLimit = 27.5f;

        [ContextMenu("Run")]
        public void Run()
        {
            Terrain terrain = ResolveTerrain();
            if (terrain == null)
            {
                Debug.LogError("TerrainWorldHeightCapper: Target Terrain is not set and no Terrain component was found on this GameObject.", this);
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null)
            {
                Debug.LogError("TerrainWorldHeightCapper: TerrainData is missing.", terrain);
                return;
            }

            float terrainBaseY = terrain.GetPosition().y;
            float terrainHeight = terrainData.size.y;
            if (terrainHeight <= Mathf.Epsilon)
            {
                Debug.LogError("TerrainWorldHeightCapper: Terrain height (size.y) must be greater than 0.", terrain);
                return;
            }

            float normalizedLimit = Mathf.Clamp01((worldHeightLimit - terrainBaseY) / terrainHeight);
            if (terrainBaseY > worldHeightLimit)
            {
                Debug.LogWarning(
                    $"TerrainWorldHeightCapper: Terrain base Y ({terrainBaseY:F3}) is already above worldHeightLimit ({worldHeightLimit:F3}). " +
                    "Heights will be set to 0, but world-space height cannot go below terrain base Y.",
                    terrain);
            }

            int resolution = terrainData.heightmapResolution;
            float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

            int changedCount = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    if (heights[y, x] > normalizedLimit)
                    {
                        heights[y, x] = normalizedLimit;
                        changedCount++;
                    }
                }
            }

            if (changedCount == 0)
            {
                Debug.Log("TerrainWorldHeightCapper: No height values exceeded the limit. No changes applied.", terrain);
                return;
            }

#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(terrainData, "Clamp Terrain Height (World Y)");
#endif
            terrainData.SetHeights(0, 0, heights);
#if UNITY_EDITOR
            EditorUtility.SetDirty(terrainData);
#endif

            Debug.Log(
                $"TerrainWorldHeightCapper: Clamped {changedCount} height samples to world Y <= {worldHeightLimit:F3} " +
                $"(normalized limit: {normalizedLimit:F4}).",
                terrain);
        }

        private Terrain ResolveTerrain()
        {
            if (targetTerrain != null)
            {
                return targetTerrain;
            }

            return GetComponent<Terrain>();
        }
    }
}
