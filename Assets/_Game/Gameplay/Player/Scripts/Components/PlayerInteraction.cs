// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using FifthSemester.Doors;
using FifthSemester.Player.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteraction : MonoBehaviour {
        [SerializeField, Range(1f, 5f)] private float _interactionRange = 3f;
        [SerializeField] private List<GameObject> _itemsNearby;
        [SerializeField] private List<Door> _doorsNearby = new List<Door>();

        [SerializeField] private SphereCollider _interactionCollider;
        private PlayerInteractionTrigger _interactionTrigger;
        private PlayerController _player;
        private PlayerEvents _playerEvents;


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
            _playerEvents = _player.PlayerEvents;

            _playerEvents.OnInteractInput += Interact;

            _interactionTrigger.OnObjectEntered += HandleTriggerEnter;
            _interactionTrigger.OnObjectExited += HandleTriggerExit;
        }

        private void OnDisable() {
            if (_playerEvents == null) return;

            _playerEvents.OnInteractInput -= Interact;

            _interactionTrigger.OnObjectEntered -= HandleTriggerEnter;
            _interactionTrigger.OnObjectExited -= HandleTriggerExit;
        }

        private void HandleTriggerEnter(Collider other) {
            if (other.TryGetComponent<Door>(out var door)) {
                if (!_doorsNearby.Contains(door)) {
                    _doorsNearby.Add(door);
                    door.EnableOutline(true);
                }
            }
        }

        private void HandleTriggerExit(Collider other) {
            if (other.TryGetComponent<Door>(out var door)) {
                _doorsNearby.Remove(door);
                door.EnableOutline(false);
            }
        }

        private void Interact() {
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
        }
    }
}
