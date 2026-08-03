using System;
using System.Diagnostics;
using ElementalDef.Gameplay.StageMaps.Generation;
using UnityEngine;
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

        [SerializeField]
        private Vector2Int headquartersCell = new(0, 6);

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

        public Tilemap GroundTilemap => groundTilemap;
        public RectInt Bounds => new(
            mapOrigin,
            new Vector2Int(width, height));
        public bool HasPreview => previewMap != null;
        public GeneratedStageMap PreviewMap => previewMap;
        public GeneratedStageRoute PreviewRoute =>
            previewMap == null ? null : lastResult?.RouteResult?.Route;
        public StageMapGenerationResult LastResult => lastResult;
        public string LastMessage => lastMessage;
        public bool HasGenerationTiming => hasGenerationTiming;
        public double LastGenerationElapsedMilliseconds =>
            lastGenerationElapsedMilliseconds;

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
                    headquartersCell,
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

        public void ClearPreview()
        {
            previewMap = null;
            lastResult = null;
            lastMessage = string.Empty;
            hasGenerationTiming = false;
            lastGenerationElapsedMilliseconds = 0d;
        }

        private void OnValidate()
        {
            ClearPreview();
        }
    }
}
