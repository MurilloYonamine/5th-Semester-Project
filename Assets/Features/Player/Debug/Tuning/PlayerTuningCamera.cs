// Autor: Murillo Gomes Yonamine
// Data: 15/02/2026

using UnityEngine;
using FifthSemester.Player.Components;
using TMPro;

namespace FifthSemester.Player.Tuning {
    public class PlayerTuningCamera : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        private PlayerCamera _camera;

        [Header("Inputs (opcional)")]
        [SerializeField] private TMP_InputField _mouseSensitivityInput;
        [SerializeField] private TMP_InputField _maxLookAngleInput;
        [SerializeField] private TMP_InputField _zoomFovInput;
        [SerializeField] private TMP_InputField _zoomStepTimeInput;
        [SerializeField] private TMP_InputField _bobSpeedInput;
        [SerializeField] private TMP_InputField _bobAmountXInput;
        [SerializeField] private TMP_InputField _bobAmountYInput;
        [SerializeField] private TMP_InputField _bobAmountZInput;

        [SerializeField, HideInInspector] private float _cameraBobX;
        [SerializeField, HideInInspector] private float _cameraBobY;
        [SerializeField, HideInInspector] private float _cameraBobZ;

        private void Awake() {
            if (_playerController != null) {
                _camera = _playerController.PlayerCameraComponent;
            }
        }

        private void OnEnable() {
            RefreshInputs();
        }

        public void RefreshInputs() {
            if (_camera == null) return;

            if (_mouseSensitivityInput != null) {
                _mouseSensitivityInput.text = _camera.MouseSensitivity.ToString("0.##");
            }

            if (_maxLookAngleInput != null) {
                _maxLookAngleInput.text = _camera.MaxLookAngle.ToString("0.##");
            }

            if (_zoomFovInput != null) {
                _zoomFovInput.text = _camera.ZoomFov.ToString("0.##");
            }

            if (_zoomStepTimeInput != null) {
                _zoomStepTimeInput.text = _camera.ZoomStepTime.ToString("0.##");
            }

            if (_bobSpeedInput != null) {
                _bobSpeedInput.text = _camera.BobSpeed.ToString("0.##");
            }

            Vector3 bob = _camera.BobAmount;
            _cameraBobX = bob.x;
            _cameraBobY = bob.y;
            _cameraBobZ = bob.z;

            if (_bobAmountXInput != null) {
                _bobAmountXInput.text = _cameraBobX.ToString("0.##");
            }

            if (_bobAmountYInput != null) {
                _bobAmountYInput.text = _cameraBobY.ToString("0.##");
            }

            if (_bobAmountZInput != null) {
                _bobAmountZInput.text = _cameraBobZ.ToString("0.##");
            }
        }

        public void SetCameraCanMove(bool value) {
            if (_camera != null) _camera.SetCameraCanMove(value);
        }

        public void SetInvertY(bool value) {
            if (_camera != null) _camera.SetInvertY(value);
        }

        public void SetMouseSensitivity(float value) {
            if (_camera != null) {
                _camera.SetMouseSensitivity(value);
            }
        }

        public void SetMaxLookAngle(float value) {
            if (_camera != null) {
                _camera.SetMaxLookAngle(value);
            }
        }

        public void SetEnableZoom(bool value) {
            if (_camera != null) _camera.SetEnableZoom(value);
        }

        public void SetHoldToZoom(bool value) {
            if (_camera != null) _camera.SetHoldToZoom(value);
        }

        public void SetZoomFov(float value) {
            if (_camera != null) {
                _camera.SetZoomFov(value);
            }
        }

        public void SetZoomStepTime(float value) {
            if (_camera != null) {
                _camera.SetZoomStepTime(value);
            }
        }

        public void SetEnableHeadBob(bool value) {
            if (_camera != null) _camera.SetEnableHeadBob(value);
        }

        public void SetBobSpeed(float value) {
            if (_camera != null) {
                _camera.SetBobSpeed(value);
            }
        }

        public void SetBobAmountX(float value) {
            _cameraBobX = value;
            if (_camera != null) {
                _camera.SetBobAmount(new Vector3(_cameraBobX, _cameraBobY, _cameraBobZ));
            }
        }

        public void SetBobAmountY(float value) {
            _cameraBobY = value;
            if (_camera != null) {
                _camera.SetBobAmount(new Vector3(_cameraBobX, _cameraBobY, _cameraBobZ));
            }
        }

        public void SetBobAmountZ(float value) {
            _cameraBobZ = value;
            if (_camera != null) {
                _camera.SetBobAmount(new Vector3(_cameraBobX, _cameraBobY, _cameraBobZ));
            }
        }

        public void SetMouseSensitivityFromText(string text) {
            if (_camera == null) return;
            if (float.TryParse(text, out float value)) {
                _camera.SetMouseSensitivity(value);
            }
        }

        public void SetMaxLookAngleFromText(string text) {
            if (_camera == null) return;
            if (float.TryParse(text, out float value)) {
                _camera.SetMaxLookAngle(value);
            }
        }

        public void SetZoomFovFromText(string text) {
            if (_camera == null) return;
            if (float.TryParse(text, out float value)) {
                _camera.SetZoomFov(value);
            }
        }

        public void SetZoomStepTimeFromText(string text) {
            if (_camera == null) return;
            if (float.TryParse(text, out float value)) {
                _camera.SetZoomStepTime(value);
            }
        }

        public void SetBobSpeedFromText(string text) {
            if (_camera == null) return;
            if (float.TryParse(text, out float value)) {
                _camera.SetBobSpeed(value);
            }
        }

        public void SetBobAmountXFromText(string text) {
            if (float.TryParse(text, out float value)) {
                SetBobAmountX(value);
            }
        }

        public void SetBobAmountYFromText(string text) {
            if (float.TryParse(text, out float value)) {
                SetBobAmountY(value);
            }
        }

        public void SetBobAmountZFromText(string text) {
            if (float.TryParse(text, out float value)) {
                SetBobAmountZ(value);
            }
        }
    }
}
