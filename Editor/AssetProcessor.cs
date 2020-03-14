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
            References references = Helper.LoadAll();
            Helper.PutInsideContainer(references);
        }

        [MenuItem("Popcron/Referencer/Clean scene")]
        public static void CleanScene()
        {
            ReferencesContainer[] containers = Object.FindObjectsOfType<ReferencesContainer>();
            foreach (ReferencesContainer container in containers)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(container.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(container.gameObject);
                }
            }

            Helper.SetSceneDirty();
        }

        private static void Add(string path, References references)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);

            //if a sprite was edited, also try to load using a sprite loader
            if (type == typeof(Texture2D))
            {
                Add(path, typeof(Sprite), references);
            }

            Add(path, type, references);
        }

        private static void Add(string path, Type type, References references)
        {
            AssetLoader loader = AssetLoader.Get(type);
            if (loader != null)
            {
                path = path.Replace("Assets/", "");
                List<Reference> items = loader.Load(path);
                foreach (Reference item in items)
                {
                    references.Add(item);
                }
            }
        }

        private static void Remove(string path, References references)
        {
            path = path.Replace("Assets/", "");
            references.Remove(path);
        }

        private static void Move(string oldPath, string newPath, References references)
        {
            oldPath = oldPath.Replace("Assets/", "");
            newPath = newPath.Replace("Assets/", "");

            Reference item = references.GetReference(oldPath);
            if (item != null)
            {
                item.Path = newPath;
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            //check if the referencer even exists
            References references;
            if (!Helper.DoesReferenceInstanceExist)
            {
                Debug.Log("[Referencer] Loading new asset but a references asset isnt present.");
                references = Helper.LoadAll();
                Helper.PutInsideContainer(references);
                return;
            }

            references = Helper.GetReferencesInstance(false);
            Settings settings = Settings.Current ?? new Settings();

            //add these assets
            foreach (string path in importedAssets)
            {
                //ignore the reference asset file
                if (path == settings.referencesAssetPath)
                {
                    continue;
                }

                if (settings.ShouldIgnorePath(path))
                {
                    continue;
                }

                Add(path, references);

                //mark as dirty
                Helper.SetDirty(references);
            }

            //remove these assets
            foreach (string path in deletedAssets)
            {
                //if the deleted file is the references asset itself
                //then do a full reload
                if (path == settings.referencesAssetPath)
                {
                    Debug.Log("[Referencer] References asset was deleted.");
                    continue;
                }

                if (settings.ShouldIgnorePath(path))
                {
                    continue;
                }

                Remove(path, references);

                //mark as dirty
                Helper.SetDirty(references);
            }

            //delete and add
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (settings.ShouldIgnorePath(movedFromAssetPaths[i]))
                {
                    continue;
                }

                if (settings.ShouldIgnorePath(movedAssets[i]))
                {
                    continue;
                }

                Move(movedFromAssetPaths[i], movedAssets[i], references);

                //mark as dirty
                Helper.SetDirty(references);
            }
        }
    }
}
