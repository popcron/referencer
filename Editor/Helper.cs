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
        private static References references;

        /// <summary>
        /// Returns true if the asset for references exists in the project.
        /// </summary>
        public static bool DoesReferenceInstanceExist
        {
            get
            {
                Settings settings = Settings.Current;
                string path = settings.ReferencesAssetPath;
                return AssetDatabase.LoadAssetAtPath<References>(path) != null;
            }
        }

        /// <summary>
        /// <para>Returns an instance of the references asset.</para>
        /// If <c>loadAll</c> is false, the asset list wont be loaded in automatically if references
        /// Doesnt exist.
        /// </summary>
        public static References GetReferencesInstance(bool loadAll)
        {
            if (!references)
            {
                //null, try to find again from assets
                Settings settings = Settings.Current;
                string path = settings.ReferencesAssetPath;
                References newReferences = AssetDatabase.LoadAssetAtPath<References>(path);
                if (!newReferences)
                {
                    //the asset dont exist, create it and load all for it
                    newReferences = ScriptableObject.CreateInstance<References>();
                    if (loadAll)
                    {
                        LoadAll(newReferences);
                    }
                }

                references = newReferences;
            }

            return references;
        }

        /// <summary>
        /// Loads all assets into the asset.
        /// Returns the true reference that was actually loaded into.
        /// </summary>
        public static References LoadAll(References references = null)
        {
            Settings settings = Settings.Current;
            if (Application.isPlaying)
            {
                Debug.Log("[Referencer] Loading everything at runtime!");
            }

            bool created = false;
            if (!references)
            {
                //create an new instance then
                references = GetReferencesInstance(false);
                created = true;
            }

            //check if this reference exists as an instance in the project
            string realPath = AssetDatabase.GetAssetPath(references);
            if (!realPath.Equals(settings.ReferencesAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                //not an actual reference instance, so create a new one and replace it
                references = ScriptableObject.CreateInstance<References>();
                created = true;
            }

            List<AssetLoader> loaders = AssetLoader.Loaders;
            if (loaders.Count == 0)
            {
                Debug.LogError("[Referencer] No loaders found");
            }

            //lol fucken cleareth they referencium
            references.Clear();

            //then loop though all loaders and load the assets of their types
            int loadedItems = 0;
            for (int l = 0; l < loaders.Count; l++)
            {
                AssetLoader loader = loaders[l];
                List<Reference> items = loader.LoadAll();
                for (int i = 0; i < items.Count; i++)
                {
                    Reference item = items[i];

                    //check against settings
                    if (Settings.Current.IsBlacklisted(item.Path))
                    {
                        continue;
                    }

                    bool added = references.Add(item);
                    if (added)
                    {
                        loadedItems++;
                    }
                }
            }

            //Debug.Log($"[Referencer] Loaded in {loadedItems} assets into the references.");
            if (created)
            {
                if (!DoesReferenceInstanceExist)
                {
                    //write to disk lmao
                    AssetDatabase.CreateAsset(references, settings.ReferencesAssetPath);
                }

                SetDirty(references);
                AssetDatabase.SaveAssets();
            }
            else
            {
                SetDirty(references);
            }

            return references;
        }

        /// <summary>
        /// Puts this references asset into a container in the current scene.
        /// </summary>
        public static void PutInsideContainer(References references)
        {
            //delete all containers ever but one
            bool dirty = false;
            ReferencesContainer[] containers = Object.FindObjectsOfType<ReferencesContainer>();
            if (containers.Length > 1)
            {
                for (int i = 1; i < containers.Length; i++)
                {
                    ReferencesContainer container = containers[i];
                    if (Application.isPlaying)
                    {
                        Object.Destroy(container.gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(container.gameObject);
                    }

                    dirty = true;
                }
            }

            ReferencesContainer newContainer;
            if (containers.Length == 1)
            {
                //use an existing one lmaoooo
                newContainer = containers[0];
            }
            else
            {
                //make a new one and put it in
                GameObject gameObject = new GameObject(nameof(References));
                newContainer = gameObject.AddComponent<ReferencesContainer>();
                dirty = true;
            }

            if (newContainer.references != references)
            {
                newContainer.references = references;
                dirty = true;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (dirty)
            {
                SetSceneDirty();
            }

            //Debug.Log($"[Referencer] Placed references into container in scene at {scene.path}");
        }

        //reflected by Relay
        public static List<string> FindAssets(string filter)
        {
            List<string> paths = new List<string>();
            string[] guids = AssetDatabase.FindAssets(filter);
            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }

        //reflected by Relay
        public static Object LoadAssetAtPath(string path, Type type) => AssetDatabase.LoadAssetAtPath(path, type);

        //reflected by Relay
        public static Object[] LoadAllAssetsAtPath(string path) => AssetDatabase.LoadAllAssetsAtPath(path);

        public static void SetSceneDirty()
        {
            Scene scene = SceneManager.GetActiveScene();

            //scene is invalid, cant set it dirty
            if (!scene.IsValid())
            {
                return;
            }

            //scene isnt loaded, cant set it dirty
            if (!scene.isLoaded)
            {
                return;
            }

            //scene isnt saved in the project, probably a new untitled scene
            if (string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            //cant do this while playing
            if (Application.isPlaying)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

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
            AssetDatabase.SaveAssets();
        }
    }
}
