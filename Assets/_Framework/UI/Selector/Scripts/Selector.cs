using FifthSemester.Shared.AudioSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.Framework.UI {
    public abstract class Selector<T> : MonoBehaviour,
        ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IMoveHandler, ISubmitHandler, ICancelHandler, IPointerClickHandler {

        [SerializeField] protected int _currentIndex = 0;
        [SerializeField] protected T[] _items;

        protected bool _isFocused = false;
        protected bool _isHovered = false;

        [SerializeField] private AudioClip _selectionSound;

        [SerializeField] protected Color _normalColor = Color.gray;
        [SerializeField] protected Color _highlightColor = Color.white;
        [SerializeField] protected Color _activeColor = Color.yellow;

        protected virtual void Start() {
            OnLoad();
            RefreshUI();
        }

        public void Next() {
            if (_items == null || _items.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % _items.Length;
            OnValueChanged();
        }

        public void Previous() {
            if (_items == null || _items.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + _items.Length) % _items.Length;
            OnValueChanged();
        }

        protected void OnValueChanged() {
            PlaySound();
            UpdateUI();
            OnItemSelected(_items[_currentIndex]);
            OnSave();
            ApplyVisuals();
        }

        protected void RefreshUI() {
            UpdateUI();
            ApplyVisuals();
        }

        protected abstract void OnItemSelected(T selectedItem);
        protected abstract void OnSave();
        protected abstract void OnLoad();
        protected abstract void UpdateUI();

        public void OnPointerClick(PointerEventData eventData) => HandleInteraction();
        public void OnSubmit(BaseEventData eventData) => HandleInteraction();

        private void HandleInteraction() {
            if (!_isFocused) {
                _isFocused = true;
                if (EventSystem.current.currentSelectedGameObject != gameObject) {
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
            else {
                Next();
            }
            ApplyVisuals();
        }

        public void OnCancel(BaseEventData eventData) {
            if (_isFocused) {
                _isFocused = false;
                ApplyVisuals();
            }
        }

        public void OnMove(AxisEventData eventData) {
            if (!_isFocused) return;
            if (eventData.moveDir == MoveDirection.Right) Next();
            else if (eventData.moveDir == MoveDirection.Left) Previous();
            eventData.Use();
        }

        public void OnSelect(BaseEventData eventData) { _isHovered = true; ApplyVisuals(); }
        public void OnDeselect(BaseEventData eventData) { _isHovered = false; _isFocused = false; ApplyVisuals(); }
        public void OnPointerEnter(PointerEventData eventData) { _isHovered = true; ApplyVisuals(); }
        public void OnPointerExit(PointerEventData eventData) { _isHovered = false; ApplyVisuals(); }

        protected virtual void ApplyVisuals() {
            Color targetColor = _normalColor;
            if (_isFocused) targetColor = _activeColor;
            else if (_isHovered) targetColor = _highlightColor;
            UpdateVisualElements(targetColor);
        }

        protected abstract void UpdateVisualElements(Color targetColor);

        private void PlaySound() {
            if (_selectionSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(_selectionSound);
        }
    }
}
