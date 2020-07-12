using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Popcron.Referencer
{
    public class Settings : ScriptableObject
    {
        private static Settings current;

        //constants
        public const string SettingsAssetName = "Referencer Settings.asset";
        public const string UniqueIdentifier = "Popcron.Referencer.Settings";
        public const string GameObjectNameKey = UniqueIdentifier + ".GameObject";

        /// <summary>
        /// The current settings data being used.
        /// </summary>
        public static Settings Current
        {
            get
            {
                if (!current)
                {
                    current = GetOrCreate();
                }

                return current;
            }
        }

        [SerializeField]
        private string[] blacklistFilter = { "Resources/", "Game/", ".html" };

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

        /// <summary>
        /// Returns an existing console settings asset, or creates a new one if none exist.
        /// </summary>
        public static Settings GetOrCreate()
        {
            //find from resources
            Settings settings = Resources.Load<Settings>(SettingsAssetName);
            bool exists = settings;
            if (!exists)
            {
                //no console settings asset exists yet, so create one
                settings = CreateInstance<Settings>();
                settings.name = SettingsAssetName;
            }

#if UNITY_EDITOR
            if (!exists)
            {
                //ensure the resources folder exists
                if (!Directory.Exists("Assets/Resources"))
                {
                    Directory.CreateDirectory("Assets/Resources");
                }

                //make a file here
                string path = $"Assets/Resources/{SettingsAssetName}";
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.Refresh();
            }
#endif

            return settings;
        }
    }
}
