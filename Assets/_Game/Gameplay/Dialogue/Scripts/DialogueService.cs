// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Dialogue
{
    public class DialogueService : MonoBehaviour, IDialogueService<TextAsset>
    {
        public GameState CurrentState { get; set; } = GameState.Gameplay;
        public DialogueMode CurrentMode { get; private set; }

        public bool IsDialogueActive { get; private set; }

        private IEventBus _eventBus;

        [Header("Views")]
        [SerializeField] private DialogueView _dialogueView;

        [Header("Speakers")]
        [SerializeField] private List<CharacterSO> _characters = new List<CharacterSO>();

        [Header("Defaults")]
        [SerializeField] private Color _defaultNameColor = Color.white;
        [SerializeField] private Color _defaultTextColor = Color.white;

        private Queue<ParsedDialogueLine> _linesQueue;
        private string _currentDialogueSourceId;

        public PlayableDirector CurrentDirector { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register<IDialogueService<TextAsset>>(this);
        }

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus == null) {
                Debug.LogError("[DialogueService] IEventBus não encontrado.");
                enabled = false;
                return;
            }

            if (_dialogueView == null) {
                Debug.LogError("[DialogueService] DialogueView não atribuído.");
                enabled = false;
                return;
            }

            _eventBus.Subscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void ToggleDialogue(bool enable)
        {
            IsDialogueActive = enable;

            if (_dialogueView != null)
            {
                if (enable)
                {
                    _dialogueView.Show();
                }
                else
                {
                    _dialogueView.Hide();
                }
            }
        }

        private void Clear()
        {
            if (_dialogueView != null)
            {
                _dialogueView.ClearDialogue();
            }
        }

        private void OnDialogueAdvanceRequested(DialogueAdvanceRequestedEvent evt)
        {
            if (!IsDialogueActive)
                return;

            if (CurrentMode == DialogueMode.Cutscene)
            {
                if (_linesQueue != null && _linesQueue.Count > 0)
                {
                    DisplayNextLine();
                }
                else
                {
                    EndDialogue();
                }
                return;
            }

            if (CurrentDirector != null && CurrentDirector.playableGraph.IsValid())
            {
                Clear();
                ToggleDialogue(false);
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }
            else
            {
                DisplayNextLine();
            }
        }

        public void StartDialogue(TextAsset dialogueFile, PlayableDirector director = null, string sourceId = null, DialogueMode mode = DialogueMode.Normal)
        {
            _linesQueue = DialogueParser.Parse(dialogueFile);
            CurrentDirector = director;
            _currentDialogueSourceId = sourceId;
            CurrentMode = mode;

            IsDialogueActive = true;
            _eventBus?.Publish(new DialogueStartedEvent());

            if (CurrentDirector != null && CurrentMode == DialogueMode.Cutscene)
            {
                ToggleDialogue(false);
                CurrentDirector.Play();
            }
            else
            {
                ToggleDialogue(true);
                DisplayNextLine();

                if (CurrentDirector != null)
                {
                    CurrentDirector.Play();
                }
            }
        }
        public void TimelineShowLine()
        {
            ToggleDialogue(true);
            DisplayNextLine();

            if (CurrentDirector != null && CurrentDirector.playableGraph.IsValid())
            {
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
            }
        }

        public void DisplayNextLine()
        {
            if (_linesQueue == null || _linesQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            ParsedDialogueLine line = _linesQueue.Dequeue();

            CharacterSO character;
            bool hasCharacter = TryGetCharacter(line.speakerName, out character);

            string speakerName = string.IsNullOrWhiteSpace(line.speakerName) ? string.Empty : line.speakerName;

            if (_dialogueView == null)
            {
                throw new InvalidOperationException("[DialogueService] DialogueView não está configurado.");
            }

            _dialogueView.SetDialogue(
                speakerName,
                line.text,
                null,
                hasCharacter ? character.nameColor : _defaultNameColor,
                hasCharacter ? character.textColor : _defaultTextColor
            );
        }

        public void EndDialogue()
        {
            string sourceId = _currentDialogueSourceId;

            Clear();
            ToggleDialogue(false);

            if (CurrentDirector != null && CurrentDirector.isActiveAndEnabled && CurrentDirector.playableGraph.IsValid())
            {
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }

            CurrentDirector = null;
            _linesQueue = null;
            _currentDialogueSourceId = null;

            _eventBus?.Publish(new DialogueEndedEvent { NpcId = sourceId });
        }

        public void OnGameStateChanged(GameStateChangedEvent evt)
        {
            CurrentState = evt.CurrentState;
        }

        private bool TryGetCharacter(string speakerName, out CharacterSO character)
        {
            character = null;

            if (string.IsNullOrWhiteSpace(speakerName))
            {
                return false;
            }

            for (int i = 0; i < _characters.Count; i++)
            {
                CharacterSO config = _characters[i];
                if (config == null)
                {
                    continue;
                }

                if (string.Equals(config.characterName, speakerName, StringComparison.OrdinalIgnoreCase))
                {
                    character = config;
                    return true;
                }
            }

            return false;
        }
    }
}
