// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using System;
<<<<<<< HEAD
using System.Collections;
using System.Collections.Generic;
=======
using System.Collections.Generic;
using TMPro;
>>>>>>> origin/main
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Dialogue {
    public class DialogueService : MonoBehaviour, IDialogueService<TextAsset> {
<<<<<<< HEAD
        private const float DIALOGUE_FADE_DURATION = 1f;

=======
>>>>>>> origin/main
        public GameState CurrentState { get; set; } = GameState.Gameplay;
        public DialogueMode CurrentMode { get; private set; }

        public bool IsDialogueActive { get; private set; }

        private IEventBus _eventBus;
<<<<<<< HEAD
        private IFadeService _fadeService;
        private IAudioService _audioService;
        private Coroutine _endHoldCoroutine;

        [Header("Views")]
        [SerializeField] private DialogueView _dialogueView;
=======

        [Header("UI")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
>>>>>>> origin/main

        [Header("Speakers")]
        [SerializeField] private List<CharacterSO> _characters = new List<CharacterSO>();

        [Header("Defaults")]
        [SerializeField] private Color _defaultNameColor = Color.white;
        [SerializeField] private Color _defaultTextColor = Color.white;
<<<<<<< HEAD
=======
        [SerializeField] private TMP_FontAsset _defaultNameFont;
        [SerializeField] private TMP_FontAsset _defaultTextFont;
>>>>>>> origin/main

        private Queue<ParsedDialogueLine> _linesQueue;
        private string _currentDialogueSourceId;

        public PlayableDirector CurrentDirector { get; private set; }

        private void Awake() {
            ServiceLocator.Register<IDialogueService<TextAsset>>(this);
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
<<<<<<< HEAD
            ServiceLocator.TryGet<IAudioService>(out _audioService);

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
=======

            _eventBus?.Subscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
>>>>>>> origin/main
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanceRequested);
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

<<<<<<< HEAD
        private void ToggleDialogue(bool enable) {
            IsDialogueActive = enable;

            if (_dialogueView != null) {
                if (enable) {
                    _dialogueView.Show();
                }
                else {
                    _dialogueView.Hide();
                }
=======
        public void ToggleDialogue(bool enable) {
            IsDialogueActive = enable;
            if (_dialoguePanel != null) {
                _dialoguePanel.SetActive(enable);
>>>>>>> origin/main
            }
        }

        private void Clear() {
<<<<<<< HEAD
            if (_dialogueView != null) {
                _dialogueView.ClearDialogue();
=======
            if (_nameText != null) {
                _nameText.text = string.Empty;
            }

            if (_dialogueText != null) {
                _dialogueText.text = string.Empty;
>>>>>>> origin/main
            }
        }

        private void OnDialogueAdvanceRequested(DialogueAdvanceRequestedEvent evt) {
            if (!IsDialogueActive)
                return;

            if (CurrentMode == DialogueMode.Cutscene) {
<<<<<<< HEAD
                // if (CurrentDirector != null && CurrentDirector.playableGraph.IsValid())
                // {
                //     CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
                // }

=======
>>>>>>> origin/main
                if (_linesQueue != null && _linesQueue.Count > 0) {
                    DisplayNextLine();
                }
                else {
                    EndDialogue();
                }
                return;
            }

            if (CurrentDirector != null && CurrentDirector.playableGraph.IsValid()) {
                Clear();
                ToggleDialogue(false);
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }
            else {
                DisplayNextLine();
            }
        }

        public void StartDialogue(TextAsset dialogueFile, PlayableDirector director = null, string sourceId = null, DialogueMode mode = DialogueMode.Normal) {
            _linesQueue = DialogueParser.Parse(dialogueFile);
            CurrentDirector = director;
            _currentDialogueSourceId = sourceId;
            CurrentMode = mode;

            IsDialogueActive = true;
            _eventBus?.Publish(new DialogueStartedEvent());

<<<<<<< HEAD
            PlayStartFade(() => BeginDialogue());
        }

        private void BeginDialogue() {
            if (CurrentDirector != null && CurrentMode == DialogueMode.Cutscene) {
=======
            if (CurrentDirector != null) {
>>>>>>> origin/main
                ToggleDialogue(false);
                CurrentDirector.Play();
            }
            else {
                ToggleDialogue(true);
<<<<<<< HEAD
                PlayDialogueStartClip();
                DisplayNextLine();

                if (CurrentDirector != null) {
                    CurrentDirector.Play();
                }
            }
        }

=======
                DisplayNextLine();
            }
        }
>>>>>>> origin/main
        public void TimelineShowLine() {
            ToggleDialogue(true);
            DisplayNextLine();

            if (CurrentDirector != null && CurrentDirector.playableGraph.IsValid()) {
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
            }
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
<<<<<<< HEAD

            if (_dialogueView == null) {
                throw new InvalidOperationException("[DialogueService] DialogueView não está configurado.");
            }

            _dialogueView.SetDialogue(
                speakerName,
                line.text,
                null,
                hasCharacter ? character.nameColor : _defaultNameColor,
                hasCharacter ? character.textColor : _defaultTextColor
            );
=======
            _nameText.text = speakerName;
            _nameText.color = hasCharacter ? character.nameColor : _defaultNameColor;
            _nameText.font = hasCharacter && character.nameFont != null ? character.nameFont : _defaultNameFont;

            _dialogueText.text = line.text;
            _dialogueText.color = hasCharacter ? character.textColor : _defaultTextColor;
            _dialogueText.font = hasCharacter && character.textFont != null ? character.textFont : _defaultTextFont;
>>>>>>> origin/main
        }

        public void EndDialogue() {
            string sourceId = _currentDialogueSourceId;

<<<<<<< HEAD
            PlayEndFade(sourceId);
        }

        private void FinalizeDialogueEnd(string sourceId) {
=======
>>>>>>> origin/main
            Clear();
            ToggleDialogue(false);

            if (CurrentDirector != null && CurrentDirector.isActiveAndEnabled && CurrentDirector.playableGraph.IsValid()) {
                CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
            }

            CurrentDirector = null;
            _linesQueue = null;
            _currentDialogueSourceId = null;

            _eventBus?.Publish(new DialogueEndedEvent { NpcId = sourceId });
        }

        public void OnGameStateChanged(GameStateChangedEvent evt) {
            CurrentState = evt.CurrentState;
        }

<<<<<<< HEAD
        private void PlayStartFade(Action onComplete) {
            EnsureFadeService();

            if (_fadeService == null) {
                onComplete?.Invoke();
                return;
            }

            _fadeService.FadeIn(DIALOGUE_FADE_DURATION, onComplete);
        }

        private void PlayEndFade(string sourceId) {
            EnsureFadeService();

            if (_fadeService == null) {
                FinalizeDialogueEnd(sourceId);
                return;
            }

            // Notificar imediatamente que o diálogo está terminando para que
            // listeners (ex.: animadores de NPC) possam parar a animação de fala
            // enquanto o fade/hold ocorre.
            _eventBus?.Publish(new DialogueEndedEvent { NpcId = sourceId });

            _fadeService.FadeOut(DIALOGUE_FADE_DURATION, () => {
                _endHoldCoroutine = StartCoroutine(HoldBlackThenRelease(sourceId));
            });
        }

        private IEnumerator HoldBlackThenRelease(string sourceId) {
            Clear();
            ToggleDialogue(false);

            yield return new WaitForSecondsRealtime(1f);

            _fadeService.FadeIn(DIALOGUE_FADE_DURATION, () => {
                FinalizeDialogueEnd(sourceId);
                _endHoldCoroutine = null;
            });
        }

        // Força o encerramento imediato do diálogo, pulando fades/esperas.
        public void ForceEndDialogueImmediate() {
            // Se houver um hold aguardando, pare-o.
            if (_endHoldCoroutine != null) {
                try {
                    StopCoroutine(_endHoldCoroutine);
                }
                catch { }
                _endHoldCoroutine = null;
            }

            // Tentar limpar overlay de fade imediatamente.
            EnsureFadeService();
            if (_fadeService != null) {
                // FadeIn com zero duration para garantir tela visível
                _fadeService.FadeIn(0f, null);
            }

            // Finaliza estado do diálogo imediatamente
            FinalizeDialogueEnd(_currentDialogueSourceId);
        }

        private void EnsureFadeService() {
            if (_fadeService == null) {
                ServiceLocator.TryGet<IFadeService>(out _fadeService);
            }
        }

=======
>>>>>>> origin/main
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
<<<<<<< HEAD

        private void PlayDialogueStartClip() {
            if (_audioService == null || _linesQueue == null || _linesQueue.Count == 0) {
                return;
            }

            ParsedDialogueLine firstLine = _linesQueue.Peek();
            if (!TryGetCharacter(firstLine.speakerName, out CharacterSO character)) {
                return;
            }

            AudioClip clip = character != null ? character.dialogueStartClip : null;
            if (clip == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }
=======
>>>>>>> origin/main
    }
}
