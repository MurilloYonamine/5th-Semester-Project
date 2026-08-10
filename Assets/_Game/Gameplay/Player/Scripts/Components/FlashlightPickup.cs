<<<<<<< HEAD
// Autor: Murillo Gomes Yonamine
=======
﻿// Autor: Murillo Gomes Yonamine
>>>>>>> origin/main
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Gameplay.Shared;
<<<<<<< HEAD
using FifthSemester.Core.Services;
using ThirdParty.QuickOutline;

namespace FifthSemester.Player.Components {
    [RequireComponent(typeof(Outline))]
=======
using ThirdParty.QuickOutline;

namespace FifthSemester.Player.Components {
>>>>>>> origin/main
    public class FlashlightPickup : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject _playerFlashlightObject;
<<<<<<< HEAD
        [SerializeField] private PlayerFlashlight _playerFlashlight;
        [SerializeField] private AudioClip _pickupSound;
=======
>>>>>>> origin/main

        private Outline _outline;
        private BoxCollider _collider;

        public bool IsInteractable => true;

        private void Awake() {
<<<<<<< HEAD
            _outline = GetComponent<Outline>();
            _collider = GetComponent<BoxCollider>();
            _outline.enabled = false;
            _collider.enabled = true;

            if (_playerFlashlightObject == null) {
                _playerFlashlight = GameObject.FindWithTag("Player")?.GetComponentInChildren<PlayerFlashlight>();
            }
        }

        public void Interact() {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService) && _pickupSound != null) {
                audioService.PlaySFX(_pickupSound, volume: 1f);
            }

            if (_playerFlashlight != null) {
                _playerFlashlight.EnableFlashlight();
            }

=======
            _outline  = GetComponent<Outline>();
            _collider = GetComponent<BoxCollider>();
            _outline.enabled = false;
            _collider.enabled = true;
        }

        public void Interact() {
>>>>>>> origin/main
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
