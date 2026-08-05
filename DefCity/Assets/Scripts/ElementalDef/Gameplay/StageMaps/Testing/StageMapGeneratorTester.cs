using System;
using System.Diagnostics;
using ElementalDef.Gameplay.StageMaps.Generation;
using ElementalDef.Gameplay.StageMaps.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Testing
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ElementalDef/Testing/Stage Map Generator Tester")]
    public sealed class StageMapGeneratorTester : MonoBehaviour
    {
        private const string PreviewSpawnId = "preview-spawn";

        [Header("Preview Coordinate Space")]
        [SerializeField]
        private Tilemap groundTilemap;

        [SerializeField]
        private Vector2Int mapOrigin = Vector2Int.zero;

        [SerializeField, Min(1)]
        private int width = 12;

        [SerializeField, Min(1)]
        private int height = 10;

        [Header("Tilemap Rendering")]
        [SerializeField]
        private StageMapTileCatalog tileCatalog;

        [Header("Route Generation")]
        [SerializeField]
        private int seed = 12345;

        [SerializeField, Range(1, 5)]
        private int patternCount = 3;

        [SerializeField, Min(0)]
        private int centerBandRadius;

        [SerializeField]
        private StageRoutePatternKinds allowedPatternKinds =
            StageRoutePatternKinds.All;

        [Header("Route Search Limits")]
        [SerializeField, Min(1)]
        private int maxPhysicalLayoutDraws =
            StageRouteGenerationSettings.DefaultMaxPhysicalLayoutDraws;

        [SerializeField, Min(1)]
        private int maxPhysicalLayoutCount =
            StageRouteGenerationSettings.DefaultMaxPhysicalLayoutCount;

        [SerializeField, Min(1)]
        private int orderVariantsPerPhysicalLayout =
            StageRouteGenerationSettings.DefaultOrderVariantsPerPhysicalLayout;

        [SerializeField, Min(1)]
        private int maxRouteCandidateCount =
            StageRouteGenerationSettings.DefaultMaxRouteCandidateCount;

        [SerializeField, Min(1)]
        private int maxSearchWorkPerCandidate =
            StageRouteGenerationSettings.DefaultMaxSearchWorkPerCandidate;

        [SerializeField, Min(1)]
        private int maxTotalSearchWork =
            StageRouteGenerationSettings.DefaultMaxTotalSearchWork;

        [SerializeField, Min(1)]
        private int maxConnectorAlternatives =
            StageRouteGenerationSettings.DefaultMaxConnectorAlternatives;

        [SerializeField, Min(0)]
        private int connectorDetourAllowance =
            StageRouteGenerationSettings.DefaultConnectorDetourAllowance;

        [Header("Manual Endpoints")]
        [SerializeField]
        private Vector2Int spawnCell = new(0, 1);

        [SerializeField]
        private Vector2Int routeGoalCell = new(1, 6);

        [FormerlySerializedAs("headquartersCell")]
        [SerializeField]
        private Vector2Int headquartersOrigin = new(0, 6);

        [SerializeField]
        private Vector2Int headquartersSize = Vector2Int.one;

        [Header("Elemental Ground And Blocking")]
        [SerializeField, Range(0f, 1f)]
        private float blockedCellRatio =
            (float)StageMapGenerationSettings.DefaultBlockedCellRatio;

        [SerializeField, Min(0)]
        private int minimumDeployableCellCount =
            StageMapGenerationSettings.DefaultMinimumDeployableCellCount;

        [SerializeField, Min(0)]
        private int minimumDeployableCellCountPerElement =
            StageMapGenerationSettings
                .DefaultMinimumDeployableCellCountPerElement;

        [SerializeField, Range(0, 4)]
        private int minimumDeployableNeighborsPerRoadCell =
            StageMapGenerationSettings
                .DefaultMinimumDeployableNeighborsPerRoadCell;

        [SerializeField, Min(0)]
        private int endpointProtectionRadius =
            StageMapGenerationSettings.DefaultEndpointProtectionRadius;

        [SerializeField, Min(1)]
        private int maximumBlockedClusterSize =
            StageMapGenerationSettings.DefaultMaximumBlockedClusterSize;

        [SerializeField, Min(1)]
        private int maxBlockedCellPlacementAttempts =
            StageMapGenerationSettings.DefaultMaxBlockedCellPlacementAttempts;

        [NonSerialized]
        private GeneratedStageMap previewMap;

        [NonSerialized]
        private StageMapGenerationResult lastResult;

        [NonSerialized]
        private string lastMessage = string.Empty;

        [NonSerialized]
        private bool hasGenerationTiming;

        [NonSerialized]
        private double lastGenerationElapsedMilliseconds;

        [NonSerialized]
        private string lastTilemapMessage = string.Empty;

        [NonSerialized]
        private bool lastTilemapOperationSucceeded;

        public Tilemap GroundTilemap => groundTilemap;
        public StageMapTileCatalog TileCatalog => tileCatalog;
        public RectInt Bounds => new(
            mapOrigin,
            new Vector2Int(width, height));
        public RectInt HeadquartersFootprint => new(
            headquartersOrigin,
            headquartersSize);
        public bool HasPreview => previewMap != null;
        public GeneratedStageMap PreviewMap => previewMap;
        public GeneratedStageRoute PreviewRoute =>
            previewMap == null ? null : lastResult?.RouteResult?.Route;
        public StageMapGenerationResult LastResult => lastResult;
        public string LastMessage => lastMessage;
        public bool HasGenerationTiming => hasGenerationTiming;
        public double LastGenerationElapsedMilliseconds =>
            lastGenerationElapsedMilliseconds;
        public string LastTilemapMessage => lastTilemapMessage;
        public bool LastTilemapOperationSucceeded =>
            lastTilemapOperationSucceeded;

        public bool GeneratePreview()
        {
            ClearPreview();

            if (groundTilemap == null)
            {
                lastMessage =
                    "Ground Tilemap must be assigned before generating a stage map preview.";
                return false;
            }

            StageMapGenerationSettings settings;
            try
            {
                StageRouteGenerationSettings routeSettings = new(
                    Bounds,
                    seed,
                    PreviewSpawnId,
                    spawnCell,
                    routeGoalCell,
                    HeadquartersFootprint,
                    patternCount,
                    centerBandRadius,
                    allowedPatternKinds,
                    maxGenerationAttempts: maxPhysicalLayoutCount,
                    maxPathSearchNodes: maxSearchWorkPerCandidate,
                    maxPhysicalLayoutDraws: maxPhysicalLayoutDraws,
                    orderVariantsPerPhysicalLayout:
                        orderVariantsPerPhysicalLayout,
                    maxRouteCandidateCount: maxRouteCandidateCount,
                    maxTotalSearchWork: maxTotalSearchWork,
                    maxConnectorAlternatives: maxConnectorAlternatives,
                    connectorDetourAllowance: connectorDetourAllowance);
                settings = new StageMapGenerationSettings(
                    routeSettings,
                    blockedCellRatio,
                    minimumDeployableCellCount,
                    minimumDeployableCellCountPerElement,
                    minimumDeployableNeighborsPerRoadCell,
                    endpointProtectionRadius,
                    maximumBlockedClusterSize,
                    maxBlockedCellPlacementAttempts);
            }
            catch (ArgumentException exception)
            {
                lastMessage = $"Invalid preview settings: {exception.Message}";
                return false;
            }

            DeterministicStageMapGenerator generator = new();
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                lastResult = generator.Generate(settings);
            }
            finally
            {
                stopwatch.Stop();
                hasGenerationTiming = true;
                lastGenerationElapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }

            if (!lastResult.Succeeded)
            {
                lastMessage =
                    $"{lastResult.FailureReason}: {lastResult.Message}";
                return false;
            }

            previewMap = lastResult.Map;
            lastMessage = lastResult.Message;
            return true;
        }

        public bool ApplyPreviewToTilemap()
        {
            ClearTilemapOperationStatus();

            if (previewMap == null)
            {
                lastTilemapMessage =
                    "A successful stage map preview is required before applying it to the Tilemap.";
                return false;
            }

            if (groundTilemap == null)
            {
                lastTilemapMessage =
                    "Ground Tilemap must be assigned before applying a stage map preview.";
                return false;
            }

            if (tileCatalog == null)
            {
                lastTilemapMessage =
                    "Stage Map Tile Catalog must be assigned before applying a stage map preview.";
                return false;
            }

            try
            {
                StageMapTilemapRenderer renderer =
                    new(groundTilemap, tileCatalog);
                renderer.Render(previewMap);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                lastTilemapMessage =
                    $"Tilemap rendering failed: {exception.Message}";
                return false;
            }

            lastTilemapOperationSucceeded = true;
            lastTilemapMessage =
                $"Rendered {previewMap.CellCount} stage map cells to " +
                $"'{groundTilemap.name}' with seed {previewMap.Seed}.";
            return true;
        }

        public bool ClearRenderedTilemap()
        {
            ClearTilemapOperationStatus();

            if (groundTilemap == null)
            {
                lastTilemapMessage =
                    "Ground Tilemap must be assigned before clearing the rendered stage map.";
                return false;
            }

            if (tileCatalog == null)
            {
                lastTilemapMessage =
                    "Stage Map Tile Catalog must be assigned before clearing the rendered stage map.";
                return false;
            }

            try
            {
                StageMapTilemapRenderer renderer =
                    new(groundTilemap, tileCatalog);
                renderer.Clear();
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                lastTilemapMessage =
                    $"Clearing the rendered Tilemap failed: {exception.Message}";
                return false;
            }

            lastTilemapOperationSucceeded = true;
            lastTilemapMessage =
                $"Cleared all stage map tiles from '{groundTilemap.name}'.";
            return true;
        }

        public void ClearPreview()
        {
            previewMap = null;
            lastResult = null;
            lastMessage = string.Empty;
            hasGenerationTiming = false;
            lastGenerationElapsedMilliseconds = 0d;
            ClearTilemapOperationStatus();
        }

        private void OnValidate()
        {
            ClearPreview();
        }

        private void ClearTilemapOperationStatus()
        {
            lastTilemapMessage = string.Empty;
            lastTilemapOperationSucceeded = false;
        }
    }
}
