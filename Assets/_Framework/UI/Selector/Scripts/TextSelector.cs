using TMPro;
using UnityEngine;

namespace FifthSemester.Framework.UI {
    public abstract class TextSelector<T> : Selector<T> {
        [SerializeField] protected TextMeshProUGUI _labelText;
        [SerializeField] protected TextMeshProUGUI _optionText;

        protected override void UpdateUI() {
            if (_optionText != null && _items != null && _items.Length > 0) {
                string valueText = _items[_currentIndex].ToString().ToUpper();
                _optionText.text = valueText;
            }
        }
        protected override void UpdateVisualElements(Color targetColor) {
            if (_labelText != null) _labelText.color = targetColor;
            if (_optionText != null) _optionText.color = targetColor;
        }
    }
}
