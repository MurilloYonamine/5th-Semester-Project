// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Enemy;

namespace FifthSemester.Player.Components {
    public class PlayerFlashlight : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Light _spotLight;
        [SerializeField, Tooltip("Transform used as origin for the flashlight (usually the player camera)")] private Transform _lightOrigin;

        [Header("Aim")]
        [SerializeField, Tooltip("Fallback distance when raycast hits nothing")] private float _aimDistance = 30f;
        [SerializeField, Tooltip("Smoothing speed for flashlight rotation")] private float _aimSmooth = 10f;
        [SerializeField] private LayerMask _aimCollisionMask = ~0;
        [SerializeField, Tooltip("Radius for sphere cast (thickness of the flashlight beam)")]
        private float _aimRadius = 0.4f;

        [SerializeField, Tooltip("Seconds of continuous illumination required to consider the target illuminated")]
        private float _illuminationDelay = 0.12f;

        // internal timer
        private float _illuminationTimer = 0f;

        [Header("Audio")] [SerializeField] private AudioClip _toggleSound;

        private bool _isOn = false;

        private IEventBus _eventBus;
        private IAudioService _audioService;
        private Camera _camera;
        private Vector2 _lookInput;
        private GameObject _currentIlluminatedTarget;

        private void Awake() {
            if (_spotLight == null) _spotLight = GetComponentInChildren<Light>();

            if (_lightOrigin == null && Camera.main != null) {
                _lightOrigin = Camera.main.transform;
            }

            if (_spotLight != null && _lightOrigin != null) {
                _spotLight.transform.SetParent(_lightOrigin, false);
                _spotLight.enabled = false;
            }
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _audioService = ServiceLocator.Get<IAudioService>();

            _eventBus?.Subscribe<FlashlightInputEvent>(HandleFlashlightInput);
            _eventBus?.Subscribe<LookInputEvent>(HandleLookInput);

            _camera = Camera.main;
            if (_camera == null) Debug.LogWarning("PlayerFlashlight: Camera.main is null. Assign camera or set _lightOrigin.");
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<FlashlightInputEvent>(HandleFlashlightInput);
            _eventBus?.Unsubscribe<LookInputEvent>(HandleLookInput);
        }

        private void HandleLookInput(LookInputEvent evt) {
            _lookInput = evt.Value;
        }

        private void HandleFlashlightInput(FlashlightInputEvent evt) {
            if (!evt.IsPressed) return;
            _isOn = !_isOn;
            UpdateLightEnabled();
            PlayToggleSound();
        }

        private void UpdateLightEnabled() {
            if (_spotLight == null) return;
            _spotLight.enabled = _isOn;
            if (_spotLight.enabled && _lightOrigin != null) {
                _spotLight.transform.position = _lightOrigin.position;
            }
        }

        private void Update() {
            if (_isOn) AimLightTowardsPointer();
        }

        private void AimLightTowardsPointer() {
            if (_spotLight == null || _lightOrigin == null || _camera == null) return;

            Vector3 screenPoint;
            if (Cursor.lockState == CursorLockMode.Locked) {
                screenPoint = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            } else {
                screenPoint = Input.mousePosition;
            }

            Ray ray = _camera.ScreenPointToRay(screenPoint);

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit, _aimDistance, _aimCollisionMask)) {
                targetPoint = hit.point;
            } else {
                targetPoint = ray.GetPoint(_aimDistance);
            }

            Vector3 origin = _lightOrigin.position;
            Vector3 desiredDir = (targetPoint - origin).normalized;

            _spotLight.transform.position = origin;
            Quaternion desiredRot = Quaternion.LookRotation(desiredDir, _lightOrigin.up);
            _spotLight.transform.rotation = Quaternion.Slerp(_spotLight.transform.rotation, desiredRot, Time.deltaTime * _aimSmooth);

            // SphereCast to give the beam a thickness
            if (_eventBus == null) return;

            GameObject detectedSeeker = null;
            if (Physics.SphereCast(origin, _aimRadius, desiredDir, out RaycastHit seekerHit, _aimDistance, _aimCollisionMask)) {
                if (seekerHit.collider != null) {
                    // try to find LightSeeker on collider or its parents
                    var seeker = seekerHit.collider.GetComponentInParent<LightSeeker>();
                    if (seeker != null) {
                        detectedSeeker = seeker.gameObject;
                        Debug.Log($"[PlayerFlashlight] spherecast hit LightSeeker: {detectedSeeker.name}");
                    }
                }
            }

            // Debounce: require continuous illumination for _illuminationDelay
            if (detectedSeeker != null) {
                if (_currentIlluminatedTarget == detectedSeeker) {
                    // keep timer topped
                    _illuminationTimer = _illuminationDelay;
                } else {
                    _illuminationTimer += Time.deltaTime;
                    if (_illuminationTimer >= _illuminationDelay) {
                        // switch targets
                        if (_currentIlluminatedTarget != null) {
                            Debug.Log($"[PlayerFlashlight] Parou de iluminar: {_currentIlluminatedTarget.name}");
                            _eventBus.Publish(new FlashlightTargetedEvent(_currentIlluminatedTarget, false));
                        }

                        Debug.Log($"[PlayerFlashlight] Começou a iluminar: {detectedSeeker.name}");
                        _eventBus.Publish(new FlashlightTargetedEvent(detectedSeeker, true));
                        _currentIlluminatedTarget = detectedSeeker;
                        _illuminationTimer = _illuminationDelay;
                    }
                }
            } else {
                // no detection: clear current target and reset timer
                if (_currentIlluminatedTarget != null) {
                    Debug.Log($"[PlayerFlashlight] Parou de iluminar: {_currentIlluminatedTarget.name}");
                    _eventBus.Publish(new FlashlightTargetedEvent(_currentIlluminatedTarget, false));
                    _currentIlluminatedTarget = null;
                }
                _illuminationTimer = 0f;
            }
        }

        private void PlayToggleSound() {
            if (_toggleSound == null || _audioService == null) return;
            _audioService.PlaySFX(_toggleSound);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_isOn || _lightOrigin == null) return;
            Gizmos.color = Color.cyan;
            Vector3 origin = _lightOrigin.position;
            Vector3 dir = _spotLight != null ? _spotLight.transform.forward : _lightOrigin.forward;
            Gizmos.DrawRay(origin, dir * _aimDistance);
        }
#endif
    }
}
