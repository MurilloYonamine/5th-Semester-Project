// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FifthSemester.Core.Managers;
using FifthSemester.Core.States;
using FifthSemester.Core.Events;

namespace FifthSemester.DialogueSystem {
    public class DialogueManager : MonoBehaviour {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;

        private Queue<DialogueLine> _linesQueue;

        private void Awake() {
            Instance = this;
            _linesQueue = new Queue<DialogueLine>();
        }

        public void StartDialogue(DialogueSO dialogue) {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ChangeState(GameState.Dialogue);

            if (InputEvents.Instance != null) {
                InputEvents.Instance.OnDialogueAdvance -= DisplayNextLine;
                InputEvents.Instance.OnDialogueAdvance += DisplayNextLine;
            }

            Debug.Log("Starting dialogue...");

            _dialoguePanel.SetActive(true);
            _linesQueue.Clear();

            foreach (var line in dialogue.lines) {
                _linesQueue.Enqueue(line);
            }

            DisplayNextLine();
        }

        public void DisplayNextLine() {
            if (_linesQueue.Count == 0) {
                EndDialogue();
                return;
            }

            DialogueLine line = _linesQueue.Dequeue();

            Debug.Log($"Displaying line: {line.text} by {line.speaker.characterName}");

            _nameText.text = line.speaker.characterName;
            _nameText.color = line.speaker.nameColor;

            _dialogueText.text = line.text;
            _dialogueText.color = line.speaker.textColor;
        }

        private void EndDialogue() {
            _dialoguePanel.SetActive(false);
            if (InputEvents.Instance != null)
                InputEvents.Instance.OnDialogueAdvance -= DisplayNextLine;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.ChangeState(GameState.Gameplay);
        }

        public bool IsPanelActive() => _dialoguePanel.activeSelf;
    }
}
