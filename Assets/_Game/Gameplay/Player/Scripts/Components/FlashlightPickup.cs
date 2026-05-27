// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Gameplay.Shared;
using ThirdParty.QuickOutline;

namespace FifthSemester.Player.Components {
    [RequireComponent(typeof(Outline))]
    public class FlashlightPickup : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject _playerFlashlightObject;
        [SerializeField] private PlayerFlashlight _playerFlashlight;

        private Outline _outline;
        private BoxCollider _collider;

        public bool IsInteractable => true;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _collider = GetComponent<BoxCollider>();
            _outline.enabled = false;
            _collider.enabled = true;

            if (_playerFlashlightObject == null) {
                _playerFlashlight = GameObject.FindWithTag("Player")?.GetComponentInChildren<PlayerFlashlight>();
            }
        }

        public void Interact() {
            if (_playerFlashlight != null) {
                _playerFlashlight.EnableFlashlight();
            }

            if (_playerFlashlightObject != null) {
                _playerFlashlightObject.SetActive(true);
            }
            Destroy(gameObject);
        }

        public void StopInteract() {
        }

        public void Highlight(bool value) {
            if (_outline != null) {
                _outline.enabled = value;
            }
        }
    }
}
