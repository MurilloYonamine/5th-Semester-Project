// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using System;
using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public class DialogueService : MonoBehaviour, IDialogueService<TextAsset> {
        public GameState CurrentState { get; set; } = GameState.Gameplay;

        public bool IsDialogueActive { get; private set; }

        private IEventBus _eventBus;

        [Header("UI")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        [Header("Speakers")]
        [SerializeField] private List<CharacterSO> _characters = new List<CharacterSO>();

        [Header("Defaults")]
        [SerializeField] private Color _defaultNameColor = Color.white;
        [SerializeField] private Color _defaultTextColor = Color.white;
        [SerializeField] private TMP_FontAsset _defaultNameFont;
        [SerializeField] private TMP_FontAsset _defaultTextFont;

        private Queue<ParsedDialogueLine> _linesQueue;

        private void Awake() {
            ServiceLocator.Register<IDialogueService<TextAsset>>(this);
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus?.Subscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        public void ToggleDialogue(bool enable) {
            IsDialogueActive = enable;
            if (_dialoguePanel != null) {
                _dialoguePanel.SetActive(enable);
            }
        }

        private void Clear() {
            if (_nameText != null) {
                _nameText.text = string.Empty;
            }

            if (_dialogueText != null) {
                _dialogueText.text = string.Empty;
            }
        }

        private void OnDialogueAdvanceRequested(DialogueAdvanceRequestedEvent evt) {
            DisplayNextLine();
        }

        public void StartDialogue(TextAsset dialogueFile) {
            _linesQueue = DialogueParser.Parse(dialogueFile);
            ToggleDialogue(true);
            _eventBus?.Publish(new DialogueStartedEvent());
        }

        public void DisplayNextLine() {
            if (_linesQueue == null || _linesQueue.Count == 0) {
                EndDialogue();
                return;
            }

            ParsedDialogueLine line = _linesQueue.Dequeue();

            CharacterSO character;
            bool hasCharacter = TryGetCharacter(line.speakerName, out character);

            string speakerName = string.IsNullOrWhiteSpace(line.speakerName) ? string.Empty : line.speakerName;
            _nameText.text = speakerName;
            _nameText.color = hasCharacter ? character.nameColor : _defaultNameColor;
            _nameText.font = hasCharacter && character.nameFont != null ? character.nameFont : _defaultNameFont;

            _dialogueText.text = line.text;
            _dialogueText.color = hasCharacter ? character.textColor : _defaultTextColor;
            _dialogueText.font = hasCharacter && character.textFont != null ? character.textFont : _defaultTextFont;
        }

        public void EndDialogue() {
            Clear();
            ToggleDialogue(false);
            _eventBus?.Publish(new DialogueEndedEvent());
        }

        public void OnGameStateChanged(GameStateChangedEvent evt) {
            CurrentState = evt.CurrentState;
        }

        private bool TryGetCharacter(string speakerName, out CharacterSO character) {
            character = null;

            if (string.IsNullOrWhiteSpace(speakerName)) {
                return false;
            }

            for (int i = 0; i < _characters.Count; i++) {
                CharacterSO config = _characters[i];
                if (config == null) {
                    continue;
                }

                if (string.Equals(config.characterName, speakerName, StringComparison.OrdinalIgnoreCase)) {
                    character = config;
                    return true;
                }
            }

            return false;
        }
    }
}
