using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
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