using UnityEditor;
using UnityEngine;
using Popcron.Referencer;

namespace Popcron.References
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : Editor
    {
        public static void Show(SerializedObject serializedObject)
        {
            serializedObject.Update();

            SerializedProperty blacklistFilter = serializedObject.FindProperty("blacklistFilter");
            SerializedProperty verbosity = serializedObject.FindProperty("verbosity");

            EditorGUILayout.PropertyField(blacklistFilter, new GUIContent("Blacklist Filter"));
            EditorGUILayout.PropertyField(verbosity, new GUIContent("Verbosity"));

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            Show(serializedObject);
        }
    }
}
