using Popcron.Referencer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Type = System.Type;

/// <summary>
/// A static pointer to the references list instance.
/// </summary>
public class Refs
{
    private static ReferencesContainer container;

    /// <summary>
    /// A static instance of the references asset.
    /// </summary>
    private static References Instance
    {
        get
        {
            if (!container)
            {
                container = Object.FindObjectOfType<ReferencesContainer>();

                //still no container
                if (!container)
                {
                    container = new GameObject(nameof(References)).AddComponent<ReferencesContainer>();
                }
            }

            //nothing on the container lol
            if (!container.references)
            {
                container.references = null;
            }

            return container.references;
        }
    }

    /// <summary>
    /// Returns a read only list of all assets.
    /// </summary>
    public static ReadOnlyCollection<Reference> Assets => Instance.Assets;

    /// <summary>
    /// Returns an asset with this type and ID.
    /// </summary>
    public static T Get<T>(long id) where T : Object => Instance.Get<T>(id);

    /// <summary>
    /// Returns an asset with this type and name.
    /// </summary>
    public static T Get<T>(string name) where T : Object => Instance.Get<T>(name);

    /// <summary>
    /// Returns an asset with this type and ID.
    /// </summary>
    public static Object Get(Type type, long id) => Instance.Get(type, id);

    /// <summary>
    /// Returns an asset with this type and name.
    /// </summary>
    public static Object Get(Type type, string name) => Instance.Get(type, name);

    /// <summary>
    /// Returns true if the references has this path registered.
    /// </summary>
    public static bool Contains(string path) => Instance.Contains(path);

    /// <summary>
    /// Returns true if the references has this asset registered.
    /// </summary>
    public static bool Contains(Object asset) => Instance.Contains(asset);

    /// <summary>
    /// Returns a list of all assets with this type.
    /// </summary>
    public static List<T> GetAll<T>() where T : Object => Instance.GetAll<T>();

    /// <summary>
    /// Returns a list of all assets with this type.
    /// </summary>
    public static List<Object> GetAll(Type type) => Instance.GetAll(type);

    /// <summary>
    /// Returns a list of all references with this type.
    /// </summary>
    public static List<Reference> GetAllReferences<T>() where T : Object => Instance.GetAllReferences<T>();

    /// <summary>
    /// Returns a list of all references with this type.
    /// </summary>
    public static List<Reference> GetAllReferences(Type type) => Instance.GetAllReferences(type);

    /// <summary>
    /// Returns a list of all references that have an ID.
    /// </summary>
    public static List<Reference> GetAllReferencesWithIDs() => Instance.GetAllReferencesWithIDs();

    /// <summary>
    /// Returns the path of this asset, if its in the references list.
    /// </summary>
    public static string GetPath(Object asset) => Instance.GetPath(asset);

    /// <summary>
    /// Returns a random object with this type.
    /// </summary>
    public static T GetRandom<T>() where T : Object => Instance.GetRandom<T>();

    /// <summary>
    /// Returns a random object with this type.
    /// </summary>
    public static Object GetRandom(Type type) => Instance.GetRandom(type);

    /// <summary>
    /// Returns a reference with this path.
    /// </summary>
    public static Reference GetReference(string path) => Instance.GetReference(path);
}