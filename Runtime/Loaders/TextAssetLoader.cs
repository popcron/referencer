using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Referencer
{
    public class TextAssetLoader : AssetLoader
    {
        public override Type Type => typeof(TextAsset);

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
            UnityEngine.Object prefab = Loader.LoadAssetAtPath(path, Type);

#if UNITY_EDITOR
            if (prefab is UnityEditorInternal.AssemblyDefinitionAsset)
            {
                return new List<Reference>();
            }
#endif

            Reference item = new Reference(prefab, Type, path);
            return new List<Reference>() { item };
        }
    }
}
