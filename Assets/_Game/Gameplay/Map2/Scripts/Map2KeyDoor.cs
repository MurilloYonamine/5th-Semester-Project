using System.Collections.Generic;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Shared;
using Sirenix.OdinInspector;
using TMPro;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.AI;
using System;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Outline))]
    public class Map2KeyDoor : MonoBehaviour, IInteractable {
        [Header("Configurações Visuais")]
        private Outline _outline;
        private TextMeshPro _textLocal;

        [Header("Configuração da Chave")]
        [SerializeField] private Map2KeyDefinitionSO _requiredKey;
        [SerializeField] private bool _requiresKey = true;
        [SerializeField] private bool _canBeOpenedByNurse = false;

        [Header("Configurações de Movimento")]
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private float _openAngle = 90f;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private bool _useDoubleDoor;

        [Header("Audio")]
        [SerializeField] private AudioClip[] _doorSfx;
        [SerializeField] private AudioClip[] _lockedSounds;
        [SerializeField] private NavMeshObstacle _navMeshObstacle;

        [ShowIf(nameof(_useDoubleDoor))]
        [SerializeField] private Transform[] _doorMeshes;

        [HideIf(nameof(_useDoubleDoor))]
        [SerializeField] private Transform _doorMesh;

        private IInventoryService<Item> _inventoryService;
        private IMap2KeyService _map2KeyService;
        private Quaternion[] _closedRotations;
        private Quaternion[] _targetRotations;
        private Transform[] _activeDoorMeshes;
        private IAudioService _audioService;
        private Color _unlockedColor;
        private string _defaultText;

        public bool IsInteractable { get; private set; } = true;
        public bool CanBeOpenedByNurse => _canBeOpenedByNurse;

        public string Id => gameObject.name;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;

            _textLocal = GetComponentInChildren<TextMeshPro>();
            if (_textLocal != null) {
                _defaultText = _textLocal.text;
                _textLocal.gameObject.SetActive(false);
            }

            if(_doorMesh == null)
                _doorMesh = gameObject.transform;

            CacheDoorMeshes();
            InitializeRotations();
            _unlockedColor = new Color32(105, 255, 144, 255);
        }

        private void Start() {
            ServiceLocator.TryGet<IInventoryService<Item>>(out _inventoryService);
            ServiceLocator.TryGet<IAudioService>(out _audioService);
            ServiceLocator.TryGet<IMap2KeyService>(out _map2KeyService);

            UpdateDoorVisuals();
        }

        private void Update() {
            if (_activeDoorMeshes == null || _targetRotations == null) {
                return;
            }

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                Transform doorMesh = _activeDoorMeshes[i];
                if (doorMesh == null) {
                    continue;
                }

                doorMesh.localRotation = Quaternion.Lerp(doorMesh.localRotation, _targetRotations[i], Time.deltaTime * _speed);
            }
        }

        public void Interact() {
            if (_requiresKey && !HasRequiredKey()) {
                PlayRandomLockedSound();
                return;
            }

            _isOpen = !_isOpen;
            PlayDoorSound();
            UpdateTargetRotations();
        }

        public void StopInteract() {
        }

        public void TryOpenByAI() {
            if (!_canBeOpenedByNurse) {
                return;
            }

            if (!_isOpen) {
                _isOpen = true;
                PlayDoorSound();
                UpdateTargetRotations();
            }

            if (_navMeshObstacle != null) {
                _navMeshObstacle.carving = false;
            }
        }

        public void TryCloseByAI() {
            if (!_canBeOpenedByNurse) {
                return;
            }

            if (_isOpen) {
                _isOpen = false;
                PlayDoorSound();
                UpdateTargetRotations();
            }

            if (_navMeshObstacle != null) {
                _navMeshObstacle.carving = true;
            }
        }

        public void Highlight(bool value) {
            if (_outline != null) {
                _outline.enabled = value;
                _outline.OutlineColor = IsDoorUnlocked() ? _unlockedColor : Color.red;
            }

            if (_textLocal != null) {
                _textLocal.gameObject.SetActive(value);
                _textLocal.color = IsDoorUnlocked() ? _unlockedColor : Color.red;
                _textLocal.text = _defaultText;
            }
        }

        private bool HasRequiredKey() {
            if (_requiredKey == null || _inventoryService == null) {
                return false;
            }

            IReadOnlyList<Item> items = _inventoryService.GetItems();
            if (items == null) {
                return false;
            }

            for (int i = 0; i < items.Count; i++) {
                if (items[i] is Map2KeyItem keyItem && keyItem.KeyDefinition == _requiredKey) {
                    return true;
                }
            }

            return false;
        }

        private void CacheDoorMeshes() {
            if (_useDoubleDoor) {
                _activeDoorMeshes = _doorMeshes ?? Array.Empty<Transform>();
                return;
            }

            _activeDoorMeshes = _doorMesh != null ? new[] { _doorMesh } : Array.Empty<Transform>();
        }

        private void InitializeRotations() {
            if (_activeDoorMeshes == null) {
                _closedRotations = Array.Empty<Quaternion>();
                _targetRotations = Array.Empty<Quaternion>();
                return;
            }

            _closedRotations = new Quaternion[_activeDoorMeshes.Length];
            _targetRotations = new Quaternion[_activeDoorMeshes.Length];

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                Transform doorMesh = _activeDoorMeshes[i];
                if (doorMesh == null) {
                    continue;
                }

                _closedRotations[i] = doorMesh.localRotation;
                _targetRotations[i] = _closedRotations[i];
            }
        }

        private void UpdateTargetRotations() {
            if (_activeDoorMeshes == null || _targetRotations == null) {
                return;
            }

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                if (_activeDoorMeshes[i] == null) {
                    continue;
                }

                if (_isOpen) {
                    float direction = _useDoubleDoor && i % 2 == 1 ? -_openAngle : _openAngle;
                    _targetRotations[i] = _closedRotations[i] * Quaternion.Euler(0f, direction, 0f);
                }
                else {
                    _targetRotations[i] = _closedRotations[i];
                }
            }
        }

        private void UpdateDoorVisuals() {
            if (_outline != null) {
                _outline.OutlineColor = IsDoorUnlocked() ? _unlockedColor : Color.red;
            }

            if (_textLocal != null) {
                _textLocal.color = IsDoorUnlocked() ? _unlockedColor : Color.red;
            }
        }

        private bool IsDoorUnlocked() {
            if (!_requiresKey) {
                return true;
            }

            if (_map2KeyService != null && _map2KeyService.HasCollectedAllKeys) {
                return true;
            }

            return HasRequiredKey();
        }

        private void PlayDoorSound() {
            if (_audioService == null || _doorSfx == null || _doorSfx.Length == 0) {
                return;
            }

            int randomIndex = UnityEngine.Random.Range(0, _doorSfx.Length);
            AudioClip clip = _doorSfx[randomIndex];
            if (clip == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }

        private void PlayDoorSound(AudioClip clip) {
            if (clip == null || _audioService == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }

        private void PlayRandomLockedSound() {
            if (_lockedSounds == null || _lockedSounds.Length == 0 || _audioService == null) return;

            int idx = UnityEngine.Random.Range(0, _lockedSounds.Length);
            AudioClip clip = _lockedSounds[idx];
            if (clip != null) _audioService.PlaySFX(clip);
        }
    }
}
