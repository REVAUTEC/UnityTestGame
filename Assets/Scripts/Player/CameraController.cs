using UnityEngine;

namespace Autobazar.Player
{
    /// <summary>
    /// Kamera za autem (chase cam). Drží se za cílem podle jeho natočení, plynule dojíždí
    /// a dívá se na auto. Žádné ovládání myší – ideální pro závodění.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float distance = 8.5f;
        [SerializeField] private float height = 3.6f;
        [SerializeField] private float lookHeight = 1.4f;
        [SerializeField] private float followSmooth = 9f;
        [SerializeField] private float rotateSmooth = 7f;

        private Transform _target;
        private float _yaw;

        public void SetTarget(Transform t)
        {
            _target = t;
            if (t != null)
            {
                _yaw = t.eulerAngles.y;
                SnapBehind();
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Plynule dorovnáváme natočení za auto.
            _yaw = Mathf.LerpAngle(_yaw, _target.eulerAngles.y, rotateSmooth * Time.deltaTime);

            Vector3 desired = DesiredPosition(_yaw);
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(_target.position + Vector3.up * lookHeight);
        }

        private void SnapBehind()
        {
            transform.position = DesiredPosition(_yaw);
            transform.LookAt(_target.position + Vector3.up * lookHeight);
        }

        private Vector3 DesiredPosition(float yaw)
        {
            Vector3 back = Quaternion.Euler(0f, yaw, 0f) * Vector3.back;
            return _target.position + back * distance + Vector3.up * height;
        }
    }
}
