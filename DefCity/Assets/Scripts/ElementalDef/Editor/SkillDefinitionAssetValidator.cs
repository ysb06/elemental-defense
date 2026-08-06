using System;
using System.Collections.Generic;
using ElementalDef.Gameplay.Combat.Skills;
using UnityEditor;
using UnityEngine;

namespace ElementalDef.EditorTools
{
    public static class SkillDefinitionAssetValidator
    {
        private const string MenuPath = "ElementalDef/Validate Skill Definitions";

        [MenuItem(MenuPath)]
        public static void ValidateSkillDefinitions()
        {
            string[] assetGuids = AssetDatabase.FindAssets(
                $"t:{nameof(SkillDefinition)}");
            string[] assetPaths = new string[assetGuids.Length];
            for (int i = 0; i < assetGuids.Length; i++)
            {
                assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            }

            Array.Sort(assetPaths, StringComparer.Ordinal);

            if (assetPaths.Length == 0)
            {
                Debug.Log("Skill definition validation succeeded: no assets were found.");
                return;
            }

            List<string> errors = new();
            Dictionary<string, string> firstPathBySkillId =
                new(StringComparer.Ordinal);

            for (int i = 0; i < assetPaths.Length; i++)
            {
                string assetPath = assetPaths[i];
                SkillDefinition definition =
                    AssetDatabase.LoadAssetAtPath<SkillDefinition>(assetPath);
                if (definition == null)
                {
                    errors.Add($"{assetPath}: The SkillDefinition asset could not be loaded.");
                    continue;
                }

                try
                {
                    definition.ValidateOrThrow();
                }
                catch (Exception exception)
                {
                    errors.Add(
                        $"{assetPath}: {exception.GetType().Name}: {exception.Message}");
                }

                string skillId = definition.SkillId;
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    errors.Add($"{assetPath}: SkillId is blank.");
                    continue;
                }

                if (firstPathBySkillId.TryGetValue(
                        skillId,
                        out string firstAssetPath))
                {
                    errors.Add(
                        $"Duplicate SkillId '{skillId}': " +
                        $"'{firstAssetPath}' and '{assetPath}'.");
                    continue;
                }

                firstPathBySkillId.Add(skillId, assetPath);
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Skill definition validation failed with {errors.Count} error(s):\n- " +
                    string.Join("\n- ", errors));
            }

            Debug.Log(
                $"Skill definition validation succeeded for {assetPaths.Length} asset(s).");
        }
    }
}
