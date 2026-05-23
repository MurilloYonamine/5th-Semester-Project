using System;
using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class TalkToNpcMission : MissionBase {
        [SerializeField]
        private string _targetNpcId;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            if (string.IsNullOrWhiteSpace(_targetNpcId) && definition != null) {
                _targetNpcId = definition.NpcId;
            }

            if (_eventBus != null) {
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            string npcId = evt.NpcId;

            if (string.IsNullOrEmpty(npcId)) return;

            if (string.Equals(npcId, _targetNpcId, StringComparison.Ordinal)) {
                _progress = "1/1";
                PublishProgress();
                Complete();
            }
        }

        public override void Cleanup() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            }

            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            }
        }
    }
}
