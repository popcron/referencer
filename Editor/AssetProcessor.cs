using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    public class AssetProcessor : AssetPostprocessor
    {
        [MenuItem("Popcron/Referencer/Load all")]
        public static void LoadAll()
        {
            //first clear
            References.Clear();

            var loaders = AssetLoader.Loaders;
            if (loaders.Count == 0)
            {
                Debug.LogError("No loaders found");
            }

            //then loop though all loaders and load the assets of their types
            foreach (var loader in loaders)
            {
                List<Reference> items = loader.LoadAll();
                foreach (var item in items)
                {
                    References.Add(item);
                }
            }

            //mark as dirty
            Helper.DirtyReferences();
        }

        [MenuItem("Popcron/Referencer/Clear")]
        public static void Clear()
        {
            References.Clear();

            //mark as dirty
            Helper.DirtyReferences();
        }

        internal static bool QueueLoad
        {
            get
            {
                return EditorPrefs.GetInt(Settings.UniqueIdentifier + ".QueueLoad", 0) == 1;
            }
            set
            {
                EditorPrefs.SetInt(Settings.UniqueIdentifier + ".QueueLoad", value ? 1 : 0);
            }
        }

        private static void Add(string path)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            AssetLoader loader = AssetLoader.Get(type);
            if (loader != null)
            {
                path = path.Replace("Assets/", "");
                List<Reference> items = loader.Load(path);
                foreach (var item in items)
                {
                    References.Add(item);
                }
            }
        }

        private static void Remove(string path)
        {
            path = path.Replace("Assets/", "");
            References.Remove(path);
        }

        private static void Move(string oldPath, string newPath)
        {
            oldPath = oldPath.Replace("Assets/", "");
            newPath = newPath.Replace("Assets/", "");

            var item = References.GetReference(oldPath);
            if (item != null) item.Path = newPath;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            Settings settings = Settings.Current ?? new Settings();

            //add these assets
            foreach (string path in importedAssets)
            {
                //ignore the reference asset file
                if (path == settings.referencesAssetPath)
                {
                    //unless a load all operation was queued
                    if (QueueLoad)
                    {
                        QueueLoad = false;
                        LoadAll();
                    }

                    continue;
                }

                if (settings.ShouldIgnorePath(path)) continue;

                Add(path);

                //mark as dirty
                Helper.DirtyReferences();
            }

            //remove these assets
            foreach (string path in deletedAssets)
            {
                if (settings.ShouldIgnorePath(path)) continue;

                Remove(path);

                //mark as dirty
                Helper.DirtyReferences();
            }

            //delete and add
            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (settings.ShouldIgnorePath(movedFromAssetPaths[i])) continue;
                if (settings.ShouldIgnorePath(movedAssets[i])) continue;

                Move(movedFromAssetPaths[i], movedAssets[i]);

                //mark as dirty
                Helper.DirtyReferences();
            }
        }
    }
}
