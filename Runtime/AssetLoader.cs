using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public abstract class AssetLoader
    {
        private static List<AssetLoader> allLoaders = null;
        private static Dictionary<Type, AssetLoader> typeToLoader = null;

        public abstract Type Type { get; }
        public abstract List<Reference> LoadAll(Settings settings);
        public abstract List<Reference> Load(string path);

        /// <summary>
        /// List of all available loaders.
        /// </summary>
        public static List<AssetLoader> Loaders
        {
            get
            {
                if (allLoaders is null)
                {
                    allLoaders = new List<AssetLoader>();
                    foreach (KeyValuePair<string, Type> element in Settings.NameToType)
                    {
                        Type type = element.Value;
                        if (!type.IsAbstract && typeof(AssetLoader).IsAssignableFrom(type))
                        {
                            AssetLoader newLoader = (AssetLoader)Activator.CreateInstance(type);
                            allLoaders.Add(newLoader);
                        }
                    }
                }

                return allLoaders;
            }
        }

        /// <summary>
        /// Returns the loader for this type.
        /// </summary>
        public static AssetLoader Get(Type type)
        {
            if (type is null)
            {
                return null;
            }

            if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                type = typeof(ScriptableObject);
            }

            //make sure the dictionary cache exists
            if (typeToLoader is null)
            {
                typeToLoader = new Dictionary<Type, AssetLoader>();
                List<AssetLoader> all = Loaders;
                for (int i = 0; i < all.Count; i++)
                {
                    AssetLoader loader = all[i];
                    typeToLoader[loader.Type] = loader;
                }
            }

            if (typeToLoader.TryGetValue(type, out AssetLoader foundLoader))
            {
                return foundLoader;
            }

            return null;
        }

        /// <summary>
        /// Returns the loader for this type.
        /// </summary>
        public static AssetLoader Get<T>() => Get(typeof(T));

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Initialize()
        {
            _ = Loaders;
        }
#endif
    }
}
