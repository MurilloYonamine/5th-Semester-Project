// Autor: Murillo Gomes Yonamine
// data: 15/03/2026

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
        [SerializeField] protected bool _isFocused = false;

        [Header("Audio")]
        [SerializeField] private AudioClip _selectionSound;

        [Header("Colors Reference")]
        [SerializeField] protected Color _focusedColor = Color.yellow;
        [SerializeField] protected Color _unfocusedColor = Color.gray;
        [SerializeField] protected Color _hoverColor = Color.white;

        protected virtual void Start() {
            OnLoad();   
            UpdateUI();
            UpdateVisualState();
            OnHighlight(false);
        }

        protected abstract void OnItemSelected(T selectedItem);
        protected abstract void OnSave();
        protected abstract void OnLoad();
        protected abstract void UpdateUI();
        protected virtual void OnHighlight(bool highlight) { }
        protected virtual void UpdateVisualState() { }

        #region Selection Handlers
        public void OnSelect(BaseEventData eventData) => OnHighlight(true);
        public void OnDeselect(BaseEventData eventData) => OnHighlight(false);
        public void OnPointerEnter(PointerEventData eventData) => OnHighlight(true);
        public void OnPointerExit(PointerEventData eventData) => OnHighlight(false);
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

            UpdateUI();
        }
        private void MovePrevious() {
            if (!_isFocused) return;
            _currentIndex = (_currentIndex - 1 + _items.Length) % _items.Length;

            if (_selectionSound != null)
                AudioManager.Instance.PlaySFX(_selectionSound);

            UpdateUI();
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
            UpdateVisualState();
        }

        private void Unfocus() {
            _isFocused = false;
            UpdateVisualState(); 

            OnItemSelected(_items[_currentIndex]);
            OnSave();
        }
        #endregion
    }
}
