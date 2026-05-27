using System;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    [Serializable]
    public class Map2PasswordState {
        [SerializeField] private string _targetCode = string.Empty;
        [SerializeField] private bool[] _revealedPositions = Array.Empty<bool>();

        public string TargetCode => _targetCode;

        public int Length => string.IsNullOrEmpty(_targetCode) ? 0 : _targetCode.Length;

        public bool IsComplete {
            get {
                if (_revealedPositions == null || _revealedPositions.Length == 0) {
                    return false;
                }

                for (int i = 0; i < _revealedPositions.Length; i++) {
                    if (!_revealedPositions[i]) {
                        return false;
                    }
                }

                return true;
            }
        }

        public void Initialize(string targetCode) {
            string safeTargetCode = targetCode ?? string.Empty;

            if (_targetCode != safeTargetCode || _revealedPositions == null || _revealedPositions.Length != safeTargetCode.Length) {
                _revealedPositions = new bool[safeTargetCode.Length];
            }

            _targetCode = safeTargetCode;
        }

        public bool CanRevealDigit(int digit) {
            if (_revealedPositions == null || _revealedPositions.Length != Length) {
                _revealedPositions = new bool[Length];
            }

            for (int i = 0; i < Length; i++) {
                char expectedDigit = _targetCode[i];
                if (_revealedPositions[i]) {
                    continue;
                }

                if (char.IsDigit(expectedDigit) && expectedDigit - '0' == digit) {
                    return true;
                }
            }

            return false;
        }

        public bool IsRevealed(int index) {
            if (index < 0 || _revealedPositions == null || index >= _revealedPositions.Length) {
                return false;
            }

            return _revealedPositions[index];
        }

        public bool TryReveal(int digit) {
            if (_revealedPositions == null || _revealedPositions.Length != Length) {
                _revealedPositions = new bool[Length];
            }

            for (int i = 0; i < Length; i++) {
                if (_revealedPositions[i]) {
                    continue;
                }

                char expectedDigit = _targetCode[i];
                if (!char.IsDigit(expectedDigit) || expectedDigit - '0' != digit) {
                    continue;
                }

                _revealedPositions[i] = true;
                return true;
            }

            return false;
        }

        public string GetDisplayCode() {
            if (string.IsNullOrEmpty(_targetCode)) {
                return string.Empty;
            }

            char[] display = _targetCode.ToCharArray();

            for (int i = 0; i < display.Length; i++) {
                if (!IsRevealed(i)) {
                    display[i] = 'X';
                }
            }

            return new string(display);
        }

        public static Map2PasswordState LoadOrCreate(string prefsKey, string targetCode) {
            Map2PasswordState state = Load(prefsKey);

            if (state == null) {
                state = new Map2PasswordState();
            }

            state.Initialize(targetCode);
            return state;
        }

        public static Map2PasswordState Load(string prefsKey) {
            if (!PlayerPrefs.HasKey(prefsKey)) {
                return null;
            }

            string json = PlayerPrefs.GetString(prefsKey);
            if (string.IsNullOrWhiteSpace(json)) {
                return null;
            }

            return JsonUtility.FromJson<Map2PasswordState>(json);
        }

        public void Save(string prefsKey) {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(prefsKey, json);
            PlayerPrefs.Save();
        }
    }
}
