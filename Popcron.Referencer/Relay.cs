using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    internal class Relay
    {
        internal static References CreateReferencesFile()
        {
            Type helper = Type.GetType("Popcron.Referencer.Helper, Popcron.Referencer.Editor");
            if (helper == null)
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
                                helper = type;
                            }
                        }
                    }
                }
            }

            if (helper != null)
            {
                MethodInfo createReferenceFileMethod = helper.GetMethod("CreateReferencesFile");
                object value = createReferenceFileMethod.Invoke(null, null);
                return value as References;
            }
            else
            {
                return null;
            }
        }
    }
}