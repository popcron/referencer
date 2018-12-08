using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Popcron.Referencer
{
    [Serializable]
    [ComVisible(true)]
    public class Reference
    {
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
                if (cachedType == null)
                {
                    if (typeName == "UnityEngine.Sprite") cachedType = typeof(Sprite);
                    else if (typeName == "UnityEngine.AudioClip") cachedType = typeof(AudioClip);
                    else if (typeName == "UnityEngine.GameObject") cachedType = typeof(GameObject);
                    else if (typeName == "UnityEngine.ScriptableObject") cachedType = typeof(ScriptableObject);
                    else cachedType = Type.GetType(typeName + ", Assembly-CSharp");

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
    }
}
