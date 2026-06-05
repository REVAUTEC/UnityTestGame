using UnityEngine;

namespace Autobazar.Player
{
    /// <summary>
    /// Kamera třetí osoby. Drží se za hráčem, horizontálně kopíruje jeho natočení
    /// (to řídí PlayerController myší), a myší ovládá náklon nahoru/dolů (pitch).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;          // hráč
        [SerializeField] private float distance = 6f;       // vzdálenost za hráčem
        [SerializeField] private float height = 2.4f;       // výška, na kterou se kamera dívá
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float followSmooth = 12f;

        private float _pitch = 15f;
        private PlayerController _playerController;

        /// <summary>Nastaví cíl (volá WorldBuilder při skládání scény).</summary>
        public void SetTarget(Transform t)
        {
            target = t;
            if (target != null) _playerController = target.GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (target != null && _playerController == null)
                _playerController = target.GetComponent<PlayerController>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            // Horizontální úhel bereme z hráče, vertikální z myši.
            float yaw = _playerController != null ? _playerController.Yaw : target.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(_pitch, yaw, 0f);

            Vector3 pivot = target.position + Vector3.up * height;
            Vector3 desiredPos = pivot - rotation * Vector3.forward * distance;

            // Plynulé dojetí kamery.
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);
            transform.LookAt(pivot);
        }
    }
}
