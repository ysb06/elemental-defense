using System;
using UnityEngine;

namespace DefCity.Gameplay.City.Roads
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RoadIntersection : MonoBehaviour
    {
        [SerializeField] private RoadNode node;

        private Mesh generatedMesh;

        public RoadNode Node => node;
        public Mesh Mesh => generatedMesh;

        internal void Initialize(RoadNode targetNode, Mesh mesh, Material material)
        {
            if (node != null || generatedMesh != null)
            {
                throw new InvalidOperationException($"{nameof(RoadIntersection)} has already been initialized.");
            }

            if (targetNode == null)
            {
                throw new ArgumentNullException(nameof(targetNode));
            }

            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            node = targetNode;
            generatedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        internal Mesh ReplaceMesh(Mesh replacement)
        {
            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            Mesh previous = generatedMesh;
            generatedMesh = replacement;
            GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            return previous;
        }

        private void OnDestroy()
        {
            if (node != null)
            {
                node.DetachIntersection(this);
            }

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
