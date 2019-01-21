using System.Reflection;
using System.Collections.Generic;
using System;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class Loader
    {
        public static List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            List<string> paths = Relay.FindAssets<T>();
            foreach (string path in paths)
            {
                T asset = Relay.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static Object LoadAssetAtPath(string path, Type type)
        {
            return Relay.LoadAssetAtPath("Assets/" + path, type);
        }

        public static Object[] LoadAllAssetsAtPath(string path)
        {
            return Relay.LoadAllAssetsAtPath("Assets/" + path);
        }

        public static List<string> FindAssets(string filter)
        {
            Settings settings = Settings.Current ?? new Settings();

            List<string> paths = new List<string>();
            List<string> assets = Relay.FindAssets(filter);
            foreach (string path in assets)
            {
                if (path == settings.referencesAssetPath) continue;

                string newPath = path.Replace("Assets/", "");

                bool ignore = false;
                foreach (var ignoredFolder in settings.ignoredFolders)
                {
                    if (newPath.StartsWith(ignoredFolder))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                {
                    break;
                }

                paths.Add(newPath);
            }

            return paths;
        }

        public static long? GetIDFromScriptableObject(Object unityObject)
        {
            if (unityObject is ScriptableObject so)
            {
                long? id = null;
                PropertyInfo property = unityObject.GetType().GetProperty("ID");
                FieldInfo field = unityObject.GetType().GetField("id");

                //found id property
                if (property != null)
                {
                    object value = property.GetValue(unityObject, null);
                    if (value != null)
                    {
                        id = Convert.ChangeType(value, typeof(long)) as long?;
                    }
                }
                else if (field != null)
                {
                    object value = field.GetValue(unityObject);
                    if (value != null)
                    {
                        id = Convert.ChangeType(value, typeof(long)) as long?;
                    }
                }

                return id;
            }
            else
            {
                return null;
            }
        }
    }
}
