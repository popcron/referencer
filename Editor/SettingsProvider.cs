using Popcron.Referencer;
using UnityEditor;
using UnityEngine;

namespace Popcron.References
{
    public class ReferencesSettingsProvider : SettingsProvider
    {
        private SerializedObject settings;

        public ReferencesSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {

        }

        public override void OnGUI(string searchContext)
        {
            if (settings == null)
            {
                settings = new SerializedObject(Settings.Current);
            }

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
