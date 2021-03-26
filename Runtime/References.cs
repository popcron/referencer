using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;
using Random = System.Random;

namespace Popcron.Referencer
{
    public class References : ScriptableObject
    {
        private static References current;
        private static Random random = null;

        [SerializeField]
        private List<Reference> assets = new List<Reference>();

        private Dictionary<string, Reference> pathToItem = null;
        private Dictionary<string, Reference> nameToItem = null;
        private Dictionary<Object, string> objectToPath = null;
        private ReadOnlyCollection<Reference> assetsReadOnly = null;

        public static References Current
        {
            get
            {
                if (!current)
                {
                    current = GetOrCreate();
                }

                return current;
            }
        }

        public ReadOnlyCollection<Reference> Assets
        {
            get
            {
                if (assetsReadOnly is null)
                {
                    assetsReadOnly = assets.AsReadOnly();
                }

                return assetsReadOnly;
            }
        }

        /// <summary>
        /// Removes a reference at this path.
        /// </summary>
        public bool Remove(string path)
        {
            path = path.Replace('\\', '/');
            for (int i = 0; i < assets.Count; i++)
            {
                Reference reference = assets[i];
                if (reference.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    assets.RemoveAt(i);
                    assetsReadOnly = assets.AsReadOnly();

                    if (objectToPath != null)
                    {
                        objectToPath.Remove(reference.Object);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a raw reference item using a path.
        /// </summary>
        public Reference GetReference(string path)
        {
            path = path.Replace('\\', '/');

            //first check built in data
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return assets[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Clears the entire reference list.
        /// </summary>
        public void Clear()
        {
            assets.Clear();
            assetsReadOnly = assets.AsReadOnly();
        }

        /// <summary>
        /// Returns true if an object with this path exists.
        /// </summary>
        public bool Contains(string path, Object asset)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            path = path.Replace('\\', '/');
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    if (assets[i].Object == asset)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if an object with this path exists.
        /// </summary>
        public bool Contains(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            path = path.Replace('\\', '/');
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if an object with this path exists.
        /// </summary>
        public bool Contains(Object asset)
        {
            if (!asset)
            {
                return false;
            }

            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Object == asset)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a random object of a type.
        /// </summary>
        public Object GetRandom(Type type)
        {
            List<Object> list = GetAll(type);

            if (list.Count == 0)
            {
                return null;
            }

            if (list.Count == 1)
            {
                return list[1];
            }

            //ensure randomness exists
            if (random is null)
            {
                random = new Random();
            }

            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// Returns a random object of a type.
        /// </summary>
        public T GetRandom<T>() where T : Object => GetRandom(typeof(T)) as T;

        /// <summary>
        /// Returns the path of an object. It will return null if the object isnt tracked.
        /// </summary>
        public string GetPath(Object value)
        {
            EnsureCacheExists();

            if (objectToPath != null && value)
            {
                if (objectToPath.TryGetValue(value, out string path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an object with using the name or path.
        /// </summary>
        public Object Get(Type type, string name)
        {
            //if name is null
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            EnsureCacheExists();

            //name cotains a / or a \\ so check in path dictionary
            if (name.IndexOf('/') != -1 || name.IndexOf('\\') != -1)
            {
                name = name.Replace('\\', '/');
                if (pathToItem != null)
                {
                    if (pathToItem.TryGetValue(name, out Reference item))
                    {
                        if (item.Type == type)
                        {
                            return item.Object;
                        }
                    }
                }
            }

            //check by name now
            if (nameToItem != null)
            {
                string key = $"{type.FullName}:{name}";
                if (nameToItem.TryGetValue(key, out Reference item))
                {
                    if (item.Type == type)
                    {
                        return item.Object;
                    }
                }
            }

            //last resort
            List<Reference> items = assets;
            for (int i = 0; i < items.Count; i++)
            {
                Reference item = items[i];
                if (item.Type == type)
                {
                    if (string.IsNullOrEmpty(item.Path))
                    {
                        continue;
                    }

                    if (item.Path.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.Object;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an object with using the name or path with this type.
        /// </summary>
        public T Get<T>(string name) where T : Object => Get(typeof(T), name) as T;

        /// <summary>
        /// Returns all objecst of a type.
        /// </summary>
        public List<Object> GetAll(Type type)
        {
            bool isComponent = typeof(Component).IsAssignableFrom(type);
            List<Object> result = new List<Object>();
            for (int i = 0; i < assets.Count; i++)
            {
                if (isComponent)
                {
                    if (assets[i].Object is GameObject prefab)
                    {
                        Object component = prefab.GetComponent(type);
                        if (component)
                        {
                            result.Add(component);
                        }
                    }
                }
                else
                {
                    if (assets[i].Type == type || type.IsAssignableFrom(assets[i].Type))
                    {
                        result.Add(assets[i].Object);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all objects of a type and the parent types too.
        /// </summary>
        public List<T> GetAll<T>() where T : Object
        {
            bool isComponent = typeof(Component).IsAssignableFrom(typeof(T));
            Type type = typeof(T);
            List<T> result = new List<T>();
            for (int i = 0; i < assets.Count; i++)
            {
                if (isComponent)
                {
                    if (assets[i].Object is GameObject prefab)
                    {
                        T component = prefab.GetComponent<T>();
                        if (component)
                        {
                            result.Add(component);
                        }
                    }
                }
                else
                {
                    if (assets[i].Type == type || type.IsAssignableFrom(assets[i].Type))
                    {
                        result.Add(assets[i].Object as T);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all raw references of a type.
        /// </summary>
        public List<Reference> GetAllReferences<T>() where T : Object
        {
            return GetAllReferences(typeof(T));
        }

        /// <summary>
        /// Returns all raw references of a type.
        /// </summary>
        public List<Reference> GetAllReferences(Type type)
        {
            List<Reference> result = new List<Reference>();
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Type == type)
                {
                    result.Add(assets[i]);
                }
                else if (type.IsAssignableFrom(assets[i].Type))
                {
                    result.Add(assets[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds an item to the list of builtin references manually. Returns true if successfull.
        /// </summary>
        public bool Add(Reference item)
        {
            //first check with path and asset if it already exists
            //if it does, dont add
            if (Contains(item.Path, item.Object))
            {
                return false;
            }

            //check if this item belongs to a path that should be ignored
            Settings settings = Settings.Current;
            if (settings.IsBlacklisted(item.Path))
            {
                return false;
            }

            //item has no object asset assigned
            //so dont add it
            Object unityObject = item.Object;
            if (!unityObject)
            {
                return false;
            }

            if (pathToItem is null)
            {
                pathToItem = new Dictionary<string, Reference>();
            }

            if (nameToItem is null)
            {
                nameToItem = new Dictionary<string, Reference>();
            }

            if (objectToPath is null)
            {
                objectToPath = new Dictionary<Object, string>();
            }

            Type type = item.Type;
            string path = item.Path.Replace('\\', '/');
            string name = unityObject.name;
            string typeNameAndName = $"{type.FullName}:{name}";

            assets.Add(item);
            assetsReadOnly = assets.AsReadOnly();

            //add to dictionaries
            pathToItem[path] = item;
            nameToItem[typeNameAndName] = item;
            objectToPath[unityObject] = path;

            return true;
        }

        internal void EnsureCacheExists(bool forceRecreate = false)
        {
            bool errorFound = false;
            if (pathToItem is null || forceRecreate)
            {
                pathToItem = new Dictionary<string, Reference>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    Reference reference = assets[i];
                    if (reference is null)
                    {
                        errorFound = true;
                        continue;
                    }

                    string key = reference.Path.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (pathToItem.ContainsKey(key))
                        {
                            continue;
                        }

                        pathToItem.Add(key, reference);
                    }
                    else
                    {
                        Debug.LogWarning($"[Referencer] {reference?.Object?.name} has no path, so not adding to reference db");
                    }
                }
            }

            if (nameToItem is null || forceRecreate)
            {
                nameToItem = new Dictionary<string, Reference>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    Reference reference = assets[i];
                    if (reference is null)
                    {
                        errorFound = true;
                        continue;
                    }

                    Type type = reference.Type;
                    if (type is null)
                    {
                        errorFound = true;
                        continue;
                    }

                    string key = $"{type.FullName}:{Path.GetFileNameWithoutExtension(reference.Path)}";
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (nameToItem.ContainsKey(key))
                        {
                            continue;
                        }

                        nameToItem.Add(key, assets[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"[Referencer] {reference?.Object?.name} has no path, so not adding to reference db");
                    }
                }
            }

            if (objectToPath is null || forceRecreate)
            {
                objectToPath = new Dictionary<Object, string>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    Reference reference = assets[i];
                    if (reference is null)
                    {
                        errorFound = true;
                        continue;
                    }

                    Object key = reference.Object;
                    if (key)
                    {
                        if (objectToPath.ContainsKey(key))
                        {
                            continue;
                        }

                        string value = reference.Path.Replace('\\', '/');
                        objectToPath.Add(key, value);
                    }
                }
            }

            //an error in the database was found, gon refresh now then
            if (errorFound)
            {
                Relay.LoadAll();
            }
        }

        /// <summary>
        /// Returns an existing console settings asset, or creates a new one if none exist.
        /// </summary>
        public static References GetOrCreate()
        {
            //find from resources
            References references = Resources.Load<References>("References");
            bool exists = references;
            if (!exists)
            {
                //no console settings asset exists yet, so create one
                references = CreateInstance<References>();
                references.name = "References";
            }

#if UNITY_EDITOR
            if (!exists)
            {
                //ensure the resources folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                //make a file here
                string path = "Assets/Resources/References.asset";
                AssetDatabase.CreateAsset(references, path);
                AssetDatabase.Refresh();
            }
#endif

            return references;
        }
    }
}