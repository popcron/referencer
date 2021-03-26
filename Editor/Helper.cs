using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class Helper
    {
        /// <summary>
        /// Loads all assets into the asset.
        /// Returns the true reference that was actually loaded into.
        /// </summary>
        public static void LoadAll()
        {
            Settings settings = Settings.Current;
            if (Application.isPlaying)
            {
                Debug.Log("[Referencer] Loading everything at runtime!");
            }

            List<AssetLoader> loaders = AssetLoader.Loaders;
            if (loaders.Count == 0)
            {
                Debug.LogError("[Referencer] No loaders found");
            }

            //lol fucken cleareth they referencium
            References references = References.Current;
            references.Clear();

            //then loop though all loaders and load the assets of their types
            for (int l = 0; l < loaders.Count; l++)
            {
                float progress = Mathf.Clamp01(l / (float)loaders.Count);
                EditorUtility.DisplayProgressBar("Referencer", "Loading all references", progress);

                AssetLoader loader = loaders[l];
                List<Reference> items = loader.LoadAll(settings);
                for (int i = 0; i < items.Count; i++)
                {
                    Reference item = items[i];
                    references.Add(item);
                }
            }

            EditorUtility.ClearProgressBar();
            SetDirty(references);
        }

        //reflected by Relay
        public static string[] FindAssets(string filter)
        {
            string[] guids = AssetDatabase.FindAssets(filter);
            string[] paths = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                paths[i] = AssetDatabase.GUIDToAssetPath(guid);
            }

            return paths;
        }

        //reflected by Relay
        public static Object LoadAssetAtPath(string path, Type type) => AssetDatabase.LoadAssetAtPath(path, type);

        //reflected by Relay
        public static Object[] LoadAllAssetsAtPath(string path) => AssetDatabase.LoadAllAssetsAtPath(path);

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

            EditorUtility.SetDirty(obj);
        }
    }
}
