using System;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RoadSegment : MonoBehaviour
    {
        [SerializeField] private RoadNode startNode;
        [SerializeField] private RoadNode endNode;
        [SerializeField] private RoadBuildSettings buildSettings;
        [SerializeField] private float startTrimDistance;
        [SerializeField] private float endTrimDistance;

        private Mesh generatedMesh;

        public RoadNode StartNode => startNode;
        public RoadNode EndNode => endNode;
        public RoadEdge ForwardEdge { get; private set; }
        public RoadEdge ReverseEdge { get; private set; }
        public Mesh Mesh => generatedMesh;
        public RoadBuildSettings BuildSettings => buildSettings;
        public float StartTrimDistance => startTrimDistance;
        public float EndTrimDistance => endTrimDistance;

        internal void Initialize(
            RoadNode targetStartNode,
            RoadNode targetEndNode,
            Mesh mesh,
            RoadBuildSettings settings,
            float targetStartTrimDistance = 0f,
            float targetEndTrimDistance = 0f)
        {
            if (startNode != null || endNode != null || generatedMesh != null)
            {
                throw new InvalidOperationException($"{nameof(RoadSegment)} has already been initialized.");
            }

            if (targetStartNode == null)
            {
                throw new ArgumentNullException(nameof(targetStartNode));
            }

            if (targetEndNode == null)
            {
                throw new ArgumentNullException(nameof(targetEndNode));
            }

            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            settings.Validate();
            ValidateTrimDistance(targetStartTrimDistance, nameof(targetStartTrimDistance));
            ValidateTrimDistance(targetEndTrimDistance, nameof(targetEndTrimDistance));

            startNode = targetStartNode;
            endNode = targetEndNode;
            buildSettings = settings;
            startTrimDistance = targetStartTrimDistance;
            endTrimDistance = targetEndTrimDistance;
            generatedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            GetComponent<MeshRenderer>().sharedMaterial = buildSettings.Material;
            ForwardEdge = new RoadEdge(startNode, endNode, this);
            ReverseEdge = new RoadEdge(endNode, startNode, this);
        }

        internal Mesh ReplaceGeometry(
            Mesh replacement,
            float targetStartTrimDistance,
            float targetEndTrimDistance)
        {
            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            ValidateTrimDistance(targetStartTrimDistance, nameof(targetStartTrimDistance));
            ValidateTrimDistance(targetEndTrimDistance, nameof(targetEndTrimDistance));

            Mesh previous = generatedMesh;
            generatedMesh = replacement;
            startTrimDistance = targetStartTrimDistance;
            endTrimDistance = targetEndTrimDistance;
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            return previous;
        }

        private static void ValidateTrimDistance(float value, string parameterName)
        {
            if (value < 0f || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Road trim distance must be non-negative and finite.");
            }
        }

        private void OnDestroy()
        {
            if (generatedMesh == null)
            {
                return;
            }

            Mesh mesh = generatedMesh;
            generatedMesh = null;
            if (TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh == mesh)
            {
                meshFilter.sharedMesh = null;
            }

            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
        }
    }
}
