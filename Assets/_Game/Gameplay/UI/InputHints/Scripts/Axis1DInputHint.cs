// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using System;
using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Gameplay.UI.InputHints {
    public class Axis1DInputHint : InputHintBase {
        [Serializable]
        public struct Axis1DSprites {
            public Sprite idle;
            public Sprite up;
            public Sprite down;
        }

        [Header("Variantes por Dispositivo (Eixo 1D)")]
        [SerializeField] private Axis1DSprites _keyboard;
        [SerializeField] private Axis1DSprites _xbox;
        [SerializeField] private Axis1DSprites _playstation;

        private Axis1DSprites GetSprites(DeviceDisplayType device) => device switch {
            DeviceDisplayType.PlayStation => _playstation,
            DeviceDisplayType.Xbox => _xbox,
            _ => _keyboard
        };

        public override void UpdateIdleSprite() {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetLastUsedDevice()).idle;
        }

        protected override void OnInputStarted(InputAction.CallbackContext context) => ProcessInput(context);
        protected override void OnInputPerformed(InputAction.CallbackContext context) => ProcessInput(context);
        protected override void OnDeviceChanged(DeviceDisplayType newDevice) {
            var sprites = GetSprites(newDevice);
            if (_hintImage != null)
                _hintImage.sprite = sprites.idle;
        }
        protected override void OnInputCanceled(InputAction.CallbackContext context) {
            if (_hintImage != null)
                _hintImage.sprite = GetSprites(GetCurrentDevice(context)).idle;
        }

        private void ProcessInput(InputAction.CallbackContext context) {
            Vector2 value = context.ReadValue<Vector2>();
            float scrollY = value.y;

            var sprites = GetSprites(GetCurrentDevice(context));

            if (_hintImage != null) {
                if (scrollY > 0) _hintImage.sprite = sprites.up;
                else if (scrollY < 0) _hintImage.sprite = sprites.down;
                else _hintImage.sprite = sprites.idle;
            }
        }
    }
}