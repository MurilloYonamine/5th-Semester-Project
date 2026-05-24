// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using FifthSemester.Gameplay.Shared;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Collider))]
    public abstract class TextTriggerBase : MonoBehaviour, IInteractable {
        [Header("Interaction")]
        [SerializeField] private GameObject _interactionHint;
        [SerializeField] private string _playerTag = "Player";

         private Outline _outline;
        [field: SerializeField] public string Id { get; private set; }

        private bool _isPlayerInside;

        public abstract bool IsInteractable { get; }

        protected virtual void Awake() {
            if (_outline == null && !TryGetComponent(out _outline)) {
                _outline = null;
            }

            if (_outline != null) {
                _outline.enabled = false;
            }

            SetHintVisible(false);
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!IsPlayer(other)) {
                return;
            }

            _isPlayerInside = true;
            SetHintVisible(true);
            OnPlayerEnteredTrigger();
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (!IsPlayer(other)) {
                return;
            }

            _isPlayerInside = false;
            SetHintVisible(false);
            OnPlayerExitedTrigger();
        }

        public virtual void Interact() {
            if (!CanInteract()) {
                return;
            }

            OnInteract();
        }

        public virtual void StopInteract() {
        }

        public virtual void Highlight(bool value) {
            if (_outline != null) {
                _outline.enabled = value && IsInteractable;
            }
        }

        protected bool IsPlayerInsideTrigger => _isPlayerInside;

        protected virtual bool CanInteract() {
            return IsInteractable;
        }

        protected virtual void OnPlayerEnteredTrigger() {
        }

        protected virtual void OnPlayerExitedTrigger() {
        }

        protected abstract void OnInteract();

        private void SetHintVisible(bool visible) {
            if (_interactionHint != null) {
                _interactionHint.SetActive(visible && IsInteractable);
            }
        }

        private bool IsPlayer(Collider other) {
            return other != null && other.CompareTag(_playerTag);
        }
    }
}