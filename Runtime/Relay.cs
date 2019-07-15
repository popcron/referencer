using System;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    //this class relays method class to the Helper class in the editor assembly
    internal class Relay
    {
        private static Type helperType = null;

        private static Type HelperType
        {
            get
            {
                //try and get directly using name and assembly
                if (helperType == null)
                {
                    helperType = Type.GetType("Popcron.Referencer.Helper, Popcron.Referencer.Editor");
                }

                //still didnt find, scrape everything
                if (helperType == null)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        if (assembly.FullName.StartsWith("Popcron.Referencer.Editor,"))
                        {
                            var types = assembly.GetTypes();
                            foreach (var type in types)
                            {
                                if (type.FullName == "Popcron.Referencer.Helper")
                                {
                                    helperType = type;
                                }
                            }
                        }
                    }
                }

                return helperType;
            }
        }

        /// <summary>
        /// Creates a Reference asset
        /// </summary>
        internal static References CreateReferencesFile()
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("CreateReferencesFile");
                object value = method.Invoke(null, null);
                return value as References;
            }
            else
            {
                return null;
            }
        }

        internal static void LoadAll()
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("LoadAll");
                method.Invoke(null, null);
            }
        }

        /// <summary>
        /// Returns asset paths of this type
        /// </summary>
        internal static List<string> FindAssets<T>() where T : class
        {
            string filter = string.Format("t:{0}", typeof(T));
            return FindAssets(filter);
        }

        /// <summary>
        /// Returns asset paths with this filter as a search query
        /// </summary>
        internal static List<string> FindAssets(string filter)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("FindAssets");
                return method.Invoke(null, new object[] { filter }) as List<string>;
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns an object from a path
        /// </summary>
        internal static T LoadAssetAtPath<T>(string path) where T : class
        {
            return LoadAssetAtPath(path, typeof(T)) as T;
        }

        /// <summary>
        /// Returns an object from a path using type
        /// </summary>
        internal static Object LoadAssetAtPath(string path, Type type)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("LoadAssetAtPath");
                return method.Invoke(null, new object[] { path, type }) as Object;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all objects at the path
        /// </summary>
        internal static Object[] LoadAllAssetsAtPath(string path)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("LoadAllAssetsAtPath");
                return method.Invoke(null, new object[] { path }) as Object[];
            }
            else
            {
                return null;
            }
        }

        internal static void DirtyScene()
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("DirtyScene");
                method.Invoke(null, null);
            }
        }

        internal static void DirtyReferences()
        {
            Type helper = HelperType;
            if (helper != null)
            {
                MethodInfo method = helper.GetMethod("DirtyReferences");
                method.Invoke(null, null);
            }
        }
    }
}