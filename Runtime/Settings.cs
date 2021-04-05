using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Popcron.Referencer
{
    public class Settings : ScriptableObject
    {
        private static Settings current;
        private static Dictionary<string, Type> nameToType;

        public static Dictionary<string, Type> NameToType
        {
            get
            {
                if (nameToType is null)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    nameToType = new Dictionary<string, Type>();
                    for (int a = 0; a < assemblies.Length; a++)
                    {
                        Assembly assembly = assemblies[a];
                        Type[] types = assembly.GetTypes();
                        for (int t = 0; t < types.Length; t++)
                        {
                            Type type = types[t];
                            nameToType[type.FullName] = type;
                        }
                    }
                }

                return nameToType;
            }
        }

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
        /// The current blacklist filter.
        /// </summary>
        public string[] BlacklistFilter
        {
            get => blacklistFilter;
            set => blacklistFilter = value;
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        private static void Initialize()
        {
            _ = NameToType;
        }

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
        /// Returns the type that this reference is of.
        /// </summary>
        public static Type GetType(string fullTypeName)
        {
            if (NameToType.TryGetValue(fullTypeName, out Type resultType))
            {
                return resultType;
            }

            return null;
        }

        /// <summary>
        /// Returns an existing console settings asset, or creates a new one if none exist.
        /// </summary>
        public static Settings GetOrCreate()
        {
            //find from resources
            string name = Path.GetFileNameWithoutExtension(SettingsAssetName);
            Settings settings = Resources.Load<Settings>(name);
            bool exists = settings;
            if (!exists)
            {
                //no console settings asset exists yet, so create one
                settings = CreateInstance<Settings>();
                settings.name = name;
            }

#if UNITY_EDITOR
            if (!exists)
            {
                //ensure the resources folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
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
