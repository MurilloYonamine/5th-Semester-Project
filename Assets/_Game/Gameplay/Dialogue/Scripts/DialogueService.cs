// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using System.Collections.Generic;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Gameplay.Dialogue {
    public class DialogueService : MonoBehaviour, IDialogueService<DialogueSO> {
        public GameState CurrentState { get; set; } = GameState.Gameplay;

        public bool IsDialogueActive { get; private set; } = false;


        private IEventBus _eventBus;

        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        private Queue<DialogueLine> _linesQueue;


        private void Start() {
            ServiceLocator.Register<IDialogueService<DialogueSO>>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus.Subscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
        private void OnDisable() {
            _eventBus?.Unsubscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        public void ToggleDialogue(bool enable) {
            IsDialogueActive = enable;
            _dialoguePanel.SetActive(enable);
        }
        private void Clear() {
            _nameText.text = "";
            _dialogueText.text = "";
        }
        private void OnDialogueAdvanceRequested(DialogueAdvanceRequestedEvent evt) {
            DisplayNextLine();
        }
        public void StartDialogue(DialogueSO dialogue) {
            ToggleDialogue(true);
            _eventBus?.Publish(new DialogueStartedEvent());

            _linesQueue = new Queue<DialogueLine>(dialogue.lines);
        }

        public void DisplayNextLine() {
            if (_linesQueue == null || _linesQueue.Count == 0) {
                EndDialogue();
                return;
            }

            DialogueLine line = _linesQueue.Dequeue();
            _nameText.text = line.speaker.characterName;
            _nameText.color = line.speaker.nameColor;

            _dialogueText.text = line.text;
            _dialogueText.color = line.speaker.textColor;
        }

        public void EndDialogue() {
            ToggleDialogue(false);
            _eventBus?.Publish(new DialogueEndedEvent());
            Clear();
        }

        public void OnGameStateChanged(GameStateChangedEvent evt) {
            CurrentState = evt.CurrentState;
        }
    }
}
