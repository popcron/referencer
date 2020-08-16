using Popcron.Referencer;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class Reference
{
    [SerializeField]
    private string path;

    [SerializeField]
    private Object unityObject;

    [SerializeField]
    private string typeName;

    private Type cachedType;

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
                cachedType = null;
                return null;
            }

            if (cachedType == null)
            {
                cachedType = Settings.GetType(typeName);
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
        set => path = value.Replace('\\', '/');
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
                Debug.LogError($"{unityObject} is not an asset of type {Type}");
            }
        }
        else
        {
            Debug.LogError($"Tried to assign reference at path {path} a null object");
        }
    }

    public Reference(Object unityObject, Type type, string path)
    {
        string key = "Assets/";
        int index = path.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            path = path.Substring(key.Length);
        }

        this.path = path;
        this.unityObject = unityObject;
        this.typeName = type.FullName;
        this.cachedType = type;
    }
}
