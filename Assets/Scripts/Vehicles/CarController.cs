using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Arcade řízení auta (bez fyziky). W = plyn, S = brzda/couvání, A/D = zatáčení,
    /// Shift = boost. Drží auto na zemi. Rychlost je k dispozici pro HUD.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        public bool ControlEnabled;

        [SerializeField] private float maxSpeed = 21f;       // m/s (~75 km/h)
        [SerializeField] private float boostSpeed = 32f;
        [SerializeField] private float reverseSpeed = 7f;
        [SerializeField] private float accel = 17f;
        [SerializeField] private float brakeDecel = 26f;
        [SerializeField] private float coastDecel = 8f;
        [SerializeField] private float turnSpeed = 95f;      // stupňů/s

        private float _speed;
        private float _groundY;

        /// <summary>Aktuální rychlost v km/h (pro HUD).</summary>
        public float SpeedKmh => Mathf.Abs(_speed) * 3.6f;

        public bool Boosting { get; private set; }

        private void Awake()
        {
            _groundY = transform.position.y;
        }

        /// <summary>Zastaví auto (před startem / restartem).</summary>
        public void ResetMotion()
        {
            _speed = 0f;
            Boosting = false;
        }

        public void SetGroundY(float y) => _groundY = y;

        private void Update()
        {
            if (!ControlEnabled)
            {
                Boosting = false;
                return;
            }

            float throttle = Input.GetAxisRaw("Vertical");
            float steer = Input.GetAxisRaw("Horizontal");
            Boosting = Input.GetKey(KeyCode.LeftShift) && throttle > 0.1f;

            float topSpeed = Boosting ? boostSpeed : maxSpeed;

            if (throttle > 0.1f)
                _speed = Mathf.MoveTowards(_speed, topSpeed, accel * Time.deltaTime);
            else if (throttle < -0.1f)
                _speed = Mathf.MoveTowards(_speed, -reverseSpeed, brakeDecel * Time.deltaTime);
            else
                _speed = Mathf.MoveTowards(_speed, 0f, coastDecel * Time.deltaTime);

            if (Mathf.Abs(_speed) > 0.4f)
            {
                float dir = Mathf.Sign(_speed);
                transform.Rotate(0f, steer * turnSpeed * dir * Time.deltaTime, 0f);
            }

            transform.position += transform.forward * (_speed * Time.deltaTime);

            var p = transform.position;
            p.y = _groundY;
            transform.position = p;
        }
    }
}
