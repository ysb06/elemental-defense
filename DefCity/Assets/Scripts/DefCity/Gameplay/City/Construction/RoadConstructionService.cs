using System;
using System.Collections.Generic;
using DefCity.Gameplay.City.Roads;
using DefCity.Gameplay.City.Roads.Geometry;
using DefCity.Gameplay.World;
using UnityEngine;

namespace DefCity.Gameplay.City.Construction
{
    public sealed class RoadConstructionService
    {
        private const float GeometryEpsilon = 0.0001f;
        private const string MismatchedIntersectionSettingsReason =
            "Intersection roads must use matching build settings.";
        private const string InsufficientTrimLengthReason =
            "Road segment is too short for the required intersection trim.";
        private const string DiagonalRoadDisabledReason =
            "Diagonal roads are disabled by the current build settings.";

        private readonly RoadNetwork roadNetwork;

        public RoadConstructionService(RoadNetwork roadNetwork)
        {
            this.roadNetwork = roadNetwork != null
                ? roadNetwork
                : throw new ArgumentNullException(nameof(roadNetwork));
        }

        public bool CanBuildStraightSegment(
            TerrainCell start,
            TerrainCell end,
            RoadBuildSettings settings,
            out string failureReason)
        {
            return TryCreateStraightSegmentBuildPlan(
                start,
                end,
                settings,
                out _,
                out failureReason);
        }

        public bool TryBuildStraightSegment(
            TerrainCell start,
            TerrainCell end,
            RoadBuildSettings settings,
            out RoadSegment segment,
            out string failureReason)
        {
            segment = null;
            if (!TryCreateStraightSegmentBuildPlan(
                    start,
                    end,
                    settings,
                    out StraightSegmentBuildPlan buildPlan,
                    out failureReason))
            {
                return false;
            }

            bool hasStartNode = buildPlan.HasStartNode;
            bool hasEndNode = buildPlan.HasEndNode;
            RoadNode startNode = buildPlan.StartNode;
            RoadNode endNode = buildPlan.EndNode;
            Vector3 startWorldPosition = buildPlan.StartWorldPosition;
            Vector3 endWorldPosition = buildPlan.EndWorldPosition;
            EndpointPlan startPlan = buildPlan.StartPlan;
            EndpointPlan endPlan = buildPlan.EndPlan;
            List<SegmentGeometryUpdate> segmentUpdates = buildPlan.SegmentUpdates;

            Mesh candidateMesh = null;
            GameObject segmentObject = null;
            RoadSegment createdSegment = null;
            bool createdStartNode = false;
            bool createdEndNode = false;
            bool segmentRegistered = false;
            List<IntersectionGeometryUpdate> intersectionUpdates = new();

            try
            {
                candidateMesh = BuildSegmentMesh(
                    startWorldPosition,
                    endWorldPosition,
                    settings,
                    startPlan.TrimDistance,
                    endPlan.TrimDistance,
                    $"Road Segment {start.RefPosition} to {end.RefPosition}");

                BuildSegmentReplacementMeshes(segmentUpdates);
                intersectionUpdates = BuildIntersectionUpdates(startPlan, endPlan, settings);

                if (!hasStartNode)
                {
                    startNode = CreateNode(start.RefPosition, startWorldPosition);
                    createdStartNode = true;
                }

                if (!hasEndNode)
                {
                    endNode = CreateNode(end.RefPosition, endWorldPosition);
                    createdEndNode = true;
                }

                segmentObject = new GameObject($"Road Segment {start.RefPosition} to {end.RefPosition}");
                segmentObject.transform.SetParent(roadNetwork.transform, false);
                segmentObject.transform.SetPositionAndRotation(startWorldPosition, Quaternion.identity);
                createdSegment = segmentObject.AddComponent<RoadSegment>();
                createdSegment.Initialize(
                    startNode,
                    endNode,
                    candidateMesh,
                    settings,
                    startPlan.TrimDistance,
                    endPlan.TrimDistance);

                PrepareNewIntersectionObjects(intersectionUpdates, settings.Material);

                if (!roadNetwork.TryRegisterSegment(createdSegment, out failureReason))
                {
                    CleanupFailedBuild(
                        segmentUpdates,
                        intersectionUpdates,
                        createdSegment,
                        segmentObject,
                        candidateMesh,
                        startNode,
                        createdStartNode,
                        endNode,
                        createdEndNode,
                        segmentRegistered: false);
                    return false;
                }

                segmentRegistered = true;
                ApplySegmentUpdates(segmentUpdates);
                ApplyIntersectionUpdates(intersectionUpdates);

                segment = createdSegment;
                DestroyReplacedMeshes(segmentUpdates, intersectionUpdates);
                return true;
            }
            catch
            {
                CleanupFailedBuild(
                    segmentUpdates,
                    intersectionUpdates,
                    createdSegment,
                    segmentObject,
                    candidateMesh,
                    startNode,
                    createdStartNode,
                    endNode,
                    createdEndNode,
                    segmentRegistered);
                throw;
            }
        }

        private bool TryCreateStraightSegmentBuildPlan(
            TerrainCell start,
            TerrainCell end,
            RoadBuildSettings settings,
            out StraightSegmentBuildPlan plan,
            out string failureReason)
        {
            plan = null;
            failureReason = string.Empty;
            settings.Validate();

            if (!settings.AllowDiagonalRoads && IsDiagonal(start.RefPosition, end.RefPosition))
            {
                failureReason = DiagonalRoadDisabledReason;
                return false;
            }

            bool hasStartNode = roadNetwork.TryGetNode(start.RefPosition, out RoadNode startNode);
            bool hasEndNode = roadNetwork.TryGetNode(end.RefPosition, out RoadNode endNode);
            Vector3 startWorldPosition = hasStartNode
                ? startNode.WorldPosition
                : GetTerrainEndpointPosition(start);
            Vector3 endWorldPosition = hasEndNode
                ? endNode.WorldPosition
                : GetTerrainEndpointPosition(end);

            if (!roadNetwork.CanRegisterStraightSegment(
                    start.RefPosition,
                    startWorldPosition,
                    end.RefPosition,
                    endWorldPosition,
                    out failureReason))
            {
                return false;
            }

            if (!TryBuildEndpointPlan(
                    startNode,
                    startWorldPosition,
                    endWorldPosition,
                    settings,
                    out EndpointPlan startPlan,
                    out failureReason)
                || !TryBuildEndpointPlan(
                    endNode,
                    endWorldPosition,
                    startWorldPosition,
                    settings,
                    out EndpointPlan endPlan,
                    out failureReason))
            {
                return false;
            }

            List<SegmentGeometryUpdate> segmentUpdates = BuildSegmentUpdateTargets(startPlan, endPlan);
            if (!CanApplyTrim(
                    startWorldPosition,
                    endWorldPosition,
                    startPlan.TrimDistance,
                    endPlan.TrimDistance)
                || !CanApplySegmentUpdates(segmentUpdates))
            {
                failureReason = InsufficientTrimLengthReason;
                return false;
            }

            plan = new StraightSegmentBuildPlan(
                hasStartNode,
                hasEndNode,
                startNode,
                endNode,
                startWorldPosition,
                endWorldPosition,
                startPlan,
                endPlan,
                segmentUpdates);
            return true;
        }

        private bool TryBuildEndpointPlan(
            RoadNode node,
            Vector3 nodePosition,
            Vector3 candidateOtherPosition,
            RoadBuildSettings candidateSettings,
            out EndpointPlan plan,
            out string failureReason)
        {
            if (node == null)
            {
                plan = new EndpointPlan(
                    null,
                    nodePosition,
                    candidateOtherPosition,
                    Array.Empty<RoadSegment>(),
                    trimDistance: 0f,
                    createsIntersection: false);
                failureReason = string.Empty;
                return true;
            }

            IReadOnlyList<RoadSegment> connectedSegments = roadNetwork.GetConnectedSegments(node);
            bool createsIntersection = connectedSegments.Count + 1 >= 3;
            if (!createsIntersection)
            {
                if (node.Intersection != null)
                {
                    throw new InvalidOperationException(
                        "A road node below intersection degree owns an intersection mesh.");
                }

                plan = new EndpointPlan(
                    node,
                    nodePosition,
                    candidateOtherPosition,
                    connectedSegments,
                    trimDistance: 0f,
                    createsIntersection: false);
                failureReason = string.Empty;
                return true;
            }

            foreach (RoadSegment connectedSegment in connectedSegments)
            {
                if (!HaveMatchingBuildSettings(connectedSegment.BuildSettings, candidateSettings))
                {
                    plan = null;
                    failureReason = MismatchedIntersectionSettingsReason;
                    return false;
                }
            }

            plan = new EndpointPlan(
                node,
                nodePosition,
                candidateOtherPosition,
                connectedSegments,
                candidateSettings.Width * 0.5f,
                createsIntersection: true);
            failureReason = string.Empty;
            return true;
        }

        private static List<SegmentGeometryUpdate> BuildSegmentUpdateTargets(
            EndpointPlan startPlan,
            EndpointPlan endPlan)
        {
            Dictionary<RoadSegment, SegmentGeometryUpdate> updatesBySegment = new();
            AddEndpointTrimTargets(startPlan, updatesBySegment);
            AddEndpointTrimTargets(endPlan, updatesBySegment);

            List<SegmentGeometryUpdate> updates = new();
            foreach (SegmentGeometryUpdate update in updatesBySegment.Values)
            {
                if (!Approximately(update.PreviousStartTrimDistance, update.TargetStartTrimDistance)
                    || !Approximately(update.PreviousEndTrimDistance, update.TargetEndTrimDistance))
                {
                    updates.Add(update);
                }
            }

            return updates;
        }

        private static void AddEndpointTrimTargets(
            EndpointPlan endpointPlan,
            IDictionary<RoadSegment, SegmentGeometryUpdate> updatesBySegment)
        {
            if (!endpointPlan.CreatesIntersection)
            {
                return;
            }

            foreach (RoadSegment connectedSegment in endpointPlan.ConnectedSegments)
            {
                if (!updatesBySegment.TryGetValue(connectedSegment, out SegmentGeometryUpdate update))
                {
                    update = new SegmentGeometryUpdate(connectedSegment);
                    updatesBySegment.Add(connectedSegment, update);
                }

                if (ReferenceEquals(connectedSegment.StartNode, endpointPlan.Node))
                {
                    update.TargetStartTrimDistance = endpointPlan.TrimDistance;
                }
                else if (ReferenceEquals(connectedSegment.EndNode, endpointPlan.Node))
                {
                    update.TargetEndTrimDistance = endpointPlan.TrimDistance;
                }
                else
                {
                    throw new InvalidOperationException("Road segment is not connected to its planned endpoint.");
                }
            }
        }

        private static bool CanApplySegmentUpdates(IReadOnlyList<SegmentGeometryUpdate> updates)
        {
            foreach (SegmentGeometryUpdate update in updates)
            {
                if (!CanApplyTrim(
                        update.Segment.StartNode.WorldPosition,
                        update.Segment.EndNode.WorldPosition,
                        update.TargetStartTrimDistance,
                        update.TargetEndTrimDistance))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanApplyTrim(
            Vector3 startPosition,
            Vector3 endPosition,
            float startTrimDistance,
            float endTrimDistance)
        {
            Vector2 delta = new(
                endPosition.x - startPosition.x,
                endPosition.z - startPosition.z);
            return delta.magnitude > startTrimDistance + endTrimDistance + GeometryEpsilon;
        }

        private static void BuildSegmentReplacementMeshes(IReadOnlyList<SegmentGeometryUpdate> updates)
        {
            foreach (SegmentGeometryUpdate update in updates)
            {
                update.ReplacementMesh = BuildSegmentMesh(
                    update.Segment.StartNode.WorldPosition,
                    update.Segment.EndNode.WorldPosition,
                    update.Segment.BuildSettings,
                    update.TargetStartTrimDistance,
                    update.TargetEndTrimDistance,
                    update.Segment.name);
            }
        }

        private static Mesh BuildSegmentMesh(
            Vector3 startWorldPosition,
            Vector3 endWorldPosition,
            RoadBuildSettings settings,
            float startTrimDistance,
            float endTrimDistance,
            string meshName)
        {
            Vector3 worldDelta = endWorldPosition - startWorldPosition;
            Vector3 horizontalDelta = new(worldDelta.x, 0f, worldDelta.z);
            float horizontalLength = horizontalDelta.magnitude;
            if (horizontalLength <= startTrimDistance + endTrimDistance + GeometryEpsilon)
            {
                throw new ArgumentException(InsufficientTrimLengthReason, nameof(endWorldPosition));
            }

            Vector3 direction = horizontalDelta / horizontalLength;
            Vector3 localStart = direction * startTrimDistance;
            Vector3 localEnd = worldDelta - (direction * endTrimDistance);
            localStart.y = 0f;
            localEnd.y = worldDelta.y;

            Mesh mesh = RoadMeshGenerator.BuildStraightStripWithThickness(
                localStart,
                localEnd,
                settings.Width,
                settings.SampleSpacing,
                settings.Thickness,
                settings.YOffset,
                settings.UvOrientation,
                settings.ThicknessUvMode);
            mesh.name = meshName;
            return mesh;
        }

        private static List<IntersectionGeometryUpdate> BuildIntersectionUpdates(
            EndpointPlan startPlan,
            EndpointPlan endPlan,
            RoadBuildSettings settings)
        {
            List<IntersectionGeometryUpdate> updates = new();
            try
            {
                AddIntersectionUpdate(startPlan, settings, updates);
                AddIntersectionUpdate(endPlan, settings, updates);
                return updates;
            }
            catch
            {
                foreach (IntersectionGeometryUpdate update in updates)
                {
                    DestroyUnityObject(update.ReplacementMesh);
                }

                throw;
            }
        }

        private static void AddIntersectionUpdate(
            EndpointPlan endpointPlan,
            RoadBuildSettings settings,
            ICollection<IntersectionGeometryUpdate> updates)
        {
            if (!endpointPlan.CreatesIntersection)
            {
                return;
            }

            List<RoadIntersectionPort> ports = new(endpointPlan.ConnectedSegments.Count + 1);
            foreach (RoadSegment connectedSegment in endpointPlan.ConnectedSegments)
            {
                RoadNode otherNode = GetOtherNode(connectedSegment, endpointPlan.Node);
                ports.Add(new RoadIntersectionPort(
                    otherNode.WorldPosition - endpointPlan.NodePosition,
                    endpointPlan.TrimDistance));
            }

            ports.Add(new RoadIntersectionPort(
                endpointPlan.CandidateOtherPosition - endpointPlan.NodePosition,
                endpointPlan.TrimDistance));

            Mesh mesh = RoadIntersectionMeshGenerator.BuildConvexHullIntersection(
                Vector3.zero,
                ports,
                settings.Width,
                settings.Thickness,
                settings.YOffset,
                settings.UvOrientation);
            mesh.name = $"Road Intersection {endpointPlan.Node.CellPosition}";
            updates.Add(new IntersectionGeometryUpdate(endpointPlan.Node, mesh));
        }

        private static RoadNode GetOtherNode(RoadSegment segment, RoadNode node)
        {
            if (ReferenceEquals(segment.StartNode, node))
            {
                return segment.EndNode;
            }

            if (ReferenceEquals(segment.EndNode, node))
            {
                return segment.StartNode;
            }

            throw new InvalidOperationException("Road segment is not connected to the intersection node.");
        }

        private static void PrepareNewIntersectionObjects(
            IReadOnlyList<IntersectionGeometryUpdate> updates,
            Material material)
        {
            foreach (IntersectionGeometryUpdate update in updates)
            {
                if (update.ExistingIntersection != null)
                {
                    continue;
                }

                GameObject intersectionObject = new($"Road Intersection {update.Node.CellPosition}");
                intersectionObject.SetActive(false);
                update.CreatedObject = intersectionObject;
                RoadIntersection intersection = intersectionObject.AddComponent<RoadIntersection>();
                update.CreatedIntersection = intersection;
                intersection.Initialize(update.Node, update.ReplacementMesh, material);
            }
        }

        private static void ApplySegmentUpdates(IReadOnlyList<SegmentGeometryUpdate> updates)
        {
            foreach (SegmentGeometryUpdate update in updates)
            {
                update.PreviousMesh = update.Segment.ReplaceGeometry(
                    update.ReplacementMesh,
                    update.TargetStartTrimDistance,
                    update.TargetEndTrimDistance);
                update.Applied = true;
            }
        }

        private static void ApplyIntersectionUpdates(IReadOnlyList<IntersectionGeometryUpdate> updates)
        {
            foreach (IntersectionGeometryUpdate update in updates)
            {
                if (update.ExistingIntersection != null)
                {
                    update.PreviousMesh = update.ExistingIntersection.ReplaceMesh(update.ReplacementMesh);
                    update.Applied = true;
                    continue;
                }

                Transform intersectionTransform = update.CreatedObject.transform;
                intersectionTransform.SetParent(update.Node.transform, false);
                intersectionTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                intersectionTransform.localScale = Vector3.one;
                update.Node.AttachIntersection(update.CreatedIntersection);
                update.CreatedObject.SetActive(true);
                update.Applied = true;
            }
        }

        private void CleanupFailedBuild(
            IReadOnlyList<SegmentGeometryUpdate> segmentUpdates,
            IReadOnlyList<IntersectionGeometryUpdate> intersectionUpdates,
            RoadSegment createdSegment,
            GameObject segmentObject,
            Mesh candidateMesh,
            RoadNode startNode,
            bool createdStartNode,
            RoadNode endNode,
            bool createdEndNode,
            bool segmentRegistered)
        {
            RollbackIntersectionUpdates(intersectionUpdates);
            RollbackSegmentUpdates(segmentUpdates);

            if (segmentRegistered && createdSegment != null)
            {
                roadNetwork.UnregisterSegment(createdSegment);
            }

            bool segmentOwnsCandidateMesh = createdSegment != null && createdSegment.Mesh == candidateMesh;
            DestroyUnityObject(segmentObject);
            if (!segmentOwnsCandidateMesh)
            {
                DestroyUnityObject(candidateMesh);
            }

            if (createdEndNode && endNode != null)
            {
                DestroyUnityObject(endNode.gameObject);
            }

            if (createdStartNode && startNode != null)
            {
                DestroyUnityObject(startNode.gameObject);
            }
        }

        private static void RollbackSegmentUpdates(IReadOnlyList<SegmentGeometryUpdate> updates)
        {
            for (int i = updates.Count - 1; i >= 0; i--)
            {
                SegmentGeometryUpdate update = updates[i];
                if (!update.Applied)
                {
                    DestroyUnityObject(update.ReplacementMesh);
                    continue;
                }

                Mesh replacement = update.Segment.ReplaceGeometry(
                    update.PreviousMesh,
                    update.PreviousStartTrimDistance,
                    update.PreviousEndTrimDistance);
                DestroyUnityObject(replacement);
            }
        }

        private static void RollbackIntersectionUpdates(IReadOnlyList<IntersectionGeometryUpdate> updates)
        {
            for (int i = updates.Count - 1; i >= 0; i--)
            {
                IntersectionGeometryUpdate update = updates[i];
                if (update.CreatedObject != null)
                {
                    bool createdIntersectionOwnsMesh = update.CreatedIntersection != null
                        && update.CreatedIntersection.Mesh == update.ReplacementMesh;
                    update.Node.DetachIntersection(update.CreatedIntersection);
                    DestroyUnityObject(update.CreatedObject);
                    if (!createdIntersectionOwnsMesh)
                    {
                        DestroyUnityObject(update.ReplacementMesh);
                    }

                    continue;
                }

                if (!update.Applied)
                {
                    DestroyUnityObject(update.ReplacementMesh);
                    continue;
                }

                Mesh replacement = update.ExistingIntersection.ReplaceMesh(update.PreviousMesh);
                DestroyUnityObject(replacement);
            }
        }

        private static void DestroyReplacedMeshes(
            IReadOnlyList<SegmentGeometryUpdate> segmentUpdates,
            IReadOnlyList<IntersectionGeometryUpdate> intersectionUpdates)
        {
            foreach (SegmentGeometryUpdate update in segmentUpdates)
            {
                DestroyUnityObject(update.PreviousMesh);
            }

            foreach (IntersectionGeometryUpdate update in intersectionUpdates)
            {
                if (update.ExistingIntersection != null)
                {
                    DestroyUnityObject(update.PreviousMesh);
                }
            }
        }

        private RoadNode CreateNode(Vector3Int cellPosition, Vector3 worldPosition)
        {
            GameObject nodeObject = new($"Road Node {cellPosition}");
            try
            {
                nodeObject.transform.SetParent(roadNetwork.transform, false);
                RoadNode node = nodeObject.AddComponent<RoadNode>();
                node.Initialize(cellPosition, worldPosition);
                return node;
            }
            catch
            {
                DestroyUnityObject(nodeObject);
                throw;
            }
        }

        private static Vector3 GetTerrainEndpointPosition(TerrainCell cell)
        {
            Vector3 position = cell.Center;
            position.y = cell.AverageWorldHeight;
            return position;
        }

        private static bool IsDiagonal(Vector3Int startCell, Vector3Int endCell)
        {
            return startCell.x != endCell.x && startCell.y != endCell.y;
        }

        private static bool HaveMatchingBuildSettings(
            RoadBuildSettings left,
            RoadBuildSettings right)
        {
            return Approximately(left.Width, right.Width)
                && Approximately(left.SampleSpacing, right.SampleSpacing)
                && Approximately(left.Thickness, right.Thickness)
                && Approximately(left.YOffset, right.YOffset)
                && left.UvOrientation == right.UvOrientation
                && left.ThicknessUvMode == right.ThicknessUvMode
                && ReferenceEquals(left.Material, right.Material);
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= GeometryEpsilon;
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private sealed class StraightSegmentBuildPlan
        {
            public bool HasStartNode { get; }
            public bool HasEndNode { get; }
            public RoadNode StartNode { get; }
            public RoadNode EndNode { get; }
            public Vector3 StartWorldPosition { get; }
            public Vector3 EndWorldPosition { get; }
            public EndpointPlan StartPlan { get; }
            public EndpointPlan EndPlan { get; }
            public List<SegmentGeometryUpdate> SegmentUpdates { get; }

            public StraightSegmentBuildPlan(
                bool hasStartNode,
                bool hasEndNode,
                RoadNode startNode,
                RoadNode endNode,
                Vector3 startWorldPosition,
                Vector3 endWorldPosition,
                EndpointPlan startPlan,
                EndpointPlan endPlan,
                List<SegmentGeometryUpdate> segmentUpdates)
            {
                HasStartNode = hasStartNode;
                HasEndNode = hasEndNode;
                StartNode = startNode;
                EndNode = endNode;
                StartWorldPosition = startWorldPosition;
                EndWorldPosition = endWorldPosition;
                StartPlan = startPlan;
                EndPlan = endPlan;
                SegmentUpdates = segmentUpdates;
            }
        }

        private sealed class EndpointPlan
        {
            public RoadNode Node { get; }
            public Vector3 NodePosition { get; }
            public Vector3 CandidateOtherPosition { get; }
            public IReadOnlyList<RoadSegment> ConnectedSegments { get; }
            public float TrimDistance { get; }
            public bool CreatesIntersection { get; }

            public EndpointPlan(
                RoadNode node,
                Vector3 nodePosition,
                Vector3 candidateOtherPosition,
                IReadOnlyList<RoadSegment> connectedSegments,
                float trimDistance,
                bool createsIntersection)
            {
                Node = node;
                NodePosition = nodePosition;
                CandidateOtherPosition = candidateOtherPosition;
                ConnectedSegments = connectedSegments;
                TrimDistance = trimDistance;
                CreatesIntersection = createsIntersection;
            }
        }

        private sealed class SegmentGeometryUpdate
        {
            public RoadSegment Segment { get; }
            public float PreviousStartTrimDistance { get; }
            public float PreviousEndTrimDistance { get; }
            public float TargetStartTrimDistance { get; set; }
            public float TargetEndTrimDistance { get; set; }
            public Mesh ReplacementMesh { get; set; }
            public Mesh PreviousMesh { get; set; }
            public bool Applied { get; set; }

            public SegmentGeometryUpdate(RoadSegment segment)
            {
                Segment = segment;
                PreviousStartTrimDistance = segment.StartTrimDistance;
                PreviousEndTrimDistance = segment.EndTrimDistance;
                TargetStartTrimDistance = PreviousStartTrimDistance;
                TargetEndTrimDistance = PreviousEndTrimDistance;
            }
        }

        private sealed class IntersectionGeometryUpdate
        {
            public RoadNode Node { get; }
            public RoadIntersection ExistingIntersection { get; }
            public Mesh ReplacementMesh { get; }
            public Mesh PreviousMesh { get; set; }
            public GameObject CreatedObject { get; set; }
            public RoadIntersection CreatedIntersection { get; set; }
            public bool Applied { get; set; }

            public IntersectionGeometryUpdate(RoadNode node, Mesh replacementMesh)
            {
                Node = node;
                ExistingIntersection = node.Intersection;
                ReplacementMesh = replacementMesh;
            }
        }
    }
}
