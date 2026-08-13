// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class CutsceneMission : MissionBase {
        private const string TAG = "<color=yellow><b>[CutsceneMission]</b></color>";
        private bool _subscribed;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
        }

        public override void StartMission() {
            base.StartMission();

            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = true;
            }

            PlayCutscene();
        }

        private void PlayCutscene() {
            if (_isComplete) {
                return;
            }

            ICutsceneService cutsceneService = ServiceLocator.Get<ICutsceneService>();
            if (cutsceneService == null) {
                Debug.LogWarning($"{TAG} ICutsceneService not found for {_definition?.MissionId}.");
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
                _eventBus.Unsubscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = false;
            }
            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<CutsceneEndedEvent>(OnCutsceneEnded);
                _subscribed = false;
            }
        }
    }
}
