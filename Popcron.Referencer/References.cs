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

        private static References Instance
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
                    if (!instanceContainer)
                    {
                        instanceContainer = new GameObject(Settings.UniqueIdentifier)
                        {
                            hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
                        };
                        container = instanceContainer.AddComponent<ReferencesContainer>();
                    }
                    else
                    {
                        container = instanceContainer.GetComponent<ReferencesContainer>();
                    }

                    FindReference(container);

                    instance = container.references;
                }

                return instance;
            }
        }

        [SerializeField]
        private List<Reference> items = new List<Reference>();

        private Dictionary<string, Reference> pathToItem = null;
        private Dictionary<string, Reference> nameToItem = null;
        private Dictionary<string, Reference> idToItem = null;
        private Dictionary<Object, string> objectToPath = null;

        private static void FindReference(ReferencesContainer container)
        {
            if (!container.references)
            {
                instance = Relay.CreateReferencesFile();
                container.references = instance;
            }
        }

        public static void Remove(string name)
        {
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name)
                {
                    items.RemoveAt(i);
                    return;
                }
            }
        }

        public static Reference GetItem(string name)
        {
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name)
                {
                    return items[i];
                }
            }

            return null;
        }

        public static void Clear()
        {
            Instance?.items?.Clear();
        }

        public static bool Contains(string name)
        {
            List<Reference> items = Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == name) return true;
            }

            return false;
        }

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

        public static T GetRandom<T>() where T : class
        {
            List<T> list = GetAll<T>();

            if (list.Count == 0) return null;
            if (list.Count == 1) return list[1];

            random = random ?? new Random();

            return list[random.Next(list.Count)];
        }

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

        public static T Get<T>(string name) where T : class
        {
            Instance.Cache();

            //name cotains a / so check in path dictionary
            if (name.IndexOf('/') != -1)
            {
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

        public static void Add(Reference item)
        {
            //first check if it already exists
            //if it does, dont add
            if (Contains(item.Path))
            {
                return;
            }

            Instance.items.Add(item);
        }

        public void Cache()
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

                    string key = items[i].Path;
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

                    string key = items[i].ID + ":" + items[i].Type.FullName;
                    if (idToItem.ContainsKey(key)) continue;

                    Reference value = items[i];
                    idToItem.Add(key, items[i]);
                }
            }

            if (queueRefresh)
            {
                Debug.LogError("Errors found when creating cache, please refresh all assets");
            }
        }
    }
}
