using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Gameplay.Inventory;

// using FifthSemester.Gameplay.Inventory; // Removido, agora usa Shared
using FifthSemester.Gameplay.Shared;
using FifthSemester.Player.Components;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [Header("Settings")]
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;

        [Header("Debug / Runtime")]
        [SerializeField] private List<IInteractable> _interactablesNearby = new();

        private SphereCollider _interactionCollider;
        private PlayerInteractionTrigger _interactionTrigger;
        private PlayerController _player;
        private PlayerEvents _playerEvents;
        
        private IAudioService _audioService;
        [SerializeField] private AudioClip _pickupSound;

        private void Awake() {
            _player = GetComponent<PlayerController>();

            _interactionCollider = GetComponentInChildren<SphereCollider>();
            _interactionCollider.radius = _interactionRange;
            _interactionCollider.isTrigger = true;

            _interactionTrigger = _interactionCollider.GetComponent<PlayerInteractionTrigger>();
            if (_interactionTrigger == null) {
                _interactionTrigger = _interactionCollider.gameObject.AddComponent<PlayerInteractionTrigger>();
            }
        }

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();

            _playerEvents = _player.PlayerEvents;
            _playerEvents.OnInteractInput += Interact;

            _interactionTrigger.OnObjectEntered += HandleTriggerEnter;
            _interactionTrigger.OnObjectExited += HandleTriggerExit;
        }

        private void OnDisable() {
            if (_playerEvents != null)
                _playerEvents.OnInteractInput -= Interact;

            if (_interactionTrigger != null) {
                _interactionTrigger.OnObjectEntered -= HandleTriggerEnter;
                _interactionTrigger.OnObjectExited -= HandleTriggerExit;
            }
        }

        private void HandleTriggerEnter(Collider other) {
            if (other.TryGetComponent<IInteractable>(out var interactable)) {
                if (!_interactablesNearby.Contains(interactable)) {
                    _interactablesNearby.Add(interactable);
                    interactable.Highlight(true);
                }
            }
        }

        private void HandleTriggerExit(Collider other) {
            if (other.TryGetComponent<IInteractable>(out var interactable)) {
                if (_interactablesNearby.Contains(interactable)) {
                    _interactablesNearby.Remove(interactable);
                    interactable.Highlight(false);
                }
            }
        }


        private void Interact() {
            var best = GetBestInteractable();

            if (best == null || !best.IsInteractable)
                return;

            best.Interact();
        }

        private IInteractable GetBestInteractable() {
            if (_interactablesNearby.Count == 0) return null;

            IInteractable best = null;
            float bestScore = -Mathf.Infinity;

            Vector3 playerPos = transform.position;
            Vector3 forward = transform.forward;

            foreach (var interactable in _interactablesNearby) {
                if (interactable == null || !interactable.IsInteractable)
                    continue;

                var mono = interactable as MonoBehaviour;
                if (mono == null) continue;

                Vector3 dir = (mono.transform.position - playerPos).normalized;

                float alignment = Vector3.Dot(forward, dir);
                float distance = Vector3.Distance(playerPos, mono.transform.position);

                float score = alignment - (distance * 0.2f);

                if (score > bestScore) {
                    bestScore = score;
                    best = interactable;
                }
            }

            return best;
        }

        private void PlayPickupFeedback() {
            if (_pickupSound != null) {
                _audioService.PlaySFX(_pickupSound);
            }
        }
    }
}