using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Gameplay.Interactables;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Save;
using FifthSemester.Gameplay.Shared;
<<<<<<< HEAD
using FifthSemester.Gameplay.Map2;
=======
>>>>>>> origin/main
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
<<<<<<< HEAD
=======
        private const string TAG = "<color=cyan>[PlayerInteraction]</color>";
>>>>>>> origin/main
        [SerializeField] private Camera _playerCamera;

        [Header("Settings")]
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private LayerMask _interactionLayer;

        [Header("Feedback")]
<<<<<<< HEAD
        [SerializeField] private AudioClip _failureSound;
=======
        [SerializeField] private AudioClip _pickupSound;
>>>>>>> origin/main

        private IInteractable _currentInteractable;
        private PlayerController _playerController;
        private IEventBus _eventBus;
        private IAudioService _audioService;
        private IInventoryService<Item> _inventoryService;
<<<<<<< HEAD
        private IDeferredInteractionCompletion _pendingDeferredCompletion;
        private string _pendingInteractableId;
        private DocumentTrigger _activeDocument;

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
=======

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
>>>>>>> origin/main
        }

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus?.Subscribe<InteractInputEvent>(Interact);
<<<<<<< HEAD
            _eventBus?.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
=======
>>>>>>> origin/main
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<InteractInputEvent>(Interact);
<<<<<<< HEAD
            _eventBus?.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
=======
>>>>>>> origin/main

            if (_currentInteractable != null) {
                _currentInteractable.Highlight(false);
                _currentInteractable = null;
            }
<<<<<<< HEAD

            _pendingDeferredCompletion = null;
            _pendingInteractableId = null;
            _activeDocument = null;
=======
>>>>>>> origin/main
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

<<<<<<< HEAD
            // 1. Find the interactable using the standard interaction layer (exactly as before)
=======
>>>>>>> origin/main
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactionLayer)) {
                return null;
            }

<<<<<<< HEAD
            // 2. Perform a targeted Line-of-Sight raycast to check for solid physical obstacles between player camera and impact point
            float checkDistance = hit.distance - 0.05f; // Subtract 5cm to avoid hitting the interactable's own collider
            if (checkDistance > 0f) {
                Vector3 direction = hit.point - ray.origin;
                int blockerMask = ~(_interactionLayer | (1 << _playerController.gameObject.layer));

                if (Physics.Raycast(ray.origin, direction, out RaycastHit obstacleHit, checkDistance, blockerMask, QueryTriggerInteraction.Ignore)) {
                    // Direct line of sight is blocked by a physical obstacle (e.g. a wall)
                    return null;
                }
            }

            // 3. Resolve which interactive component to return (preserving original logic)
=======
>>>>>>> origin/main
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

<<<<<<< HEAD
=======
                if (component is SavePoint) {
                    if (selectedInteractable == null) {
                        selectedInteractable = interactable;
                    }
                    continue;
                }

>>>>>>> origin/main
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
<<<<<<< HEAD
            if (_activeDocument != null) {
                DocumentTrigger documentToClose = _activeDocument;
                _activeDocument = null;
                documentToClose.Interact();

                if (_currentInteractable != documentToClose) {
                    documentToClose.Highlight(false);
                }
                return;
            }

            if (_currentInteractable == null || !_currentInteractable.IsInteractable) {
                PlayFailureFeedback();
=======
            if (_currentInteractable == null || !_currentInteractable.IsInteractable) {
>>>>>>> origin/main
                return;
            }

            if (_currentInteractable is Item item) {
                HandleItemPickup(item);
<<<<<<< HEAD
            }
            else if (_currentInteractable is DeliveryPoint deliveryPoint) {
                deliveryPoint.Interact();
                HandleInteractionCompleted(_currentInteractable);
            }
            else {
                if (_currentInteractable is DocumentTrigger documentTrigger) {
                    _activeDocument = documentTrigger;
                }
                _currentInteractable.Interact();
                HandleInteractionCompleted(_currentInteractable);
            }
        }

        private void HandleInteractionCompleted(IInteractable interactable) {
            if (interactable == null) {
                return;
            }

            if (interactable is IDeferredInteractionCompletion deferredCompletion && !deferredCompletion.PublishInteractionOnInput) {
                _pendingDeferredCompletion = deferredCompletion;
                _pendingInteractableId = interactable.Id;
                return;
            }

            PublishSuccessfulInteraction(interactable.Id);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            _activeDocument = null;

            if (_pendingDeferredCompletion == null || string.IsNullOrWhiteSpace(_pendingInteractableId)) {
                return;
            }

            if (!_pendingDeferredCompletion.TryCompleteDeferredInteraction(evt.NpcId)) {
                return;
            }

            string interactableId = _pendingInteractableId;
            _pendingDeferredCompletion = null;
            _pendingInteractableId = null;
            PublishSuccessfulInteraction(interactableId);
        }

        private void PublishSuccessfulInteraction(string interactableId) {
            if (string.IsNullOrWhiteSpace(interactableId)) {
                return;
            }

            _eventBus?.Publish(new ObjectSuccessfullyInteractedEvent { ObjectId = interactableId });
        }

        private void HandleItemPickup(Item item) {
            if (_inventoryService == null) {
                Debug.LogError("IInventoryService não encontrado. Não é possível pegar o item.");
                PlayFailureFeedback();
                return;
            }

            if (item is Map2KeyItem keyItem) {
                if (ServiceLocator.TryGet<IMap2KeyService>(out var keyService)) {
                    keyService.TryPrepareForLastKey(keyItem);
                }
            }

            bool wasAdded = _inventoryService.AddItem(item);

            if (wasAdded) {
                _eventBus?.Publish(new ItemPickedUpEvent(item.Id, item.gameObject));
                item.Interact();
            }
            else {
                PlayFailureFeedback();
            }
        }

        private void PlayFailureFeedback() {
            if (_failureSound == null || _audioService == null) {
                return;
            }

            _audioService.PlaySFX(_failureSound);
        }
        private void OnDrawGizmos() {
=======
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
>>>>>>> origin/main
            Camera cam = _playerCamera != null ? _playerCamera : Camera.main;
            if (cam == null) return;

            Gizmos.color = Color.yellow;
            Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Vector3 direction = cam.transform.forward;
            Gizmos.DrawLine(origin, origin + direction * _interactionRange);
        }
    }
}
