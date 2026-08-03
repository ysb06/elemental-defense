using System;
using System.Collections.Generic;
using QuikGraph;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public sealed class MutableEnemyRouteGraphBuilder
    {
        private readonly Dictionary<int, RouteNode> nodesById = new();
        private readonly Dictionary<string, int> spawnStartNodeIds =
            new(StringComparer.Ordinal);
        private readonly List<RouteCrossingDefinition> disconnectedCrossings = new();
        private readonly HashSet<Vector2Int> disconnectedCrossingCells = new();
        private readonly HashSet<int> disconnectedCrossingNodeIds = new();
        private readonly AdjacencyGraph<int, SEquatableEdge<int>> graph =
            new(allowParallelEdges: false);

        private int? goalNodeId;

        public int NodeCount => nodesById.Count;

        public void AddNode(RouteNode node)
        {
            if (nodesById.ContainsKey(node.Id))
            {
                throw new InvalidOperationException(
                    $"Route node ID {node.Id} is already registered.");
            }

            nodesById.Add(node.Id, node);
            try
            {
                if (!graph.AddVertex(node.Id))
                {
                    throw new InvalidOperationException(
                        $"Failed to register graph vertex {node.Id}.");
                }
            }
            catch
            {
                nodesById.Remove(node.Id);
                throw;
            }
        }

        public void AddEdge(int sourceNodeId, int targetNodeId)
        {
            EnsureNodeExists(sourceNodeId);
            EnsureNodeExists(targetNodeId);

            if (sourceNodeId == targetNodeId)
            {
                throw new InvalidOperationException(
                    "A route graph cannot contain a self-loop.");
            }

            if (graph.ContainsEdge(sourceNodeId, targetNodeId))
            {
                throw new InvalidOperationException(
                    $"Route edge {sourceNodeId} -> {targetNodeId} is already registered.");
            }

            SEquatableEdge<int> edge = new(sourceNodeId, targetNodeId);
            if (!graph.AddEdge(edge))
            {
                throw new InvalidOperationException(
                    $"Failed to register route edge {sourceNodeId} -> {targetNodeId}.");
            }
        }

        public void AddSpawn(string spawnId, int startNodeId)
        {
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                throw new ArgumentException("A spawn ID is required.", nameof(spawnId));
            }

            EnsureNodeExists(startNodeId);

            if (spawnStartNodeIds.ContainsKey(spawnId))
            {
                throw new InvalidOperationException(
                    $"Spawn ID '{spawnId}' is already registered.");
            }

            spawnStartNodeIds.Add(spawnId, startNodeId);
        }

        public void SetGoal(int nodeId)
        {
            EnsureNodeExists(nodeId);

            if (goalNodeId.HasValue)
            {
                throw new InvalidOperationException(
                    $"Route goal node {goalNodeId.Value} is already registered.");
            }

            goalNodeId = nodeId;
        }

        public void AddDisconnectedCrossing(
            Vector2Int cell,
            int horizontalNodeId,
            int verticalNodeId)
        {
            AddDisconnectedCrossing(
                new RouteCrossingDefinition(
                    cell,
                    horizontalNodeId,
                    verticalNodeId));
        }

        public void AddDisconnectedCrossing(RouteCrossingDefinition crossing)
        {
            if (crossing.HorizontalNodeId < 0 || crossing.VerticalNodeId < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(crossing),
                    crossing,
                    "Disconnected crossing node IDs cannot be negative.");
            }

            if (crossing.HorizontalNodeId == crossing.VerticalNodeId)
            {
                throw new ArgumentException(
                    "A disconnected crossing requires two different route nodes.",
                    nameof(crossing));
            }

            EnsureNodeExists(crossing.HorizontalNodeId);
            EnsureNodeExists(crossing.VerticalNodeId);

            if (disconnectedCrossingCells.Contains(crossing.Cell))
            {
                throw new InvalidOperationException(
                    $"A disconnected crossing is already registered at {crossing.Cell}.");
            }

            if (disconnectedCrossingNodeIds.Contains(crossing.HorizontalNodeId))
            {
                throw new InvalidOperationException(
                    $"Route node {crossing.HorizontalNodeId} is already registered to a disconnected crossing.");
            }

            if (disconnectedCrossingNodeIds.Contains(crossing.VerticalNodeId))
            {
                throw new InvalidOperationException(
                    $"Route node {crossing.VerticalNodeId} is already registered to a disconnected crossing.");
            }

            disconnectedCrossings.Add(crossing);
            disconnectedCrossingCells.Add(crossing.Cell);
            disconnectedCrossingNodeIds.Add(crossing.HorizontalNodeId);
            disconnectedCrossingNodeIds.Add(crossing.VerticalNodeId);
        }

        public EnemyRouteGraph Freeze()
        {
            if (nodesById.Count == 0)
            {
                throw new InvalidOperationException(
                    "A route graph requires at least one node.");
            }

            if (spawnStartNodeIds.Count == 0)
            {
                throw new InvalidOperationException(
                    "A route graph requires at least one spawn.");
            }

            if (!goalNodeId.HasValue)
            {
                throw new InvalidOperationException(
                    "A route graph requires a goal node.");
            }

            ArrayAdjacencyGraph<int, SEquatableEdge<int>> frozenGraph =
                graph.ToArrayAdjacencyGraph();

            return new EnemyRouteGraph(
                nodesById,
                frozenGraph,
                spawnStartNodeIds,
                goalNodeId.Value,
                disconnectedCrossings);
        }

        private void EnsureNodeExists(int nodeId)
        {
            if (!nodesById.ContainsKey(nodeId))
            {
                throw new ArgumentException(
                    $"Route node {nodeId} is not registered.",
                    nameof(nodeId));
            }
        }
    }
}
