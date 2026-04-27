using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace FifthSemester.Framework.UI {
    public class OptionSelector : Selectable {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _value;

        [Header("Optional Buttons")]
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        [Header("Input References")]
        [SerializeField] private InputActionReference _nextInput;
        [SerializeField] private InputActionReference _previousInput;

        private List<string> _options = new();
        private int _currentIndex;

        public Action<int> OnValueChanged;

        protected override void OnEnable() {
            base.OnEnable();

            if (_nextInput != null)
                _nextInput.action.started += HandleNextInput;

            if (_previousInput != null)
                _previousInput.action.started += HandlePreviousInput;
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (_nextInput != null)
                _nextInput.action.started -= HandleNextInput;

            if (_previousInput != null)
                _previousInput.action.started -= HandlePreviousInput;
        }

        private bool IsSelected() {
            return EventSystem.current != null &&
                   EventSystem.current.currentSelectedGameObject == gameObject;
        }

        private void HandleNextInput(InputAction.CallbackContext context) {
            if (!IsSelected()) return;
            if (context.started) Next();
        }

        private void HandlePreviousInput(InputAction.CallbackContext context) {
            if (!IsSelected()) return;
            if (context.started) Previous();
        }

        public void Initialize(List<string> options, int startIndex = 0) {
            _options = options;
            _currentIndex = Mathf.Clamp(startIndex, 0, _options.Count - 1);

            Refresh();
            BindButtons();
        }

        private void BindButtons() {
            if (_leftButton != null)
                _leftButton.onClick.AddListener(Previous);

            if (_rightButton != null)
                _rightButton.onClick.AddListener(Next);
        }

        public void Next() {
            if (_options.Count == 0) return;

            _currentIndex = (_currentIndex + 1) % _options.Count;
            Change();
        }

        public void Previous() {
            if (_options.Count == 0) return;

            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = _options.Count - 1;

            Change();
        }

        private void Change() {
            Refresh();
            OnValueChanged?.Invoke(_currentIndex);
        }

        private void Refresh() {
            if (_value != null && _options.Count > 0)
                _value.text = _options[_currentIndex];
        }

        public int GetIndex() => _currentIndex;

        public void SetIndex(int index) {
            _currentIndex = Mathf.Clamp(index, 0, _options.Count - 1);
            Refresh();
        }
    }
}