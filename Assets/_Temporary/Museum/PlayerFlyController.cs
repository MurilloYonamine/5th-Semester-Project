using UnityEngine;

namespace FifthSemester.Dev {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerFlyController : MonoBehaviour {
        [Header("Movement Settings")]
        [Range(0f, 20f)] public float moveSpeed = 8f;
        [Range(0f, 20f)] public float verticalSpeed = 6f;
        [Range(0f, 10f)] public float lookSensitivity = 2f;

        private CharacterController _controller;
        private float _yaw;
        private float _pitch;

        void Start() {
            _controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update() {
            _yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float up = 0f;

            if (Input.GetKey(KeyCode.E)) up += 1f;      
            if (Input.GetKey(KeyCode.Q)) up -= 1f;     

            Vector3 move = (transform.forward * vertical + transform.right * horizontal + transform.up * up) * moveSpeed;
            _controller.Move(move * Time.deltaTime);
        }
    }
}
