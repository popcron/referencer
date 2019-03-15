﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class PrefabLoader : AssetLoader
    {
        public override Type Type => typeof(GameObject);

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
            Mesh mesh = (Mesh)Loader.LoadAssetAtPath(path, typeof(Mesh));
            var prefab = Loader.LoadAssetAtPath(path, Type);

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
