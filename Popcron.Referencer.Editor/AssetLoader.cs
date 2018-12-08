using System;
using System.Collections.Generic;

namespace Popcron.Referencer
{
    public abstract class AssetLoader
    {
        public abstract Type Type { get; }
        public abstract List<Reference> LoadAll();
        public abstract List<Reference> Load(string path);
    }
}
