// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using FifthSemester.Core.Services;
using FifthSemester.Core.Enums;

namespace FifthSemester.Gameplay.UI.InputHints {
    public abstract class InputHintBase : MonoBehaviour {
        [Header("Referências Base")]
        [SerializeField] protected Image _hintImage;
        [SerializeField] protected InputActionReference _inputAction;

        protected IInputDeviceService _hintService;
        private bool _isBound;


        protected virtual void Start() {
            _hintService = ServiceLocator.Get<IInputDeviceService>();
            BindDeviceService();
        }

        protected virtual void OnEnable() {
            if (_inputAction != null) {
                _inputAction.action.Enable();
                _inputAction.action.started += OnInputStarted;
                _inputAction.action.performed += OnInputPerformed;
                _inputAction.action.canceled += OnInputCanceled;
            }

            if (_hintService == null)
                ServiceLocator.TryGet<IInputDeviceService>(out _hintService);

            BindDeviceService();

            UpdateIdleSprite();
        }

        protected virtual void OnDisable() {
            if (_inputAction != null) {
                _inputAction.action.started -= OnInputStarted;
                _inputAction.action.performed -= OnInputPerformed;
                _inputAction.action.canceled -= OnInputCanceled;
            }

            if (_hintService != null && _isBound) {
                _hintService.OnDeviceChanged -= OnDeviceChanged;
                _isBound = false;
            }
        }

        private void BindDeviceService() {
            if (_hintService == null || _isBound) return;

            _hintService.OnDeviceChanged += OnDeviceChanged;
            _isBound = true;
            OnDeviceChanged(_hintService.CurrentDevice);
        }

        protected abstract void OnDeviceChanged(DeviceDisplayType newDevice);

        protected DeviceDisplayType GetCurrentDevice(InputAction.CallbackContext context) {
            if (context.control == null) return DeviceDisplayType.Keyboard;
            return ParseDeviceName(context.control.device.name, context.control.device);
        }

        protected DeviceDisplayType GetLastUsedDevice() {
            if (_hintService != null)
                return _hintService.CurrentDevice;

            if (_inputAction != null && _inputAction.action.activeControl != null)
                return ParseDeviceName(_inputAction.action.activeControl.device.name, _inputAction.action.activeControl.device);

            if (Gamepad.current != null)
                return ParseDeviceName(Gamepad.current.name, Gamepad.current);

            return DeviceDisplayType.Keyboard;
        }

        private DeviceDisplayType ParseDeviceName(string deviceName, InputDevice device) {
            string name = deviceName ?? string.Empty;
            string layout = device.layout ?? string.Empty;
            string displayName = device.displayName ?? string.Empty;

            if (name.Contains("DualShock") || name.Contains("DualSense") ||
                layout.Contains("DualShock") || layout.Contains("DualSense") ||
                displayName.Contains("DualShock") || displayName.Contains("DualSense") ||
                displayName.Contains("PlayStation"))
                return DeviceDisplayType.PlayStation;

            if (name.Contains("XInput") || name.Contains("Xbox") ||
                layout.Contains("XInput") || layout.Contains("Xbox") ||
                displayName.Contains("Xbox"))
                return DeviceDisplayType.Xbox;

            if (device is Gamepad)
                return DeviceDisplayType.Xbox;

            return DeviceDisplayType.Keyboard;
        }

        protected abstract void OnInputStarted(InputAction.CallbackContext context);
        protected abstract void OnInputPerformed(InputAction.CallbackContext context);
        protected abstract void OnInputCanceled(InputAction.CallbackContext context);
        public abstract void UpdateIdleSprite();
    }
}