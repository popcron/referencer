using System.Collections.Generic;
using UnityEditor;
using System;

using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class Loader
    {
        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static Object LoadAssetAtPath(string path, Type type)
        {
            return AssetDatabase.LoadAssetAtPath("Assets/" + path, type);
        }

        public static Object[] LoadAllAssetsAtPath(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath("Assets/" + path);
        }

        public static List<string> FindAssets(string filter)
        {
            Settings settings = Settings.Current ?? new Settings();

            List<string> paths = new List<string>();
            string[] assets = AssetDatabase.FindAssets(filter);
            foreach (string guid in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == settings.referencesAssetPath) continue;

                path = path.Replace("Assets/", "");

                bool ignore = false;
                foreach (var ignoredFolder in settings.ignoredFolders)
                {
                    if (path.StartsWith(ignoredFolder))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                {
                    break;
                }

                paths.Add(path);
            }

            return paths;
        }
    }
}
