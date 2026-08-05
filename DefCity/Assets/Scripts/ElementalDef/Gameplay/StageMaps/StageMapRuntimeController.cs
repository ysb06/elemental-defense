using System;
using ElementalDef.Gameplay.Entities;
using ElementalDef.Gameplay.Flow;
using ElementalDef.Gameplay.Placement;
using ElementalDef.Gameplay.StageMaps.Generation;
using ElementalDef.Gameplay.StageMaps.Rendering;
using ElementalDef.Gameplay.World;
using ElementalDef.Runtime;
using ElementalDef.Gameplay.Flow.Settings;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Runtime
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class StageMapRuntimeController : MonoBehaviour
    {
        [Header("Map Generation")]
        [SerializeField] private StageMapGenerationProfile generationProfile;

        [Header("Map Rendering")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private StageMapTileCatalog tileCatalog;

        [Header("Runtime Bindings")]
        [SerializeField] private Tile3DCellManager tile3DCellManager;
        [SerializeField] private EnemyRoute enemyRoute;
        [SerializeField] private HeadquartersBuilding headquartersBuilding;
        [SerializeField] private TowerPlacementValidator towerPlacementValidator;
        [SerializeField] private NavMeshSurface navMeshSurface;

        [Header("Gameplay Gate")]
        [SerializeField] private WaveBundleController waveBundleController;
        [SerializeField] private TowerInteractionController towerInteractionController;

        [Header("Direct Scene Play Fallback")]
        [SerializeField] private WaveBundle directPlayFallbackStage;

        public GeneratedStageMap CurrentMap { get; private set; } = null;

        private StageRunContext stageRunContext;

        private void Start()
        {
            towerInteractionController.enabled = false;
            Initialize();
        }

        private void Initialize()
        {
            ElementalDefApplicationRoot applicationRoot = ElementalDefApplicationRoot.Instance;
            stageRunContext = applicationRoot?.StageLaunch?.Current;

            if (stageRunContext == null)
            {
                Debug.LogWarning($"No stage run context found. Using direct play fallback stage '{directPlayFallbackStage.name}'.");
                stageRunContext = applicationRoot.StageLaunch.Prepare(directPlayFallbackStage);
            }
            
            int mapSeed = 0;
            if (stageRunContext == null)
            {
                Debug.LogWarning($"No stage run context found. Using default map seed {mapSeed}.");
            }
            else
            {
                mapSeed = stageRunContext.SelectedStage.MapSeed;
            }

            StageMapGenerationSettings settings = generationProfile.CreateSettings(mapSeed);
            DeterministicStageMapGenerator generator = new();
            StageMapGenerationResult result = generator.Generate(settings);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Stage map generation failed for stage '{stageRunContext.SelectedStage.StageId}' " +
                    $"with seed {stageRunContext.SelectedStage.MapSeed}. " +
                    $"{result.FailureReason}: {result.Message}");
            }

            GeneratedStageMap generatedMap = result.Map;
            StageMapTilemapRenderer tilemapRenderer = new(groundTilemap, tileCatalog);
            tilemapRenderer.Render(generatedMap);
            Physics.SyncTransforms();

            enemyRoute.Initialize(result.RouteResult.Route.OrderedPath);

            // Sync only the headquarters position so its authored rotation and scale remain unchanged.
            RectInt headquartersFootprint = generatedMap.HeadquartersFootprint;
            if (!tile3DCellManager.TryGetSurfaceCenter(headquartersFootprint, out Vector3 headquartersPosition))
            {
                throw new InvalidOperationException($"Failed to resolve the surface center for Headquarters footprint {headquartersFootprint}.");
            }

            headquartersBuilding.transform.position = headquartersPosition;
            Physics.SyncTransforms();

            towerPlacementValidator.Initialize(generatedMap);
            navMeshSurface.BuildNavMesh();

            // CurrentMap is assigned only after the full runtime setup succeeds.
            CurrentMap = generatedMap;
        }

        public void ActivateGameplay()
        {
            waveBundleController.Initialize(stageRunContext.SelectedStage);
            towerInteractionController.enabled = true;
        }
    }
}
