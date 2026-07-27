using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using ElementalDef.Gameplay.World;

namespace ElementalDef
{
    [CustomEditor(typeof(MapGenerator))]
    public sealed class MapGeneratorEditor : Editor
    {
        private MapGenerator EditTarget => (MapGenerator)target;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                RegisterGenerationUndo("Generate Map");
                EditTarget.GenerateMap(
                    EditTarget.MapOrigin,
                    EditTarget.Width,
                    EditTarget.Height);
                MarkGeneratedDataDirty();
            }

            if (GUILayout.Button("Generate Demo"))
            {
                RegisterGenerationUndo("Generate Demo Map");
                EditTarget.GenerateDemo();
                MarkGeneratedDataDirty();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Clear"))
            {
                RegisterGenerationUndo("Clear Map");
                EditTarget.ClearMap();
                MarkGeneratedDataDirty();
                SceneView.RepaintAll();
            }

            if (!string.IsNullOrEmpty(EditTarget.LastDemoMessage))
            {
                MessageType messageType =
                    EditTarget.LastDemoFailureReason ==
                    PathSearchFailureReason.None
                    ? MessageType.Info
                    : MessageType.Error;
                EditorGUILayout.HelpBox(
                    EditTarget.LastDemoMessage,
                    messageType);
            }
        }

        [MenuItem("CONTEXT/MapGenerator/Generate Demo")]
        private static void GenerateDemoFromContextMenu(MenuCommand command)
        {
            MapGenerator generator = (MapGenerator)command.context;
            if (!Application.isPlaying)
            {
                RegisterGenerationUndo(generator, "Generate Demo Map");
            }

            generator.GenerateDemo();
            if (!Application.isPlaying)
            {
                MarkGeneratedDataDirty(generator);
            }

            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            if (!EditTarget.HasSuccessfulDemo ||
                EditTarget.GroundTilemap == null)
            {
                return;
            }

            DrawPath(
                EditTarget.GroundTilemap,
                EditTarget.LastDemoPath,
                new Color(0f, 0.75f, 1f, 0.9f),
                5f);

            for (int patternIndex = 0;
                 patternIndex < EditTarget.LastDemoPatterns.Count;
                 patternIndex++)
            {
                MapPattern pattern =
                    EditTarget.LastDemoPatterns[patternIndex];
                DrawPath(
                    EditTarget.GroundTilemap,
                    pattern.FixedPath,
                    new Color(1f, 0.65f, 0f, 1f),
                    9f);

                Vector3 entryPosition = GetCellTop(
                    EditTarget.GroundTilemap,
                    pattern.Entry);
                Vector3 exitPosition = GetCellTop(
                    EditTarget.GroundTilemap,
                    pattern.Exit);
                Handles.color = new Color(1f, 0.25f, 0.75f, 1f);
                Handles.SphereHandleCap(
                    0,
                    entryPosition,
                    Quaternion.identity,
                    0.22f,
                    EventType.Repaint);
                Handles.color = new Color(1f, 0.9f, 0.1f, 1f);
                Handles.SphereHandleCap(
                    0,
                    exitPosition,
                    Quaternion.identity,
                    0.22f,
                    EventType.Repaint);
                Handles.Label(
                    entryPosition + Vector3.up * 0.15f,
                    $"P{patternIndex + 1} Entry");
                Handles.Label(
                    exitPosition + Vector3.up * 0.15f,
                    $"P{patternIndex + 1} Exit");
            }

            DrawEndpoint(
                EditTarget.GroundTilemap,
                EditTarget.LastDemoStart,
                Color.green,
                "Start");
            DrawEndpoint(
                EditTarget.GroundTilemap,
                EditTarget.LastDemoEnd,
                Color.red,
                "End");
        }

        private static void DrawPath(
            Tilemap tilemap,
            System.Collections.Generic.IReadOnlyList<Vector2Int> path,
            Color color,
            float width)
        {
            if (path.Count < 2)
            {
                return;
            }

            Vector3[] points = new Vector3[path.Count];
            for (int index = 0; index < path.Count; index++)
            {
                points[index] = GetCellTop(tilemap, path[index]);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(width, points);
        }

        private static void DrawEndpoint(
            Tilemap tilemap,
            Vector2Int cell,
            Color color,
            string label)
        {
            Vector3 position = GetCellTop(tilemap, cell);
            Handles.color = color;
            Handles.SphereHandleCap(
                0,
                position,
                Quaternion.identity,
                0.32f,
                EventType.Repaint);
            Handles.Label(position + Vector3.up * 0.2f, label);
        }

        private static Vector3 GetCellTop(Tilemap tilemap, Vector2Int cell)
        {
            return tilemap.GetCellCenterWorld(
                new Vector3Int(cell.x, cell.y, 0)) + Vector3.up * 0.55f;
        }

        private void RegisterGenerationUndo(string label)
        {
            RegisterGenerationUndo(EditTarget, label);
        }

        private static void RegisterGenerationUndo(
            MapGenerator generator,
            string label)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (generator.GroundTilemap == null)
            {
                Undo.RegisterCompleteObjectUndo(generator, label);
                return;
            }

            Undo.RegisterCompleteObjectUndo(
                new Object[] { generator, generator.GroundTilemap },
                label);
        }

        private void MarkGeneratedDataDirty()
        {
            MarkGeneratedDataDirty(EditTarget);
        }

        private static void MarkGeneratedDataDirty(MapGenerator generator)
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorUtility.SetDirty(generator);
            if (generator.GroundTilemap != null)
            {
                EditorUtility.SetDirty(generator.GroundTilemap);
            }
        }

        private void HandleUndoRedo()
        {
            Repaint();
            SceneView.RepaintAll();
        }
    }
}
