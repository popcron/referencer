using Popcron.Referencer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Type = System.Type;

/// <summary>
/// A static pointer to the references list instance.
/// </summary>
public struct Refs
{
    /// <summary>
    /// A static instance of the references asset.
    /// </summary>
    public static References Instance => References.Current;

    /// <summary>
    /// Returns a read only list of all assets.
    /// </summary>
    public static ReadOnlyCollection<Reference> Assets => Instance.Assets;

    /// <summary>
    /// Returns an asset with this type and name.
    /// </summary>
    public static T Get<T>(string name) where T : Object => Instance.Get<T>(name);

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
    /// Adds a new reference to the internal list manually.
    /// Returns true if it was successfull.
    /// </summary>
    public static bool Add(Reference reference) => Instance.Add(reference);

    /// <summary>
    /// Removes a reference from the internal list.
    /// Returns true if successfull.
    /// </summary>
    public static bool Remove(Reference reference)
    {
        for (int i = 0; i < Instance.Assets.Count; i++)
        {
            if (Instance.Assets[i] == reference)
            {
                return Instance.Remove(reference.Path);
            }
        }

        return false;
    }

    /// <summary>
    /// Removes a reference from the internal list using the path.
    /// Returns true if successfull.
    /// </summary>
    public static bool Remove(string path) => Instance.Remove(path);

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