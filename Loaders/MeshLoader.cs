using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class MeshLoader : AssetLoader
    {
        public override Type Type => typeof(Mesh);

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
            UnityEngine.Object[] meshes = Loader.LoadAllAssetsAtPath(path);
            List<Reference> items = new List<Reference>();
            for (int i = 0; i < meshes.Length; i++)
            {
                string pathToMesh = path + "/" + meshes[i].name;
                Reference item = new Reference(meshes[i] as Mesh, Type, pathToMesh);
                items.Add(item);
            }

            return items;
        }
    }
}
