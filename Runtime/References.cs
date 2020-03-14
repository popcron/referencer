﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Popcron.Referencer
{
    using Object = UnityEngine.Object;
    using Random = System.Random;

    public class References : ScriptableObject
    {
        private static Random random = null;

        [SerializeField]
        private List<Reference> assets = new List<Reference>();
        private Dictionary<string, Reference> pathToItem = null;
        private Dictionary<string, Reference> nameToItem = null;
        private Dictionary<string, Reference> idToItem = null;
        private Dictionary<Object, string> objectToPath = null;
        private ReadOnlyCollection<Reference> assetsReadOnly = null;

        public ReadOnlyCollection<Reference> Assets
        {
            get
            {
                if (assetsReadOnly == null)
                {
                    assetsReadOnly = assets.AsReadOnly();
                }

                return assetsReadOnly;
            }
        }

        /// <summary>
        /// Removes a reference at this path.
        /// </summary>
        public void Remove(string path)
        {
            path = path.Replace('\\', '/');
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    assets.RemoveAt(i);
                    assetsReadOnly = assets.AsReadOnly();
                    return;
                }
            }
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
        public bool Contains(string path)
        {
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
        /// Returns an object with a matching ID field or property.
        /// </summary>
        public Object Get(Type type, long id)
        {
            string typeName = type.FullName;
            EnsureCacheExists();

            if (idToItem != null)
            {
                string key = $"{id}:{typeName}";
                if (idToItem.TryGetValue(key, out Reference item))
                {
                    if (item.Type == type)
                    {
                        return item.Object;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].ID == id && assets[i].Type == type)
                {
                    return assets[i].Object;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an object with a matching ID field or property.
        /// </summary>
        public T Get<T>(long id) where T : class => Get(typeof(T), id) as T;

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
            if (random == null)
            {
                random = new Random();
            }

            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// Returns a random object of a type.
        /// </summary>
        public T GetRandom<T>() where T : class => GetRandom(typeof(T)) as T;

        /// <summary>
        /// Returns the path of an object. It will return null if the object isnt tracked.
        /// </summary>
        public string GetPath(Object value)
        {
            EnsureCacheExists();

            if (objectToPath != null)
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
        public T Get<T>(string name) where T : class => Get(typeof(T), name) as T;

        /// <summary>
        /// Returns all objecst of a type.
        /// </summary>
        public List<Object> GetAll(Type type)
        {
            List<Object> result = new List<Object>();
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].Type == type || type.IsAssignableFrom(assets[i].Type))
                {
                    result.Add(assets[i].Object);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all objects of a type and the parent types too.
        /// </summary>
        public List<T> GetAll<T>(bool customOnly = false) where T : class
        {
            Type type = typeof(T);
            List<T> result = new List<T>();
            if (!customOnly)
            {
                for (int i = 0; i < assets.Count; i++)
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
        /// Returns all references that have an ID property, or an id field.
        /// </summary>
        public List<Reference> GetAllReferencesWithIDs()
        {
            List<Reference> result = new List<Reference>();
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i].ID.HasValue)
                {
                    result.Add(assets[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all raw references of a type.
        /// </summary>
        public List<Reference> GetAllReferences<T>() where T : class
        {
            Type type = typeof(T);
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
        /// Adds an item to the list of references manually. Returns true if successful
        /// </summary>
        public bool Add(Reference item)
        {
#if UNITY_EDITOR
            //add as long as its not in the game folder
            if (item.Path.IndexOf("Assets/Game", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return false;
            }
#endif
            //first check if it already exists
            //if it does, dont add
            if (Contains(item.Path))
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

            if (pathToItem == null)
            {
                pathToItem = new Dictionary<string, Reference>();
            }

            if (nameToItem == null)
            {
                nameToItem = new Dictionary<string, Reference>();
            }

            if (idToItem == null)
            {
                idToItem = new Dictionary<string, Reference>();
            }

            if (objectToPath == null)
            {
                objectToPath = new Dictionary<Object, string>();
            }

            long? id = Loader.GetIDFromScriptableObject(unityObject);
            Type type = item.Type;
            string path = item.Path.Replace('\\', '/');
            string name = unityObject.name;
            string typeNameAndName = $"{type.FullName}:{name}";

            assets.Add(item);
            assetsReadOnly = assets.AsReadOnly();

            //add to dictionaries
            if (!pathToItem.ContainsKey(path))
            {
                pathToItem.Add(path, item);
            }

            if (!nameToItem.ContainsKey(typeNameAndName))
            {
                nameToItem.Add(typeNameAndName, item);
            }

            if (id != null)
            {
                string idAndTypeName = $"{id.Value}:{type.FullName}";
                if (!idToItem.ContainsKey(idAndTypeName))
                {
                    idToItem.Add(idAndTypeName, item);
                }
            }

            if (!objectToPath.ContainsKey(unityObject))
            {
                objectToPath.Add(unityObject, path);
            }

            return true;
        }

        internal void EnsureCacheExists()
        {
            bool errorFound = false;
            if (pathToItem == null)
            {
                pathToItem = new Dictionary<string, Reference>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    if (assets[i] == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    string key = assets[i].Path.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (pathToItem.ContainsKey(key))
                        {
                            continue;
                        }

                        pathToItem.Add(key, assets[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"[Referencer] {assets[i]?.Object?.name} has no path, so not adding to reference db");
                    }
                }
            }

            if (nameToItem == null)
            {
                nameToItem = new Dictionary<string, Reference>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    if (assets[i] == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    Type type = assets[i].Type;
                    if (type == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    string key = $"{type.FullName}:{Path.GetFileNameWithoutExtension(assets[i].Path)}";
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
                        Debug.LogWarning($"[Referencer] {assets[i]?.Object?.name} has no path, so not adding to reference db");
                    }
                }
            }

            if (idToItem == null)
            {
                idToItem = new Dictionary<string, Reference>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    if (assets[i] == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    long? id = assets[i].ID;
                    if (id == null)
                    {
                        continue;
                    }

                    Type type = assets[i].Type;
                    if (type == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    string key = $"{id.Value}:{type.FullName}";
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (idToItem.ContainsKey(key))
                        {
                            continue;
                        }

                        idToItem.Add(key, assets[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"[Referencer] {assets[i]?.Object?.name} has no path, so not adding to reference db");
                    }
                }
            }

            if (objectToPath == null)
            {
                objectToPath = new Dictionary<Object, string>();

                //built in
                for (int i = 0; i < assets.Count; i++)
                {
                    if (assets[i] == null)
                    {
                        errorFound = true;
                        continue;
                    }

                    Object key = assets[i].Object;
                    if (key)
                    {
                        if (objectToPath.ContainsKey(key))
                        {
                            continue;
                        }

                        string value = assets[i].Path.Replace('\\', '/');
                        objectToPath.Add(key, value);
                    }
                }
            }

            //an error in the database was found, gon refresh now then
            if (errorFound)
            {
                Relay.LoadAll(this);
            }
        }
    }
}
