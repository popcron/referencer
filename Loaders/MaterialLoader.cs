using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class MaterialLoader : AssetLoader
    {
        public override Type Type => typeof(Material);

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
            UnityEngine.Object material = Loader.LoadAssetAtPath(path, Type);
            Reference item = new Reference(material, Type, path);
            return new List<Reference>() { item };
        }
    }
}
