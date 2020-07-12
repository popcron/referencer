using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class AssetProcessor : AssetPostprocessor
    {
        [MenuItem("Popcron/Referencer/Load all")]
        public static void LoadAll()
        {
            Helper.LoadAll();
        }

        private static bool Add(string path, References references)
        {
            bool added = false;
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            //if a sprite was edited, also try to load using a sprite loader
            if (type == typeof(Texture2D))
            {
                added |= Add(path, typeof(Sprite), references);
            }

            added |= Add(path, type, references);
            return added;
        }

        private static bool Add(string path, Type type, References references)
        {
            bool added = false;
            AssetLoader loader = AssetLoader.Get(type);
            if (loader != null)
            {
                path = path.Replace("Assets/", "");
                List<Reference> items = loader.Load(path);
                foreach (Reference item in items)
                {
                    added |= references.Add(item);
                }
            }

            return added;
        }

        private static bool Remove(string path, References references)
        {
            path = path.Replace("Assets/", "");
            return references.Remove(path);
        }

        private static bool Move(string oldPath, string newPath, References references)
        {
            oldPath = oldPath.Replace("Assets/", "");
            newPath = newPath.Replace("Assets/", "");

            Reference item = references.GetReference(oldPath);
            if (item != null)
            {
                item.Path = newPath;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            //check if the referencer even exists
            References references = References.Current;
            Settings settings = Settings.Current;
            bool dirty = false;

            //add these assets
            for (int i = 0; i < importedAssets.Length; i++)
            {
                string path = importedAssets[i];
                if (settings.IsBlacklisted(path))
                {
                    continue;
                }

                dirty |= Add(path, references);
            }

            //remove these assets
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                string path = deletedAssets[i];
                if (settings.IsBlacklisted(path))
                {
                    continue;
                }

                dirty |= Remove(path, references);
            }

            //delete and add
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (settings.IsBlacklisted(movedFromAssetPaths[i]))
                {
                    continue;
                }
                else if (settings.IsBlacklisted(movedAssets[i]))
                {
                    continue;
                }

                dirty |= Move(movedFromAssetPaths[i], movedAssets[i], references);
            }

            if (dirty)
            {
                //mark as dirty
                Helper.SetDirty(references);
            }
        }
    }
}
