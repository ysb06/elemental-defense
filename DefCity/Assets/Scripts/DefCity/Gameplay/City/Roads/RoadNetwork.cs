using System;
using System.Collections.Generic;
using QuikGraph;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    [DisallowMultipleComponent]
    public sealed class RoadNetwork : MonoBehaviour
    {
        private const float IntersectionEpsilon = 0.0001f;

        private readonly Dictionary<Vector3Int, RoadNode> nodesByCell = new();
        private readonly List<RoadSegment> segments = new();
        private BidirectionalGraph<RoadNode, RoadEdge> graph;

        public IBidirectionalGraph<RoadNode, RoadEdge> Graph => GraphStorage;
        public IReadOnlyList<RoadSegment> Segments => segments;
        public int NodeCount => GraphStorage.VertexCount;
        public int RoadCount => segments.Count;
        public int EdgeCount => GraphStorage.EdgeCount;
        public int IntersectionCount
        {
            get
            {
                int count = 0;
                foreach (RoadNode node in GraphStorage.Vertices)
                {
                    if (GraphStorage.OutDegree(node) >= 3)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private BidirectionalGraph<RoadNode, RoadEdge> GraphStorage =>
            graph ??= new BidirectionalGraph<RoadNode, RoadEdge>(allowParallelEdges: false);

        private void Awake()
        {
            _ = GraphStorage;
        }

        public bool TryGetNode(Vector3Int cellPosition, out RoadNode node)
        {
            return nodesByCell.TryGetValue(cellPosition, out node);
        }

        public int GetRoadDegree(RoadNode node)
        {
            ValidateRegisteredNode(node);
            return GraphStorage.OutDegree(node);
        }

        public bool IsIntersection(RoadNode node)
        {
            return GetRoadDegree(node) >= 3;
        }

        internal IReadOnlyList<RoadSegment> GetConnectedSegments(RoadNode node)
        {
            ValidateRegisteredNode(node);
            List<RoadSegment> connected = new();
            foreach (RoadEdge edge in GraphStorage.OutEdges(node))
            {
                connected.Add(edge.Segment);
            }

            return connected;
        }

        internal bool CanRegisterStraightSegment(
            Vector3Int startCell,
            Vector3 startPosition,
            Vector3Int endCell,
            Vector3 endPosition,
            out string failureReason)
        {
            EnsureStateIsConsistent();

            if (startCell == endCell)
            {
                failureReason = "Road endpoints must use different terrain cells.";
                return false;
            }

            Vector2 horizontalDelta = new(
                endPosition.x - startPosition.x,
                endPosition.z - startPosition.z);
            if (horizontalDelta.magnitude <= IntersectionEpsilon)
            {
                failureReason = "Road endpoints must have different XZ positions.";
                return false;
            }

            foreach (RoadSegment existing in segments)
            {
                RoadNode existingStart = existing.StartNode;
                RoadNode existingEnd = existing.EndNode;
                bool sameEndpoints = startCell == existingStart.CellPosition
                    && endCell == existingEnd.CellPosition;
                bool reverseEndpoints = startCell == existingEnd.CellPosition
                    && endCell == existingStart.CellPosition;
                if (sameEndpoints || reverseEndpoints)
                {
                    failureReason = "A road already connects the selected endpoint cells.";
                    return false;
                }

                RoadSegmentConflict conflict = RoadSegmentConflictDetector.GetConflict(
                    startCell,
                    startPosition,
                    endCell,
                    endPosition,
                    existing,
                    IntersectionEpsilon);
                if (conflict == RoadSegmentConflict.Overlap)
                {
                    failureReason = "The road centerline overlaps an existing road.";
                    return false;
                }

                if (conflict == RoadSegmentConflict.Intersection)
                {
                    failureReason = "The road centerline intersects an existing road away from a shared endpoint.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        internal bool TryRegisterSegment(RoadSegment segment, out string failureReason)
        {
            ValidateSegmentTopology(segment);

            RoadNode startNode = segment.StartNode;
            RoadNode endNode = segment.EndNode;
            Vector3Int startCell = startNode.CellPosition;
            Vector3Int endCell = endNode.CellPosition;

            if (!CanRegisterStraightSegment(
                    startCell,
                    startNode.WorldPosition,
                    endCell,
                    endNode.WorldPosition,
                    out failureReason))
            {
                return false;
            }

            if (!CanUseNode(startCell, startNode, out failureReason)
                || !CanUseNode(endCell, endNode, out failureReason))
            {
                return false;
            }

            if (segments.Contains(segment))
            {
                failureReason = "The road segment is already registered.";
                return false;
            }

            if (GraphStorage.ContainsEdge(startNode, endNode)
                || GraphStorage.ContainsEdge(endNode, startNode))
            {
                throw new InvalidOperationException(
                    "The road graph contains endpoint edges that are not represented by a registered road segment.");
            }

            bool startNodeAdded = false;
            bool endNodeAdded = false;
            bool forwardEdgeAdded = false;
            bool reverseEdgeAdded = false;
            bool segmentAdded = false;

            try
            {
                startNodeAdded = RegisterNodeIfNeeded(startCell, startNode);
                endNodeAdded = RegisterNodeIfNeeded(endCell, endNode);

                forwardEdgeAdded = GraphStorage.AddEdge(segment.ForwardEdge);
                reverseEdgeAdded = forwardEdgeAdded && GraphStorage.AddEdge(segment.ReverseEdge);
                if (!forwardEdgeAdded || !reverseEdgeAdded)
                {
                    failureReason = "Failed to register both directed road edges.";
                    RollbackRegistration(
                        segment,
                        startCell,
                        startNode,
                        startNodeAdded,
                        endCell,
                        endNode,
                        endNodeAdded,
                        forwardEdgeAdded,
                        reverseEdgeAdded,
                        segmentAdded);
                    return false;
                }

                segments.Add(segment);
                segmentAdded = true;
                EnsureStateIsConsistent();
                failureReason = string.Empty;
                return true;
            }
            catch
            {
                RollbackRegistration(
                    segment,
                    startCell,
                    startNode,
                    startNodeAdded,
                    endCell,
                    endNode,
                    endNodeAdded,
                    forwardEdgeAdded,
                    reverseEdgeAdded,
                    segmentAdded);
                throw;
            }
        }

        internal void UnregisterSegment(RoadSegment segment)
        {
            ValidateSegmentTopology(segment);
            if (!segments.Contains(segment)
                || !GraphStorage.ContainsEdge(segment.ForwardEdge)
                || !GraphStorage.ContainsEdge(segment.ReverseEdge))
            {
                throw new InvalidOperationException("Road segment is not fully registered in this road network.");
            }

            RoadNode startNode = segment.StartNode;
            RoadNode endNode = segment.EndNode;
            GraphStorage.RemoveEdge(segment.ReverseEdge);
            GraphStorage.RemoveEdge(segment.ForwardEdge);
            segments.Remove(segment);
            RemoveNodeIfOrphaned(startNode);
            RemoveNodeIfOrphaned(endNode);
            EnsureStateIsConsistent();
        }

        private bool CanUseNode(
            Vector3Int cellPosition,
            RoadNode candidate,
            out string failureReason)
        {
            if (!nodesByCell.TryGetValue(cellPosition, out RoadNode registered))
            {
                if (GraphStorage.ContainsVertex(candidate))
                {
                    throw new InvalidOperationException(
                        $"Road node {cellPosition} exists in the graph but is missing from the cell lookup.");
                }

                failureReason = string.Empty;
                return true;
            }

            if (!ReferenceEquals(registered, candidate))
            {
                failureReason = $"Terrain cell {cellPosition} is already assigned to another road node.";
                return false;
            }

            if (!GraphStorage.ContainsVertex(candidate))
            {
                throw new InvalidOperationException(
                    $"Road node {cellPosition} exists in the cell lookup but is missing from the graph.");
            }

            failureReason = string.Empty;
            return true;
        }

        private void ValidateRegisteredNode(RoadNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!nodesByCell.TryGetValue(node.CellPosition, out RoadNode registered)
                || !ReferenceEquals(registered, node)
                || !GraphStorage.ContainsVertex(node))
            {
                throw new ArgumentException("Road node is not registered in this road network.", nameof(node));
            }
        }

        private void RemoveNodeIfOrphaned(RoadNode node)
        {
            if (!GraphStorage.ContainsVertex(node) || GraphStorage.OutDegree(node) > 0)
            {
                return;
            }

            if (GraphStorage.InDegree(node) > 0)
            {
                throw new InvalidOperationException("Road node has incoming edges without matching outgoing edges.");
            }

            if (!GraphStorage.RemoveVertex(node))
            {
                throw new InvalidOperationException($"Failed to unregister road node {node.CellPosition}.");
            }

            RemoveNodeLookup(node.CellPosition, node);
        }

        private bool RegisterNodeIfNeeded(Vector3Int cellPosition, RoadNode node)
        {
            if (nodesByCell.ContainsKey(cellPosition))
            {
                return false;
            }

            nodesByCell.Add(cellPosition, node);
            try
            {
                if (!GraphStorage.AddVertex(node))
                {
                    throw new InvalidOperationException($"Failed to register road node {cellPosition}.");
                }
            }
            catch
            {
                nodesByCell.Remove(cellPosition);
                throw;
            }

            return true;
        }

        private void EnsureStateIsConsistent()
        {
            if (GraphStorage.EdgeCount != segments.Count * 2)
            {
                throw new InvalidOperationException(
                    "Road network state is inconsistent: each road segment must own exactly two graph edges.");
            }

            if (GraphStorage.VertexCount != nodesByCell.Count)
            {
                throw new InvalidOperationException(
                    "Road network state is inconsistent: graph vertices and cell nodes differ.");
            }

            foreach (KeyValuePair<Vector3Int, RoadNode> entry in nodesByCell)
            {
                if (entry.Value == null
                    || entry.Value.CellPosition != entry.Key
                    || !GraphStorage.ContainsVertex(entry.Value))
                {
                    throw new InvalidOperationException(
                        $"Road network state is inconsistent at terrain cell {entry.Key}.");
                }
            }

            foreach (RoadSegment registeredSegment in segments)
            {
                ValidateSegmentTopology(registeredSegment);
                if (!GraphStorage.ContainsEdge(registeredSegment.ForwardEdge)
                    || !GraphStorage.ContainsEdge(registeredSegment.ReverseEdge))
                {
                    throw new InvalidOperationException(
                        "Road network state is inconsistent: a registered segment edge is missing from the graph.");
                }
            }
        }

        private static void ValidateSegmentTopology(RoadSegment segment)
        {
            if (segment == null)
            {
                throw new ArgumentNullException(nameof(segment));
            }

            RoadNode startNode = segment.StartNode;
            RoadNode endNode = segment.EndNode;
            RoadEdge forwardEdge = segment.ForwardEdge;
            RoadEdge reverseEdge = segment.ReverseEdge;
            if (startNode == null
                || endNode == null
                || segment.Mesh == null
                || forwardEdge == null
                || reverseEdge == null)
            {
                throw new InvalidOperationException("Road segment must be fully initialized before registration.");
            }

            if (ReferenceEquals(startNode, endNode)
                || !ReferenceEquals(forwardEdge.Source, startNode)
                || !ReferenceEquals(forwardEdge.Target, endNode)
                || !ReferenceEquals(forwardEdge.Segment, segment)
                || !ReferenceEquals(reverseEdge.Source, endNode)
                || !ReferenceEquals(reverseEdge.Target, startNode)
                || !ReferenceEquals(reverseEdge.Segment, segment))
            {
                throw new InvalidOperationException("Road segment edges do not match its endpoint topology.");
            }
        }

        private void RollbackRegistration(
            RoadSegment segment,
            Vector3Int startCell,
            RoadNode startNode,
            bool startNodeAdded,
            Vector3Int endCell,
            RoadNode endNode,
            bool endNodeAdded,
            bool forwardEdgeAdded,
            bool reverseEdgeAdded,
            bool segmentAdded)
        {
            if (segmentAdded)
            {
                segments.Remove(segment);
            }

            if (reverseEdgeAdded)
            {
                GraphStorage.RemoveEdge(segment.ReverseEdge);
            }

            if (forwardEdgeAdded)
            {
                GraphStorage.RemoveEdge(segment.ForwardEdge);
            }

            if (endNodeAdded)
            {
                GraphStorage.RemoveVertex(endNode);
                RemoveNodeLookup(endCell, endNode);
            }

            if (startNodeAdded)
            {
                GraphStorage.RemoveVertex(startNode);
                RemoveNodeLookup(startCell, startNode);
            }
        }

        private void RemoveNodeLookup(Vector3Int cellPosition, RoadNode expectedNode)
        {
            if (nodesByCell.TryGetValue(cellPosition, out RoadNode registered)
                && ReferenceEquals(registered, expectedNode))
            {
                nodesByCell.Remove(cellPosition);
            }
        }
    }
}
