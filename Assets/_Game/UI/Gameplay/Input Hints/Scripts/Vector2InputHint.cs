// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using System;
using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FifthSemester.Gameplay.UI.InputHints {
    public class Vector2InputHint : InputHintBase {
        [Serializable]
        public struct Vector2Sprites {
            public Sprite idle;
            public Sprite up;
            public Sprite down;
            public Sprite left;
            public Sprite right;
        }

        [Header("Variantes por Dispositivo (Movimentação)")]
        [SerializeField] private Vector2Sprites _keyboard;
        [SerializeField] private Vector2Sprites _xbox;
        [SerializeField] private Vector2Sprites _playstation;

        private Vector2Sprites GetSprites(DeviceDisplayType device) => device switch {
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
            var sprites = GetSprites(GetCurrentDevice(context));

            if (_hintImage == null) return;

            if (value == Vector2.zero) {
                _hintImage.sprite = sprites.idle;
                return;
            }

            if (Mathf.Abs(value.x) > Mathf.Abs(value.y)) {
                _hintImage.sprite = value.x > 0 ? sprites.right : sprites.left;
            }
            else {
                _hintImage.sprite = value.y > 0 ? sprites.up : sprites.down;
            }
        }
    }
}