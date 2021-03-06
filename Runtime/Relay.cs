﻿using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    /// <summary>
    /// This class relays method class to the Helper class in the editor assembly.
    /// </summary>
    public class Relay
    {
        private static Type helperType = null;
        private static MethodInfo loadAllMethod = null;
        private static MethodInfo findAssetsMethod = null;
        private static MethodInfo loadAssetAtPathMethod = null;
        private static MethodInfo loadAllAssetsAtPathMethod = null;

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
        /// Calls upon Helper.LoadAll();
        /// </summary>
        public static void LoadAll()
        {
            Type helper = HelperType;
            if (helper != null)
            {
                if (loadAllMethod == null)
                {
                    loadAllMethod = helper.GetMethod("LoadAll");
                }

                loadAllMethod.Invoke(null, null);
            }
        }

        /// <summary>
        /// Returns asset paths of this type.
        /// </summary>
        internal static string[] FindAssets<T>() where T : class => FindAssets($"t:{typeof(T)}");

        /// <summary>
        /// Returns asset paths with this filter as a search query.
        /// </summary>
        internal static string[] FindAssets(string filter)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                if (findAssetsMethod == null)
                {
                    findAssetsMethod = helper.GetMethod("FindAssets");
                }

                return (string[])findAssetsMethod.Invoke(null, new object[] { filter });
            }
            else
            {
                return new string[] { };
            }
        }

        /// <summary>
        /// Returns an object from a path.
        /// </summary>
        internal static T LoadAssetAtPath<T>(string path) where T : class
        {
            return LoadAssetAtPath(path, typeof(T)) as T;
        }

        /// <summary>
        /// Returns an object from a path using type.
        /// </summary>
        internal static Object LoadAssetAtPath(string path, Type type)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                if (loadAssetAtPathMethod == null)
                {
                    loadAssetAtPathMethod = helper.GetMethod("LoadAssetAtPath");
                }

                return (Object)loadAssetAtPathMethod.Invoke(null, new object[] { path, type });
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns all objects at the path.
        /// </summary>
        internal static Object[] LoadAllAssetsAtPath(string path)
        {
            Type helper = HelperType;
            if (helper != null)
            {
                if (loadAllAssetsAtPathMethod == null)
                {
                    loadAllAssetsAtPathMethod = helper.GetMethod("LoadAllAssetsAtPath");
                }

                return (Object[])loadAllAssetsAtPathMethod.Invoke(null, new object[] { path });
            }
            else
            {
                return null;
            }
        }
    }
}