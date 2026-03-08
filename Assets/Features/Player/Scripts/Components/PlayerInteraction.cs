using System;
using System.Collections.Generic;
using FifthSemester.Items;
using FifthSemester.Player.Components;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private List<GameObject> _itemsNearby;
        [SerializeField] private SphereCollider _interactionCollider;
        private PlayerController _player;
        private PlayerEvents _playerEvents;

        private void Awake() {
            _player = GetComponent<PlayerController>();
            _playerEvents = _player.InputEvents;
            _interactionCollider = _player.gameObject.GetComponentInChildren<SphereCollider>();
            _interactionCollider.radius = _interactionRange;
        }
        private void OnEnable() {
            _playerEvents.OnInteractInput += Interact;
        }
        private void OnDisable() {
            _playerEvents.OnInteractInput -= Interact;
        }
        private void Interact() {
            Debug.Log("3 Interacting...");
            if (_itemsNearby.Count > 0) {
                foreach (var item in _itemsNearby) {
                    if (item.TryGetComponent(out IInteractable interactable)) {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
