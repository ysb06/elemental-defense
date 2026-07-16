using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DefCity.Gameplay.City.Roads.Geometry;

namespace DefCity.Gameplay.City.Roads.Geometry.Editor
{
    [CustomEditor(typeof(RoadMeshGeneratorPreview))]
    [CanEditMultipleObjects]
    public class RoadMeshGeneratorPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                foreach (Object selectedTarget in targets)
                {
                    RoadMeshGeneratorPreview preview = (RoadMeshGeneratorPreview)selectedTarget;
                    preview.TryGetComponent(out MeshFilter meshFilter);
                    preview.TryGetComponent(out MeshRenderer meshRenderer);

                    List<Object> undoTargets = new() { preview };
                    if (meshFilter != null)
                    {
                        undoTargets.Add(meshFilter);
                    }

                    if (meshRenderer != null)
                    {
                        undoTargets.Add(meshRenderer);
                    }

                    Undo.RecordObjects(undoTargets.ToArray(), "Generate Road Mesh Preview");

                    preview.Generate();
                    EditorUtility.SetDirty(preview);

                    if (meshFilter != null)
                    {
                        EditorUtility.SetDirty(meshFilter);
                    }

                    if (meshRenderer != null)
                    {
                        EditorUtility.SetDirty(meshRenderer);
                    }
                }
            }
        }
    }
}
