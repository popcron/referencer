using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    public class SettingsWindow
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Project/Referencer", SettingsScope.Project)
            {
                label = "Referencer",
                guiHandler = (searchContext) =>
                {
                    OnGUI();
                },

                keywords = new HashSet<string>(new[] { "Referencer", "References" })
            };

            return provider;
        }

        private static void OnGUI()
        {
            Settings current = Settings.Current;
            SerializedObject settings = new SerializedObject(current);
            SerializedProperty referencesAssetPath = settings.FindProperty("referencesAssetPath");
            SerializedProperty blacklistFilter = settings.FindProperty("blacklistFilter");

            EditorGUILayout.PropertyField(referencesAssetPath, new GUIContent("Referencer Asset Path"));
            EditorGUILayout.PropertyField(blacklistFilter, new GUIContent("Blacklist Filter"));

            current.Save();
            settings.ApplyModifiedProperties();
        }
    }
}
