using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class PrefabLoader : AssetLoader
    {
        public override Type Type => typeof(GameObject);

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
            Mesh mesh = (Mesh)Loader.LoadAssetAtPath(path, typeof(Mesh));
            UnityEngine.Object prefab = Loader.LoadAssetAtPath(path, Type);

            //a mesh exists here, so dont load it
            if (mesh && prefab)
            {
                return new List<Reference>();
            }

            Reference item = new Reference(prefab, Type, path);
            return new List<Reference>() { item };
        }
    }
}
