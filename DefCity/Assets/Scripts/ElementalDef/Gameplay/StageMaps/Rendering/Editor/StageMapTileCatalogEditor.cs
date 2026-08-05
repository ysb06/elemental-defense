using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ElementalDef.Gameplay.StageMaps.Rendering.Editor
{
    [CustomEditor(typeof(StageMapTileCatalog))]
    public sealed class StageMapTileCatalogEditor : UnityEditor.Editor
    {
        private string[] validationErrors = Array.Empty<string>();

        private void OnEnable()
        {
            RefreshValidation();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Validate Catalog"))
            {
                RefreshValidation();
            }

            if (changed)
            {
                RefreshValidation();
            }

            DrawValidationResult();
        }

        private void RefreshValidation()
        {
            if (target is StageMapTileCatalog catalog)
            {
                validationErrors = catalog.GetValidationErrors().ToArray();
            }
            else
            {
                validationErrors = Array.Empty<string>();
            }
        }

        private void DrawValidationResult()
        {
            EditorGUILayout.Space();
            if (validationErrors.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "The tile catalog is valid.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                $"The tile catalog has {validationErrors.Length} error(s).",
                MessageType.Error);

            for (int index = 0; index < validationErrors.Length; index++)
            {
                EditorGUILayout.HelpBox(
                    validationErrors[index],
                    MessageType.Error);
            }
        }
    }
}
