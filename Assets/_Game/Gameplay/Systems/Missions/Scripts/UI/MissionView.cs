using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(CanvasGroup))]
    public class MissionUIView : MonoBehaviour {
        [Header("UI Components")]
        [SerializeField] private GameObject _missionPanel;

        [Header("Canvas")]
        private CanvasGroup _canvasGroup;

        [SerializeField] private TextMeshProUGUI _missionTitleText;
        [SerializeField] private TextMeshProUGUI _missionDescriptionText;
        [SerializeField] private TextMeshProUGUI _missionProgressionText;

        private IEventBus _eventBus;
        private string _currentMissionId;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

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

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            RefreshCurrentMission();
            StartCoroutine(RefreshAfterDelay());
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

        private System.Collections.IEnumerator RefreshAfterDelay() {
            yield return null;
            RefreshCurrentMission();
            yield return new WaitForSeconds(0.1f);
            RefreshCurrentMission();
        }

        private void RefreshCurrentMission() {
            IMissionService missionService = ServiceLocator.Get<IMissionService>();
            if (missionService == null) return;

            MissionDefinition current = missionService.GetCurrentMission();
            if (current != null) {
                _currentMissionId = current.MissionId;
                if (_missionTitleText != null) _missionTitleText.text = current.Title;
                if (_missionDescriptionText != null) _missionDescriptionText.text = current.Description;

                if (missionService.CurrentMission != null) {
                    UpdateProgressText(missionService.CurrentMission.Progress);
                }
            }
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            _currentMissionId = evt.MissionId;
            if (_missionTitleText != null) _missionTitleText.text = evt.Title;
            if (_missionDescriptionText != null) _missionDescriptionText.text = evt.Description;

            IMissionService missionService = ServiceLocator.Get<IMissionService>();
            if (missionService?.CurrentMission != null) {
                UpdateProgressText(missionService.CurrentMission.Progress);
            }
        }

        private void OnMissionProgress(MissionProgressEvent evt) {
            if (!string.IsNullOrEmpty(_currentMissionId) && evt.MissionId != _currentMissionId) {
                return;
            }

            _currentMissionId = evt.MissionId;
            UpdateProgressText(evt.Progress);
        }

        private void UpdateProgressText(string progress) {
            if (_missionProgressionText == null) return;

            if (string.IsNullOrWhiteSpace(progress)) {
                _missionProgressionText.gameObject.SetActive(false);
            }
            else {
                _missionProgressionText.gameObject.SetActive(true);
                _missionProgressionText.text = progress;
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
            bool shouldShow = currentState == GameState.Gameplay;

            if (_canvasGroup != null) {
                _canvasGroup.alpha = shouldShow ? 1f : 0f;
            }
            else {
                if (_missionPanel == null) return;
                _missionPanel.SetActive(shouldShow);
            }
        }
    }
}
