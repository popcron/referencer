using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class Helper
    {
        public static References CreateReferencesFile()
        {
            Settings settings = Settings.Current ?? new Settings();

            References references = null;
            List<References> allReferences = Loader.FindAssetsByType<References>();
            for (int i = 0; i < allReferences.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(allReferences[i]);
                if (path == settings.referencesAssetPath)
                {
                    references = allReferences[i];
                    break;
                }
            }

            //still null, so create a new one
            if (!references)
            {
                references = ScriptableObject.CreateInstance<References>();
                AssetDatabase.CreateAsset(references, settings.referencesAssetPath);
                AssetDatabase.Refresh();
                AssetProcessor.QueueLoad = true;
            }

            return references;
        }

        public static List<string> FindAssets(string filter)
        {
            List<string> paths = new List<string>();
            string[] guids = AssetDatabase.FindAssets(filter);
            foreach (var guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }

        public static Object LoadAssetAtPath(string path, Type type)
        {
            return AssetDatabase.LoadAssetAtPath(path, type);
        }

        public static Object[] LoadAllAssetsAtPath(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path);
        }

        public static void DirtyScene()
        {
            Scene scene = SceneManager.GetActiveScene();

            //scene is invalid, cant set it dirty
            if (!scene.IsValid()) return;

            //scene isnt loaded, cant set it dirty
            if (!scene.isLoaded) return;

            //scene isnt saved in the project, probably a new untitled scene
            if (string.IsNullOrEmpty(scene.path)) return;

            //cant do this while playing
            if (Application.isPlaying) return;

            EditorSceneManager.MarkSceneDirty(scene);
        }

        public static void DirtyReferences()
        {
            References references = References.Instance;

            //references somehow doesnt exist
            if (references == null) return;

            //cant do this while playing
            if (Application.isPlaying) return;

            EditorUtility.SetDirty(references);
        }
    }
}
