using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Arcade řízení auta (bez fyziky). W = plyn, S = brzda/couvání, A/D = zatáčení,
    /// Shift = boost. Auto se naklání do zatáček. Rychlost je k dispozici pro HUD.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        public bool ControlEnabled;

        [SerializeField] private float maxSpeed = 21f;
        [SerializeField] private float boostSpeed = 32f;
        [SerializeField] private float reverseSpeed = 7f;
        [SerializeField] private float accel = 17f;
        [SerializeField] private float brakeDecel = 26f;
        [SerializeField] private float coastDecel = 8f;
        [SerializeField] private float turnSpeed = 95f;
        [SerializeField] private float leanAmount = 8f;

        private float _speed;
        private float _groundY;
        private float _lean;

        private Transform _visual;
        private Quaternion _visualBaseRot = Quaternion.identity;

        public float SpeedKmh => Mathf.Abs(_speed) * 3.6f;
        public float Speed01 => Mathf.Clamp01(Mathf.Abs(_speed) / maxSpeed);
        public bool Boosting { get; private set; }

        private void Awake()
        {
            _groundY = transform.position.y;
            _visual = transform.Find("Model");
            if (_visual != null) _visualBaseRot = _visual.localRotation;
        }

        public void ResetMotion()
        {
            _speed = 0f;
            _lean = 0f;
            Boosting = false;
        }

        public void SetGroundY(float y) => _groundY = y;

        private void Update()
        {
            float throttle = 0f, steer = 0f;

            if (ControlEnabled)
            {
                throttle = Input.GetAxisRaw("Vertical");
                steer = Input.GetAxisRaw("Horizontal");
                Boosting = Input.GetKey(KeyCode.LeftShift) && throttle > 0.1f;

                float top = Boosting ? boostSpeed : maxSpeed;
                if (throttle > 0.1f) _speed = Mathf.MoveTowards(_speed, top, accel * Time.deltaTime);
                else if (throttle < -0.1f) _speed = Mathf.MoveTowards(_speed, -reverseSpeed, brakeDecel * Time.deltaTime);
                else _speed = Mathf.MoveTowards(_speed, 0f, coastDecel * Time.deltaTime);

                if (Mathf.Abs(_speed) > 0.4f)
                    transform.Rotate(0f, steer * turnSpeed * Mathf.Sign(_speed) * Time.deltaTime, 0f);

                transform.position += transform.forward * (_speed * Time.deltaTime);
                var p = transform.position; p.y = _groundY; transform.position = p;
            }
            else
            {
                Boosting = false;
            }

            // Náklon karoserie do zatáčky.
            float targetLean = -steer * Speed01 * leanAmount;
            _lean = Mathf.Lerp(_lean, targetLean, 8f * Time.deltaTime);
            if (_visual != null) _visual.localRotation = _visualBaseRot * Quaternion.Euler(0f, 0f, _lean);
        }
    }
}
