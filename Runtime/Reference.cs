using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Popcron.Referencer
{
    [Serializable]
    public class Reference
    {
        private static Dictionary<string, Type> nameToType = null;

        [SerializeField]
        private string path;

        [SerializeField]
        private UnityEngine.Object unityObject;

        [SerializeField]
        private string typeName;

        [SerializeField]
        private long id;

        private Type cachedType;
        private bool idIsNull = true;

        public UnityEngine.Object Object
        {
            get
            {
                return unityObject;
            }
        }

        public Type Type
        {
            get
            {
                //null type name is null cached type
                if (string.IsNullOrEmpty(typeName))
                {
                    cachedType = null;
                    return null;
                }

                if (cachedType == null)
                {
                    //try to get the type from the dictionary cache
                    if (nameToType != null && nameToType.TryGetValue(typeName, out Type newType))
                    {
                        cachedType = newType;
                        return cachedType;
                    }

                    //try to get common types
                    if (typeName == "UnityEngine.Sprite") cachedType = typeof(Sprite);
                    else if (typeName == "UnityEngine.AudioClip") cachedType = typeof(AudioClip);
                    else if (typeName == "UnityEngine.Material") cachedType = typeof(Material);
                    else if (typeName == "UnityEngine.Texture") cachedType = typeof(Texture);
                    else if (typeName == "UnityEngine.Font") cachedType = typeof(Font);
                    else if (typeName == "UnityEngine.GameObject") cachedType = typeof(GameObject);
                    else if (typeName == "UnityEngine.ScriptableObject") cachedType = typeof(ScriptableObject);
                    else cachedType = Type.GetType(typeName + ", Assembly-CSharp");

                    //if the cached type is still null, brute force search
                    if (cachedType == null)
                    {
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies)
                        {
                            var types = assembly.GetTypes();
                            foreach (var type in types)
                            {
                                if (type.FullName == typeName)
                                {
                                    cachedType = type;
                                    break;
                                }
                            }

                            if (cachedType != null) break;
                        }
                    }

                    //if the type was found, then cache it
                    if (cachedType != null)
                    {
                        if (nameToType == null)
                        {
                            nameToType = new Dictionary<string, Type>();
                        }

                        //doesnt exist in the dictionary, so add it
                        if (!nameToType.ContainsKey(typeName))
                        {
                            nameToType.Add(typeName, cachedType);
                        }
                    }
                }

                return cachedType;
            }
        }

        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }

        public long? ID
        {
            get
            {
                if (idIsNull) return null;

                return id;
            }
            set
            {
                idIsNull = value == null;

                if (value != null)
                {
                    id = value.Value;
                }
            }
        }

        public Reference(UnityEngine.Object unityObject, Type type, string name)
        {
            if (name.StartsWith("Assets/"))
            {
                name = name.Replace("Assets/", "");
            }

            this.path = name;
            this.unityObject = unityObject;
            this.typeName = type.FullName;
            this.cachedType = type;
        }

        public static implicit operator Reference(string path)
        {
            List<Reference> items = References.Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == path)
                {
                    return items[i];
                }
            }

            return null;
        }

        public static implicit operator Reference(long id)
        {
            List<Reference> items = References.Instance.items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == null) continue;
                if (items[i].ID == id)
                {
                    return items[i];
                }
            }

            return null;
        }
    }
}
