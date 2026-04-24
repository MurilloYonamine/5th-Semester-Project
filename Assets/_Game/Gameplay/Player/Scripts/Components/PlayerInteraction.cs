using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Shared;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField] private Camera _playerCamera;

        [Header("Settings")]
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private LayerMask _interactionLayer;

        [Header("Feedback")]
        [SerializeField] private AudioClip _pickupSound;

        private IInteractable _currentInteractable;
        private IEventBus _eventBus;
        private IAudioService _audioService;
        private IInventoryService<Item> _inventoryService;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus?.Subscribe<InteractInputEvent>(Interact);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<InteractInputEvent>(Interact);

            if (_currentInteractable != null) {
                _currentInteractable.Highlight(false);
                _currentInteractable = null;
            }
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
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer)) {
                return hit.collider.GetComponent<IInteractable>();
            }

            return null;
        }

        private void Interact(InteractInputEvent evt) {
            if (_currentInteractable == null || !_currentInteractable.IsInteractable) {
                return;
            }

            if (_currentInteractable is Item item) {
                HandleItemPickup(item);
            } else {
                _currentInteractable.Interact();
            }
        }

        private void HandleItemPickup(Item item) {
            if (_inventoryService == null) {
                Debug.LogError("IInventoryService não encontrado. Não é possível pegar o item.");
                return;
            }

            bool wasAdded = _inventoryService.AddItem(item);

            if (wasAdded) {
                PlayPickupFeedback();
                item.Interact();
            }
        }

        private void PlayPickupFeedback() {
            if (_pickupSound != null && _audioService != null) {
                _audioService.PlaySFX(_pickupSound);
            }
        }
    }
}