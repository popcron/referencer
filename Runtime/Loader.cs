using System;
using System.Collections.Generic;
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
            string[] paths = Utils.FindAssets<T>();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                T asset = Utils.LoadAssetAtPath<T>(path);
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
        public static Object LoadAssetAtPath(string path, Type type) => Utils.LoadAssetAtPath($"Assets/{path}", type);

        /// <summary>
        /// Loads all assets from this relative path.
        /// <code>
        /// Example: Prefabs/Projectiles
        /// </code>
        /// </summary>
        public static Object[] LoadAllAssetsAtPath(string path) => Utils.LoadAllAssetsAtPath($"Assets/{path}");

        /// <summary>
        /// Returns paths to all assets that match this filter using AssetDatabase.
        /// </summary>
        public static string[] FindAssets(string filter)
        {
            string[] paths = Utils.FindAssets(filter);
            for (int i = 0; i < paths.Length; i++)
            {
                ref string path = ref paths[i];
                path = path.Replace("Assets/", "");
            }

            return paths;
        }
    }
}
