using System;
using System.Collections.Generic;
using System.Linq;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Generation
{
    public sealed class DeterministicStageMapGenerator
    {
        public const string GeneratorVersion = "deterministic-stage-map-v1";

        private static readonly ElementType[] SupportedElements =
        {
            ElementType.Water,
            ElementType.Fire,
            ElementType.Earth,
        };

        private readonly DeterministicStageRouteGenerator routeGenerator;
        private readonly IStageElementPlacementStrategy elementPlacementStrategy;
        private readonly IStageBlockedCellPlacementStrategy blockedCellPlacementStrategy;
        private readonly StageMapValidator validator;

        public DeterministicStageMapGenerator()
            : this(
                new DeterministicStageRouteGenerator(),
                new DeterministicElementRegionPlacementStrategy(),
                new DeterministicBlockedCellNoiseStrategy(),
                new StageMapValidator())
        {
        }

        public DeterministicStageMapGenerator(
            DeterministicStageRouteGenerator routeGenerator,
            IStageElementPlacementStrategy elementPlacementStrategy,
            IStageBlockedCellPlacementStrategy blockedCellPlacementStrategy,
            StageMapValidator validator)
        {
            this.routeGenerator = routeGenerator ??
                throw new ArgumentNullException(nameof(routeGenerator));
            this.elementPlacementStrategy = elementPlacementStrategy ??
                throw new ArgumentNullException(nameof(elementPlacementStrategy));
            this.blockedCellPlacementStrategy = blockedCellPlacementStrategy ??
                throw new ArgumentNullException(nameof(blockedCellPlacementStrategy));
            this.validator = validator ??
                throw new ArgumentNullException(nameof(validator));

            EnsureStrategyIdentity(
                elementPlacementStrategy.StrategyId,
                elementPlacementStrategy.Version,
                nameof(elementPlacementStrategy));
            EnsureStrategyIdentity(
                blockedCellPlacementStrategy.StrategyId,
                blockedCellPlacementStrategy.Version,
                nameof(blockedCellPlacementStrategy));
        }

        public StageMapGenerationResult Generate(
            StageMapGenerationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            EnsureStrategyIdentity(
                elementPlacementStrategy.StrategyId,
                elementPlacementStrategy.Version,
                nameof(elementPlacementStrategy));
            EnsureStrategyIdentity(
                blockedCellPlacementStrategy.StrategyId,
                blockedCellPlacementStrategy.Version,
                nameof(blockedCellPlacementStrategy));

            StageRouteGenerationResult routeResult =
                routeGenerator.Generate(settings.RouteSettings);
            if (!routeResult.Succeeded)
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.RouteGenerationFailed,
                    $"Stage route generation failed: {routeResult.Message}");
            }

            GeneratedStageRoute route = routeResult.Route;
            HashSet<Vector2Int> roadCells = new(route.RoadCells);
            List<Vector2Int> groundCells = GetGroundCells(route, roadCells);
            long minimumRequiredGroundCellCount = Math.Max(
                (long)settings.MinimumDeployableCellCount,
                (long)settings.MinimumDeployableCellCountPerElement *
                SupportedElements.Length);
            if (groundCells.Count < minimumRequiredGroundCellCount)
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.InsufficientGroundCells,
                    $"Route leaves {groundCells.Count} elemental ground cells, but " +
                    $"at least {minimumRequiredGroundCellCount} deployable cells are required.",
                    groundCellCount: groundCells.Count);
            }

            int minimumRequiredGroundCells =
                checked((int)minimumRequiredGroundCellCount);

            StageElementPlacementContext elementContext = new(
                route.Bounds,
                route.Seed,
                groundCells,
                settings.MinimumDeployableCellCountPerElement);
            IReadOnlyDictionary<Vector2Int, ElementType> elementsByCell =
                elementPlacementStrategy.PlaceElements(elementContext);
            if (!TryValidateElementPlacement(
                    groundCells,
                    elementsByCell,
                    settings.MinimumDeployableCellCountPerElement,
                    out string elementError))
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.InvalidElementPlacement,
                    elementError,
                    groundCellCount: groundCells.Count);
            }

            int targetBlockedCellCount =
                settings.GetTargetBlockedCellCount(groundCells.Count);
            int maximumBlockedCellCount = groundCells.Count -
                minimumRequiredGroundCells;
            if (targetBlockedCellCount > maximumBlockedCellCount)
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.BlockedCellTargetNotReached,
                    $"The requested {targetBlockedCellCount} blocked cells would leave " +
                    $"fewer than {minimumRequiredGroundCells} deployable cells.",
                    groundCellCount: groundCells.Count,
                    requestedBlockedCellCount: targetBlockedCellCount);
            }

            StageBlockedCellPlacementContext blockedContext = new(
                route.Bounds,
                route.Seed,
                groundCells,
                elementsByCell,
                route.RoadCells,
                new[]
                {
                    route.Spawn.Cell,
                    route.RouteGoalCell,
                    route.HeadquartersCell,
                },
                targetBlockedCellCount,
                settings.MinimumDeployableCellCount,
                settings.MinimumDeployableCellCountPerElement,
                settings.MinimumDeployableNeighborsPerRoadCell,
                settings.EndpointProtectionRadius,
                settings.MaximumBlockedClusterSize,
                settings.MaxBlockedCellPlacementAttempts);
            IReadOnlyList<Vector2Int> selectedBlockedCells =
                blockedCellPlacementStrategy.SelectBlockedCells(blockedContext);
            if (!TryValidateBlockedCellPlacement(
                    blockedContext,
                    selectedBlockedCells,
                    out HashSet<Vector2Int> blockedCells,
                    out string blockedError))
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.InvalidBlockedCellPlacement,
                    blockedError,
                    groundCellCount: groundCells.Count,
                    requestedBlockedCellCount: targetBlockedCellCount,
                    blockedCellCount: selectedBlockedCells?.Count ?? 0);
            }

            if (blockedCells.Count != targetBlockedCellCount)
            {
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.BlockedCellTargetNotReached,
                    $"Blocked-cell noise placed {blockedCells.Count} of the requested " +
                    $"{targetBlockedCellCount} cells without violating playability constraints.",
                    groundCellCount: groundCells.Count,
                    requestedBlockedCellCount: targetBlockedCellCount,
                    blockedCellCount: blockedCells.Count);
            }

            GeneratedStageMap map = BuildMap(
                route,
                roadCells,
                elementsByCell,
                blockedCells);
            StageMapValidationRules validationRules = new(
                settings.MinimumDeployableCellCount,
                settings.MinimumDeployableCellCountPerElement,
                settings.RequireAcyclicRoutes,
                settings.RequireRoadAdjacencyMatchesGraph,
                settings.MinimumDeployableNeighborsPerRoadCell,
                settings.EndpointProtectionRadius,
                settings.MaximumBlockedClusterSize);
            StageMapValidationReport validationReport =
                validator.Validate(map, validationRules);
            if (!validationReport.IsValid)
            {
                string firstError = validationReport.Errors[0].Message;
                return StageMapGenerationResult.Failure(
                    routeResult,
                    StageMapGenerationFailureReason.ValidationFailed,
                    $"Generated stage map failed validation with " +
                    $"{validationReport.Errors.Count} error(s). First error: {firstError}",
                    groundCellCount: groundCells.Count,
                    requestedBlockedCellCount: targetBlockedCellCount,
                    blockedCellCount: blockedCells.Count,
                    validationReport: validationReport);
            }

            return StageMapGenerationResult.Success(
                map,
                routeResult,
                validationReport,
                groundCells.Count,
                targetBlockedCellCount,
                blockedCells.Count);
        }

        private GeneratedStageMap BuildMap(
            GeneratedStageRoute route,
            ISet<Vector2Int> roadCells,
            IReadOnlyDictionary<Vector2Int, ElementType> elementsByCell,
            ISet<Vector2Int> blockedCells)
        {
            MutableStageMapBuilder builder = new(
                route.Bounds,
                route.Seed,
                BuildGeneratorVersion(route),
                route.PatternId);

            for (int y = route.Bounds.yMin; y < route.Bounds.yMax; y++)
            {
                for (int x = route.Bounds.xMin; x < route.Bounds.xMax; x++)
                {
                    Vector2Int cell = new(x, y);
                    StageMapCell mapCell;
                    if (roadCells.Contains(cell))
                    {
                        StageCellMarker marker = StageCellMarker.None;
                        if (cell == route.Spawn.Cell)
                        {
                            marker = StageCellMarker.Spawn;
                        }
                        else if (cell == route.RouteGoalCell)
                        {
                            marker = StageCellMarker.RouteGoal;
                        }

                        mapCell = new StageMapCell(
                            StageTerrainKind.Road,
                            ElementType.Neutral,
                            marker);
                    }
                    else if (cell == route.HeadquartersCell)
                    {
                        mapCell = new StageMapCell(
                            StageTerrainKind.Object,
                            ElementType.Neutral,
                            StageCellMarker.Headquarters);
                    }
                    else
                    {
                        ElementType element = elementsByCell[cell];
                        StageTerrainKind terrain = blockedCells.Contains(cell)
                            ? StageTerrainKind.Object
                            : StageTerrainKind.Deployable;
                        mapCell = new StageMapCell(terrain, element);
                    }

                    builder.SetCell(cell, mapCell);
                }
            }

            builder.AddSpawn(route.Spawn);
            builder.SetHeadquarters(route.HeadquartersCell);
            builder.SetRouteGoal(route.RouteGoalCell);
            builder.SetRouteGraph(route.RouteGraph);
            return builder.Freeze();
        }

        private string BuildGeneratorVersion(GeneratedStageRoute route)
        {
            return $"{GeneratorVersion}|route={route.GeneratorVersion}/" +
                   $"{route.StrategyId}@{route.StrategyVersion}|" +
                   $"elements={elementPlacementStrategy.StrategyId}@" +
                   $"{elementPlacementStrategy.Version}|blocked=" +
                   $"{blockedCellPlacementStrategy.StrategyId}@" +
                   $"{blockedCellPlacementStrategy.Version}";
        }

        private static List<Vector2Int> GetGroundCells(
            GeneratedStageRoute route,
            ISet<Vector2Int> roadCells)
        {
            List<Vector2Int> cells = new();
            for (int y = route.Bounds.yMin; y < route.Bounds.yMax; y++)
            {
                for (int x = route.Bounds.xMin; x < route.Bounds.xMax; x++)
                {
                    Vector2Int cell = new(x, y);
                    if (!roadCells.Contains(cell) &&
                        cell != route.HeadquartersCell)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells;
        }

        private static bool TryValidateElementPlacement(
            IReadOnlyList<Vector2Int> expectedCells,
            IReadOnlyDictionary<Vector2Int, ElementType> elementsByCell,
            int minimumCountPerElement,
            out string error)
        {
            error = null;
            if (elementsByCell == null)
            {
                error = "The element placement strategy returned null.";
                return false;
            }

            if (elementsByCell.Count != expectedCells.Count)
            {
                error = $"Element placement returned {elementsByCell.Count} cells; " +
                        $"{expectedCells.Count} were expected.";
                return false;
            }

            HashSet<Vector2Int> expected = new(expectedCells);
            Dictionary<ElementType, int> counts = SupportedElements.ToDictionary(
                element => element,
                _ => 0);
            foreach (KeyValuePair<Vector2Int, ElementType> entry in elementsByCell)
            {
                if (!expected.Contains(entry.Key))
                {
                    error = $"Element placement contains unexpected cell {entry.Key}.";
                    return false;
                }

                if (!counts.ContainsKey(entry.Value))
                {
                    error = $"Element placement uses unsupported element " +
                            $"{entry.Value} at {entry.Key}.";
                    return false;
                }

                counts[entry.Value]++;
            }

            foreach (ElementType element in SupportedElements)
            {
                if (counts[element] < minimumCountPerElement)
                {
                    error = $"Element placement contains {counts[element]} {element} " +
                            $"cells; {minimumCountPerElement} are required.";
                    return false;
                }
            }

            return true;
        }

        private static bool TryValidateBlockedCellPlacement(
            StageBlockedCellPlacementContext context,
            IReadOnlyList<Vector2Int> selectedCells,
            out HashSet<Vector2Int> blockedCells,
            out string error)
        {
            blockedCells = new HashSet<Vector2Int>();
            error = null;
            if (selectedCells == null)
            {
                error = "The blocked-cell placement strategy returned null.";
                return false;
            }

            HashSet<Vector2Int> candidates = new(context.CandidateCells);
            foreach (Vector2Int cell in selectedCells)
            {
                if (!candidates.Contains(cell))
                {
                    error = $"Blocked-cell placement contains non-ground cell {cell}.";
                    return false;
                }

                if (!blockedCells.Add(cell))
                {
                    error = $"Blocked-cell placement duplicates cell {cell}.";
                    return false;
                }
            }

            if (blockedCells.Count > context.TargetBlockedCellCount)
            {
                error = $"Blocked-cell placement returned {blockedCells.Count} cells; " +
                        $"the target is {context.TargetBlockedCellCount}.";
                return false;
            }

            if (!SatisfiesEndpointProtection(context, blockedCells, out error) ||
                !SatisfiesMaximumBlockedClusterSize(
                    context,
                    blockedCells,
                    out error))
            {
                return false;
            }

            int deployableCount = context.CandidateCells.Count - blockedCells.Count;
            if (deployableCount < context.MinimumDeployableCellCount)
            {
                error = $"Blocked-cell placement leaves only {deployableCount} " +
                        "deployable cells.";
                return false;
            }

            foreach (ElementType element in SupportedElements)
            {
                int count = 0;
                foreach (Vector2Int cell in context.CandidateCells)
                {
                    if (!blockedCells.Contains(cell) &&
                        context.ElementsByCell[cell] == element)
                    {
                        count++;
                    }
                }

                if (count < context.MinimumDeployableCellCountPerElement)
                {
                    error = $"Blocked-cell placement leaves only {count} deployable " +
                            $"{element} cells.";
                    return false;
                }
            }

            if (!HasRequiredRoadNeighbors(context, blockedCells, out error))
            {
                return false;
            }

            return true;
        }

        private static bool SatisfiesEndpointProtection(
            StageBlockedCellPlacementContext context,
            IEnumerable<Vector2Int> blockedCells,
            out string error)
        {
            error = null;
            foreach (Vector2Int blockedCell in blockedCells)
            {
                foreach (Vector2Int endpoint in context.EndpointCells)
                {
                    long distance = Math.Abs((long)blockedCell.x - endpoint.x) +
                                    Math.Abs((long)blockedCell.y - endpoint.y);
                    if (distance <= context.EndpointProtectionRadius)
                    {
                        error = $"Blocked cell {blockedCell} is inside the " +
                                $"protection radius of endpoint {endpoint}.";
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool SatisfiesMaximumBlockedClusterSize(
            StageBlockedCellPlacementContext context,
            ISet<Vector2Int> blockedCells,
            out string error)
        {
            error = null;
            HashSet<Vector2Int> visited = new();
            foreach (Vector2Int start in blockedCells)
            {
                if (!visited.Add(start))
                {
                    continue;
                }

                int clusterSize = 0;
                Queue<Vector2Int> pending = new();
                pending.Enqueue(start);
                while (pending.Count > 0)
                {
                    Vector2Int cell = pending.Dequeue();
                    clusterSize++;
                    foreach (Vector2Int offset in CardinalOffsets)
                    {
                        Vector2Int neighbor = cell + offset;
                        if (blockedCells.Contains(neighbor) &&
                            visited.Add(neighbor))
                        {
                            pending.Enqueue(neighbor);
                        }
                    }
                }

                if (clusterSize > context.MaximumBlockedClusterSize)
                {
                    error = $"Blocked-cell cluster at {start} contains " +
                            $"{clusterSize} cells; at most " +
                            $"{context.MaximumBlockedClusterSize} are allowed.";
                    return false;
                }
            }

            return true;
        }

        private static bool HasRequiredRoadNeighbors(
            StageBlockedCellPlacementContext context,
            ISet<Vector2Int> blockedCells,
            out string error)
        {
            error = null;
            HashSet<Vector2Int> candidates = new(context.CandidateCells);
            foreach (Vector2Int roadCell in context.RoadCells)
            {
                int groundNeighborCount = 0;
                int deployableNeighborCount = 0;
                foreach (Vector2Int offset in CardinalOffsets)
                {
                    Vector2Int neighbor = roadCell + offset;
                    if (!candidates.Contains(neighbor))
                    {
                        continue;
                    }

                    groundNeighborCount++;
                    if (!blockedCells.Contains(neighbor))
                    {
                        deployableNeighborCount++;
                    }
                }

                int required = Math.Min(
                    context.MinimumDeployableNeighborsPerRoadCell,
                    groundNeighborCount);
                if (deployableNeighborCount < required)
                {
                    error = $"Road cell {roadCell} has {deployableNeighborCount} " +
                            $"deployable ground neighbors; {required} are required.";
                    return false;
                }
            }

            return true;
        }

        private static void EnsureStrategyIdentity(
            string strategyId,
            string version,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(strategyId) ||
                string.IsNullOrWhiteSpace(version))
            {
                throw new ArgumentException(
                    "A strategy requires a non-empty ID and version.",
                    parameterName);
            }
        }

        private static readonly Vector2Int[] CardinalOffsets =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };
    }
}
