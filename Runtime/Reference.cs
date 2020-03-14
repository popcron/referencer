using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
                switch (typeName)
                {
                    case "UnityEngine.Sprite":
                        cachedType = typeof(Sprite);
                        break;
                    case "UnityEngine.AudioClip":
                        cachedType = typeof(AudioClip);
                        break;
                    case "UnityEngine.Material":
                        cachedType = typeof(Material);
                        break;
                    case "UnityEngine.Texture":
                        cachedType = typeof(Texture);
                        break;
                    case "UnityEngine.Font":
                        cachedType = typeof(Font);
                        break;
                    case "UnityEngine.GameObject":
                        cachedType = typeof(GameObject);
                        break;
                    case "UnityEngine.ScriptableObject":
                        cachedType = typeof(ScriptableObject);
                        break;
                    case "UnityEngine.Shader":
                        cachedType = typeof(Shader);
                        break;
                    case "UnityEngine.TextAsset":
                        cachedType = typeof(TextAsset);
                        break;
                    default:
                        cachedType = Type.GetType($"{typeName}, Assembly-CSharp");
                        break;
                }

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
    /// It will be null if this asset has no ID.
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
