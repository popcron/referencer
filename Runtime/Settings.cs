using System;
using System.IO;
using UnityEngine;

namespace Popcron.Referencer
{
    public class Settings : ScriptableObject
    {
        private static Settings current;

        //constants
        public const string UniqueIdentifier = "Popcron.Referencer.Settings";
        public const string SettingsKey = UniqueIdentifier + ".IgnoredFolders";
        public const string GameObjectNameKey = UniqueIdentifier + ".GameObject";

        /// <summary>
        /// Path to the file that should store the settings asset.
        /// </summary>
        private static string PathToFile
        {
            get
            {
                string dir = Directory.GetParent(Application.dataPath).FullName;
                dir = Path.Combine(dir, "ProjectSettings");
                string fileName = "ReferencerSettings.asset";
                return Path.Combine(dir, fileName);
            }
        }

        /// <summary>
        /// The current settings data being used.
        /// If set to null, it will use defaults
        /// </summary>
        public static Settings Current
        {
            get
            {
                if (!current)
                {
                    current = CreateInstance<Settings>();
                    if (File.Exists(PathToFile))
                    {
                        //try to load from file first
                        string json = File.ReadAllText(PathToFile);
                        try
                        {
                            JsonUtility.FromJsonOverwrite(json, current);
                        }
                        catch
                        {

                        }
                    }
                }

                return current;
            }
        }

        /// <summary>
        /// Saves this settings to a local file.
        /// </summary>
        public void Save()
        {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(PathToFile, json);
        }

        [SerializeField]
        private string referencesAssetPath = "Assets/References.asset";

        [SerializeField]
        private string[] blacklistFilter = { "Resources/", "Game/", ".html" };

        /// <summary>
        /// Path to the references asset file.
        /// Example: Assets/References.asset
        /// </summary>
        public string ReferencesAssetPath => referencesAssetPath;

        /// <summary>
        /// Returns true if this path has overlap with the blacklist filter.
        /// </summary>
        public bool IsBlacklisted(string path)
        {
            for (int i = 0; i < blacklistFilter.Length; i++)
            {
                if (path.IndexOf(blacklistFilter[i], StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
