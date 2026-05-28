using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Gameplay.Interactables;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Save;
using FifthSemester.Gameplay.Shared;
using FifthSemester.Gameplay.Map2;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField] private Camera _playerCamera;

        [Header("Settings")]
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private LayerMask _interactionLayer;

        [Header("Feedback")]
        [SerializeField] private AudioClip _failureSound;

        private IInteractable _currentInteractable;
        private PlayerController _playerController;
        private IEventBus _eventBus;
        private IAudioService _audioService;
        private IInventoryService<Item> _inventoryService;
        private IDeferredInteractionCompletion _pendingDeferredCompletion;
        private string _pendingInteractableId;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[16];

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
            _eventBus?.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<InteractInputEvent>(Interact);
            _eventBus?.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);

            if (_currentInteractable != null) {
                _currentInteractable.Highlight(false);
                _currentInteractable = null;
            }

            _pendingDeferredCompletion = null;
            _pendingInteractableId = null;
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
            int numHits = Physics.RaycastNonAlloc(ray, _raycastHits, _interactionRange, ~0, QueryTriggerInteraction.Collide);

            IInteractable closestInteractable = null;
            float closestInteractableDist = float.MaxValue;
            float closestBlockerDist = float.MaxValue;

            for (int i = 0; i < numHits; i++) {
                RaycastHit hit = _raycastHits[i];
                if (hit.collider == null) continue;

                // Check if this collider belongs to an object in the interaction layer
                bool isInteractableLayer = ((1 << hit.collider.gameObject.layer) & _interactionLayer) != 0;

                if (isInteractableLayer) {
                    MonoBehaviour[] components = hit.collider.GetComponents<MonoBehaviour>();
                    IInteractable selectedInteractable = null;

                    for (int j = 0; j < components.Length; j++) {
                        MonoBehaviour component = components[j];
                        if (component is not IInteractable interactable || !interactable.IsInteractable) continue;

                        if (component is Item) {
                            selectedInteractable = interactable;
                            break;
                        }

                        if (component is DeliveryPoint) {
                            selectedInteractable = interactable;
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

                    if (selectedInteractable != null) {
                        if (hit.distance < closestInteractableDist) {
                            closestInteractableDist = hit.distance;
                            closestInteractable = selectedInteractable;
                        }
                    }
                }

                // Any solid collider (not a trigger) can act as a physical blocker (like a wall, door frame, etc.)
                if (!hit.collider.isTrigger) {
                    if (hit.distance < closestBlockerDist) {
                        closestBlockerDist = hit.distance;
                    }
                }
            }

            // Clear the array references to prevent memory leaks/holding onto Unity objects
            for (int i = 0; i < numHits; i++) {
                _raycastHits[i] = default;
            }

            // If a solid physical blocker is closer than the closest interactable, then the interactable is blocked (behind a wall)
            if (closestBlockerDist < closestInteractableDist - 0.001f) {
                return null;
            }

            return closestInteractable;
        }

        private void Interact(InteractInputEvent evt) {
            if (_currentInteractable == null || !_currentInteractable.IsInteractable) {
                PlayFailureFeedback();
                return;
            }

            if (_currentInteractable is Item item) {
                HandleItemPickup(item);
            }
            else if (_currentInteractable is DeliveryPoint deliveryPoint) {
                deliveryPoint.Interact();
                HandleInteractionCompleted(_currentInteractable);
            }
            else {
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
            Camera cam = _playerCamera != null ? _playerCamera : Camera.main;
            if (cam == null) return;

            Gizmos.color = Color.yellow;
            Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Vector3 direction = cam.transform.forward;
            Gizmos.DrawLine(origin, origin + direction * _interactionRange);
        }
    }
}
