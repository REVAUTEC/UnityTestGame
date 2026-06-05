using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Arcade řízení auta s „grip" pocitem: auto má setrvačnost, ve vyšší rychlosti se
    /// míň stáčí a v ostrých zatáčkách lehce smýká (drift). W plyn, S brzda/couvání,
    /// A/D zatáčení, Shift boost. Auto se naklání do zatáček. Stabilní (bez fyziky).
    /// </summary>
    public class CarController : MonoBehaviour
    {
        public bool ControlEnabled;

        [SerializeField] private float maxSpeed = 22f;
        [SerializeField] private float boostSpeed = 33f;
        [SerializeField] private float reverseSpeed = 7f;
        [SerializeField] private float accel = 16f;
        [SerializeField] private float brakeDecel = 26f;
        [SerializeField] private float coastDecel = 8f;
        [SerializeField] private float turnSpeed = 105f;
        [SerializeField] private float gripHigh = 9f;   // přilnavost při nízké rychlosti
        [SerializeField] private float gripLow = 4.5f;  // přilnavost při vysoké rychlosti (víc smyku)
        [SerializeField] private float leanAmount = 8f;

        private float _speed;
        private float _groundY;
        private float _lean;
        private Vector3 _moveDir = Vector3.forward;

        private Transform _visual;
        private Quaternion _visualBaseRot = Quaternion.identity;

        public float SpeedKmh => Mathf.Abs(_speed) * 3.6f;
        public float Speed01 => Mathf.Clamp01(Mathf.Abs(_speed) / maxSpeed);
        public bool Boosting { get; private set; }

        private void Awake()
        {
            _groundY = transform.position.y;
            _moveDir = transform.forward;
            _visual = transform.Find("Model");
            if (_visual != null) _visualBaseRot = _visual.localRotation;
        }

        public void ResetMotion()
        {
            _speed = 0f;
            _lean = 0f;
            Boosting = false;
            _moveDir = transform.forward;
        }

        public void SetGroundY(float y) => _groundY = y;

        private void Update()
        {
            float steer = 0f;

            if (ControlEnabled)
            {
                float throttle = Input.GetAxisRaw("Vertical");
                steer = Input.GetAxisRaw("Horizontal");
                Boosting = Input.GetKey(KeyCode.LeftShift) && throttle > 0.1f;

                float top = Boosting ? boostSpeed : maxSpeed;
                if (throttle > 0.1f) _speed = Mathf.MoveTowards(_speed, top, accel * Time.deltaTime);
                else if (throttle < -0.1f) _speed = Mathf.MoveTowards(_speed, -reverseSpeed, brakeDecel * Time.deltaTime);
                else _speed = Mathf.MoveTowards(_speed, 0f, coastDecel * Time.deltaTime);

                // Ve vyšší rychlosti se auto stáčí pomaleji (není to twitchy).
                if (Mathf.Abs(_speed) > 0.4f)
                {
                    float turn = turnSpeed * Mathf.Lerp(1f, 0.6f, Speed01) * Mathf.Sign(_speed);
                    transform.Rotate(0f, steer * turn * Time.deltaTime, 0f);
                }

                // Setrvačnost / smyk: směr pohybu dojíždí k tomu, kam auto míří.
                float grip = Mathf.Lerp(gripHigh, gripLow, Speed01) * (Boosting ? 0.8f : 1f);
                _moveDir = Vector3.Slerp(_moveDir, transform.forward, grip * Time.deltaTime).normalized;

                transform.position += _moveDir * (_speed * Time.deltaTime);
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
