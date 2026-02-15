// Autor: Murillo Gomes Yonamine
// Data: 15/02/2026


using UnityEngine;
using FifthSemester.Player;
using FifthSemester.Player.Components;
using TMPro;

namespace FifthSemester.Player.Tuning {
    public class PlayerTuningMovement : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;

        [Header("Inputs (opcional)")]
        [SerializeField] private TMP_InputField _walkSpeedInput;
        [SerializeField] private TMP_InputField _maxVelocityChangeInput;
        [SerializeField] private TMP_InputField _sprintSpeedInput;
        [SerializeField] private TMP_InputField _sprintDurationInput;
        [SerializeField] private TMP_InputField _sprintCooldownSecondsInput;
        [SerializeField] private TMP_InputField _crouchHeightInput;
        [SerializeField] private TMP_InputField _speedReductionInput;

        private PlayerMovement _movement;

        private void Awake() {
            if (_playerController != null) {
                _movement = _playerController.PlayerMovement;
            }
        }

        private void OnEnable() {
            RefreshInputs();
        }

        public void RefreshInputs() {
            if (_movement == null) return;

            if (_walkSpeedInput != null) {
                _walkSpeedInput.text = _movement.WalkSpeed.ToString("0.##");
            }

            if (_maxVelocityChangeInput != null) {
                _maxVelocityChangeInput.text = _movement.MaxVelocityChange.ToString("0.##");
            }

            if (_sprintSpeedInput != null) {
                _sprintSpeedInput.text = _movement.SprintSpeed.ToString("0.##");
            }

            if (_sprintDurationInput != null) {
                _sprintDurationInput.text = _movement.SprintDuration.ToString("0.##");
            }

            if (_sprintCooldownSecondsInput != null) {
                _sprintCooldownSecondsInput.text = _movement.SprintCooldownSeconds.ToString("0.##");
            }

            if (_crouchHeightInput != null) {
                _crouchHeightInput.text = _movement.CrouchHeight.ToString("0.##");
            }

            if (_speedReductionInput != null) {
                _speedReductionInput.text = _movement.SpeedReduction.ToString("0.##");
            }
        }

        public void SetPlayerCanMove(bool value) {
            if (_movement != null) _movement.SetPlayerCanMove(value);
        }

        public void SetWalkSpeed(float value) {
            if (_movement != null) {
                _movement.SetWalkSpeed(value);
            }
        }

        public void SetMaxVelocityChange(float value) {
            if (_movement != null) {
                _movement.SetMaxVelocityChange(value);
            }
        }

        public void SetEnableSprint(bool value) {
            if (_movement != null) _movement.SetEnableSprint(value);
        }

        public void SetUnlimitedSprint(bool value) {
            if (_movement != null) _movement.SetUnlimitedSprint(value);
        }

        public void SetSprintSpeed(float value) {
            if (_movement != null) {
                _movement.SetSprintSpeed(value);
            }
        }

        public void SetSprintDuration(float value) {
            if (_movement != null) {
                _movement.SetSprintDuration(value);
            }
        }

        public void SetSprintCooldownSeconds(float value) {
            if (_movement != null) {
                _movement.SetSprintCooldownSeconds(value);
            }
        }

        public void SetEnableCrouch(bool value) {
            if (_movement != null) _movement.SetEnableCrouch(value);
        }

        public void SetHoldToCrouch(bool value) {
            if (_movement != null) _movement.SetHoldToCrouch(value);
        }

        public void SetCrouchHeight(float value) {
            if (_movement != null) {
                _movement.SetCrouchHeight(value);
            }
        }

        public void SetSpeedReduction(float value) {
            if (_movement != null) {
                _movement.SetSpeedReduction(value);
            }
        }

        public void SetWalkSpeedFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetWalkSpeed(value);
            }
        }

        public void SetMaxVelocityChangeFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetMaxVelocityChange(value);
            }
        }

        public void SetSprintSpeedFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetSprintSpeed(value);
            }
        }

        public void SetSprintDurationFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetSprintDuration(value);
            }
        }

        public void SetSprintCooldownSecondsFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetSprintCooldownSeconds(value);
            }
        }

        public void SetCrouchHeightFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetCrouchHeight(value);
            }
        }

        public void SetSpeedReductionFromText(string text) {
            if (_movement == null) return;
            if (float.TryParse(text, out float value)) {
                _movement.SetSpeedReduction(value);
            }
        }
    }
}
