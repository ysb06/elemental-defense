using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class GeneratedStageRoute
    {
        private readonly IReadOnlyList<Vector2Int> orderedPath;
        private readonly IReadOnlyList<Vector2Int> roadCells;
        private readonly IReadOnlyList<StageRoutePatternPlacement> patternPlacements;
        private readonly IReadOnlyList<StageRoutePatternPassage>
            orderedPatternPassages;

        public RectInt Bounds { get; }
        public int Seed { get; }
        public string GeneratorVersion { get; }
        public string StrategyId { get; }
        public string StrategyVersion { get; }
        public string PatternId { get; }
        public IReadOnlyList<Vector2Int> OrderedPath => orderedPath;
        public IReadOnlyList<Vector2Int> RoadCells => roadCells;
        public IReadOnlyList<StageRoutePatternPlacement> PatternPlacements =>
            patternPlacements;
        public IReadOnlyList<StageRoutePatternPassage> OrderedPatternPassages =>
            orderedPatternPassages;
        public SpawnDefinition Spawn { get; }
        public Vector2Int HeadquartersCell { get; }
        public Vector2Int RouteGoalCell { get; }
        public EnemyRouteGraph RouteGraph { get; }
        public IReadOnlyList<RouteCrossingDefinition> DisconnectedCrossings =>
            RouteGraph.DisconnectedCrossings;

        internal GeneratedStageRoute(
            StageRouteGenerationSettings settings,
            string generatorVersion,
            string strategyId,
            string strategyVersion,
            StageRoutePatternLayout layout,
            IReadOnlyList<Vector2Int> sourceOrderedPath,
            IReadOnlyList<Vector2Int> sourceRoadCells,
            SpawnDefinition spawn,
            EnemyRouteGraph routeGraph)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            EnsureRequiredText(generatorVersion, nameof(generatorVersion));
            EnsureRequiredText(strategyId, nameof(strategyId));
            EnsureRequiredText(strategyVersion, nameof(strategyVersion));

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            if (sourceOrderedPath == null || sourceOrderedPath.Count == 0)
            {
                throw new ArgumentException(
                    "A generated stage route requires an ordered path.",
                    nameof(sourceOrderedPath));
            }

            if (sourceRoadCells == null || sourceRoadCells.Count == 0)
            {
                throw new ArgumentException(
                    "A generated stage route requires Road cells.",
                    nameof(sourceRoadCells));
            }

            Vector2Int[] pathCopies = new Vector2Int[sourceOrderedPath.Count];
            for (int index = 0; index < sourceOrderedPath.Count; index++)
            {
                pathCopies[index] = sourceOrderedPath[index];
            }

            Vector2Int[] roadCopies = new Vector2Int[sourceRoadCells.Count];
            HashSet<Vector2Int> uniqueRoadCells = new();
            for (int index = 0; index < sourceRoadCells.Count; index++)
            {
                Vector2Int cell = sourceRoadCells[index];
                if (!uniqueRoadCells.Add(cell))
                {
                    throw new ArgumentException(
                        $"Road cell {cell} is duplicated.",
                        nameof(sourceRoadCells));
                }

                roadCopies[index] = cell;
            }

            Array.Sort(roadCopies, CompareCellsRowMajor);

            StageRoutePatternPlacement[] placementCopies =
                new StageRoutePatternPlacement[layout.Placements.Count];
            for (int index = 0; index < placementCopies.Length; index++)
            {
                placementCopies[index] = layout.Placements[index];
            }

            StageRoutePatternPassage[] orderedPassageCopies =
                new StageRoutePatternPassage[layout.OrderedPassages.Count];
            for (int index = 0; index < orderedPassageCopies.Length; index++)
            {
                orderedPassageCopies[index] = layout.OrderedPassages[index];
            }

            Bounds = settings.Bounds;
            Seed = settings.Seed;
            GeneratorVersion = generatorVersion;
            StrategyId = strategyId;
            StrategyVersion = strategyVersion;
            PatternId = layout.LayoutId;
            orderedPath = Array.AsReadOnly(pathCopies);
            roadCells = Array.AsReadOnly(roadCopies);
            patternPlacements = Array.AsReadOnly(placementCopies);
            orderedPatternPassages = Array.AsReadOnly(orderedPassageCopies);
            Spawn = spawn;
            HeadquartersCell = settings.HeadquartersCell;
            RouteGoalCell = settings.RouteGoalCell;
            RouteGraph = routeGraph ?? throw new ArgumentNullException(nameof(routeGraph));
        }

        private static int CompareCellsRowMajor(Vector2Int first, Vector2Int second)
        {
            int yComparison = first.y.CompareTo(second.y);
            return yComparison != 0 ? yComparison : first.x.CompareTo(second.x);
        }

        private static void EnsureRequiredText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }
        }
    }
}
