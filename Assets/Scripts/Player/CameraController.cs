using UnityEngine;
using Autobazar.Vehicles;

namespace Autobazar.Player
{
    /// <summary>
    /// Kamera za autem (chase cam). Drží se za cílem podle jeho natočení, plynule dojíždí,
    /// dívá se na auto a při rychlosti/boostu rozšiřuje zorné pole (pocit rychlosti).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float distance = 8.5f;
        [SerializeField] private float height = 3.6f;
        [SerializeField] private float lookHeight = 1.4f;
        [SerializeField] private float lookAhead = 4.5f;
        [SerializeField] private float followSmooth = 9f;
        [SerializeField] private float rotateSmooth = 7f;

        [SerializeField] private float baseFov = 62f;
        [SerializeField] private float speedFov = 12f;
        [SerializeField] private float boostFov = 9f;

        private Transform _target;
        private CarController _car;
        private Camera _cam;
        private float _yaw;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        public void SetTarget(Transform t)
        {
            _target = t;
            _car = t != null ? t.GetComponent<CarController>() : null;
            if (t != null)
            {
                _yaw = t.eulerAngles.y;
                SnapBehind();
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            _yaw = Mathf.LerpAngle(_yaw, _target.eulerAngles.y, rotateSmooth * Time.deltaTime);

            Vector3 desired = DesiredPosition(_yaw);
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(LookTarget());

            if (_cam != null && _car != null)
            {
                float target = baseFov + _car.Speed01 * speedFov + (_car.Boosting ? boostFov : 0f);
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, target, 5f * Time.deltaTime);
            }
        }

        private void SnapBehind()
        {
            transform.position = DesiredPosition(_yaw);
            transform.LookAt(LookTarget());
        }

        private Vector3 LookTarget()
        {
            return _target.position + _target.forward * lookAhead + Vector3.up * lookHeight;
        }

        private Vector3 DesiredPosition(float yaw)
        {
            Vector3 back = Quaternion.Euler(0f, yaw, 0f) * Vector3.back;
            return _target.position + back * distance + Vector3.up * height;
        }
    }
}
