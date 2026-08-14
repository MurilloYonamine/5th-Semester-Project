// Autor: Murillo Gomes Yonamine
// Data: 14/08/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

using Sirenix.OdinInspector;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : TextTriggerBase {
        private IDialogueService<TextAsset> _dialogueService;
        private ISettingsService _settingsService;
        private IEventBus _eventBus;
        private string _dialogueId;

        [SerializeField, Title("Textos do Diálogo")]
        private LocalizedTextAsset _dialogueFiles;

        public override bool IsInteractable => _dialogueFiles.Portuguese != null || _dialogueFiles.English != null;

        [SerializeField] private PlayableDirector _director;
        [SerializeField] private Animator _animator;
        [SerializeField] private NPCMovement _npcMovement;

        [Header("Look at Player Settings")]
        [SerializeField] private bool _lookAtPlayer = true;
        [SerializeField] private float _turnSpeed = 6f;

        private bool _isInteracting;
        private Transform _playerTransform;

        protected override void Awake() {
            base.Awake();

            if (_animator == null) {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_npcMovement == null) {
                _npcMovement = GetComponent<NPCMovement>();
                if (_npcMovement == null) {
                    _npcMovement = GetComponentInParent<NPCMovement>();
                }
            }
        }

        protected virtual void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_dialogueService == null) {
                Debug.LogError($"[DialogueTrigger] IDialogueService<TextAsset> não encontrado em {name}.");
                enabled = false;
                return;
            }

            if (_settingsService == null) {
                Debug.LogError($"[DialogueTrigger] ISettingsService não encontrado em {name}.");
                enabled = false;
                return;
            }

            _eventBus?.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            _isInteracting = false;
        }

        private void Update() {
            if (!_isInteracting || !_lookAtPlayer || _npcMovement != null) return;

            if (_playerTransform == null) {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null) _playerTransform = player.transform;
                else if (Camera.main != null) _playerTransform = Camera.main.transform;
            }

            if (_playerTransform != null) {
                Vector3 direction = _playerTransform.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f) {
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
                }
            }
        }

        protected override bool CanInteract() {
            return _dialogueService != null && !_dialogueService.IsDialogueActive && IsInteractable;
        }

        protected override void OnInteract() {
            if (_dialogueFiles.Portuguese == null && _dialogueFiles.English == null) {
                Debug.LogError($"[DialogueTrigger] Nenhum texto de diálogo configurado em {name}.");
                return;
            }

            Language currentLanguage = _settingsService != null ? _settingsService.Language : Language.Portuguese;

            TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (correctDialogue == null) {
                Debug.LogWarning($"[Dialogue] Nenhum ficheiro TXT encontrado para o idioma {currentLanguage} no NPC {gameObject.name}!");
                return;
            }

            Highlight(false);

            _dialogueId = string.IsNullOrWhiteSpace(Id) ? gameObject.name : Id;

            Transform playerTransform = null;
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) {
                playerTransform = playerObj.transform;
            } else if (Camera.main != null) {
                playerTransform = Camera.main.transform;
            }

            _playerTransform = playerTransform;
            _isInteracting = true;

            if (_npcMovement != null) {
                _npcMovement.StartTalking(playerTransform);
            }

            if (_director == null && _animator != null) {
                _animator.SetBool("IsTalking", true);
            }

            _dialogueService.StartDialogue(correctDialogue, _director, _dialogueId);
        }

        public override void StopInteract() {
            _isInteracting = false;

            if (_npcMovement != null) {
                _npcMovement.StopTalking();
            }

            _dialogueService?.EndDialogue();
        }

        public override void Highlight(bool value) {
            bool isDialogueActive = _dialogueService != null && _dialogueService.IsDialogueActive;
            base.Highlight(!isDialogueActive && value);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (!string.IsNullOrWhiteSpace(evt.NpcId) && evt.NpcId != _dialogueId) {
                return;
            }

            _isInteracting = false;

            if (_npcMovement != null) {
                _npcMovement.StopTalking();
            }

            if (_director != null || _animator == null) {
                return;
            }

            _animator.SetBool("IsTalking", false);
        }
    }
}
