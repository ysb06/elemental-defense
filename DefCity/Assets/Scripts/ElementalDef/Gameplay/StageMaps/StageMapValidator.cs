using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public sealed class StageMapValidator
    {
        private static readonly Vector2Int[] CardinalOffsets =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        public StageMapValidationReport Validate(GeneratedStageMap map, StageMapValidationRules rules)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            List<StageMapValidationError> errors = new();

            ValidateHeadquartersFootprint(map, errors);
            ValidateCellMarkers(map, errors);
            ValidateElementCounts(map, rules, errors);
            ValidateRoadAdjacentDeployableCells(map, rules, errors);
            ValidateBlockedCellLayout(map, rules, errors);
            ValidateRouteGraph(map, rules, errors);

            return new StageMapValidationReport(errors);
        }

        private static void ValidateBlockedCellLayout(
            GeneratedStageMap map,
            StageMapValidationRules rules,
            ICollection<StageMapValidationError> errors)
        {
            List<Vector2Int> blockedCells = new();
            HashSet<Vector2Int> blockedCellSet = new();
            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (entry.Cell.Terrain == StageTerrainKind.Object &&
                    entry.Cell.Marker == StageCellMarker.None &&
                    IsElementalGround(entry.Cell))
                {
                    blockedCells.Add(entry.Coordinates);
                    blockedCellSet.Add(entry.Coordinates);
                }
            }

            if (rules.EndpointProtectionRadius > 0)
            {
                List<Vector2Int> endpoints = new();
                foreach (SpawnDefinition spawn in map.Spawns)
                {
                    endpoints.Add(spawn.Cell);
                }

                endpoints.Add(map.RouteGoalCell);
                RectInt headquartersFootprint = map.HeadquartersFootprint;
                for (int y = headquartersFootprint.yMin;
                     y < headquartersFootprint.yMax;
                     y++)
                {
                    for (int x = headquartersFootprint.xMin;
                         x < headquartersFootprint.xMax;
                         x++)
                    {
                        endpoints.Add(new Vector2Int(x, y));
                    }
                }
                foreach (Vector2Int blockedCell in blockedCells)
                {
                    foreach (Vector2Int endpoint in endpoints)
                    {
                        long distance = Math.Abs((long)blockedCell.x - endpoint.x) +
                                        Math.Abs((long)blockedCell.y - endpoint.y);
                        if (distance > rules.EndpointProtectionRadius)
                        {
                            continue;
                        }

                        AddError(
                            errors,
                            StageMapValidationErrorCode
                                .BlockedCellInsideEndpointProtectionRadius,
                            $"Blocked elemental cell {blockedCell} is inside the " +
                            $"protection radius of endpoint {endpoint}.",
                            blockedCell);
                        break;
                    }
                }
            }

            if (rules.MaximumBlockedClusterSize == 0)
            {
                return;
            }

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
                        if (blockedCellSet.Contains(neighbor) &&
                            visited.Add(neighbor))
                        {
                            pending.Enqueue(neighbor);
                        }
                    }
                }

                if (clusterSize > rules.MaximumBlockedClusterSize)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.BlockedCellClusterTooLarge,
                        $"Blocked elemental cluster at {start} contains " +
                        $"{clusterSize} cells; at most " +
                        $"{rules.MaximumBlockedClusterSize} are allowed.",
                        start);
                }
            }
        }

        private static void ValidateRoadAdjacentDeployableCells(
            GeneratedStageMap map,
            StageMapValidationRules rules,
            ICollection<StageMapValidationError> errors)
        {
            if (rules.MinimumDeployableNeighborsPerRoadCell == 0)
            {
                return;
            }

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (!entry.Cell.IsRouteCell)
                {
                    continue;
                }

                int elementalGroundNeighborCount = 0;
                int deployableNeighborCount = 0;
                foreach (Vector2Int offset in CardinalOffsets)
                {
                    Vector2Int neighbor = entry.Coordinates + offset;
                    if (!map.TryGetCell(neighbor, out StageMapCell neighborCell) ||
                        neighborCell.Marker != StageCellMarker.None ||
                        !IsElementalGround(neighborCell))
                    {
                        continue;
                    }

                    elementalGroundNeighborCount++;
                    if (neighborCell.IsDeployable)
                    {
                        deployableNeighborCount++;
                    }
                }

                int requiredCount = Math.Min(
                    rules.MinimumDeployableNeighborsPerRoadCell,
                    elementalGroundNeighborCount);
                if (deployableNeighborCount < requiredCount)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode
                            .InsufficientRoadAdjacentDeployableCells,
                        $"Road cell {entry.Coordinates} has " +
                        $"{deployableNeighborCount} deployable elemental neighbor(s); " +
                        $"{requiredCount} are required.",
                        entry.Coordinates);
                }
            }
        }

        private static bool IsElementalGround(StageMapCell cell)
        {
            if (cell.Terrain != StageTerrainKind.Deployable &&
                cell.Terrain != StageTerrainKind.Object)
            {
                return false;
            }

            return cell.Element == ElementType.Water ||
                   cell.Element == ElementType.Fire ||
                   cell.Element == ElementType.Earth;
        }

        private static void ValidateHeadquartersFootprint(
            GeneratedStageMap map,
            ICollection<StageMapValidationError> errors)
        {
            foreach (SpawnDefinition spawn in map.Spawns)
            {
                if (!map.IsHeadquartersCell(spawn.Cell))
                {
                    continue;
                }

                AddError(
                    errors,
                    StageMapValidationErrorCode.HeadquartersFootprintOverlapsSpawn,
                    $"Headquarters footprint {map.HeadquartersFootprint} overlaps " +
                    $"spawn '{spawn.Id}' at {spawn.Cell}.",
                    spawn.Cell,
                    spawn.StartNodeId);
            }

            if (map.IsHeadquartersCell(map.RouteGoalCell))
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode
                        .HeadquartersFootprintOverlapsRouteGoal,
                    $"Headquarters footprint {map.HeadquartersFootprint} overlaps " +
                    $"the route goal at {map.RouteGoalCell}.",
                    map.RouteGoalCell,
                    map.RouteGraph.GoalNodeId);
            }

            if (!IsCardinallyAdjacentToHeadquarters(
                    map,
                    map.RouteGoalCell))
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode
                        .RouteGoalNotAdjacentToHeadquarters,
                    $"Route goal {map.RouteGoalCell} must be cardinally adjacent " +
                    $"to Headquarters footprint {map.HeadquartersFootprint}.",
                    map.RouteGoalCell,
                    map.RouteGraph.GoalNodeId);
            }

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (!map.IsHeadquartersCell(entry.Coordinates) ||
                    !entry.Cell.IsRouteCell)
                {
                    continue;
                }

                AddError(
                    errors,
                    StageMapValidationErrorCode.HeadquartersFootprintOverlapsRoad,
                    $"Headquarters footprint {map.HeadquartersFootprint} overlaps " +
                    $"Road cell {entry.Coordinates}.",
                    entry.Coordinates);
            }

            foreach (RouteNode node in map.RouteGraph.Nodes)
            {
                if (!map.IsHeadquartersCell(node.Cell))
                {
                    continue;
                }

                AddError(
                    errors,
                    StageMapValidationErrorCode
                        .HeadquartersFootprintOverlapsRouteNode,
                    $"Headquarters footprint {map.HeadquartersFootprint} overlaps " +
                    $"route node {node.Id} at {node.Cell}.",
                    node.Cell,
                    node.Id);
            }
        }

        private static bool IsCardinallyAdjacentToHeadquarters(
            GeneratedStageMap map,
            Vector2Int cell)
        {
            if (map.IsHeadquartersCell(cell))
            {
                return false;
            }

            foreach (Vector2Int offset in CardinalOffsets)
            {
                if (map.IsHeadquartersCell(cell + offset))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateCellMarkers(
            GeneratedStageMap map,
            ICollection<StageMapValidationError> errors)
        {
            HashSet<Vector2Int> definedSpawnCells = new();
            foreach (SpawnDefinition spawn in map.Spawns)
            {
                definedSpawnCells.Add(spawn.Cell);
                StageMapCell spawnCell = map.GetCell(spawn.Cell);
                if (spawnCell.Marker != StageCellMarker.Spawn)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.SpawnMarkerMismatch,
                        $"Spawn '{spawn.Id}' at {spawn.Cell} does not use the Spawn marker.",
                        spawn.Cell,
                        spawn.StartNodeId);
                }
            }

            RectInt headquartersFootprint = map.HeadquartersFootprint;
            for (int y = headquartersFootprint.yMin;
                 y < headquartersFootprint.yMax;
                 y++)
            {
                for (int x = headquartersFootprint.xMin;
                     x < headquartersFootprint.xMax;
                     x++)
                {
                    Vector2Int coordinates = new(x, y);
                    StageMapCell footprintCell = map.GetCell(coordinates);
                    if (footprintCell.Marker == StageCellMarker.Headquarters)
                    {
                        continue;
                    }

                    AddError(
                        errors,
                        StageMapValidationErrorCode.HeadquartersMarkerMismatch,
                        $"Headquarters footprint cell {coordinates} does not use " +
                        "the Headquarters marker.",
                        coordinates);
                }
            }

            StageMapCell routeGoalCell = map.GetCell(map.RouteGoalCell);
            if (routeGoalCell.Marker != StageCellMarker.RouteGoal)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.RouteGoalMarkerMismatch,
                    $"Route goal cell {map.RouteGoalCell} does not use the RouteGoal marker.",
                    map.RouteGoalCell,
                    map.RouteGraph.GoalNodeId);
            }

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                switch (entry.Cell.Marker)
                {
                    case StageCellMarker.Spawn:
                        if (!definedSpawnCells.Contains(entry.Coordinates))
                        {
                            AddError(
                                errors,
                                StageMapValidationErrorCode.UnexpectedSpawnMarker,
                                $"Cell {entry.Coordinates} has a Spawn marker without a spawn definition.",
                                entry.Coordinates);
                        }

                        break;

                    case StageCellMarker.Headquarters:
                        if (!map.IsHeadquartersCell(entry.Coordinates))
                        {
                            AddError(
                                errors,
                                StageMapValidationErrorCode.UnexpectedHeadquartersMarker,
                                $"Cell {entry.Coordinates} has an unexpected Headquarters marker.",
                                entry.Coordinates);
                        }

                        break;

                    case StageCellMarker.RouteGoal:
                        if (entry.Coordinates != map.RouteGoalCell)
                        {
                            AddError(
                                errors,
                                StageMapValidationErrorCode.UnexpectedRouteGoalMarker,
                                $"Cell {entry.Coordinates} has an unexpected RouteGoal marker.",
                                entry.Coordinates);
                        }

                        break;
                }
            }
        }

        private static void ValidateElementCounts(
            GeneratedStageMap map,
            StageMapValidationRules rules,
            ICollection<StageMapValidationError> errors)
        {
            int deployableCount = 0;
            int waterCount = 0;
            int fireCount = 0;
            int earthCount = 0;

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (!entry.Cell.IsDeployable)
                {
                    continue;
                }

                deployableCount++;
                switch (entry.Cell.Element)
                {
                    case ElementType.Water:
                        waterCount++;
                        break;
                    case ElementType.Fire:
                        fireCount++;
                        break;
                    case ElementType.Earth:
                        earthCount++;
                        break;
                }
            }

            if (deployableCount < rules.MinimumDeployableCellCount)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.InsufficientDeployableCells,
                    $"Map has {deployableCount} deployable cells; " +
                    $"at least {rules.MinimumDeployableCellCount} are required.");
            }

            AddElementCountErrorIfNeeded(
                errors,
                StageMapValidationErrorCode.InsufficientWaterCells,
                "Water",
                waterCount,
                rules.MinimumCellCountPerElement);
            AddElementCountErrorIfNeeded(
                errors,
                StageMapValidationErrorCode.InsufficientFireCells,
                "Fire",
                fireCount,
                rules.MinimumCellCountPerElement);
            AddElementCountErrorIfNeeded(
                errors,
                StageMapValidationErrorCode.InsufficientEarthCells,
                "Earth",
                earthCount,
                rules.MinimumCellCountPerElement);
        }

        private static void ValidateRouteGraph(
            GeneratedStageMap map,
            StageMapValidationRules rules,
            ICollection<StageMapValidationError> errors)
        {
            EnemyRouteGraph graph = map.RouteGraph;
            HashSet<int> knownNodeIds = new();
            Dictionary<Vector2Int, List<int>> nodeIdsByCell = new();
            Dictionary<int, int> incomingEdgeCounts = new();
            Dictionary<int, List<int>> reverseEdges = new();

            foreach (RouteNode node in graph.Nodes)
            {
                knownNodeIds.Add(node.Id);
                incomingEdgeCounts.Add(node.Id, 0);
                reverseEdges.Add(node.Id, new List<int>());

                if (!nodeIdsByCell.TryGetValue(
                        node.Cell,
                        out List<int> nodeIdsAtCell))
                {
                    nodeIdsAtCell = new List<int>();
                    nodeIdsByCell.Add(node.Cell, nodeIdsAtCell);
                }

                nodeIdsAtCell.Add(node.Id);

                if (!map.Contains(node.Cell))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RouteNodeOutOfBounds,
                        $"Route node {node.Id} at {node.Cell} is outside the map bounds.",
                        node.Cell,
                        node.Id);
                    continue;
                }

                if (!map.GetCell(node.Cell).IsRouteCell)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RouteNodeNotOnRoad,
                        $"Route node {node.Id} at {node.Cell} is not on a Road cell.",
                        node.Cell,
                        node.Id);
                }
            }

            ValidateDuplicateRouteNodeCells(graph, nodeIdsByCell, errors);

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (entry.Cell.IsRouteCell &&
                    !nodeIdsByCell.ContainsKey(entry.Coordinates))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RoadCellMissingRouteNode,
                        $"Road cell {entry.Coordinates} has no route node.",
                        entry.Coordinates);
                }
            }

            foreach (RouteNode source in graph.Nodes)
            {
                IReadOnlyList<RouteNode> outgoingNodes =
                    graph.GetOutgoingNodes(source.Id);
                foreach (RouteNode target in outgoingNodes)
                {
                    incomingEdgeCounts[target.Id]++;
                    reverseEdges[target.Id].Add(source.Id);

                    Vector2Int offset = target.Cell - source.Cell;
                    int distance = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);
                    if (distance != 1)
                    {
                        AddError(
                            errors,
                            StageMapValidationErrorCode.NonCardinalRouteEdge,
                            $"Route edge {source.Id} -> {target.Id} does not connect cardinally adjacent cells.",
                            source.Cell,
                            source.Id);
                    }
                }
            }

            HashSet<string> mapSpawnIds = new(StringComparer.Ordinal);
            HashSet<int> declaredSpawnNodeIds = new();
            HashSet<int> graphSpawnNodeIds = new();
            foreach (SpawnDefinition spawn in map.Spawns)
            {
                mapSpawnIds.Add(spawn.Id);
                declaredSpawnNodeIds.Add(spawn.StartNodeId);

                if (!graph.TryGetSpawnStartNode(spawn.Id, out RouteNode graphStart))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.MissingGraphSpawn,
                        $"Spawn '{spawn.Id}' is not registered in the route graph.",
                        spawn.Cell,
                        spawn.StartNodeId);
                    continue;
                }

                graphSpawnNodeIds.Add(graphStart.Id);
                if (graphStart.Id != spawn.StartNodeId || graphStart.Cell != spawn.Cell)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.SpawnStartNodeMismatch,
                        $"Spawn '{spawn.Id}' declares route node {spawn.StartNodeId} " +
                        $"at {spawn.Cell}, but the route graph uses node " +
                        $"{graphStart.Id} at {graphStart.Cell}.",
                        spawn.Cell,
                        spawn.StartNodeId);
                }
            }

            foreach (string graphSpawnId in graph.SpawnIds)
            {
                if (!graph.TryGetSpawnStartNode(
                        graphSpawnId,
                        out RouteNode graphStart))
                {
                    continue;
                }

                graphSpawnNodeIds.Add(graphStart.Id);
                if (!mapSpawnIds.Contains(graphSpawnId))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.UnexpectedGraphSpawn,
                        $"Route graph spawn '{graphSpawnId}' has no stage map spawn definition.",
                        graphStart.Cell,
                        graphStart.Id);
                }
            }

            if (graph.GoalNode.Cell != map.RouteGoalCell)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.RouteGoalNodeMismatch,
                    $"Route graph goal node {graph.GoalNodeId} is at {graph.GoalNode.Cell}, " +
                    $"but the map route goal is {map.RouteGoalCell}.",
                    graph.GoalNode.Cell,
                    graph.GoalNodeId);
            }

            if (graph.GetOutgoingNodes(graph.GoalNodeId).Count > 0)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.GoalHasOutgoingEdge,
                    $"Route goal node {graph.GoalNodeId} has outgoing edges.",
                    graph.GoalNode.Cell,
                    graph.GoalNodeId);
            }

            HashSet<int> spawnNodeIdsToCheck = new(declaredSpawnNodeIds);
            spawnNodeIdsToCheck.UnionWith(graphSpawnNodeIds);
            foreach (int spawnNodeId in spawnNodeIdsToCheck)
            {
                if (incomingEdgeCounts.TryGetValue(spawnNodeId, out int incomingCount) &&
                    incomingCount > 0)
                {
                    RouteNode spawnNode = graph.GetNode(spawnNodeId);
                    AddError(
                        errors,
                        StageMapValidationErrorCode.SpawnHasIncomingEdge,
                        $"Spawn route node {spawnNodeId} has {incomingCount} incoming edges.",
                        spawnNode.Cell,
                        spawnNodeId);
                }
            }

            ValidateDisconnectedCrossings(
                graph,
                nodeIdsByCell,
                incomingEdgeCounts,
                reverseEdges,
                spawnNodeIdsToCheck,
                errors);

            if (rules.RequireRoadAdjacencyMatchesGraph)
            {
                ValidateRoadAdjacencyMatchesGraph(
                    map,
                    graph,
                    nodeIdsByCell,
                    errors);
            }

            foreach (RouteNode node in graph.Nodes)
            {
                int outgoingCount = graph.GetOutgoingNodes(node.Id).Count;
                int incomingCount = incomingEdgeCounts[node.Id];
                if (incomingCount == 0 && outgoingCount == 0)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.IsolatedRouteNode,
                        $"Route node {node.Id} is isolated.",
                        node.Cell,
                        node.Id);
                }

                if (node.Id != graph.GoalNodeId && outgoingCount == 0)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RouteDeadEnd,
                        $"Route node {node.Id} is a dead end that is not the route goal.",
                        node.Cell,
                        node.Id);
                }
            }

            HashSet<int> nodesReachableFromSpawn = TraverseForward(
                graph,
                graphSpawnNodeIds,
                knownNodeIds);
            HashSet<int> nodesThatReachGoal = TraverseReverse(
                graph.GoalNodeId,
                reverseEdges);

            foreach (RouteNode node in graph.Nodes)
            {
                if (!nodesReachableFromSpawn.Contains(node.Id))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RouteNodeUnreachableFromSpawn,
                        $"Route node {node.Id} cannot be reached from any map spawn.",
                        node.Cell,
                        node.Id);
                }

                if (!nodesThatReachGoal.Contains(node.Id))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.RouteNodeCannotReachGoal,
                        $"Route node {node.Id} cannot reach the route goal.",
                        node.Cell,
                        node.Id);
                }
            }

            if (rules.RequireAcyclicRoutes && !graph.IsDirectedAcyclic())
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.RouteCycleDetected,
                    "The route graph contains a directed cycle.");
            }
        }

        private static void ValidateDuplicateRouteNodeCells(
            EnemyRouteGraph graph,
            IReadOnlyDictionary<Vector2Int, List<int>> nodeIdsByCell,
            ICollection<StageMapValidationError> errors)
        {
            foreach (KeyValuePair<Vector2Int, List<int>> entry in nodeIdsByCell)
            {
                IReadOnlyList<int> nodeIds = entry.Value;
                if (nodeIds.Count <= 1)
                {
                    continue;
                }

                bool isRegisteredCrossing =
                    graph.TryGetDisconnectedCrossing(
                        entry.Key,
                        out RouteCrossingDefinition crossing) &&
                    nodeIds.Count == 2 &&
                    ContainsNodeId(nodeIds, crossing.HorizontalNodeId) &&
                    ContainsNodeId(nodeIds, crossing.VerticalNodeId);
                if (isRegisteredCrossing)
                {
                    continue;
                }

                for (int index = 1; index < nodeIds.Count; index++)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.DuplicateRouteNodeCell,
                        $"Multiple route nodes use cell {entry.Key} without a matching disconnected crossing registration.",
                        entry.Key,
                        nodeIds[index]);
                }
            }
        }

        private static void ValidateDisconnectedCrossings(
            EnemyRouteGraph graph,
            IReadOnlyDictionary<Vector2Int, List<int>> nodeIdsByCell,
            IReadOnlyDictionary<int, int> incomingEdgeCounts,
            IReadOnlyDictionary<int, List<int>> reverseEdges,
            ISet<int> spawnNodeIds,
            ICollection<StageMapValidationError> errors)
        {
            foreach (RouteCrossingDefinition crossing in graph.DisconnectedCrossings)
            {
                if (!nodeIdsByCell.TryGetValue(
                        crossing.Cell,
                        out List<int> nodeIdsAtCell) ||
                    nodeIdsAtCell.Count != 2 ||
                    !ContainsNodeId(nodeIdsAtCell, crossing.HorizontalNodeId) ||
                    !ContainsNodeId(nodeIdsAtCell, crossing.VerticalNodeId))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                        $"Disconnected crossing at {crossing.Cell} does not match exactly two registered route nodes.",
                        crossing.Cell);
                }

                RouteNode horizontalNode = graph.GetNode(crossing.HorizontalNodeId);
                RouteNode verticalNode = graph.GetNode(crossing.VerticalNodeId);
                if (horizontalNode.Cell != crossing.Cell)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                        $"Horizontal crossing node {horizontalNode.Id} is at {horizontalNode.Cell}, not {crossing.Cell}.",
                        crossing.Cell,
                        horizontalNode.Id);
                }

                if (verticalNode.Cell != crossing.Cell)
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                        $"Vertical crossing node {verticalNode.Id} is at {verticalNode.Cell}, not {crossing.Cell}.",
                        crossing.Cell,
                        verticalNode.Id);
                }

                ValidateCrossingEndpointRole(
                    graph,
                    crossing,
                    crossing.HorizontalNodeId,
                    "Horizontal",
                    spawnNodeIds,
                    errors);
                ValidateCrossingEndpointRole(
                    graph,
                    crossing,
                    crossing.VerticalNodeId,
                    "Vertical",
                    spawnNodeIds,
                    errors);

                if (graph.ContainsEdge(
                        crossing.HorizontalNodeId,
                        crossing.VerticalNodeId) ||
                    graph.ContainsEdge(
                        crossing.VerticalNodeId,
                        crossing.HorizontalNodeId))
                {
                    AddError(
                        errors,
                        StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                        $"Disconnected crossing nodes {crossing.HorizontalNodeId} and " +
                        $"{crossing.VerticalNodeId} must not have an edge between them.",
                        crossing.Cell);
                }

                ValidateCrossingPassage(
                    graph,
                    crossing,
                    crossing.HorizontalNodeId,
                    isHorizontal: true,
                    incomingEdgeCounts,
                    reverseEdges,
                    errors);
                ValidateCrossingPassage(
                    graph,
                    crossing,
                    crossing.VerticalNodeId,
                    isHorizontal: false,
                    incomingEdgeCounts,
                    reverseEdges,
                    errors);
            }
        }

        private static void ValidateCrossingEndpointRole(
            EnemyRouteGraph graph,
            RouteCrossingDefinition crossing,
            int nodeId,
            string passageName,
            ISet<int> spawnNodeIds,
            ICollection<StageMapValidationError> errors)
        {
            if (nodeId == graph.GoalNodeId || spawnNodeIds.Contains(nodeId))
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                    $"{passageName} crossing node {nodeId} cannot be a Spawn or RouteGoal node.",
                    crossing.Cell,
                    nodeId);
            }
        }

        private static void ValidateCrossingPassage(
            EnemyRouteGraph graph,
            RouteCrossingDefinition crossing,
            int nodeId,
            bool isHorizontal,
            IReadOnlyDictionary<int, int> incomingEdgeCounts,
            IReadOnlyDictionary<int, List<int>> reverseEdges,
            ICollection<StageMapValidationError> errors)
        {
            int incomingCount = incomingEdgeCounts[nodeId];
            IReadOnlyList<RouteNode> outgoingNodes = graph.GetOutgoingNodes(nodeId);
            string passageName = isHorizontal ? "Horizontal" : "Vertical";
            if (incomingCount != 1 || outgoingNodes.Count != 1)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                    $"{passageName} crossing node {nodeId} must have exactly one incoming and one outgoing edge.",
                    crossing.Cell,
                    nodeId);
                return;
            }

            int incomingNodeId = reverseEdges[nodeId][0];
            RouteNode incomingNode = graph.GetNode(incomingNodeId);
            RouteNode outgoingNode = outgoingNodes[0];
            Vector2Int incomingOffset = incomingNode.Cell - crossing.Cell;
            Vector2Int outgoingOffset = outgoingNode.Cell - crossing.Cell;

            bool isStraightPassage = isHorizontal
                ? IsOppositeHorizontalNeighbor(incomingOffset, outgoingOffset)
                : IsOppositeVerticalNeighbor(incomingOffset, outgoingOffset);
            if (!isStraightPassage)
            {
                AddError(
                    errors,
                    StageMapValidationErrorCode.InvalidDisconnectedCrossing,
                    $"{passageName} crossing node {nodeId} must connect opposite cardinal neighbors on its declared axis.",
                    crossing.Cell,
                    nodeId);
            }
        }

        private static bool IsOppositeHorizontalNeighbor(
            Vector2Int firstOffset,
            Vector2Int secondOffset)
        {
            return firstOffset.y == 0 &&
                   secondOffset.y == 0 &&
                   Mathf.Abs(firstOffset.x) == 1 &&
                   Mathf.Abs(secondOffset.x) == 1 &&
                   firstOffset.x == -secondOffset.x;
        }

        private static bool IsOppositeVerticalNeighbor(
            Vector2Int firstOffset,
            Vector2Int secondOffset)
        {
            return firstOffset.x == 0 &&
                   secondOffset.x == 0 &&
                   Mathf.Abs(firstOffset.y) == 1 &&
                   Mathf.Abs(secondOffset.y) == 1 &&
                   firstOffset.y == -secondOffset.y;
        }

        private static void ValidateRoadAdjacencyMatchesGraph(
            GeneratedStageMap map,
            EnemyRouteGraph graph,
            IReadOnlyDictionary<Vector2Int, List<int>> nodeIdsByCell,
            ICollection<StageMapValidationError> errors)
        {
            Vector2Int[] neighborOffsets =
            {
                Vector2Int.right,
                Vector2Int.up,
            };

            foreach (StageMapCellEntry entry in map.EnumerateCells())
            {
                if (!entry.Cell.IsRouteCell)
                {
                    continue;
                }

                foreach (Vector2Int offset in neighborOffsets)
                {
                    Vector2Int neighborCell = entry.Coordinates + offset;
                    if (!map.TryGetCell(neighborCell, out StageMapCell neighbor) ||
                        !neighbor.IsRouteCell)
                    {
                        continue;
                    }

                    if (HasGraphConnectionBetweenCells(
                            graph,
                            nodeIdsByCell,
                            entry.Coordinates,
                            neighborCell))
                    {
                        continue;
                    }

                    AddError(
                        errors,
                        StageMapValidationErrorCode.UnexpectedRoadAdjacency,
                        $"Adjacent Road cells {entry.Coordinates} and {neighborCell} have no route graph edge between them.",
                        entry.Coordinates);
                }
            }
        }

        private static bool HasGraphConnectionBetweenCells(
            EnemyRouteGraph graph,
            IReadOnlyDictionary<Vector2Int, List<int>> nodeIdsByCell,
            Vector2Int firstCell,
            Vector2Int secondCell)
        {
            if (!nodeIdsByCell.TryGetValue(firstCell, out List<int> firstNodeIds) ||
                !nodeIdsByCell.TryGetValue(secondCell, out List<int> secondNodeIds))
            {
                return false;
            }

            foreach (int firstNodeId in firstNodeIds)
            {
                foreach (int secondNodeId in secondNodeIds)
                {
                    if (graph.ContainsEdge(firstNodeId, secondNodeId) ||
                        graph.ContainsEdge(secondNodeId, firstNodeId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ContainsNodeId(
            IReadOnlyList<int> nodeIds,
            int expectedNodeId)
        {
            for (int index = 0; index < nodeIds.Count; index++)
            {
                if (nodeIds[index] == expectedNodeId)
                {
                    return true;
                }
            }

            return false;
        }

        private static HashSet<int> TraverseForward(
            EnemyRouteGraph graph,
            IEnumerable<int> startNodeIds,
            ISet<int> knownNodeIds)
        {
            HashSet<int> visited = new();
            Queue<int> pending = new();

            foreach (int startNodeId in startNodeIds)
            {
                if (knownNodeIds.Contains(startNodeId) && visited.Add(startNodeId))
                {
                    pending.Enqueue(startNodeId);
                }
            }

            while (pending.Count > 0)
            {
                int nodeId = pending.Dequeue();
                foreach (RouteNode outgoingNode in graph.GetOutgoingNodes(nodeId))
                {
                    if (visited.Add(outgoingNode.Id))
                    {
                        pending.Enqueue(outgoingNode.Id);
                    }
                }
            }

            return visited;
        }

        private static HashSet<int> TraverseReverse(
            int goalNodeId,
            IReadOnlyDictionary<int, List<int>> reverseEdges)
        {
            HashSet<int> visited = new();
            Queue<int> pending = new();
            if (reverseEdges.ContainsKey(goalNodeId))
            {
                visited.Add(goalNodeId);
                pending.Enqueue(goalNodeId);
            }

            while (pending.Count > 0)
            {
                int nodeId = pending.Dequeue();
                foreach (int predecessorId in reverseEdges[nodeId])
                {
                    if (visited.Add(predecessorId))
                    {
                        pending.Enqueue(predecessorId);
                    }
                }
            }

            return visited;
        }

        private static void AddElementCountErrorIfNeeded(
            ICollection<StageMapValidationError> errors,
            StageMapValidationErrorCode errorCode,
            string elementName,
            int actualCount,
            int requiredCount)
        {
            if (actualCount >= requiredCount)
            {
                return;
            }

            AddError(
                errors,
                errorCode,
                $"Map has {actualCount} {elementName} deployable cells; " +
                $"at least {requiredCount} are required.");
        }

        private static void AddError(
            ICollection<StageMapValidationError> errors,
            StageMapValidationErrorCode code,
            string message,
            Vector2Int? cell = null,
            int? nodeId = null)
        {
            errors.Add(new StageMapValidationError(code, message, cell, nodeId));
        }
    }
}
