using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public class MissionUIView : MonoBehaviour {
        [Header("UI Components")]
        [SerializeField] private GameObject _missionPanel;
        [SerializeField] private TextMeshProUGUI _missionTitleText;
        [SerializeField] private TextMeshProUGUI _missionDescriptionText;
        [SerializeField] private TextMeshProUGUI _missionProgressionText;

        private IEventBus _eventBus;
        private string _currentMissionId;

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
                _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
                _eventBus.Subscribe<MissionProgressEvent>(OnMissionProgress);
                _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
                _eventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            }

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            if (gameStateService != null) {
                ApplyGameState(gameStateService.CurrentState);
            }
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
                _eventBus.Unsubscribe<MissionProgressEvent>(OnMissionProgress);
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
                _eventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            }
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            _currentMissionId = evt.MissionId;
            _missionTitleText.text = evt.Title;
            _missionDescriptionText.text = evt.Description;
        }

        private void OnMissionProgress(MissionProgressEvent evt) {
            if (!string.IsNullOrEmpty(_currentMissionId) && evt.MissionId != _currentMissionId) {
                return;
            }

            if (string.IsNullOrEmpty(evt.Progress)) {
                _missionProgressionText.gameObject.SetActive(false);
            }
            else {
                _missionProgressionText.gameObject.SetActive(true);
                _missionProgressionText.text = evt.Progress;
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            ApplyGameState(evt.CurrentState);
        }

        private void OnDialogueStarted(DialogueStartedEvent evt) {
            ApplyGameState(GameState.Cutscene);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            ApplyGameState(gameStateService != null ? gameStateService.CurrentState : GameState.Gameplay);
        }

        private void ApplyGameState(GameState currentState) {
            if (_missionPanel == null) return;

            bool shouldShow = currentState == GameState.Gameplay;
            _missionPanel.SetActive(shouldShow);
        }
    }
}
