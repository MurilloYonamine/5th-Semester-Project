// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public class CutsceneMission : MissionBase {
        private bool _subscribed;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
        }

        public override void StartMission() {
            base.StartMission();

            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
                _subscribed = true;
            }

            ICutsceneService cutsceneService = ServiceLocator.Get<ICutsceneService>();
            if (cutsceneService != null) {
                cutsceneService.PlayCutscene(_definition.TargetCutscene);
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (_isComplete) return;

            PublishProgress();
            Complete();
        }

        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _subscribed = false;
            }
            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _subscribed = false;
            }
        }
    }
}
