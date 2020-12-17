using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Popcron.Referencer
{
    public class SpriteLoader : AssetLoader
    {
        public override Type Type => typeof(Sprite);

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
            string pathFileName = Path.GetFileNameWithoutExtension(path);
            List<Reference> items = new List<Reference>();
            UnityEngine.Object[] sprites = Loader.LoadAllAssetsAtPath(path);
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
                foreach (UnityEngine.Object sprite in sprites)
                {
                    //dont load the main asset itself if theres more than 1 sprite, its not a sprite
                    if (sprite.name == pathFileName)
                    {
                        continue;
                    }

                    Reference item = new Reference(sprite, Type, path + "/" + sprite.name);
                    items.Add(item);
                }
            }
            return items;
        }
    }
}
