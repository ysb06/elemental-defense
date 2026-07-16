using UnityEngine;
using UnityEngine.Serialization;

namespace DefCity.Gameplay.City.Roads.Geometry
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadMeshGeneratorPreview : MonoBehaviour
    {
        public Vector3 StartNode = Vector3.zero;
        public Vector3 EndNode = new(10f, 0f, 0f);
        [Min(0.01f)] public float Width = 6f;
        [Min(0.01f)] public float SampleSpacing = 10f;
        [FormerlySerializedAs("Height")]
        [Min(0.01f)] public float Thickness = 0.2f;
        public RoadUvOrientation UvOrientation = RoadUvOrientation.AlongRoad;
        public RoadThicknessUvMode ThicknessUvMode = RoadThicknessUvMode.ContinuousAcrossWidth;
        public float YOffset;
        public Material RoadMaterial;

        private Mesh generatedMesh;

        public void Generate()
        {
            Mesh mesh = RoadMeshGenerator.BuildStraightStripWithThickness(
                StartNode,
                EndNode,
                Width,
                SampleSpacing,
                Thickness,
                yOffset: YOffset,
                uvOrientation: UvOrientation,
                thicknessUvMode: ThicknessUvMode);
            mesh.name = $"{name} Road Preview Mesh";
            ReplaceGeneratedMesh(mesh);

            if (RoadMaterial != null && TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.sharedMaterial = RoadMaterial;
            }
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
