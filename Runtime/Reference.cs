using System;
using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// Reference to the asset itself.
        /// </summary>
        public UnityEngine.Object Object => unityObject;

        /// <summary>
        /// The type of this referenced asset.
        /// </summary>
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
                    else if (typeName == "UnityEngine.Shader") cachedType = typeof(Shader);
                    else cachedType = Type.GetType(typeName + ", Assembly-CSharp");

                    //if the cached type is still null, brute force search
                    if (cachedType == null)
                    {
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (Assembly assembly in assemblies)
                        {
                            Type[] types = assembly.GetTypes();
                            foreach (Type type in types)
                            {
                                if (type.FullName == typeName)
                                {
                                    cachedType = type;
                                    break;
                                }
                            }

                            if (cachedType != null)
                            {
                                break;
                            }
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

        /// <summary>
        /// Path to the asset.
        /// </summary>
        public string Path
        {
            get => path;
            set => path = value;
        }

        /// <summary>
        /// ID of this asset based on the ID property or id field.
        /// </summary>
        public long? ID
        {
            get
            {
                return idIsNull ? null : (long?)id;
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

        public Reference(UnityEngine.Object unityObject, Type type, string path)
        {
            if (path.StartsWith("Assets/"))
            {
                path = path.Replace("Assets/", "");
            }

            this.path = path;
            this.unityObject = unityObject;
            this.typeName = type.FullName;
            this.cachedType = type;
        }
    }
}
