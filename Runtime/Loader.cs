using System.Reflection;
using System.Collections.Generic;
using System;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class Loader
    {
        /// <summary>
        /// Loads all assets of this kind using AssetDatabase.
        /// </summary>
        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            List<string> paths = Relay.FindAssets<T>();
            foreach (string path in paths)
            {
                T asset = Relay.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        /// <summary>
        /// Loads the primary asset from this relative path.
        /// <code>
        /// Example: Prefabs/Projectiles/Bullet.prefab
        /// </code>
        /// </summary>
        public static Object LoadAssetAtPath(string path, Type type) => Relay.LoadAssetAtPath($"Assets/{path}", type);

        /// <summary>
        /// Loads all assets from this relative path.
        /// <code>
        /// Example: Prefabs/Projectiles
        /// </code>
        /// </summary>
        public static Object[] LoadAllAssetsAtPath(string path) => Relay.LoadAllAssetsAtPath($"Assets/{path}");

        /// <summary>
        /// Returns paths to all assets that match this filter using AssetDatabase.
        /// </summary>
        public static List<string> FindAssets(string filter)
        {
            Settings settings = Settings.Current;
            List<string> paths = new List<string>();
            List<string> assets = Relay.FindAssets(filter);
            for (int i = 0; i < assets.Count; i++)
            {
                string path = assets[i];
                if (path.Equals(settings.ReferencesAssetPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                path = path.Replace("Assets/", "");
                if (!settings.IsBlacklisted(path))
                {
                    paths.Add(path);
                }
            }

            return paths;
        }

        /// <summary>
        /// Returns an ID from this asset.
        /// </summary>
        public static long? GetIDFromScriptableObject(Object unityObject)
        {
            if (!unityObject)
            {
                return (long?)null;
            }

            if (unityObject is ScriptableObject)
            {
                long? id = null;
                PropertyInfo property = unityObject.GetType().GetProperty("ID");
                FieldInfo field = unityObject.GetType().GetField("id");

                try
                {
                    //found id property
                    if (property != null)
                    {
                        object value = property.GetValue(unityObject, null);
                        if (value != null)
                        {
                            id = Convert.ChangeType(value, typeof(long)) as long?;
                        }
                    }
                    else if (field != null)
                    {
                        object value = field.GetValue(unityObject);
                        if (value != null)
                        {
                            id = Convert.ChangeType(value, typeof(long)) as long?;
                        }
                    }
                }
                catch
                {

                }

                return id;
            }
            else
            {
                return (long?)null;
            }
        }
    }
}
