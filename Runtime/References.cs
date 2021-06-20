﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
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
        private HashSet<string> assetPaths = null;

        public static References Current
        {
            get
            {
                if (!current)
                {
                    current = Utils.GetOrCreate<References>("Assets/References.asset");
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

        private void OnEnable()
        {
            current = this;
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
                if (reference.Path.Equals(path))
                {
                    return Remove(i);
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a reference instance at this index.
        /// </summary>
        public bool Remove(int index)
        {
            if (index >= 0 && index < assets.Count)
            {
                Reference reference = assets[index];
                assets.RemoveAt(index);
                assetsReadOnly = assets.AsReadOnly();

                if (objectToPath != null)
                {
                    objectToPath.Remove(reference.Object);
                }

                return true;
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
                Reference reference = assets[i];
                if (reference.Path.Equals(path))
                {
                    return reference;
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
            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (reference.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    if (reference.Object == asset)
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

            //rebuild path hashset
            if (assetPaths is null)
            {
                assetPaths = new HashSet<string>();
                int count = assets.Count;
                for (int i = 0; i < count; i++)
                {
                    Reference reference = assets[i];
                    if (!assetPaths.Contains(reference.Path))
                    {
                        assetPaths.Add(reference.Path);
                    }
                }
            }

            return assetPaths.Contains(path);
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

            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (reference.Object == asset)
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
            bool isComponent = typeof(Component).IsAssignableFrom(type);

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
                        else if (isComponent)
                        {
                            if (item.Object is GameObject prefab)
                            {
                                Object component = prefab.GetComponent(type);
                                if (component)
                                {
                                    return component;
                                }
                            }
                        }
                    }
                }
            }

            //check by name now
            if (nameToItem != null)
            {
                string key;
                if (isComponent)
                {
                    key = $"UnityEngine.GameObject:{name}";
                }
                else
                {
                    key = $"{type.FullName}:{name}";
                }

                if (nameToItem.TryGetValue(key, out Reference item))
                {
                    if (isComponent)
                    {
                        if (item.Object is GameObject prefab)
                        {
                            Object component = prefab.GetComponent(type);
                            if (component)
                            {
                                return component;
                            }
                        }
                    }
                    else if (item.Type == type)
                    {
                        return item.Object;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first asset of this type.
        /// </summary>
        public Object Get(Type type)
        {
            bool isComponent = typeof(Component).IsAssignableFrom(type);
            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (isComponent)
                {
                    if (reference.Object is GameObject prefab)
                    {
                        Object component = prefab.GetComponent(type);
                        if (component)
                        {
                            return component;
                        }
                    }
                }
                else
                {
                    if (reference.Type == type || type.IsAssignableFrom(reference.Type))
                    {
                        return reference.Object;
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
        /// Returns the first asset of this type.
        /// </summary>
        public T Get<T>() where T : Object => Get(typeof(T)) as T;

        /// <summary>
        /// Returns all objecst of a type.
        /// </summary>
        public List<Object> GetAll(Type type)
        {
            bool isComponent = typeof(Component).IsAssignableFrom(type);
            List<Object> result = new List<Object>();
            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (isComponent)
                {
                    if (reference.Object is GameObject prefab)
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
                    if (reference.Type == type || type.IsAssignableFrom(reference.Type))
                    {
                        result.Add(reference.Object);
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
            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (isComponent)
                {
                    if (reference.Object is GameObject prefab)
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
                    if (reference.Type == type || type.IsAssignableFrom(reference.Type))
                    {
                        result.Add(reference.Object as T);
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
            int count = assets.Count;
            for (int i = 0; i < count; i++)
            {
                Reference reference = assets[i];
                if (reference.Type == type)
                {
                    result.Add(reference);
                }
                else if (type.IsAssignableFrom(reference.Type))
                {
                    result.Add(reference);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds an item to the list of builtin references manually. Returns true if successfull.
        /// </summary>
        public bool Add(Reference item, Settings settings = null)
        {
            //first check with path and asset if it already exists
            //if it does, dont add
            if (Contains(item.Path, item.Object))
            {
                return false;
            }

            if (!settings)
            {
                settings = Settings.Current;
            }

            //check if this item belongs to a path that should be ignored
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

            assets.Add(item);
            assetsReadOnly = assets.AsReadOnly();

            //add to dictionaries
            pathToItem[item.Path] = item;
            nameToItem[item.Key] = item;
            try
            {
                objectToPath[unityObject] = item.Path;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Exception when caching {unityObject}\n{exception}");
            }

            return true;
        }

        internal void EnsureCacheExists(bool forceRecreate = false)
        {
            Settings settings = Settings.Current;
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
                        if ((settings.Verbosity & Settings.LogVerbosity.Errors) == Settings.LogVerbosity.Errors)
                        {
                            Debug.LogWarning($"[Referencer] {reference?.Object?.name} has no path, so not adding to reference db");
                        }
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

                    if (!string.IsNullOrEmpty(reference.Key))
                    {
                        if (nameToItem.ContainsKey(reference.Key))
                        {
                            continue;
                        }

                        nameToItem.Add(reference.Key, assets[i]);
                    }
                    else
                    {
                        if ((settings.Verbosity & Settings.LogVerbosity.Errors) == Settings.LogVerbosity.Errors)
                        {
                            Debug.LogWarning($"[Referencer] {reference?.Object?.name} has no path, so not adding to reference db");
                        }
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
                Utils.LoadAll();
                if ((settings.Verbosity & Settings.LogVerbosity.LogLoadReasons) == Settings.LogVerbosity.LogLoadReasons)
                {
                    Debug.Log("[References] Loading all because an error was found in the db");
                }
            }
        }
    }
}