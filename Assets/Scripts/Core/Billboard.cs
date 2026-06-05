using UnityEngine;

namespace Autobazar.Core
{
    /// <summary>
    /// Otočí objekt (typicky plovoucí text nad autem) tak, aby byl
    /// vždy čelem ke kameře a dal se přečíst z jakéhokoli úhlu.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Transform _cam;

        private void LateUpdate()
        {
            if (_cam == null)
            {
                if (Camera.main != null) _cam = Camera.main.transform;
                else return;
            }

            // Text v Unity "kouká" směrem +Z, takže forward natočíme směrem od kamery.
            transform.forward = (transform.position - _cam.position).normalized;
        }
    }
}
