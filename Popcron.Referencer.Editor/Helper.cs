using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    public class Helper
    {
        public static References CreateReferencesFile()
        {
            References references = null;
            List<References> allReferences = Loader.FindAssetsByType<References>();
            for (int i = 0; i < allReferences.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(allReferences[i]);
                if (path == Settings.Current.referencesAssetPath)
                {
                    references = allReferences[i];
                    break;
                }
            }

            //still null, so create a new one
            if (!references)
            {
                references = ScriptableObject.CreateInstance<References>();
                AssetDatabase.CreateAsset(references, Settings.Current.referencesAssetPath);
                AssetDatabase.Refresh();
            }

            return references;
        }
    }
}
