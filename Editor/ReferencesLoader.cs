using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    [InitializeOnLoad]
    public static class ReferencesLoader
    {
        private static FileSystemWatcher fileSystemWatcher;
        private static string projectFolder;
        private static Stack<FileSystemEventArgs> fileSystemEvent = new Stack<FileSystemEventArgs>();

        static ReferencesLoader()
        {
            projectFolder = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
            fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = Application.dataPath;
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.Deleted += OnDeleted;
            fileSystemWatcher.Created += OnCreated;

            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            References references = References.Current;
            bool hasChanged = false;
            while (fileSystemEvent.Count > 0)
            {
                FileSystemEventArgs fileEvent = fileSystemEvent.Pop();
                if (fileEvent != null)
                {
                    Profiler.BeginSample("Process file system event");

                    string assetPath = fileEvent.FullPath.Replace(".meta", "");
                    assetPath = assetPath.Replace("\\", "/");
                    assetPath = assetPath.Substring(projectFolder.Length + 1);
                    if (File.Exists(fileEvent.FullPath))
                    {
                        bool success = Add(assetPath, references);
                        hasChanged |= success;
                    }
                    else
                    {
                        bool success = Remove(assetPath, references);
                        hasChanged |= success;
                    }

                    Profiler.EndSample();
                }
            }

            if (hasChanged)
            {
                Utils.SetDirty(references);
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (IsMetaFile(e))
            {
                fileSystemEvent.Push(e);
            }
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (IsMetaFile(e))
            {
                fileSystemEvent.Push(e);
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (IsMetaFile(e))
            {
                fileSystemEvent.Push(e);
            }
        }

        private static bool IsMetaFile(FileSystemEventArgs e)
        {
            return e.FullPath.IndexOf(".meta", StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>
        /// Loads all assets into the asset.
        /// Returns the true reference that was actually loaded into.
        /// </summary>
        [MenuItem("Popcron/Referencer/Load all %#_x")]
        private static void LoadAllThroughMenu()
        {
            Settings settings = Settings.Current;
            if ((settings.Verbosity & Settings.LogVerbosity.LogLoadReasons) == Settings.LogVerbosity.LogLoadReasons)
            {
                Debug.Log("[Referencer] Loading all because user asked");
            }

            LoadAll();
        }

        /// <summary>
        /// Loads all assets into the asset.
        /// Returns the true reference that was actually loaded into.
        /// </summary>
        public static References LoadAll(Settings settings = null)
        {
            if (!settings)
            {
                settings = Settings.Current;
            }

            if (Application.isPlaying)
            {
                if ((settings.Verbosity & Settings.LogVerbosity.LogLoadReasons) == Settings.LogVerbosity.LogLoadReasons)
                {
                    Debug.Log("[Referencer] Loading everything at runtime!");
                }
            }

            List<AssetLoader> loaders = AssetLoader.Loaders;
            if (loaders.Count == 0)
            {
                if ((settings.Verbosity & Settings.LogVerbosity.Errors) == Settings.LogVerbosity.Errors)
                {
                    Debug.LogError("[Referencer] No loaders found");
                }
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
                    references.Add(item, settings);
                }
            }

            EditorUtility.ClearProgressBar();
            Utils.SetDirty(references);

            //log load count
            if (Application.isBatchMode || (settings.Verbosity & Settings.LogVerbosity.LogLoadCount) == Settings.LogVerbosity.LogLoadCount)
            {
                Debug.Log($"[Referencer] Loaded {references.Assets.Count} assets");
            }

            //log settings
            if (Application.isBatchMode || (settings.Verbosity & Settings.LogVerbosity.LogSettings) == Settings.LogVerbosity.LogSettings)
            {
                Debug.Log($"[Referencer] Settings used: {JsonUtility.ToJson(settings, true)}");
            }

            return references;
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
            Settings settings = Settings.Current;
            bool added = false;
            AssetLoader loader = AssetLoader.Get(type);
            if (loader != null)
            {
                path = path.Substring(7);
                List<Reference> items = loader.Load(path);
                foreach (Reference item in items)
                {
                    added |= references.Add(item, settings);
                }
            }

            return added;
        }

        private static bool Remove(string path, References references)
        {
            path = path.Replace("Assets/", "");
            return references.Remove(path);
        }
    }
}
