using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class TextureLoader : AssetLoader
    {
        public override Type Type => typeof(Texture);

        public override List<Reference> LoadAll()
        {
            List<Reference> items = new List<Reference>();
            List<string> paths = Loader.FindAssets("t:" + Type.Name);
            List<string> processedPaths = new List<string>();
            foreach (string path in paths)
            {
                if (processedPaths.Contains(path)) continue;

                processedPaths.Add(path);

                items.AddRange(Load(path));
            }

            return items;
        }

        public override List<Reference> Load(string path)
        {
            var prefab = Loader.LoadAssetAtPath(path, Type);
            Reference item = new Reference(prefab, Type, path);
            return new List<Reference>() { item };
        }
    }
}
