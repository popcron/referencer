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
        internal List<Reference> builtin = new List<Reference>();

        [SerializeField]
        internal List<Reference> custom = new List<Reference>();

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
            List<Reference> items = Instance.builtin;
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

            //first check built in data
            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path)
                {
                    return items[i];
                }
            }

            //then check customs
            items = Instance.custom;
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
            if (Application.isPlaying)
            {
                Instance?.custom?.Clear();
            }
            else
            {
                Instance?.builtin?.Clear();
            }
        }

        /// <summary>
        /// Returns true if an object with this path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Contains(string path)
        {
            path = path.Replace('\\', '/');
            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path) return true;
            }

            items = Instance.custom;
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
            Instance.CheckCache();

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

            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id && items[i].Type == typeof(T))
                {
                    return items[i].Object as T;
                }
            }

            items = Instance.custom;
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
            Instance.CheckCache();

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
            Instance.CheckCache();

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
            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name && items[i].Type == typeof(T))
                {
                    return items[i].Object as T;
                }
            }

            items = Instance.custom;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name && items[i].Type == typeof(T))
                {
                    return items[i].Object as T;
                }
            }

            return null;
        }

        public static List<Reference> GetAll(bool customOnly = false)
        {
            Instance.CheckCache();

            if (customOnly)
            {
                return Instance.custom;
            }
            else
            {
                List<Reference> items = Instance.builtin;
                items.AddRange(Instance.custom);
                return items;
            }
        }

        /// <summary>
        /// Returns all objecst of a type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Object> GetAll(Type type, bool customOnly = false)
        {
            Instance.CheckCache();

            List<Object> result = new List<Object>();
            List<Reference> items = Instance.builtin;
            if (!customOnly)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Type == type)
                    {
                        result.Add(items[i].Object);
                    }
                }
            }

            items = Instance.custom;
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
        public static List<T> GetAll<T>(bool customOnly = false) where T : class
        {
            Instance.CheckCache();

            Type type = typeof(T);
            List<T> result = new List<T>();
            List<Reference> items = Instance.builtin;
            if (!customOnly)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Type == type)
                    {
                        result.Add(items[i].Object as T);
                    }
                }
            }

            items = Instance.custom;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == type)
                {
                    result.Add(items[i].Object as T);
                }
            }

            return result;
        }

        public static List<Reference> GetAllReferencesWithIDs()
        {
            Instance.CheckCache();

            List<Reference> result = new List<Reference>();
            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID.HasValue)
                {
                    result.Add(items[i]);
                }
            }

            items = Instance.custom;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID.HasValue)
                {
                    result.Add(items[i]);
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
            Instance.CheckCache();

            Type type = typeof(T);
            List<Reference> result = new List<Reference>();
            List<Reference> items = Instance.builtin;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == type)
                {
                    result.Add(items[i]);
                }
            }

            items = Instance.custom;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Type == type)
                {
                    result.Add(items[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds an item to the list of references manually.
        /// </summary>
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
            if (Application.isPlaying)
            {
                Instance.custom.Add(item);
            }
            else
            {
                Instance.builtin.Add(item);
            }

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

        internal void CheckCache()
        {
            bool queueRefresh = false;
            if (pathToItem == null)
            {
                pathToItem = new Dictionary<string, Reference>();
                //built in
                for (int i = 0; i < builtin.Count; i++)
                {
                    if (builtin[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = builtin[i].Path.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(key))
					{
						if (pathToItem.ContainsKey(key)) continue;
						Reference value = builtin[i];
						pathToItem.Add(key, builtin[i]);
					}
					else
					{
						Debug.LogWarning(builtin[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
                }

                //custom
                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = custom[i].Path.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(key))
					{
						if (pathToItem.ContainsKey(key)) continue;

						Reference value = custom[i];
						pathToItem.Add(key, custom[i]);
					}
					else
					{
						Debug.LogWarning(custom[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
                }
            }

            if (nameToItem == null)
            {
                nameToItem = new Dictionary<string, Reference>();
                //built in
                for (int i = 0; i < builtin.Count; i++)
                {
                    if (builtin[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Type type = builtin[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = type.FullName + ":" + Path.GetFileNameWithoutExtension(builtin[i].Path);
                    if (!string.IsNullOrEmpty(key))
					{
						if (nameToItem.ContainsKey(key)) continue;

						Reference value = builtin[i];
						nameToItem.Add(key, builtin[i]);
					}
					else
					{
						Debug.LogWarning(builtin[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
				}

                //custom
                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Type type = custom[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = type.FullName + ":" + Path.GetFileNameWithoutExtension(custom[i].Path);
                    if (!string.IsNullOrEmpty(key))
					{
						if (nameToItem.ContainsKey(key)) continue;

						Reference value = custom[i];
						nameToItem.Add(key, custom[i]);
					}
					else
					{
						Debug.LogWarning(custom[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
                }
            }

            if (idToItem == null)
            {
                idToItem = new Dictionary<string, Reference>();
                //built in
                for (int i = 0; i < builtin.Count; i++)
                {
                    if (builtin[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    long? id = builtin[i].ID;
                    if (id == null)
                    {
                        continue;
                    }

                    Type type = builtin[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = id.Value + ":" + type.FullName;
                    if (!string.IsNullOrEmpty(key))
					{
						if (idToItem.ContainsKey(key)) continue;

						Reference value = builtin[i];
						idToItem.Add(key, builtin[i]);
					}
					else
					{
						Debug.LogWarning(builtin[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
				}

                //custom
                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    long? id = custom[i].ID;
                    if (id == null)
                    {
                        continue;
                    }

                    Type type = custom[i].Type;
                    if (type == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    string key = id.Value + ":" + type.FullName;
					if (!string.IsNullOrEmpty(key))
					{
						if (idToItem.ContainsKey(key)) continue;

						Reference value = custom[i];
						idToItem.Add(key, custom[i]);
					}
					else
					{
						Debug.LogWarning(custom[i]?.Object?.name + " has no path. Not adding to reference list.");
					}
				}
            }

            if (objectToPath == null)
            {
                objectToPath = new Dictionary<Object, string>();
                //built in
                for (int i = 0; i < builtin.Count; i++)
                {
                    if (builtin[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Object key = builtin[i].Object;
                    if (key)
					{
						if (objectToPath.ContainsKey(key)) continue;

						string value = builtin[i].Path.Replace('\\', '/');
						objectToPath.Add(key, value);
					}
				}

                //custom
                for (int i = 0; i < custom.Count; i++)
                {
                    if (custom[i] == null)
                    {
                        queueRefresh = true;
                        continue;
                    }

                    Object key = custom[i].Object;
                    if (key)
					{
						if (objectToPath.ContainsKey(key)) continue;

						string value = custom[i].Path.Replace('\\', '/');
						objectToPath.Add(key, value);
					}
                }
            }

            if (queueRefresh)
            {
                Debug.LogError("Errors found when creating cache, please refresh all assets");
            }
        }
    }
}
