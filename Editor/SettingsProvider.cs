using Popcron.Referencer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Popcron.References
{
    public class ReferencesSettingsProvider : SettingsProvider
    {
        private SerializedObject settings;

        public ReferencesSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {

        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            settings = new SerializedObject(Settings.Current);
        }

        public override void OnGUI(string searchContext)
        {
            float x = 7;
            float y = 10;
            Rect rect = new Rect(x, y, Screen.width - x, Screen.height - y);
            GUILayout.BeginArea(rect);
            SettingsInspector.Show(settings);
            GUILayout.EndArea();
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            ReferencesSettingsProvider provider = new ReferencesSettingsProvider("Project/References", SettingsScope.Project)
            {
                keywords = new string[] { "references" }
            };

            return provider;
        }
    }
}
