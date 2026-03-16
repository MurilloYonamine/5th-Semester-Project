// Autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Shared.AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FifthSemester.Framework.UI {
    public abstract class TextSelector<T> : Selector<T>{

        [Header("Text References")]
        [SerializeField] protected TextMeshProUGUI _labelText;
        [SerializeField] protected TextMeshProUGUI _optionText;
        protected override void UpdateUI() {
            if (_optionText != null)
                _optionText.text = _items[_currentIndex].ToString();
        }
        protected override void OnHighlight(bool highlight) {
            if (_isFocused) return;

            Color targetColor = highlight ? _hoverColor : _unfocusedColor;

            if (_labelText != null) _labelText.color = targetColor;
            if (_optionText != null) _optionText.color = targetColor;
        }
        protected override void UpdateVisualState() {
            Color targetColor = _isFocused ? _focusedColor : _unfocusedColor;

            if (_optionText != null) {
                _optionText.color = targetColor;
            }
        }
    }
}
