using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class ScriptableObjectLoader : AssetLoader
    {
        public override Type Type => typeof(ScriptableObject);

        public override List<Reference> LoadAll(Settings settings)
        {
            List<Reference> items = new List<Reference>();
            string[] paths = Loader.FindAssets($"t:{Type.Name}");
            for (int i = 0; i < paths.Length; i++)
            {
                items.AddRange(Load(paths[i]));
            }

            return items;
        }

        public override List<Reference> Load(string path)
        {
            UnityEngine.Object scriptableObject = Loader.LoadAssetAtPath(path, Type);
            if (!scriptableObject)
            {
                return new List<Reference>();
            }

            Reference item = new Reference(scriptableObject, scriptableObject.GetType(), path);
            return new List<Reference>() { item };
        }
    }
}
