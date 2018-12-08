using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Popcron.Referencer
{
    public class SpriteLoader : AssetLoader
    {
        public override Type Type => typeof(Sprite);

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
            string pathFileName = Path.GetFileNameWithoutExtension(path);
            List<Reference> items = new List<Reference>();
            var sprites = Loader.LoadAllAssetsAtPath(path);
            if (sprites.Length == 2)
            {
                if (sprites[0] is Sprite)
                {
                    Reference item = new Reference(sprites[0], Type, path);
                    items.Add(item);
                }
                else if (sprites[1] is Sprite)
                {
                    Reference item = new Reference(sprites[1], Type, path);
                    items.Add(item);
                }
            }
            else
            {
                foreach (var sprite in sprites)
                {
                    //dont load the main asset itself if theres more than 1 sprite, its not a sprite
                    if (sprite.name == pathFileName) continue;

                    Reference item = new Reference(sprite, Type, path + "/" + sprite.name);
                    items.Add(item);
                }
            }
            return items;
        }
    }
}
