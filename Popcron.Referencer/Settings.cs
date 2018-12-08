using System;
using System.IO;
using UnityEngine;

namespace Popcron.Referencer
{
    [Serializable]
    public class Settings
    {
        //constants
        public const string UniqueIdentifier = "Popcron.Referencer.Settings";
        public const string SettingsKey = UniqueIdentifier + ".IgnoredFolders";
        public const string GameObjectNameKey = UniqueIdentifier + ".GameObject";

        /// <summary>
        /// The current settings data being used.
        /// If set to null, it will use defaults
        /// </summary>
        public static Settings Current
        {
            get
            {
                string value = PlayerPrefs.GetString(SettingsKey);
                if (!string.IsNullOrEmpty(value))
                {
                    Settings settings = JsonUtility.FromJson<Settings>(value);
                    return settings;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    PlayerPrefs.DeleteKey(SettingsKey);
                }
                else
                {
                    string json = JsonUtility.ToJson(value);
                    PlayerPrefs.SetString(SettingsKey, json);
                }
            }
        }

        //actual settings
        public string referencesAssetPath = "Assets/References.asset";
        public string[] ignoredFolders = { };
        public string[] ignoreExtensions = { };

        public bool ShouldIgnorePath(string path)
        {
            //check folders
            for (int i = 0; i < ignoredFolders.Length; i++)
            {
                if (path.StartsWith(ignoredFolders[i]))
                {
                    return true;
                }
            }

            //check extensions
            string extension = Path.GetExtension(path).ToLower();
            for (int i = 0; i < ignoreExtensions.Length; i++)
            {
                if (extension == ignoreExtensions[i].ToLower())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
