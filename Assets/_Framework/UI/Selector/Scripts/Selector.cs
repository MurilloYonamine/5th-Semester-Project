// Autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Shared.AudioSystem;
using System.Collections;
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

        private Coroutine _navigationCoroutine;
        private bool _isMoving = false;

        [Header("Hold Settings")]
        [SerializeField] private float _initialDelay = 0.4f;
        [SerializeField] private float _repeatRate = 0.08f;

        protected virtual void Start() {
            OnLoad();   
            UpdateUI();
            UpdateVisualState();
            OnHighlight(false);
        }

        /// <summary>
        /// Called when an item is successfully selected and focus is lost.
        /// </summary>
        /// <param name="selectedItem">The item that was selected.</param>
        protected abstract void OnItemSelected(T selectedItem);
        /// <summary>
        /// Called to save the state of the selected item.
        /// </summary>
        protected abstract void OnSave();
        /// <summary>
        /// Called on start to load the initial selection state.
        /// </summary>
        protected abstract void OnLoad();
        /// <summary>
        /// Called to update the UI elements representing the current item.
        /// </summary>
        protected abstract void UpdateUI();
        /// <summary>
        /// Called when the selector gains or loses highlight (e.g., pointer enter/exit, selection).
        /// </summary>
        /// <param name="highlight">True if the selector is highlighted; otherwise, false.</param>
        protected virtual void OnHighlight(bool highlight) { }
        /// <summary>
        /// Called to update the visual presentation of the selector based on its focused state.
        /// </summary>
        protected virtual void UpdateVisualState() { }

        #region Selection Handlers
        public void OnSelect(BaseEventData eventData) => OnHighlight(true);
        public void OnDeselect(BaseEventData eventData) => OnHighlight(false);
        public void OnPointerEnter(PointerEventData eventData) => OnHighlight(true);
        public void OnPointerExit(PointerEventData eventData) => OnHighlight(false);
        #endregion

        #region Navigation Methods
        public void OnMove(AxisEventData eventData) {
            if (!_isFocused || _isMoving) return;

            int direction = 0;
            if (eventData.moveDir == MoveDirection.Right || eventData.moveDir == MoveDirection.Down) direction = 1;
            else if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Up) direction = -1;

            if (direction != 0) {
                _navigationCoroutine = StartCoroutine(HoldNavigationRoutine(direction));
                eventData.Use();
            }
        }
        private IEnumerator HoldNavigationRoutine(int direction) {
            _isMoving = true;

            if (direction > 0) MoveNext(); else MovePrevious();

            yield return new WaitForSecondsRealtime(_initialDelay);

            while (_isFocused && IsInputActive(direction)) {
                if (direction > 0) MoveNext(); else MovePrevious();
                yield return new WaitForSecondsRealtime(_repeatRate);
            }

            _isMoving = false;
            _navigationCoroutine = null;
        }
        private bool IsInputActive(int direction) {
            Vector2 moveVal = Vector2.zero;
            if (Gamepad.current != null) moveVal = Gamepad.current.leftStick.ReadValue() + Gamepad.current.dpad.ReadValue();
            else if (Keyboard.current != null) {
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) moveVal.x = 1;
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) moveVal.x = -1;
            }

            return direction > 0 ? moveVal.x > 0.5f : moveVal.x < -0.5f;
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
