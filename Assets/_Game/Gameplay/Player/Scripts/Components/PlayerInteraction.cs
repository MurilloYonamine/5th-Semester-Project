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
        [SerializeField] private LayerMask _interactionLayer;
        private IInteractable _currentInteractable;

        private PlayerController _player;
        private PlayerEvents _playerEvents;

        private IAudioService _audioService;
        [SerializeField] private AudioClip _pickupSound;

        private void Awake() {
            _player = GetComponent<PlayerController>();
        }

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();

            _playerEvents = _player.PlayerEvents;
            _playerEvents.OnInteractInput += Interact;
        }

        private void OnDisable() {
            if (_playerEvents != null)
                _playerEvents.OnInteractInput -= Interact;
        }

        private void Update() {
            var newInteractable = GetInteractableFromRay();

            if (_currentInteractable != newInteractable) {
                _currentInteractable?.Highlight(false);

                if (newInteractable != null && newInteractable.IsInteractable) {
                    newInteractable.Highlight(true);
                }

                _currentInteractable = newInteractable;
            }
        }

        private IInteractable GetInteractableFromRay() {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer)) {
                return hit.collider.GetComponent<IInteractable>();
            }

            return null;
        }

        private void Interact() {
            var interactable = GetInteractableFromRay();

            if (interactable == null || !interactable.IsInteractable)
                return;

            if (interactable is Item item) {
                var inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
                bool added = inventoryService?.AddItem(item) ?? false;

                if (added) {
                    PlayPickupFeedback();
                    item.Interact();
                }
                return;
            }

            interactable.Interact();
        }
        private void PlayPickupFeedback() {
            if (_pickupSound != null) {
                _audioService.PlaySFX(_pickupSound);
            }
        }
    }
}