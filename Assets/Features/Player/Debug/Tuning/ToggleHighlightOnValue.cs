// Autor: Murillo Gomes Yonamine
// Data: 15/02/2026

using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Player.Tuning {
    [RequireComponent(typeof(Toggle))]
    public class ToggleHighlightOnValue : MonoBehaviour {
        [Header("Colors")]
        [SerializeField] private Image _targetImage;
        [SerializeField] private Color _offColor = Color.white;
        [SerializeField] private Color _onColor = Color.yellow;

        private Toggle _toggle;

        private void Awake() {
            _toggle = GetComponent<Toggle>();

            if (_toggle != null) {
                _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }

            if (_targetImage == null && _toggle != null) {
                _targetImage = _toggle.GetComponentInChildren<Image>();
            }

            UpdateColor(_toggle != null && _toggle.isOn);
        }

        private void OnDestroy() {
            if (_toggle != null) {
                _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        public void OnToggleValueChanged(bool isOn) {
            UpdateColor(isOn);
        }

        private void UpdateColor(bool isOn) {
            if (_targetImage == null) return;
            _targetImage.color = isOn ? _onColor : _offColor;
        }
    }
}
