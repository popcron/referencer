using UnityEngine;

namespace Popcron.Referencer
{
    public class ReferencesContainer : MonoBehaviour
    {
        public References references;

        private void Awake()
        {
            if (references)
            {
                references.EnsureCacheExists();
            }
        }

        private void OnEnable()
        {
            if (references)
            {
                references.EnsureCacheExists();
            }
        }
    }
}