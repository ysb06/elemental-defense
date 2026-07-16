using UnityEditor;
using UnityEngine;

namespace DefCity.Gameplay.City.Construction.Editor
{
    [CustomEditor(typeof(RoadBuilder))]
    public sealed class RoadBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Test", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to test road construction.",
                    MessageType.Info);
                return;
            }

            RoadBuilder roadBuilder = (RoadBuilder)target;
            EditorGUILayout.LabelField(
                "Build Mode",
                roadBuilder.IsBuildModeActive ? "Active" : "Inactive");
            EditorGUILayout.LabelField(
                "Start Cell",
                roadBuilder.StartCell.HasValue
                    ? roadBuilder.StartCell.Value.RefPosition.ToString()
                    : "Not selected");

            if (!roadBuilder.IsBuildModeActive)
            {
                if (GUILayout.Button("Begin Road Build"))
                {
                    roadBuilder.BeginBuild();
                }

                return;
            }

            if (GUILayout.Button("End Road Build"))
            {
                roadBuilder.EndBuild();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }
    }
}
