using FifthSemester.Shared.AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FifthSemester.Framework.UI {
    public abstract class Selector<T> : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler {

        [Header("Selector Settings")]
        [SerializeField] protected int _currentIndex = 0;
        [SerializeField] protected T[] _items;

        [Header("Audio")]
        [SerializeField] private AudioClip _selectionSound;

        [Header("Text Reference")]
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private TextMeshProUGUI _optionText;

        [Header("Colors Reference")]
        [SerializeField] private Color _focusedColor = Color.yellow;
        [SerializeField] private Color _unfocusedColor = Color.gray;
        [SerializeField] private Color _hoverColor = Color.white;

        [SerializeField] private bool _isFocused = false;

        private void Start() {
            _labelText.color = _unfocusedColor;
            _optionText.color = _unfocusedColor;
        }
        protected abstract void OnItemSelected(T selectedItem);
        protected abstract void OnSave();

        #region Selection Handlers
        public void OnSelect(BaseEventData eventData) => Highlight();
        public void OnDeselect(BaseEventData eventData) => Unhighlight();
        public void OnPointerEnter(PointerEventData eventData) => Highlight();
        public void OnPointerExit(PointerEventData eventData) => Unhighlight();
        private void Highlight() {
            if (_labelText != null) {
                _optionText.color = _hoverColor;
                _labelText.color = _hoverColor;
            }
        }
        private void Unhighlight() {
            if (_labelText != null) {
                _optionText.color = _unfocusedColor;
                _labelText.color = _unfocusedColor;
            }
        }
        #endregion

        #region Navigation Methods
        public void OnMove(AxisEventData eventData) {
            if (!_isFocused) return;

            if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Up) {
                MovePrevious();
                eventData.Use();
            }
            else if (eventData.moveDir == MoveDirection.Right || eventData.moveDir == MoveDirection.Down) {
                MoveNext();
                eventData.Use();
            }
        }
        private void MoveNext() {
            if (!_isFocused) return;
            _currentIndex = (_currentIndex + 1) % _items.Length;

            if (_selectionSound != null)
                AudioManager.Instance.PlaySFX(_selectionSound);

            UpdateText();
        }
        private void MovePrevious() {
            if (!_isFocused) return;
            _currentIndex = (_currentIndex - 1 + _items.Length) % _items.Length;

            if (_selectionSound != null)
                AudioManager.Instance.PlaySFX(_selectionSound);

            UpdateText();
        }
        #endregion

        #region Click and Submit Handlers
        public void OnPointerClick(PointerEventData eventData) {
            ToggleFocus();
        }
        public void OnSubmit(BaseEventData eventData) {
            ToggleFocus();
            eventData.Use();
        }
        private void ToggleFocus() {
            if (_isFocused) {
                Unfocus();
            }
            else {
                Focus();
            }
        }
        private void Focus() {
            _isFocused = true;
            _optionText.color = _focusedColor;
        }

        private void Unfocus() {
            _isFocused = false;
            _optionText.color = _unfocusedColor;

            OnItemSelected(_items[_currentIndex]);
        }
        #endregion
        public void Reset() {
            _currentIndex = 0;
            UpdateText();
        }
        private void UpdateText() {
            if (_optionText != null) {
                _optionText.text = _items[_currentIndex].ToString();
            }
        }
    }
}
