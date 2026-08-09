using System;
using System.Diagnostics;
using ElementalDef.Gameplay.StageMaps.Decoration;
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
        private Tilemap decorationTilemap;

        [SerializeField]
        private Vector2Int mapOrigin = Vector2Int.zero;

        [SerializeField, Min(1)]
        private int width = 12;

        [SerializeField, Min(1)]
        private int height = 10;

        [Header("Tilemap Rendering")]
        [SerializeField]
        private StageMapTileCatalog tileCatalog;

        [SerializeField]
        private StageMapDecorationTileCatalog decorationTileCatalog;

        [Header("Decoration")]
        [SerializeField, Min(0)]
        [Tooltip(
            "Extra radius outside the play map. Applied only when Ground " +
            "Decoration is enabled.")]
        private int decorationOuterPadding =
            StageDecorationGenerationSettings.DefaultOuterPadding;

        [SerializeField]
        private bool generateGroundDecoration =
            StageDecorationGenerationSettings.DefaultGenerateGroundDecoration;

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

        [Header("Ground Types And Blocking")]
        [SerializeField, Range(0f, 1f)]
        private float blockedCellRatio =
            (float)StageMapGenerationSettings.DefaultBlockedCellRatio;

        [SerializeField, Min(0)]
        private int minimumDeployableCellCount =
            StageMapGenerationSettings.DefaultMinimumDeployableCellCount;

        [SerializeField, Min(0)]
        [InspectorName("Minimum Deployable Cell Count Per Ground Type")]
        [Tooltip(
            "Minimum deployable cells retained for each of Neutral, Water, Fire, and Earth.")]
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

        [NonSerialized]
        private bool lastTilemapOperationReachedRenderer;

        [NonSerialized]
        private GeneratedStageDecoration previewDecoration;

        [NonSerialized]
        private StageDecorationGenerationResult lastDecorationResult;

        [NonSerialized]
        private string lastDecorationMessage = string.Empty;

        [NonSerialized]
        private bool hasDecorationGenerationTiming;

        [NonSerialized]
        private double lastDecorationGenerationElapsedMilliseconds;

        public Tilemap GroundTilemap => groundTilemap;
        public Tilemap DecorationTilemap => decorationTilemap;
        public StageMapTileCatalog TileCatalog => tileCatalog;
        public StageMapDecorationTileCatalog DecorationTileCatalog =>
            decorationTileCatalog;
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
        public bool LastTilemapOperationReachedRenderer =>
            lastTilemapOperationReachedRenderer;
        public int DecorationOuterPadding => decorationOuterPadding;
        public bool GenerateGroundDecoration => generateGroundDecoration;
        public bool HasDecorationPreview => previewDecoration != null;
        public GeneratedStageDecoration PreviewDecoration =>
            previewDecoration;
        public StageDecorationGenerationResult LastDecorationResult =>
            lastDecorationResult;
        public string LastDecorationMessage => lastDecorationMessage;
        public bool HasDecorationGenerationTiming =>
            hasDecorationGenerationTiming;
        public double LastDecorationGenerationElapsedMilliseconds =>
            lastDecorationGenerationElapsedMilliseconds;

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
            GenerateDecorationPreview();
            return true;
        }

        public bool ApplyPreviewToTilemaps()
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

            if (decorationTilemap == null)
            {
                lastTilemapMessage =
                    "Decoration Tilemap must be assigned before applying a stage map preview.";
                return false;
            }

            if (tileCatalog == null)
            {
                lastTilemapMessage =
                    "Stage Map Tile Catalog must be assigned before applying a stage map preview.";
                return false;
            }

            if (previewDecoration != null &&
                decorationTileCatalog == null)
            {
                lastTilemapMessage =
                    "Stage Map Decoration Tile Catalog must be assigned before applying a decoration preview.";
                return false;
            }

            try
            {
                StageMapTilemapRenderer mapRenderer =
                    new(groundTilemap, tileCatalog);
                StageMapDecorationTilemapRenderer decorationRenderer =
                    previewDecoration == null
                        ? null
                        : new StageMapDecorationTilemapRenderer(
                            decorationTilemap,
                            decorationTileCatalog);

                // Resolve both datasets before either Tilemap is mutated so a
                // catalog or cell-contract failure cannot leave mixed output.
                mapRenderer.ValidateRender(previewMap);
                if (previewDecoration != null)
                {
                    decorationRenderer.ValidateRender(previewDecoration);
                }

                lastTilemapOperationReachedRenderer = true;
                mapRenderer.Render(previewMap);
                if (previewDecoration == null)
                {
                    ClearTilemapDirectly(decorationTilemap);
                }
                else
                {
                    decorationRenderer.Render(previewDecoration);
                }
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
            lastTilemapMessage = previewDecoration == null
                ? $"Rendered {previewMap.CellCount} stage map cells to " +
                  $"'{groundTilemap.name}' and cleared " +
                  $"'{decorationTilemap.name}' because no decoration preview " +
                  "is available."
                : $"Rendered {previewMap.CellCount} stage map cells and " +
                  $"{previewDecoration.ElementalGroundCellCount} decoration " +
                  $"ground cells plus " +
                  $"{previewDecoration.BoundaryWallCellCount} boundary wall " +
                  $"cells to " +
                  $"'{groundTilemap.name}' and '{decorationTilemap.name}' " +
                  $"with seed {previewMap.Seed}.";
            return true;
        }

        public bool ApplyPreviewToTilemap()
        {
            return ApplyPreviewToTilemaps();
        }

        public bool ClearRenderedTilemaps()
        {
            ClearTilemapOperationStatus();

            if (groundTilemap == null)
            {
                lastTilemapMessage =
                    "Ground Tilemap must be assigned before clearing rendered stage tiles.";
                return false;
            }

            if (decorationTilemap == null)
            {
                lastTilemapMessage =
                    "Decoration Tilemap must be assigned before clearing rendered stage tiles.";
                return false;
            }

            try
            {
                lastTilemapOperationReachedRenderer = true;
                ClearTilemap(
                    groundTilemap,
                    tileCatalog == null
                        ? null
                        : new StageMapTilemapRenderer(
                            groundTilemap,
                            tileCatalog));
                ClearTilemap(
                    decorationTilemap,
                    decorationTileCatalog == null
                        ? null
                        : new StageMapDecorationTilemapRenderer(
                            decorationTilemap,
                            decorationTileCatalog));
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
                $"Cleared all stage map, decoration ground, and boundary " +
                $"wall tiles from " +
                $"'{groundTilemap.name}' and '{decorationTilemap.name}'.";
            return true;
        }

        private static void ClearTilemap(
            Tilemap tilemap,
            StageMapTilemapRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.Clear();
                return;
            }

            ClearTilemapDirectly(tilemap);
        }

        private static void ClearTilemap(
            Tilemap tilemap,
            StageMapDecorationTilemapRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.Clear();
                return;
            }

            ClearTilemapDirectly(tilemap);
        }

        private static void ClearTilemapDirectly(Tilemap tilemap)
        {
            tilemap.ClearAllTiles();
            tilemap.RefreshAllTiles();
            tilemap.CompressBounds();
        }

        public bool ClearRenderedTilemap()
        {
            return ClearRenderedTilemaps();
        }

        public void ClearPreview()
        {
            previewMap = null;
            lastResult = null;
            lastMessage = string.Empty;
            hasGenerationTiming = false;
            lastGenerationElapsedMilliseconds = 0d;
            ClearDecorationPreview();
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
            lastTilemapOperationReachedRenderer = false;
        }

        private bool GenerateDecorationPreview()
        {
            ClearDecorationPreview();

            StageDecorationGenerationSettings settings;
            try
            {
                settings = new StageDecorationGenerationSettings(
                    decorationOuterPadding,
                    generateGroundDecoration);
            }
            catch (ArgumentException exception)
            {
                lastDecorationMessage =
                    $"Invalid decoration settings: {exception.Message}";
                return false;
            }

            DeterministicStageDecorationGenerator generator = new();
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                lastDecorationResult = generator.Generate(
                    previewMap,
                    settings);
            }
            finally
            {
                stopwatch.Stop();
                hasDecorationGenerationTiming = true;
                lastDecorationGenerationElapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }

            if (!lastDecorationResult.Succeeded)
            {
                lastDecorationMessage =
                    $"{lastDecorationResult.FailureReason}: " +
                    lastDecorationResult.Message;
                return false;
            }

            previewDecoration = lastDecorationResult.Decoration;
            lastDecorationMessage = lastDecorationResult.Message;
            return true;
        }

        private void ClearDecorationPreview()
        {
            previewDecoration = null;
            lastDecorationResult = null;
            lastDecorationMessage = string.Empty;
            hasDecorationGenerationTiming = false;
            lastDecorationGenerationElapsedMilliseconds = 0d;
        }
    }
}
