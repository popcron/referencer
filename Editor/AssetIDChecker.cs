using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    [InitializeOnLoad]
    public class AssetIDChecker
    {
        private const double CheckRate = 1.0;
        private static double nextCheck;

        static AssetIDChecker()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            //only check every 1 second
            if (EditorApplication.timeSinceStartup < nextCheck) return;

            nextCheck = EditorApplication.timeSinceStartup + CheckRate;
            bool changed = false;

            //check all reference items with an ID
            List<Reference> references = References.GetAllReferencesWithIDs();
            foreach (Reference reference in references)
            {
                if (reference.Object is ScriptableObject scriptableObject)
                {
                    long? id = Loader.GetIDFromScriptableObject(scriptableObject);
                    if (reference.ID != id)
                    {
                        reference.ID = id;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                Helper.DirtyReferences();
            }
        }
    }
}