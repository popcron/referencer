using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private static Type referencesLoaderType = null;
        private static MethodInfo loadAllMethod = null;

        static Utils()
        {
            if (referencesLoaderType is null)
            {
                referencesLoaderType = Type.GetType("Popcron.Referencer.ReferencesLoader, Popcron.Referencer.Editor");
            }

            //still didnt find, scrape everything
            if (referencesLoaderType is null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly.FullName.StartsWith("Popcron.Referencer.Editor,"))
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.FullName == "Popcron.Referencer.ReferencesLoader")
                            {
                                referencesLoaderType = type;
                            }
                        }
                    }
                }
            }

            loadAllMethod = referencesLoaderType.GetMethod("LoadAll");
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