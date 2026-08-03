using System;
using System.Diagnostics;
using ElementalDef.Gameplay.StageMaps.Generation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ElementalDef.Gameplay.StageMaps.Testing
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ElementalDef/Testing/Stage Map Path Tester")]
    public sealed class StageMapPathTester : MonoBehaviour
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

        [NonSerialized]
        private GeneratedStageRoute previewRoute;

        [NonSerialized]
        private StageRouteGenerationResult lastResult;

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
        public bool HasPreview => previewRoute != null;
        public GeneratedStageRoute PreviewRoute => previewRoute;
        public StageRouteGenerationResult LastResult => lastResult;
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
                    "Ground Tilemap must be assigned before generating a path preview.";
                return false;
            }

            StageRouteGenerationSettings settings;
            try
            {
                settings = new StageRouteGenerationSettings(
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
            }
            catch (ArgumentException exception)
            {
                lastMessage = $"Invalid preview settings: {exception.Message}";
                return false;
            }

            DeterministicStageRouteGenerator generator = new();
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

            previewRoute = lastResult.Route;
            lastMessage = lastResult.Message;
            return true;
        }

        public void ClearPreview()
        {
            previewRoute = null;
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
