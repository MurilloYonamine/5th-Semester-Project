using System;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(CanvasGroup))]
    public class Map2PasswordView : MonoBehaviour {
        [Header("Password Display")]
        [SerializeField] private TextMeshProUGUI _passwordText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Completion")]
        [SerializeField] private GameObject _completedRoot;
        [SerializeField] private string _completedMessage = "Senha liberada";

        [Header("Visibility")]
        [SerializeField] private bool _hideMessageWhenEmpty = true;

        private CanvasGroup _canvasGroup;
        private Color _defaultPasswordColor;
        private Color _defaultMessageColor;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_passwordText != null) {
                _defaultPasswordColor = _passwordText.color;
            }

            if (_messageText != null) {
                _defaultMessageColor = _messageText.color;
            }

            SetSolved(false);
            SetMessage(string.Empty);
        }

        public void SetPassword(string password) {
            if (_passwordText == null) {
                return;
            }

            _passwordText.text = password ?? string.Empty;
        }

        public void SetMessage(string message) {
            if (_messageText == null) {
                return;
            }

            string safeMessage = message ?? string.Empty;
            _messageText.text = safeMessage;
            _messageText.gameObject.SetActive(!_hideMessageWhenEmpty || !string.IsNullOrWhiteSpace(safeMessage));
        }

        public void SetSolved(bool solved) {
            if (_completedRoot != null) {
                _completedRoot.SetActive(solved);
            }

            if (_passwordText != null) {
                _passwordText.color = solved ? Color.green : _defaultPasswordColor;
            }

            if (_messageText != null) {
                _messageText.color = solved ? Color.green : _defaultMessageColor;
                if (solved && string.IsNullOrWhiteSpace(_messageText.text)) {
                    _messageText.text = _completedMessage;
                    _messageText.gameObject.SetActive(true);
                }
            }
        }

        public void Show() {
            if (_canvasGroup == null) {
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide() {
            if (_canvasGroup == null) {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
