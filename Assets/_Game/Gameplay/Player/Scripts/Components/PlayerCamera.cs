// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using Sirenix.OdinInspector;
using Unity.Cinemachine;

namespace FifthSemester.Player.Components {
    public class PlayerCamera : MonoBehaviour {
        [Header("Cinemachine References")]
        [SerializeField] private CinemachineCamera _vCam;
        [SerializeField, Required] private Transform _cameraTarget;

        [Header("Settings")]
        [SerializeField] private bool _cameraCanMove = true;
        [SerializeField, Range(0f, 1f)] private float _mouseSensitivity = 0.5f;

        [Header("Zoom")]
        [SerializeField] private bool _enableZoom = true;
        [FoldoutGroup("Zoom"), ShowIf("_enableZoom")]
        [SerializeField] private bool _holdToZoom = false;
        [FoldoutGroup("Zoom"), ShowIf("_enableZoom")]
        [SerializeField] private float _zoomFov = 30f;
        [FoldoutGroup("Zoom"), ShowIf("_enableZoom")]
        [SerializeField] private float _zoomStepTime = 5f;

        private float _defaultFov;
        private bool _isZoomed;
        private bool _zoomPressed;
        private bool _zoomPrevPressed;

        private Vector2 _lookInput;
        private PlayerMovement _movement;
        private PlayerController _player;
        private IEventBus _eventBus;

        private CinemachinePanTilt _panTilt;

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _movement = _player?.GetComponent<PlayerMovement>();

            if (_vCam != null) {
                _defaultFov = _vCam.Lens.FieldOfView;
                _panTilt = _vCam.GetComponent<CinemachinePanTilt>();
            }
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<LookInputEvent>(HandleLookInput);
            _eventBus?.Subscribe<ZoomInputEvent>(HandleZoomInput);

            // Trava o cursor se necessário
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<LookInputEvent>(HandleLookInput);
            _eventBus?.Unsubscribe<ZoomInputEvent>(HandleZoomInput);
        }

        public Transform GetCameraTarget() => _cameraTarget;

        private void Update() {
            if (!_cameraCanMove || _vCam == null || _player == null) return;

            ApplyRotation();

            float cameraYaw = _vCam.State.RawOrientation.eulerAngles.y;
            _player.transform.rotation = Quaternion.Euler(0, cameraYaw, 0);

            HandleZoom();
            HandleNoiseAmplitude();
        }

        private void HandleLookInput(LookInputEvent evt) {
            _lookInput = evt.Value;
        }

        private void HandleZoomInput(ZoomInputEvent evt) {
            _zoomPressed = evt.IsPressed;
        }

        private void ApplyRotation() {
            if (_panTilt == null) return;

            _panTilt.PanAxis.Value += _lookInput.x * _mouseSensitivity;
            _panTilt.TiltAxis.Value -= _lookInput.y * _mouseSensitivity;
        }

        private void HandleZoom() {
            if (!_enableZoom) return;

            bool isSprinting = _movement != null && _movement.IsSprinting;

            if (isSprinting) _isZoomed = false;
            else if (_holdToZoom) _isZoomed = _zoomPressed;
            else if (_zoomPressed && !_zoomPrevPressed) _isZoomed = !_isZoomed;

            _zoomPrevPressed = _zoomPressed;

            float targetFov = _isZoomed ? _zoomFov : _defaultFov;

            // Atualiza a lente da câmera virtual
            var lens = _vCam.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFov, _zoomStepTime * Time.deltaTime);
            _vCam.Lens = lens;
        }

        private void HandleNoiseAmplitude() {
            // Em vez de calcular seno/cosseno manualmente, alteramos a força do ruído
            var noise = _vCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null || _movement == null) return;

            float targetAmplitude = 0f;
            if (_movement.IsWalking) {
                targetAmplitude = _movement.IsSprinting ? 2.0f : 1.0f;
            }

            noise.AmplitudeGain = Mathf.Lerp(noise.AmplitudeGain, targetAmplitude, Time.deltaTime * 5f);
        }
    }
}
