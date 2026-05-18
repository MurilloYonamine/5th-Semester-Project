using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Gameplay.Shared;
using TMPro;

namespace FifthSemester.Doors {
    [RequireComponent(typeof(Outline))]
    public class Door : MonoBehaviour, IInteractable {
        [Header("Configurações Visuais")]
        [SerializeField] private Outline _outline;
        [SerializeField] private Transform _doorMesh;
        [SerializeField] private TextMeshPro _textLocal;

        [Header("Configurações de Movimento")]
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _speed = 5f;

        private Quaternion _closedRotation;
        private Quaternion _targetRotation;
        private bool _isLocked = false;
        private Color _unlockedColor;

        public bool IsInteractable { get; private set; } = true;

        public string Id => gameObject.name;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
            _textLocal.gameObject.SetActive(false);

            _closedRotation = _doorMesh.localRotation;
            _targetRotation = _closedRotation;
            _unlockedColor = new Color32(105, 255, 144, 255); // 69FF90
        }

        private void Update() {
            _doorMesh.localRotation = Quaternion.Lerp(_doorMesh.localRotation, _targetRotation, Time.deltaTime * _speed);
        }

        public void Interact() {
            if (_isLocked) return;

            _isOpen = !_isOpen;

            if (_isOpen) {
                _targetRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
            }
            else {
                _targetRotation = _closedRotation;
            }
        }

        public void StopInteract() { }

        public void Highlight(bool value) {
            if (_outline != null)
                _outline.enabled = value;

            if (_textLocal != null)
                _textLocal.gameObject.SetActive(value);
        }

        public void Lock() {
            _isLocked = true;

            if (_outline != null)
                _outline.OutlineColor = Color.red;

            if (_textLocal != null) {
                _textLocal.color = Color.red;
                _textLocal.text = "TRANCADA";
            }
        }

        public void Unlock() {
            _isLocked = false;

            if (_outline != null)
                _outline.OutlineColor = _unlockedColor;

            if (_textLocal != null) {
                _textLocal.color = _unlockedColor;
                _textLocal.text = "ABRIR";
            }
        }
    }
}
