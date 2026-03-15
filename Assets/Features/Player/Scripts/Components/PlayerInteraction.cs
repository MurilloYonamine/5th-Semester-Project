using System;
using System.Collections.Generic;
using UnityEngine;
using FifthSemester.Player.Components;
using FifthSemester.Items;
using FifthSemester.Inventory;
using FifthSemester.Shared.AudioSystem;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private List<GameObject> _itemsNearby;
        [SerializeField] private SphereCollider _interactionCollider;

        private PlayerInteractionTrigger _interactionTrigger;
        private PlayerController _player;
        private PlayerEvents _playerEvents;

        private InventoryController _inventory;
        [SerializeField] private InventoryUI _inventoryUI;

        [SerializeField] private AudioClip _pickupSound;

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _inventory = _player.Inventory;

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
        private void OnEnable() {
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
            if (other.TryGetComponent<IInteractable>(out var interactable)) {
                if (!_itemsNearby.Contains(other.gameObject)) {
                    _itemsNearby.Add(other.gameObject);
                }
            }
        }

        private void HandleTriggerExit(Collider other) {
            if (other.TryGetComponent<IInteractable>(out var interactable)) {
                _itemsNearby.Remove(other.gameObject);
            }
        }

        private void Interact() {
            if (_itemsNearby.Count == 0) return;

            bool itemAdded = false;

            foreach (var item in _itemsNearby) {
                if (item.TryGetComponent<IInteractable>(out var interactable)) {
                    _inventory.AddItem(interactable);
                    interactable.Interact();
                    itemAdded = true;
                }
            }
            AudioManager.Instance.PlaySFX(_pickupSound);

            if (itemAdded && !_inventory.IsInventoryOpen) {
                _inventory.ToggleInventory();
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
