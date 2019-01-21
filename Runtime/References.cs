using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;

namespace Popcron.Referencer
{
    using Random = System.Random;
    using Object = UnityEngine.Object;

    public class References : ScriptableObject
    {
        private static References instance;
        private static Random random;

        public static References Instance
        {
            get
            {
                //if its null, find the instance using asset database
                //and apply it onto a game object
                if (instance == null)
                {
                    //try to find a container first
                    GameObject instanceContainer = GameObject.Find(Settings.UniqueIdentifier);
                    ReferencesContainer container = null;
                    bool dirty = false;
                    if (!instanceContainer)
                    {
                        //game object is not found
                        instanceContainer = new GameObject(Settings.UniqueIdentifier)
                        {
                            hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
                        };
                        container = instanceContainer.AddComponent<ReferencesContainer>();
                        dirty = true;
                    }
                    else
                    {
                        //game object is found and the component is also found
                        container = instanceContainer.GetComponent<ReferencesContainer>();
                        if (container == null)
                        {
                            container = instanceContainer.AddComponent<ReferencesContainer>();
                            dirty = true;
                        }
                    }

                    //no references file is on container, create new one
                    if (!container.references)
                    {
                        container.references = Relay.CreateReferencesFile();
                        dirty = true;
                    }

                    //dirty call was queued, hehe
                    //mark scene as dirty
                    if (dirty)
                    {
                        Relay.DirtyScene();
                    }

                    instance = container.references;
                }

                return instance;
            }
        }

        [SerializeField]
        internal List<Reference> items = new List<Reference>();

        private Dictionary<string, Reference> pathToItem = null;
        private Dictionary<string, Reference> nameToItem = null;
        private Dictionary<string, Reference> idToItem = null;
        private Dictionary<Object, string> objectToPath = null;

        /// <summary>
        /// Removes a reference at this path
        /// </summary>
        /// <param name="path"></param>
        public static void Remove(string path)
        {
            path = path.Replace('\\', '/');
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path)
                {
                    items.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns a raw reference item using a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Reference GetReference(string path)
        {
            path = path.Replace('\\', '/');
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path)
                {
                    return items[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Clears the entire reference list
        /// </summary>
        public static void Clear()
        {
            Instance?.items?.Clear();
        }

        /// <summary>
        /// Returns true if an object with this path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Contains(string path)
        {
            path = path.Replace('\\', '/');
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an object with a matching ID field or property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get<T>(long? id) where T : class
        {
            if (id == null) return null;

            string typeName = typeof(T).FullName;
            Instance.Cache();

            if (Instance.idToItem != null)
            {
                if (Instance.idToItem.TryGetValue(id + ":" + typeName, out Reference item))
                {
                    if (item.Type == typeof(T))
                    {
                        return item.Object as T;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id && items[i].Type == typeof(T))
                {
                    return items[i].Object as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a random object of a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRandom<T>() where T : class
        {
            List<T> list = GetAll<T>();

            if (list.Count == 0) return null;
            if (list.Count == 1) return list[1];

            random = random ?? new Random();

            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// Returns the path of an object. It will return null if the object isnt tracked.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetPath(Object value)
        {
            Instance.Cache();

            if (Instance.objectToPath != null)
            {
                if (Instance.objectToPath.TryGetValue(value, out string path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an object with using the name or path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name) where T : class
        {
            Instance.Cache();

            //name cotains a / or a \\ so check in path dictionary
            if (name.IndexOf('/') != -1 || name.IndexOf('\\') != -1)
            {
                name = name.Replace('\\', '/');
                if (Instance.pathToItem != null)
                {
                    if (Instance.pathToItem.TryGetValue(name, out Reference item))
                    {
                        if (item.Type == typeof(T))
                        {
                            return item.Object as T;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            else
            {
                if (Instance.nameToItem != null)
                {
                    if (Instance.nameToItem.TryGetValue(typeof(T).FullName + ":" + name, out Reference item))
                    {
                        if (item.Type == typeof(T))
                        {
                            return item.Object as T;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            //last resort
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name && items[i].Type == typeof(T))
                {
                    return items[i].Object as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all objecst of a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Object> GetAll(Type type)
        {
            Instance.Cache();

            List<Object> result = new List<Object>();
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == type)
                {
                    result.Add(items[i].Object);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all objects of a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAll<T>() where T : class
        {
            Instance.Cache();

            List<T> result = new List<T>();
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == typeof(T))
                {
                    result.Add(items[i].Object as T);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all raw references of a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Reference> GetAllReferences<T>() where T : class
        {
            Instance.Cache();

            List<Reference> result = new List<Reference>();
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == typeof(T))
                {
                    result.Add(items[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds an item to the list of references manually.
        /// </summary>
        /// <param name="item"></param>
        public static void Add(Reference item)
        {
            //first check if it already exists
            //if it does, dont add
            if (Contains(item.Path)) return;

            //item has no object asset assigned
            //so dont add it
            Object unityObject = item.Object;
            if (unityObject == null) return;

            //somehow null?
            if (Instance.pathToItem == null)
            {
                Instance.pathToItem = new Dictionary<string, Reference>();
            }
            if (Instance.nameToItem == null)
            {
                Instance.nameToItem = new Dictionary<string, Reference>();
            }
            if (Instance.idToItem == null)
            {
                Instance.idToItem = new Dictionary<string, Reference>();
            }
            if (Instance.objectToPath == null)
            {
                Instance.objectToPath = new Dictionary<Object, string>();
            }

            //ok add it
            Instance.items.Add(item);

            long? id = Loader.GetIDFromScriptableObject(unityObject);
            Type type = item.Type;
            string path = item.Path.Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string typeNameAndName = type.FullName + ":" + name;

            if (!Instance.pathToItem.ContainsKey(path))
            {
                Instance.pathToItem.Add(path, item);
            }

            if (!Instance.nameToItem.ContainsKey(typeNameAndName))
            {
                Instance.nameToItem.Add(typeNameAndName, item);
            }

            if (id != null)
            {
                string idAndTypeName = id.Value + ":" + type.FullName;
                if (!Instance.idToItem.ContainsKey(idAndTypeName))
                {
                    Instance.idToItem.Add(idAndTypeName, item);
                }
            }

            if (!Instance.objectToPath.ContainsKey(unityObject))
            {
                Instance.objectToPath.Add(unityObject, path);
            }
        }

        internal void Cache()
        {
            bool queueRefresh = false;
            if (pathToItem == null)
            {
                pathToItem = new Dictionary<string, Reference>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = items[i].Path.Replace('\\', '/');
                    if (pathToItem.ContainsKey(key)) continue;

                    Reference value = items[i];
                    pathToItem.Add(key, items[i]);
                }
            }

            if (nameToItem == null)
            {
                nameToItem = new Dictionary<string, Reference>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Type type = items[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = type.FullName + ":" + Path.GetFileNameWithoutExtension(items[i].Path);
                    if (nameToItem.ContainsKey(key)) continue;

                    Reference value = items[i];
                    nameToItem.Add(key, items[i]);
                }
            }

            if (idToItem == null)
            {
                idToItem = new Dictionary<string, Reference>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    long? id = items[i].ID;
                    if (id == null)
                    {
                        continue;
                    }

                    Type type = items[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = id.Value + ":" + type.FullName;
                    if (idToItem.ContainsKey(key)) continue;

                    Reference value = items[i];
                    idToItem.Add(key, items[i]);
                }
            }

            if (objectToPath == null)
            {
                objectToPath = new Dictionary<Object, string>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Object key = items[i].Object;
                    if (objectToPath.ContainsKey(key)) continue;

                    string value = items[i].Path.Replace('\\', '/');
                    objectToPath.Add(key, value);
                }
            }

            if (queueRefresh)
            {
                Debug.LogError("Errors found when creating cache, please refresh all assets");
            }
        }
    }
}
