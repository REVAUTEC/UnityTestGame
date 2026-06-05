using UnityEngine;

namespace Autobazar.Player
{
    /// <summary>
    /// Pohyb hráče (postava jako kapsle) pomocí WASD, otáčení myší (osa X),
    /// sprint na Shift a jednoduchá gravitace. Vyžaduje CharacterController.
    /// Otáčení nahoru/dolů (pitch) řeší CameraController, aby se kamera a tělo nervaly.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Pohyb")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float gravity = 20f;

        [Header("Myš")]
        [SerializeField] private float mouseSensitivity = 3f;

        private CharacterController _controller;
        private float _yaw;            // natočení hráče kolem osy Y
        private float _verticalVelocity;

        /// <summary>Aktuální natočení hráče kolem Y – čte ho kamera.</summary>
        public float Yaw => _yaw;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Start()
        {
            LockCursor(true);
        }

        private void Update()
        {
            HandleCursorLock();
            HandleRotation();
            HandleMovement();
        }

        private void HandleRotation()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S

            Vector3 input = (transform.right * h + transform.forward * v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

            // Gravitace – drží hráče na zemi.
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity -= gravity * Time.deltaTime;

            Vector3 velocity = input * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void HandleCursorLock()
        {
            // ESC uvolní myš (např. pro práci v editoru), kliknutí ji zase zamkne.
            if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);
        }

        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
