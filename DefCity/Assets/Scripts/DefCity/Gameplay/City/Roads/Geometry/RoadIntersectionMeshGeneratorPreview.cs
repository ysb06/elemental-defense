using UnityEngine;

namespace DefCity.Gameplay.City.Roads.Geometry
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadIntersectionMeshGeneratorPreview : MonoBehaviour
    {
        public Vector3 Center = Vector3.zero;
        public Vector3[] Directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
        [Min(0.01f)] public float Width = 6f;
        [Min(0f)] public float ConnectionDistance = 3f;
        [Min(0.01f)] public float Thickness = 0.2f;
        public RoadUvOrientation UvOrientation = RoadUvOrientation.AcrossRoad;
        public float YOffset;
        public Material RoadMaterial;

        private Mesh generatedMesh;

        public void Generate()
        {
            RoadIntersectionPort[] ports = BuildPorts();
            Mesh mesh = RoadIntersectionMeshGenerator.BuildConvexHullIntersection(
                Center,
                ports,
                Width,
                Thickness,
                yOffset: YOffset,
                uvOrientation: UvOrientation);
            mesh.name = $"{name} Road Intersection Preview Mesh";
            ReplaceGeneratedMesh(mesh);

            if (RoadMaterial != null && TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterial = RoadMaterial;
            }
        }

        private RoadIntersectionPort[] BuildPorts()
        {
            int directionCount = Directions != null ? Directions.Length : 0;
            RoadIntersectionPort[] ports = new RoadIntersectionPort[directionCount];
            for (int i = 0; i < directionCount; i++)
            {
                ports[i] = new RoadIntersectionPort(Directions[i], ConnectionDistance);
            }

            return ports;
        }

        private void ReplaceGeneratedMesh(Mesh mesh)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh previousMesh = generatedMesh;
            generatedMesh = mesh;
            meshFilter.sharedMesh = generatedMesh;

            if (previousMesh != null)
            {
                DestroyMesh(previousMesh);
            }
        }

        private void OnDestroy()
        {
            if (generatedMesh != null)
            {
                DestroyMesh(generatedMesh);
                generatedMesh = null;
            }
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (Application.isPlaying)
            {
                Destroy(mesh);
                return;
            }

            DestroyImmediate(mesh);
        }
    }
}
