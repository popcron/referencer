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
                Helper.SetDirty(references);
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

        [MenuItem("Popcron/Referencer/Load all %#_x")]
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
    }
}
