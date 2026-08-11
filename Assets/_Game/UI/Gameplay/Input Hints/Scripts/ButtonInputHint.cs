// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026
using System;
using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Gameplay.UI {
    public class ButtonInputHint : InputHintBase {
        [Serializable]
        public struct ButtonSprites {
            public Sprite normal;
            public Sprite pressed;
        }

        [Header("Variantes por Dispositivo")]
        [SerializeField] private ButtonSprites _keyboard;
        [SerializeField] private ButtonSprites _xbox;
        [SerializeField] private ButtonSprites _playstation;

        private ButtonSprites GetSprites(DeviceDisplayType device) => device switch {
            DeviceDisplayType.PlayStation => _playstation,
            DeviceDisplayType.Xbox => _xbox,
            _ => _keyboard
        };

        public override void UpdateIdleSprite() {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetLastUsedDevice()).normal;
        }

        protected override void OnInputStarted(InputAction.CallbackContext context) {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetCurrentDevice(context)).pressed;
        }

        protected override void OnInputPerformed(InputAction.CallbackContext context) {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetCurrentDevice(context)).pressed;
        }

        protected override void OnInputCanceled(InputAction.CallbackContext context) {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetCurrentDevice(context)).normal;
        }

        protected override void OnDeviceChanged(DeviceDisplayType newDevice) {
            var sprites = GetSprites(newDevice);
            if (_hintImage != null)
                _hintImage.sprite = sprites.normal;
        }
    }
}