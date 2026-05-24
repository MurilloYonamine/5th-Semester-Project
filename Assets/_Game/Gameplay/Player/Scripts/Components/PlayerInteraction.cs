using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Gameplay.Interactables;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Save;
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
        private PlayerController _playerController;
        private IEventBus _eventBus;
        private IAudioService _audioService;
        private IInventoryService<Item> _inventoryService;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();

            if (_playerCamera == null) {
                Debug.LogError("[PlayerInteraction] PlayerCamera não atribuído.");
                enabled = false;
                return;
            }

            if (_playerController == null) {
                Debug.LogError("[PlayerInteraction] PlayerController ausente no mesmo GameObject.");
                enabled = false;
            }
        }

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

            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer)) {
                return null;
            }

            MonoBehaviour[] components = hit.collider.GetComponents<MonoBehaviour>();
            IInteractable selectedInteractable = null;

            for (int i = 0; i < components.Length; i++) {
                MonoBehaviour component = components[i];
                if (component is not IInteractable interactable || !interactable.IsInteractable) continue;

                if (component is Item) {
                    return interactable;
                }

                if (component is DeliveryPoint) {
                    selectedInteractable = interactable;
                    continue;
                }

                if (component is SavePoint) {
                    if (selectedInteractable == null) {
                        selectedInteractable = interactable;
                    }
                    continue;
                }

                if (component is DialogueTrigger && selectedInteractable == null) {
                    selectedInteractable = interactable;
                    continue;
                }

                if (selectedInteractable == null) {
                    selectedInteractable = interactable;
                }
            }

            return selectedInteractable;
        }

        private void Interact(InteractInputEvent evt) {
            if (_currentInteractable == null || !_currentInteractable.IsInteractable) {
                return;
            }

            if (_currentInteractable is Item item) {
                HandleItemPickup(item);
            } else if (_currentInteractable is DeliveryPoint deliveryPoint) {
                deliveryPoint.Interact();
            } else if (_currentInteractable is SavePoint savePoint) {
                savePoint.SetPlayerController(_playerController);
                savePoint.Interact();
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
                _eventBus?.Publish(new ItemPickedUpEvent(item.Id, item.gameObject));
                item.Interact();
            }
        }

        private void PlayPickupFeedback() {
            if (_pickupSound != null && _audioService != null) {
                _audioService.PlaySFX(_pickupSound);
            }
        }
        private void OnDrawGizmos()
        {
            Camera cam = _playerCamera != null ? _playerCamera : Camera.main;
            if (cam == null) return;

            Gizmos.color = Color.yellow;
            Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Vector3 direction = cam.transform.forward;
            Gizmos.DrawLine(origin, origin + direction * _interactionRange);
        }
    }
}
