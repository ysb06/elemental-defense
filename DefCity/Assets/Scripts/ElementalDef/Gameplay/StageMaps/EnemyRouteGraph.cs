using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps
{
    public sealed class EnemyRouteGraph
    {
        private static readonly IReadOnlyList<Vector2Int> EmptyPath = Array.Empty<Vector2Int>();
        private readonly ArrayAdjacencyGraph<int, SEquatableEdge<int>> graph;
        private readonly IReadOnlyDictionary<int, RouteNode> nodesById;
        private readonly IReadOnlyDictionary<string, int> spawnStartNodeIds;
        private readonly IReadOnlyList<RouteNode> orderedNodes;
        private readonly IReadOnlyList<string> orderedSpawnIds;
        private readonly IReadOnlyList<RouteCrossingDefinition> disconnectedCrossings;
        private readonly IReadOnlyDictionary<Vector2Int, RouteCrossingDefinition>
            disconnectedCrossingsByCell;

        public IReadOnlyList<RouteCrossingDefinition> DisconnectedCrossings =>
            disconnectedCrossings;

        internal int GoalNodeId { get; }
        internal IReadOnlyList<RouteNode> Nodes => orderedNodes;
        internal IReadOnlyList<string> SpawnIds => orderedSpawnIds;
        internal RouteNode GoalNode => GetNode(GoalNodeId);

        internal EnemyRouteGraph(
            IReadOnlyDictionary<int, RouteNode> sourceNodesById,
            ArrayAdjacencyGraph<int, SEquatableEdge<int>> frozenGraph,
            IReadOnlyDictionary<string, int> sourceSpawnStartNodeIds,
            int goalNodeId)
            : this(
                sourceNodesById,
                frozenGraph,
                sourceSpawnStartNodeIds,
                goalNodeId,
                Array.Empty<RouteCrossingDefinition>())
        {
        }

        internal EnemyRouteGraph(
            IReadOnlyDictionary<int, RouteNode> sourceNodesById,
            ArrayAdjacencyGraph<int, SEquatableEdge<int>> frozenGraph,
            IReadOnlyDictionary<string, int> sourceSpawnStartNodeIds,
            int goalNodeId,
            IReadOnlyList<RouteCrossingDefinition> sourceDisconnectedCrossings)
        {
            if (sourceNodesById == null)
            {
                throw new ArgumentNullException(nameof(sourceNodesById));
            }

            if (frozenGraph == null)
            {
                throw new ArgumentNullException(nameof(frozenGraph));
            }

            if (sourceSpawnStartNodeIds == null)
            {
                throw new ArgumentNullException(nameof(sourceSpawnStartNodeIds));
            }

            if (sourceDisconnectedCrossings == null)
            {
                throw new ArgumentNullException(nameof(sourceDisconnectedCrossings));
            }

            Dictionary<int, RouteNode> nodeCopies = new(sourceNodesById.Count);
            foreach (KeyValuePair<int, RouteNode> entry in sourceNodesById)
            {
                nodeCopies.Add(entry.Key, entry.Value);
            }

            Dictionary<string, int> spawnCopies =
                new(sourceSpawnStartNodeIds.Count, StringComparer.Ordinal);
            foreach (KeyValuePair<string, int> entry in sourceSpawnStartNodeIds)
            {
                spawnCopies.Add(entry.Key, entry.Value);
            }

            if (!nodeCopies.ContainsKey(goalNodeId))
            {
                throw new ArgumentException(
                    $"Goal route node {goalNodeId} is not registered.",
                    nameof(goalNodeId));
            }

            if (frozenGraph.VertexCount != nodeCopies.Count)
            {
                throw new ArgumentException(
                    "The frozen graph vertex count does not match its route node metadata.",
                    nameof(frozenGraph));
            }

            foreach (int vertex in frozenGraph.Vertices)
            {
                if (!nodeCopies.ContainsKey(vertex))
                {
                    throw new ArgumentException(
                        $"Graph vertex {vertex} has no route node metadata.",
                        nameof(frozenGraph));
                }
            }

            foreach (KeyValuePair<string, int> spawn in spawnCopies)
            {
                if (!nodeCopies.ContainsKey(spawn.Value))
                {
                    throw new ArgumentException(
                        $"Spawn '{spawn.Key}' references unknown route node {spawn.Value}.",
                        nameof(sourceSpawnStartNodeIds));
                }
            }

            RouteCrossingDefinition[] crossingCopies =
                new RouteCrossingDefinition[sourceDisconnectedCrossings.Count];
            Dictionary<Vector2Int, RouteCrossingDefinition> crossingsByCell = new();
            HashSet<int> crossingNodeIds = new();
            for (int index = 0; index < sourceDisconnectedCrossings.Count; index++)
            {
                RouteCrossingDefinition crossing = sourceDisconnectedCrossings[index];
                if (!nodeCopies.ContainsKey(crossing.HorizontalNodeId) ||
                    !nodeCopies.ContainsKey(crossing.VerticalNodeId))
                {
                    throw new ArgumentException(
                        $"Disconnected crossing at {crossing.Cell} references an unknown route node.",
                        nameof(sourceDisconnectedCrossings));
                }

                if (!crossingsByCell.TryAdd(crossing.Cell, crossing))
                {
                    throw new ArgumentException(
                        $"Multiple disconnected crossings use cell {crossing.Cell}.",
                        nameof(sourceDisconnectedCrossings));
                }

                if (!crossingNodeIds.Add(crossing.HorizontalNodeId) ||
                    !crossingNodeIds.Add(crossing.VerticalNodeId))
                {
                    throw new ArgumentException(
                        $"A route node is assigned to multiple disconnected crossings at {crossing.Cell}.",
                        nameof(sourceDisconnectedCrossings));
                }

                crossingCopies[index] = crossing;
            }

            Array.Sort(
                crossingCopies,
                (left, right) =>
                {
                    int yComparison = left.Cell.y.CompareTo(right.Cell.y);
                    if (yComparison != 0)
                    {
                        return yComparison;
                    }

                    int xComparison = left.Cell.x.CompareTo(right.Cell.x);
                    if (xComparison != 0)
                    {
                        return xComparison;
                    }

                    int horizontalComparison =
                        left.HorizontalNodeId.CompareTo(right.HorizontalNodeId);
                    return horizontalComparison != 0
                        ? horizontalComparison
                        : left.VerticalNodeId.CompareTo(right.VerticalNodeId);
                });

            graph = frozenGraph;
            nodesById = new ReadOnlyDictionary<int, RouteNode>(nodeCopies);
            spawnStartNodeIds =
                new ReadOnlyDictionary<string, int>(spawnCopies);
            orderedNodes = Array.AsReadOnly(
                nodeCopies.Values.OrderBy(node => node.Id).ToArray());
            orderedSpawnIds = Array.AsReadOnly(
                spawnCopies.Keys.OrderBy(id => id, StringComparer.Ordinal).ToArray());
            disconnectedCrossings = Array.AsReadOnly(crossingCopies);
            disconnectedCrossingsByCell =
                new ReadOnlyDictionary<Vector2Int, RouteCrossingDefinition>(
                    crossingsByCell);
            GoalNodeId = goalNodeId;
        }

        public RouteNode GetNode(int nodeId)
        {
            if (!nodesById.TryGetValue(nodeId, out RouteNode node))
            {
                throw new KeyNotFoundException(
                    $"Route node {nodeId} is not registered.");
            }

            return node;
        }

        public IReadOnlyList<RouteNode> GetOutgoingNodes(int nodeId)
        {
            EnsureNodeExists(nodeId);

            RouteNode[] outgoingNodes = graph
                .OutEdges(nodeId)
                .Select(edge => nodesById[edge.Target])
                .OrderBy(node => node.Id)
                .ToArray();

            return Array.AsReadOnly(outgoingNodes);
        }

        public bool TryGetSpawnStartNode(string spawnId, out RouteNode node)
        {
            node = default;
            if (string.IsNullOrWhiteSpace(spawnId) ||
                !spawnStartNodeIds.TryGetValue(spawnId, out int nodeId))
            {
                return false;
            }

            node = nodesById[nodeId];
            return true;
        }

        public bool TryGetDisconnectedCrossing(
            Vector2Int cell,
            out RouteCrossingDefinition crossing)
        {
            return disconnectedCrossingsByCell.TryGetValue(cell, out crossing);
        }

        public bool TryBuildPrimaryPath(
            string spawnId,
            out IReadOnlyList<Vector2Int> path)
        {
            return TryBuildPathCore(
                spawnId,
                (_, orderedCandidates) => orderedCandidates[0],
                out path);
        }

        public bool TryBuildPath(
            string spawnId,
            IRouteChoicePolicy policy,
            out IReadOnlyList<Vector2Int> path)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            return TryBuildPathCore(
                spawnId,
                policy.ChooseNextNode,
                out path);
        }

        internal bool IsDirectedAcyclic()
        {
            return graph.IsDirectedAcyclicGraph();
        }

        internal bool ContainsEdge(int sourceNodeId, int targetNodeId)
        {
            EnsureNodeExists(sourceNodeId);
            EnsureNodeExists(targetNodeId);
            return graph.ContainsEdge(sourceNodeId, targetNodeId);
        }

        private bool TryBuildPathCore(
            string spawnId,
            Func<int, IReadOnlyList<int>, int> selectNextNode,
            out IReadOnlyList<Vector2Int> path)
        {
            path = EmptyPath;
            if (string.IsNullOrWhiteSpace(spawnId) ||
                !spawnStartNodeIds.TryGetValue(spawnId, out int currentNodeId))
            {
                return false;
            }

            List<Vector2Int> result = new(nodesById.Count);
            HashSet<int> visitedNodeIds = new();

            while (visitedNodeIds.Add(currentNodeId))
            {
                result.Add(nodesById[currentNodeId].Cell);
                if (currentNodeId == GoalNodeId)
                {
                    path = Array.AsReadOnly(result.ToArray());
                    return true;
                }

                int[] candidateNodeIds = graph
                    .OutEdges(currentNodeId)
                    .Select(edge => edge.Target)
                    .OrderBy(nodeId => nodeId)
                    .ToArray();
                if (candidateNodeIds.Length == 0)
                {
                    return false;
                }

                IReadOnlyList<int> orderedCandidates =
                    Array.AsReadOnly(candidateNodeIds);
                int nextNodeId = selectNextNode(
                    currentNodeId,
                    orderedCandidates);
                if (Array.BinarySearch(candidateNodeIds, nextNodeId) < 0)
                {
                    throw new InvalidOperationException(
                        $"The route choice policy selected node {nextNodeId}, which is not " +
                        $"an outgoing candidate of node {currentNodeId}.");
                }

                currentNodeId = nextNodeId;
            }

            return false;
        }

        private void EnsureNodeExists(int nodeId)
        {
            if (!nodesById.ContainsKey(nodeId))
            {
                throw new KeyNotFoundException(
                    $"Route node {nodeId} is not registered.");
            }
        }
    }
}
