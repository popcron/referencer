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
            SerializedProperty blacklistFilter = serializedObject.FindProperty("blacklistFilter");

            EditorGUILayout.PropertyField(blacklistFilter, new GUIContent("Blacklist Filter"));

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            Show(serializedObject);
        }
    }
}
