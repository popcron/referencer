using Popcron.Referencer;
using UnityEditor;
using UnityEngine;

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
