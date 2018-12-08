using UnityEngine;

namespace Popcron.Referencer
{
    public class ReferencesContainer : MonoBehaviour
    {
        public References references;

        private void Awake()
        {
            references?.Cache();
        }

        private void OnEnable()
        {
            references?.Cache();
        }
    }
}