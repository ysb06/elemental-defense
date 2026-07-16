using UnityEditor;
using UnityEngine;
using DefCity.Tools;

namespace DefCity.Tools.Editor
{
    [CustomEditor(typeof(TerrainWorldHeightCapper))]
    public class TerrainWorldHeightCapperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Run"))
            {
                TerrainWorldHeightCapper capper = (TerrainWorldHeightCapper)target;
                capper.Run();
            }
        }
    }
}
