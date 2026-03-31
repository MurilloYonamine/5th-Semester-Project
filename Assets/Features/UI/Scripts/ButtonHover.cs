// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    [RequireComponent(typeof(Button))]
    public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _highlightColor = Color.white;
        [SerializeField] private Color _activeColor = Color.yellow;

        [SerializeField] private TextMeshProUGUI _buttonText;

        private Button _button;
        private void Awake() {
            _button = GetComponent<Button>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();

            if (_buttonText != null) {
                _buttonText.color = _normalColor;
            }
        }
        private void OnEnable() {
            if (_buttonText != null) {
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject) {
                    _buttonText.color = _activeColor;
                }
                else {
                    _buttonText.color = _normalColor;
                }
            }
        }
        private void OnDisable() {
            if (_buttonText != null) {
                _buttonText.color = _normalColor;
            }
        }
        public void OnPointerEnter(PointerEventData eventData) {
            if (_button != null && _buttonText != null) {
                _buttonText.color = _normalColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (_button != null && _buttonText != null) {
                _buttonText.color = _normalColor;
            }
        }

        public void OnSelect(BaseEventData eventData) {
            if (_button != null && _buttonText != null) {
                _buttonText.color = _activeColor;
            }
        }

        public void OnDeselect(BaseEventData eventData) {
            if (_button != null && _buttonText != null) {
                _buttonText.color = _normalColor;
            }
        }
    }
}
