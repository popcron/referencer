using UnityEditor;

namespace Popcron.Referencer
{
    [InitializeOnLoad]
    public class Initializer
    {
        static Initializer()
        {
            //refresh if the references file is empty or doesnt exist
            var references = Loader.FindAssetsByType<References>();
            if (references.Count == 0)
            {
                AssetProcessor.LoadAll();
            }
        }
    }
}
