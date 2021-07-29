using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Popcron.Referencer
{
    /// <summary>
    /// This class relays method class to the Helper class in the editor assembly.
    /// </summary>
    public static class Utils
    {
        private static Dictionary<string, Type> nameToType = new Dictionary<string, Type>();

        public static Dictionary<string, Type> NameToType
        {
            get
            {
                if (nameToType is null || nameToType.Count == 0)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    int assemblyCount = assemblies.Length;
                    for (int i = assemblyCount - 1; i >= 0; i--)
                    {
                        Type[] types = assemblies[i].GetTypes();
                        int typeCount = types.Length;
                        for (int j = 0; j < typeCount; j++)
                        {
                            Type type = types[j];
                            CacheType(type);
                        }
                    }
                }

                return nameToType;
            }
        }

        /// <summary>
        /// Returns the type that this reference is of.
        /// </summary>
        public static Type GetType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return null;
            }

            if (nameToType.TryGetValue(assemblyQualifiedName, out Type resultType))
            {
                return resultType;
            }
            else
            {
                resultType = Type.GetType(assemblyQualifiedName);
                if (resultType != null)
                {
                    CacheType(resultType);
                    return resultType;
                }
            }

            return null;
        }

        private static void CacheType(Type type)
        {
            nameToType[type.AssemblyQualifiedName] = type;
            nameToType[type.FullName] = type;
            nameToType[type.Name] = type;
        }

        /// <summary>
        /// Loads all assets into the asset.
        /// </summary>
        public static void LoadAll()
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool("LoadAllReferences", true);
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