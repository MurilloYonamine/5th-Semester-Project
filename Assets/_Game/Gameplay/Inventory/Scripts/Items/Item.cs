// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Player;
using FifthSemester.Gameplay.Shared;
<<<<<<< HEAD
using FifthSemester.Core.Services;
=======
>>>>>>> origin/main

namespace FifthSemester.Gameplay.Inventory {
    [RequireComponent(typeof(Outline))]
    public class Item : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        public bool IsInteractable => true;
        private Outline _outline;
        private BoxCollider _collider;

<<<<<<< HEAD
        [SerializeField] private AudioClip[] _pickupSounds;

        protected virtual void Awake() {
=======
        private void Awake() {
>>>>>>> origin/main
            _outline = GetComponent<Outline>();

            if (!TryGetComponent(out BoxCollider collider)) {
                _collider = GetComponentInChildren<BoxCollider>();
            } else {
                _collider = collider;
            }

<<<<<<< HEAD
            if (_outline != null) {
                _outline.enabled = false;
            }
=======
            _outline.enabled = false;
>>>>>>> origin/main

            if (_collider != null)
                _collider.enabled = true;
        }

<<<<<<< HEAD
        public virtual void Interact() {
            if (_outline != null) {
                _outline.enabled = false;
            }

            if (_collider != null) {
                _collider.enabled = false;
            }

            if (ServiceLocator.TryGet<IAudioService>(out var audioService) && _pickupSounds != null && _pickupSounds.Length > 0) {
                int idx = UnityEngine.Random.Range(0, _pickupSounds.Length);
                var clip = _pickupSounds[idx];
                if (clip != null) audioService.PlaySFX(clip, volume: 1f);
            }
=======
        public void Interact() {
            _outline.enabled = false;
            _collider.enabled = false;
>>>>>>> origin/main
        }

        public void StopInteract() {
        }

        public void Highlight(bool value) {
<<<<<<< HEAD
            if (_outline == null && !TryGetComponent(out _outline)) {
                return;
            }

=======
>>>>>>> origin/main
            _outline.enabled = value;
        }
        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
