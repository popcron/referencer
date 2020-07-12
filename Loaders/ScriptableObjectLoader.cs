﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class ScriptableObjectLoader : AssetLoader
    {
        public override Type Type => typeof(ScriptableObject);

        public override List<Reference> LoadAll()
        {
            List<Reference> items = new List<Reference>();
            List<string> paths = Loader.FindAssets($"t:{Type.Name}");
            List<string> processedPaths = new List<string>();
            foreach (string path in paths)
            {
                if (!processedPaths.Contains(path))
                {
                    processedPaths.Add(path);
                    items.AddRange(Load(path));
                }
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
