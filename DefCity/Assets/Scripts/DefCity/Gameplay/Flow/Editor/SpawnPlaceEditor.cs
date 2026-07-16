using UnityEditor;
using UnityEngine;

namespace DefCity.Gameplay.Flow.Editor
{
    [CustomEditor(typeof(SpawnPlace))]
    public class SpawnPlaceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Recalculate Spawnable Cells"))
            {
                SpawnPlace spawnPlace = (SpawnPlace)target;
                Undo.RecordObject(spawnPlace, "Recalculate Spawnable Cells");
                spawnPlace.RecalculateSpawnableCells();
                EditorUtility.SetDirty(spawnPlace);
            }
        }
    }
}
