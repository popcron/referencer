using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    [Serializable]
    public class Reference
    {
        [SerializeField]
        private string path;

        [SerializeField]
        private Object unityObject;

        [SerializeField]
        private string typeName;

        [NonSerialized]
        private Type systemType;

        [NonSerialized]
        private string key;

        /// <summary>
        /// Reference to the asset itself.
        /// </summary>
        public Object Object => unityObject;

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
                    systemType = null;
                    return null;
                }

                if (systemType is null)
                {
                    systemType = Settings.GetType(typeName);
                }

                return systemType;
            }
        }

        /// <summary>
        /// Path to the asset.
        /// </summary>
        public string Path
        {
            get => path;
            set
            {
                path = value.Replace('\\', '/');
                UpdateKey();
            }
        }

        /// <summary>
        /// A semi-unique key used in dictionaries that represents Type:Name.
        /// </summary>
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    UpdateKey();
                }

                return key;
            }
        }

        public Reference(Object unityObject, Type type, string path)
        {
            this.path = path;
            this.unityObject = unityObject;
            this.typeName = type.FullName;
            this.systemType = type;
            UpdateKey();
        }

        /// <summary>
        /// Updates the asset that this reference is pointing to.
        /// </summary>
        public void SetObject(Object unityObject)
        {
            if (unityObject)
            {
                if (Type == unityObject.GetType())
                {
                    this.unityObject = unityObject;
                }
                else
                {
                    if ((Settings.Current.Verbosity & Settings.LogVerbosity.Errors) == Settings.LogVerbosity.Errors)
                    {
                        Debug.LogError($"[Referencer] Cannot assign {unityObject} because its not an asset of type {Type}");
                    }
                }
            }
            else
            {
                if ((Settings.Current.Verbosity & Settings.LogVerbosity.Errors) == Settings.LogVerbosity.Errors)
                {
                    Debug.LogError($"[Referencer] Tried to assign reference at path {path} to a null object");
                }
            }
        }

        private void UpdateKey()
        {
            if (unityObject)
            {
                key = $"{typeName}:{unityObject.name}";
            }
        }
    }
}