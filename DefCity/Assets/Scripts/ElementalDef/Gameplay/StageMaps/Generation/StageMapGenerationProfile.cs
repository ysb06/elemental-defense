using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    [CreateAssetMenu(
        fileName = "ElementalDef Stage Map Generation Profile",
        menuName = "ElementalDef/Stage Maps/Generation Profile")]
    public sealed class StageMapGenerationProfile : ScriptableObject
    {
        public const string DefaultSpawnId = "primary-spawn";

        [Header("Map Geometry")]
        [SerializeField] private Vector2Int mapOrigin = Vector2Int.zero;
        [SerializeField, Min(1)] private int width = 12;
        [SerializeField, Min(1)] private int height = 10;
        [SerializeField] private Vector2Int spawnCell = new(0, 1);
        [SerializeField] private Vector2Int routeGoalCell = new(2, 8);
        [FormerlySerializedAs("headquartersCell")]
        [SerializeField] private Vector2Int headquartersOrigin = new(2, 9);
        [SerializeField] private Vector2Int headquartersSize = Vector2Int.one;

        [Header("Route Patterns")]
        [SerializeField, Range(1, 5)]
        private int patternCount = 4;
        [SerializeField, Min(0)]
        private int centerBandRadius = 1;
        [SerializeField] private StageRoutePatternKinds allowedPatternKinds = StageRoutePatternKinds.All;

        [Header("Route Search Limits")]
        [SerializeField, Min(1)]
        private int maxPhysicalLayoutDraws = StageRouteGenerationSettings.DefaultMaxPhysicalLayoutDraws;
        [SerializeField, Min(1)]
        private int maxPhysicalLayoutCount = StageRouteGenerationSettings.DefaultMaxPhysicalLayoutCount;
        [SerializeField, Min(1)]
        private int orderVariantsPerPhysicalLayout = StageRouteGenerationSettings.DefaultOrderVariantsPerPhysicalLayout;
        [SerializeField, Min(1)]
        private int maxRouteCandidateCount = StageRouteGenerationSettings.DefaultMaxRouteCandidateCount;
        [SerializeField, Min(1)]
        private int maxSearchWorkPerCandidate = StageRouteGenerationSettings.DefaultMaxSearchWorkPerCandidate;
        [SerializeField, Min(1)]
        private int maxTotalSearchWork = StageRouteGenerationSettings.DefaultMaxTotalSearchWork;
        [SerializeField, Min(1)]
        private int maxConnectorAlternatives = StageRouteGenerationSettings.DefaultMaxConnectorAlternatives;
        [SerializeField, Min(0)]
        private int connectorDetourAllowance = StageRouteGenerationSettings.DefaultConnectorDetourAllowance;

        [Header("Elemental Ground And Blocking")]
        [SerializeField, Range(0f, 1f)]
        private float blockedCellRatio = (float)StageMapGenerationSettings.DefaultBlockedCellRatio;
        [SerializeField, Min(0)]
        private int minimumDeployableCellCount = StageMapGenerationSettings.DefaultMinimumDeployableCellCount;
        [SerializeField, Min(0)]
        private int minimumDeployableCellCountPerElement = StageMapGenerationSettings.DefaultMinimumDeployableCellCountPerElement;
        [SerializeField, Range(0, 4)]
        private int minimumDeployableNeighborsPerRoadCell = StageMapGenerationSettings.DefaultMinimumDeployableNeighborsPerRoadCell;
        [SerializeField, Min(0)]
        private int endpointProtectionRadius = StageMapGenerationSettings.DefaultEndpointProtectionRadius;
        [SerializeField, Min(1)]
        private int maximumBlockedClusterSize = StageMapGenerationSettings.DefaultMaximumBlockedClusterSize;
        [SerializeField, Min(1)]
        private int maxBlockedCellPlacementAttempts = StageMapGenerationSettings.DefaultMaxBlockedCellPlacementAttempts;

        [Header("Validation")]
        [SerializeField] private bool requireAcyclicRoutes = true;
        [SerializeField] private bool requireRoadAdjacencyMatchesGraph = true;

        public RectInt Bounds => new(mapOrigin, new Vector2Int(width, height));
        public Vector2Int SpawnCell => spawnCell;
        public Vector2Int RouteGoalCell => routeGoalCell;
        public RectInt HeadquartersFootprint =>
            new(headquartersOrigin, headquartersSize);
        public int PatternCount => patternCount;
        public int CenterBandRadius => centerBandRadius;
        public StageRoutePatternKinds AllowedPatternKinds => allowedPatternKinds;
        public double BlockedCellRatio => blockedCellRatio;

        public bool IsHeadquartersCell(Vector2Int cell)
        {
            return HeadquartersFootprint.Contains(cell);
        }

        public StageMapGenerationSettings CreateSettings(int seed)
        {
            return CreateSettings(seed, DefaultSpawnId);
        }

        public StageMapGenerationSettings CreateSettings(int seed, string spawnId)
        {
            StageRouteGenerationSettings routeSettings = new(
                Bounds,
                seed,
                spawnId,
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

            return new StageMapGenerationSettings(
                routeSettings,
                blockedCellRatio,
                minimumDeployableCellCount,
                minimumDeployableCellCountPerElement,
                minimumDeployableNeighborsPerRoadCell,
                endpointProtectionRadius,
                maximumBlockedClusterSize,
                maxBlockedCellPlacementAttempts,
                requireAcyclicRoutes,
                requireRoadAdjacencyMatchesGraph);
        }

        public void ValidateOrThrow()
        {
            try
            {
                _ = CreateSettings(seed: 0);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException ||
                exception is OverflowException)
            {
                throw new InvalidOperationException(
                    $"{name} has an invalid stage map generation configuration: " +
                    exception.Message,
                    exception);
            }
        }

        private void OnValidate()
        {
            try
            {
                allowedPatternKinds &= StageRoutePatternKinds.All;
                ValidateOrThrow();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
            }
        }
    }
}
