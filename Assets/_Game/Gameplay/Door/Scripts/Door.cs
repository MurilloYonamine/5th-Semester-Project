using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Gameplay.Shared;

namespace FifthSemester.Doors {
    public class Door : MonoBehaviour, IInteractable {
        [Header("Configurações Visuais")]
        [SerializeField] private Outline _outline;
        [SerializeField] private Transform _doorMesh;
        [SerializeField] private GameObject _textLocal;

        [Header("Configurações de Movimento")]
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _speed = 5f;

        private Quaternion _closedRotation;
        private Quaternion _targetRotation;

        public bool IsInteractable { get; private set; } = true;

        public string Id => gameObject.name;    

        private void Awake() {
            if (_outline != null) _outline.enabled = false;

            _closedRotation = _doorMesh.localRotation;
            _targetRotation = _closedRotation;
        }

        private void Update() {
            _doorMesh.localRotation = Quaternion.Lerp(_doorMesh.localRotation, _targetRotation, Time.deltaTime * _speed);
        }

        public void Interact() {
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
                _textLocal.SetActive(value);
        }
    }
}
