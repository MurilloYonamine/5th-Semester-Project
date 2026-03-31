// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using FifthSemester.Core.Managers;
using FifthSemester.Delivery;
using FifthSemester.DialogueSystem;
using FifthSemester.Doors;
using FifthSemester.Inventory;
using FifthSemester.Items;
using FifthSemester.Player.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private List<GameObject> _itemsNearby;
        [SerializeField] private List<DeliveryPoint> _deliveryPointsNearby;
        [SerializeField] private List<DialogueTrigger> _dialogueTriggersNearby = new List<DialogueTrigger>();
        [SerializeField] private List<Door> _doorsNearby = new List<Door>();

        [SerializeField] private SphereCollider _interactionCollider;
        private PlayerInteractionTrigger _interactionTrigger;
        private PlayerController _player;
        private PlayerEvents _playerEvents;

        private InventoryController _inventory;
        [SerializeField] private InventoryUI _inventoryUI;

        [SerializeField] private AudioClip _pickupSound;

        public event Action<IInteractable, DeliveryPoint> OnItemDelivered;
        public static event Action<IInteractable> OnItemPickedUp;

        private void Awake() {
            _deliveryPointsNearby = new List<DeliveryPoint>();
            _player = GetComponent<PlayerController>();
            _inventory = GetComponent<InventoryController>();

            _interactionCollider = GetComponentInChildren<SphereCollider>();
            _interactionCollider.radius = _interactionRange;
            _interactionCollider.isTrigger = true;

            _interactionTrigger = _interactionCollider.GetComponent<PlayerInteractionTrigger>();
            if (_interactionTrigger == null) {
                _interactionTrigger = _interactionCollider.gameObject.AddComponent<PlayerInteractionTrigger>();
            }

            if (_inventoryUI != null) {
                _inventoryUI.SetInventory(_inventory);
            }
        }
        public void DeliverItemToCurrentPoint(IInteractable item, DeliveryPoint deliveryPoint) {
            if (item == null || deliveryPoint == null) return;
            deliveryPoint.ReceiveItem(item);
        }

        private void HandleItemDelivered(IInteractable item, DeliveryPoint deliveryPoint) {
            _inventory.RemoveItem(item);
            OnItemDelivered?.Invoke(item, deliveryPoint);
        }
        private void Start() {
            _playerEvents = _player.PlayerEvents;

            _playerEvents.OnInteractInput += Interact;
            _playerEvents.OnNext += HandleNext;
            _playerEvents.OnPrevious += HandlePrevious;

            _interactionTrigger.OnObjectEntered += HandleTriggerEnter;
            _interactionTrigger.OnObjectExited += HandleTriggerExit;
        }

        private void OnDisable() {
            if (_playerEvents == null) return;

            _playerEvents.OnInteractInput -= Interact;
            _playerEvents.OnNext -= HandleNext;
            _playerEvents.OnPrevious -= HandlePrevious;

            _interactionTrigger.OnObjectEntered -= HandleTriggerEnter;
            _interactionTrigger.OnObjectExited -= HandleTriggerExit;
        }

        private void HandleTriggerEnter(Collider other) {
            if (other.TryGetComponent<Item>(out var interactable)) {
                if (!_itemsNearby.Contains(other.gameObject)) {
                    _itemsNearby.Add(other.gameObject);
                    interactable.Highlight();
                }
            }
            if (other.TryGetComponent<DeliveryPoint>(out var deliveryPoint)) {
                if (!_deliveryPointsNearby.Contains(deliveryPoint)) {
                    _deliveryPointsNearby.Add(deliveryPoint);
                    deliveryPoint.EnableOutline(true);
                }
            }
            if (other.TryGetComponent<DialogueTrigger>(out var dialogueTrigger)) {
                if (!_dialogueTriggersNearby.Contains(dialogueTrigger)) {
                    _dialogueTriggersNearby.Add(dialogueTrigger);
                    dialogueTrigger.TurnOutline(true);
                }
            }
            if (other.TryGetComponent<Door>(out var door)) {
                if (!_doorsNearby.Contains(door)) {
                    _doorsNearby.Add(door);
                    door.EnableOutline(true);
                }
            }
        }

        private void HandleTriggerExit(Collider other) {
            if (other.TryGetComponent<Item>(out var interactable)) {
                _itemsNearby.Remove(other.gameObject);
                interactable.Unhighlight();
            }
            if (other.TryGetComponent<DeliveryPoint>(out var deliveryPoint)) {
                _deliveryPointsNearby.Remove(deliveryPoint);
                deliveryPoint.EnableOutline(false);
            }
            if (other.TryGetComponent<DialogueTrigger>(out var dialogueTrigger)) {
                _dialogueTriggersNearby.Remove(dialogueTrigger);
                dialogueTrigger.TurnOutline(false);
            }
            if (other.TryGetComponent<Door>(out var door)) {
                _doorsNearby.Remove(door);
                door.EnableOutline(false);
            }
        }
        public void TryDeliverToNearestPoint() {
            if (_deliveryPointsNearby.Count == 0 || _inventory == null) return;
            var deliveryPoint = _deliveryPointsNearby[0];
            if (_inventory.IsInventoryOpen) return;
            deliveryPoint.TryDeliverFromInventory(_inventory);
        }

        private void Interact() {
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPanelActive()) {
                DialogueManager.Instance.DisplayNextLine();
                return;
            }

            if (_dialogueTriggersNearby != null && _dialogueTriggersNearby.Count > 0) {
                _dialogueTriggersNearby[0].TriggerDialogue();
                return;
            }

            if (_deliveryPointsNearby != null && _deliveryPointsNearby.Count > 0) {
                TryDeliverToNearestPoint();
                return;
            }

            if (_doorsNearby.Count > 0) {
                Door closestDoor = null;
                float minDistance = Mathf.Infinity;
                Vector3 playerPoss = transform.position;

                for (int i = 0; i < _doorsNearby.Count; i++) {
                    Door currentDoor = _doorsNearby[i];

                    if (currentDoor == null) continue;

                    float distance = Vector3.Distance(playerPoss, currentDoor.transform.position);

                    if (distance < minDistance) {
                        minDistance = distance;
                        closestDoor = currentDoor;
                    }
                }

                if (closestDoor != null) {
                    Debug.Log($"[Interaction] Alvo selecionado: {closestDoor.gameObject.name}");
                    closestDoor.Interact();
                }
                return;
            }
            GameObject bestItem = null;
            float bestAlignment = -1f;

            Vector3 playerForward = transform.forward;
            Vector3 playerPos = transform.position;

            foreach (var item in _itemsNearby) {
                if (item == null) continue;

                Vector3 directionToItem = (item.transform.position - playerPos).normalized;

                float alignment = Vector3.Dot(playerForward, directionToItem);

                if (alignment > bestAlignment) {
                    bestAlignment = alignment;
                    bestItem = item;
                }
            }

            if (bestItem != null && bestItem.TryGetComponent<IInteractable>(out var interactable)) {
                _inventory.AddItem(interactable);
                interactable.Interact();

                OnItemPickedUp?.Invoke(interactable); 

                _itemsNearby.Remove(bestItem);

                AudioManager.Instance.PlaySFX(_pickupSound);

                if (!_inventory.IsInventoryOpen) {
                    _inventory.ToggleInventory();
                }
            }
        }

        private void HandleNext() {
            if (_inventory.IsInventoryOpen) {
                _inventoryUI?.NavigateNext();
            }
            else {
                _inventory.ToggleInventory();
            }
        }

        private void HandlePrevious() {
            if (_inventory.IsInventoryOpen) {
                _inventoryUI?.NavigatePrevious();
            }
            else {
                _inventory.ToggleInventory();
            }
        }
    }
}
