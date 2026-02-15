// Autor: Murillo Gomes Yonamine
// Data: 15/02/2026

using UnityEngine;
using FifthSemester.Player.Components;
using TMPro;

namespace FifthSemester.Player.Tuning {
    public class PlayerTuningJump : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;

        [Header("Inputs (opcional)")]
        [SerializeField] private TMP_InputField _jumpUpVelocityInput;
        [SerializeField] private TMP_InputField _hangTimeInput;
        [SerializeField] private TMP_InputField _fallGravityMultiplierInput;

        private PlayerJump _jump;

        private void Awake() {
            if (_playerController != null) {
                _jump = _playerController.PlayerJumpComponent;
            }
        }

        private void OnEnable() {
            RefreshInputs();
        }

        public void RefreshInputs() {
            if (_jump == null) return;

            if (_jumpUpVelocityInput != null) {
                _jumpUpVelocityInput.text = _jump.JumpUpVelocity.ToString("0.##");
            }

            if (_hangTimeInput != null) {
                _hangTimeInput.text = _jump.HangTime.ToString("0.##");
            }

            if (_fallGravityMultiplierInput != null) {
                _fallGravityMultiplierInput.text = _jump.FallGravityMultiplier.ToString("0.##");
            }
        }

        public void SetEnableJump(bool value) {
            if (_jump != null) _jump.SetEnableJump(value);
        }

        public void SetJumpUpVelocity(float value) {
            if (_jump != null) {
                _jump.SetJumpUpVelocity(value);
            }
        }

        public void SetHangTime(float value) {
            if (_jump != null) {
                _jump.SetHangTime(value);
            }
        }

        public void SetFallGravityMultiplier(float value) {
            if (_jump != null) {
                _jump.SetFallGravityMultiplier(value);
            }
        }
        public void SetJumpUpVelocityFromText(string text) {
            if (_jump == null) return;
            if (float.TryParse(text, out float value)) {
                _jump.SetJumpUpVelocity(value);
            }
        }

        public void SetHangTimeFromText(string text) {
            if (_jump == null) return;
            if (float.TryParse(text, out float value)) {
                _jump.SetHangTime(value);
            }
        }

        public void SetFallGravityMultiplierFromText(string text) {
            if (_jump == null) return;
            if (float.TryParse(text, out float value)) {
                _jump.SetFallGravityMultiplier(value);
            }
        }
    }
}
