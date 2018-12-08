using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Popcron.Referencer
{
    public abstract class AssetLoader
    {
        private static Dictionary<Type, AssetLoader> typeToLoader = new Dictionary<Type, AssetLoader>();

        public abstract Type Type { get; }
        public abstract List<Reference> LoadAll();
        public abstract List<Reference> Load(string path);

        public static List<AssetLoader> Loaders
        {
            get
            {
                RefreshCache();
                List<AssetLoader> loaders = new List<AssetLoader>();

                for (int i = 0; i < typeToLoader.Count; i++)
                {
                    loaders.Add(typeToLoader.ElementAt(i).Value);
                }

                return loaders;
            }
        }

        private static void RefreshCache()
        {
            if (typeToLoader == null)
            {
                typeToLoader = new Dictionary<Type, AssetLoader>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsAbstract) continue;
                        if (type.IsSubclassOf(typeof(AssetLoader)))
                        {
                            AssetLoader assetLoader = (AssetLoader)Activator.CreateInstance(type);
                            typeToLoader.Add(assetLoader.Type, assetLoader);

                            Debug.Log("Created " + assetLoader.GetType().FullName);
                        }
                    }
                }

                Debug.Log("Found " + typeToLoader.Count + " loaders");
            }
        }

        public static AssetLoader Get(Type type)
        {
            RefreshCache();

            if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                type = typeof(ScriptableObject);
            }

            if (typeToLoader.TryGetValue(type, out AssetLoader loader))
            {
                return loader;
            }
            else
            {
                return null;
            }
        }

        public static AssetLoader Get<T>()
        {
            RefreshCache();

            Type type = typeof(T);
            if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                type = typeof(ScriptableObject);
            }

            if (typeToLoader.TryGetValue(type, out AssetLoader loader))
            {
                return loader;
            }
            else
            {
                return null;
            }
        }
    }
}
