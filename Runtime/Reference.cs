using Popcron.Referencer;
using System;
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

    [NonSerialized]
    private Type systemType;

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

            if (systemType == null)
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
        this.path = path;
        this.unityObject = unityObject;
        this.typeName = type.FullName;
        this.systemType = type;
    }
}
