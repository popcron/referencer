using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Popcron.Referencer
{
    /// <summary>
    /// This class relays method class to the Helper class in the editor assembly.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class Utils
    {
        private static Dictionary<string, Type> nameToType = null;
        private static Type referencesLoaderType = null;
        private static MethodInfo loadAllMethod = null;

        public static Dictionary<string, Type> NameToType
        {
            get
            {
                if (nameToType == null)
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
                            nameToType[type.Name] = type;
                        }
                    }
                }

                return nameToType;
            }
        }

        static Utils()
        {
            if (referencesLoaderType is null)
            {
                referencesLoaderType = GetType("Popcron.Referencer.ReferencesLoader");
            }

            if (referencesLoaderType != null)
            {
                loadAllMethod = referencesLoaderType.GetMethod("LoadAll");
            }
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
        /// Loads all assets into the asset.
        /// Returns the true reference that was actually loaded into.
        /// </summary>
        public static References LoadAll()
        {
#if UNITY_EDITOR
            return (References)loadAllMethod.Invoke(null, null);
#else
            return References.Current;
#endif
        }

        /// <summary>
        /// Marks this object as dirty.
        /// </summary>
        public static void SetDirty(Object obj)
        {
#if UNITY_EDITOR
            //references somehow doesnt exist
            if (!obj)
            {
                return;
            }

            //cant do this while playing
            if (Application.isPlaying)
            {
                return;
            }

            EditorUtility.SetDirty(obj);
#endif
        }

        /// <summary>
        /// Returns asset paths of this type.
        /// </summary>
        public static string[] FindAssets<T>() where T : class => FindAssets($"t:{typeof(T)}");

        /// <summary>
        /// Returns the asset paths with this filter as a search query.
        /// </summary>
        public static string[] FindAssets(string filter)
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets(filter);
            string[] paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                paths[i] = AssetDatabase.GUIDToAssetPath(guid);
            }

            return paths;
#else
            return null;
#endif
        }

        /// <summary>
        /// Returns an object from a path.
        /// </summary>
        public static T LoadAssetAtPath<T>(string path) where T : class => LoadAssetAtPath(path, typeof(T)) as T;

        /// <summary>
        /// Returns an object from a path using type.
        /// </summary>
        public static Object LoadAssetAtPath(string path, Type type)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath(path, type);
#else
            return null;
#endif
        }

        /// <summary>
        /// Returns all objects at the path.
        /// </summary>
        public static Object[] LoadAllAssetsAtPath(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAllAssetsAtPath(path);
#else
            return null;
#endif
        }

        /// <summary>
        /// Returns an existing asset of this type if it exists, or creates a new one at this path otherwise.
        /// </summary>
        public static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
#if UNITY_EDITOR
            //find in preloaded assets
            Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            for (int i = 0; i < preloadedAssets.Length; i++)
            {
                Object existingAsset = preloadedAssets[i];
                if (existingAsset is T)
                {
                    return existingAsset as T;
                }
            }

            //find in asset database but not as a preloaded asset
            string[] assetPaths = FindAssets<T>();
            if (assetPaths.Length > 0)
            {
                for (int i = 0; i < assetPaths.Length; i++)
                {
                    T existingAsset = LoadAssetAtPath<T>(assetPaths[0]);
                    if (existingAsset)
                    {
                        preloadedAssets[i] = existingAsset;
                        PlayerSettings.SetPreloadedAssets(preloadedAssets);
                        return existingAsset;
                    }
                }
            }

            //attempt to assign a new instance to an empty slot
            for (int i = 0; i < preloadedAssets.Length; i++)
            {
                if (!preloadedAssets[i])
                {
                    preloadedAssets[i] = CreateInstance<T>(path);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets);
                    return preloadedAssets[i] as T;
                }
            }

            //didnt assign to an empty slot, add to the list
            List<Object> preloadedAssetsList = preloadedAssets.ToList();
            T newAsset = CreateInstance<T>(path);
            preloadedAssetsList.Add(newAsset);
            PlayerSettings.SetPreloadedAssets(preloadedAssetsList.ToArray());
            return newAsset;

#else
            T asset = null;
            return asset;
#endif
        }

#if UNITY_EDITOR
        private static T CreateInstance<T>(string path) where T : ScriptableObject
        {
            string name = Path.GetFileNameWithoutExtension(path);
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;

            //make a file here
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.Refresh();
            return asset;
        }
#endif
    }
}