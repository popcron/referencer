using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;

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

            loadAllMethod = referencesLoaderType.GetMethod("LoadAll");
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
            Settings settings = Settings.Current;
            return (References)loadAllMethod.Invoke(null, new object[] { settings });
        }

        /// <summary>
        /// Marks this object as dirty.
        /// </summary>
        public static void SetDirty(Object obj)
        {
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

#if UNITY_EDITOR
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
    }
}