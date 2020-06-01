using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Popcron.Referencer
{
    public class AudioClipLoader : AssetLoader
    {
        public override Type Type => typeof(AudioClip);

        public override List<Reference> LoadAll()
        {
            List<Reference> items = new List<Reference>();
            List<string> paths = Loader.FindAssets($"t:{Type.Name}");
            List<string> processedPaths = new List<string>();
            for (int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];
                if (processedPaths.Contains(path))
                {
                    continue;
                }

                processedPaths.Add(path);
                items.AddRange(Load(path));
            }

            return items;
        }

        public override List<Reference> Load(string path)
        {
            Object audioClip = Loader.LoadAssetAtPath(path, Type);
            Reference item = new Reference(audioClip, Type, path);
            return new List<Reference>() { item };
        }
    }
}
