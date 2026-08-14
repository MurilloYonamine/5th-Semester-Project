using FifthSemester.Shared;
using System;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using Sirenix.OdinInspector;
using TMPro;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.Doors {
    [RequireComponent(typeof(Outline))]
    public class Door : MonoBehaviour, IInteractable {
        [Header("Configurações Visuais")]
        [SerializeField] private Outline _outline;
        [SerializeField] private TextMeshPro _textLocal;

        [Header("Configurações de Movimento")]
        [SerializeField] private bool _isOpen = false;
        [SerializeField] private float _slideDistance = 1.5f;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private bool _useDoubleDoor = false;

        [HideIf(nameof(_useDoubleDoor))]
        [SerializeField] private DoorOpenDirection _openDirection = DoorOpenDirection.Right;

        [SerializeField] private Vector3 _slideAxis = new Vector3(1f, 0f, 0f);

        [Header("Audio")]
        [SerializeField] private AudioClip[] _doorSfx;

        [ShowIf(nameof(_useDoubleDoor))]
        [SerializeField] private Transform[] _doorMeshes;

        [HideIf(nameof(_useDoubleDoor))]
        [SerializeField] private Transform _doorMesh;

        [Header("Map Registry")]
        [SerializeField] private DoorType _doorType = DoorType.None;

        private IMapService _mapService;
        private Vector3[] _closedPositions;
        private Vector3[] _targetPositions;
        private Transform[] _activeDoorMeshes;
        private bool _isLocked = false;
        private Color _unlockedColor;
        private IAudioService _audioService;

        public bool IsInteractable { get; private set; } = true;

        public string Id => gameObject.name;
        private string _defaultText;

        private void Awake() {
            _outline = GetComponent<Outline>();
            if (_outline != null) {
                _outline.enabled = false;
            }

            if (_textLocal != null) {
                _textLocal.gameObject.SetActive(false);
                _defaultText = _textLocal.text;
            }

            if (_doorMesh == null)
                _doorMesh = gameObject.transform;

            CacheDoorMeshes();
            InitializePositions();

            _unlockedColor = new Color32(105, 255, 144, 255); // 69FF90
        }

        private void Start() {
            ServiceLocator.TryGet<IAudioService>(out _audioService);

            if (_doorType == DoorType.None) return;

            _mapService = ServiceLocator.Get<IMapService>();
            _mapService?.Register(_doorType, gameObject);
            Debug.Log($"[Door] Registered door '{gameObject.name}' with DoorType '{_doorType}' in MapService.");
        }

        private void OnDestroy() {
            if (_doorType == DoorType.None || _mapService == null) return;
            _mapService.Unregister(_doorType);
        }

        private void Update() {
            if (_activeDoorMeshes == null || _targetPositions == null) return;

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                Transform doorMesh = _activeDoorMeshes[i];
                if (doorMesh == null) continue;

                doorMesh.localPosition = Vector3.Lerp(doorMesh.localPosition, _targetPositions[i], Time.deltaTime * _speed);
            }
        }

        public void Interact() {
            if (_isLocked) {
                Debug.Log($"[Door] Player tried to open LOCKED door '{gameObject.name}' (DoorType: {_doorType}).");
                return;
            }

            _isOpen = !_isOpen;
            Debug.Log($"[Door] Player opened/closed door '{gameObject.name}' (DoorType: {_doorType}). _isOpen is now {_isOpen}.");
            PlayDoorSound();
            UpdateTargetPositions();
        }

        public void StopInteract() { }

        public void Highlight(bool value) {
            if (_outline != null)
                _outline.enabled = value;

            if (_textLocal != null)
                _textLocal.gameObject.SetActive(value);
        }

        public void Lock() {
            _isLocked = true;

            if (_isOpen) {
                _isOpen = false;
                UpdateTargetPositions();
            }

            if (_outline != null)
                _outline.OutlineColor = Color.red;

            if (_textLocal != null) {
                _textLocal.color = Color.red;
            }

            Debug.Log($"[Door] Door '{gameObject.name}' (DoorType: {_doorType}) is now LOCKED.");
        }

        public void Unlock() {
            _isLocked = false;

            if (_outline != null)
                _outline.OutlineColor = _unlockedColor;

            if (_textLocal != null) {
                _textLocal.color = _unlockedColor;
                _textLocal.text = _defaultText;
            }

            Debug.Log($"[Door] Door '{gameObject.name}' (DoorType: {_doorType}) is now UNLOCKED.");
        }

        private void CacheDoorMeshes() {
            if (_useDoubleDoor) {
                _activeDoorMeshes = _doorMeshes ?? Array.Empty<Transform>();
                return;
            }

            _activeDoorMeshes = _doorMesh != null ? new[] { _doorMesh } : new[] { transform };
        }

        private void InitializePositions() {
            if (_activeDoorMeshes == null) {
                _closedPositions = Array.Empty<Vector3>();
                _targetPositions = Array.Empty<Vector3>();
                return;
            }

            _closedPositions = new Vector3[_activeDoorMeshes.Length];
            _targetPositions = new Vector3[_activeDoorMeshes.Length];

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                Transform doorMesh = _activeDoorMeshes[i];
                if (doorMesh == null) continue;

                _closedPositions[i] = doorMesh.localPosition;
                _targetPositions[i] = _closedPositions[i];
            }

            if (_isOpen) {
                UpdateTargetPositions();
                for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                    if (_activeDoorMeshes[i] != null) {
                        _activeDoorMeshes[i].localPosition = _targetPositions[i];
                    }
                }
            }
        }

        private void UpdateTargetPositions() {
            if (_activeDoorMeshes == null || _targetPositions == null) return;

            for (int i = 0; i < _activeDoorMeshes.Length; i++) {
                if (_activeDoorMeshes[i] == null) continue;

                if (_isOpen) {
                    if (_useDoubleDoor) {
                        float slideDirection = i % 2 == 0 ? -_slideDistance : _slideDistance;
                        _targetPositions[i] = _closedPositions[i] + _slideAxis * slideDirection;
                    }
                    else {
                        float slideDirection = _openDirection == DoorOpenDirection.Left ? -_slideDistance : _slideDistance;
                        _targetPositions[i] = _closedPositions[i] + _slideAxis * slideDirection;
                    }
                }
                else {
                    _targetPositions[i] = _closedPositions[i];
                }
            }
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
    }
}
