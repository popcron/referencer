using UnityEngine;

namespace Popcron.Referencer
{
    public class ReferencesContainer : MonoBehaviour
    {
        public References references;

        private void Awake()
        {
            references?.CheckCache();
        }

        private void OnEnable()
        {
            references?.CheckCache();
        }
    }
}