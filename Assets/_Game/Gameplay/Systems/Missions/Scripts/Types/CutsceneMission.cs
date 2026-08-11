// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class CutsceneMission : MissionBase {
        private bool _subscribed;
        private bool _cutscenePending;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
        }

        public override void StartMission() {
            base.StartMission();

            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
                _eventBus.Subscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = true;
            }

            _cutscenePending = true;

            IDialogueService<TextAsset> dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            if (dialogueService == null || !dialogueService.IsDialogueActive) {
                PlayCutscene();
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (_isComplete || !_cutscenePending) return;

            PlayCutscene();
        }

        private void PlayCutscene() {
            if (!_cutscenePending || _isComplete) {
                return;
            }

            _cutscenePending = false;

            ICutsceneService cutsceneService = ServiceLocator.Get<ICutsceneService>();
            if (cutsceneService == null) {
                Debug.LogWarning($"[CutsceneMission] ICutsceneService não encontrado para {_definition?.MissionId}.");
                return;
            }

            cutsceneService.PlayCutscene(_definition.TargetCutscene);
        }

        private void OnCutsceneEnded(CutsceneEndedEvent evt) {
            if (evt.CutsceneID == _definition.TargetCutscene) {
                PublishProgress();
                Complete();
            }
        }
        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _eventBus.Unsubscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = false;
            }
            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _eventBus.Unsubscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = false;
            }
        }
    }
}
