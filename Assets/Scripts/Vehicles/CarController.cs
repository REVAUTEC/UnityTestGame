using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Jednoduché arcade řízení auta (bez fyziky/Rigidbody) pro testovací jízdu.
    /// W = plyn, S = brzda/couvání, A/D = zatáčení. Přidává se na auto jen po dobu jízdy.
    /// </summary>
    public class CarController : MonoBehaviour
    {
        public bool ControlEnabled = true;

        [SerializeField] private float maxSpeed = 16f;       // m/s (~58 km/h)
        [SerializeField] private float reverseSpeed = 6f;
        [SerializeField] private float accel = 14f;
        [SerializeField] private float brakeDecel = 24f;
        [SerializeField] private float coastDecel = 7f;
        [SerializeField] private float turnSpeed = 85f;      // stupňů/s

        private float _speed;
        private float _groundY;

        private void OnEnable()
        {
            _groundY = transform.position.y;
            _speed = 0f;
        }

        private void Update()
        {
            if (!ControlEnabled) return;

            float throttle = Input.GetAxisRaw("Vertical");
            float steer = Input.GetAxisRaw("Horizontal");

            if (throttle > 0.1f)
                _speed = Mathf.MoveTowards(_speed, maxSpeed, accel * Time.deltaTime);
            else if (throttle < -0.1f)
                _speed = Mathf.MoveTowards(_speed, -reverseSpeed, brakeDecel * Time.deltaTime);
            else
                _speed = Mathf.MoveTowards(_speed, 0f, coastDecel * Time.deltaTime);

            // Zatáčet jde jen za pohybu.
            if (Mathf.Abs(_speed) > 0.4f)
                transform.Rotate(0f, steer * turnSpeed * Time.deltaTime, 0f);

            transform.position += transform.forward * (_speed * Time.deltaTime);

            // Drž auto na zemi (arcade, bez fyziky).
            var p = transform.position;
            p.y = _groundY;
            transform.position = p;
        }
    }
}
