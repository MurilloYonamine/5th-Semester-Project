// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using System;
using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class InteractMission : MissionBase {
        [Header("Configuração de Interação")]
        [SerializeField] private string _interactableTargetId;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            if (string.IsNullOrWhiteSpace(_interactableTargetId) && definition != null) {
                _interactableTargetId = definition.InteractableTargetId;
            }

            _eventBus.Subscribe<ObjectSuccessfullyInteractedEvent>(OnObjectInteracted);
        }

        private void OnObjectInteracted(ObjectSuccessfullyInteractedEvent evt) {
            if (string.IsNullOrEmpty(evt.ObjectId)) return;

            if (string.Equals(evt.ObjectId, _interactableTargetId, StringComparison.Ordinal)) {
                _progress = "1/1";
                PublishProgress();
                Complete();
            }
        }

        public override void Cleanup() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<ObjectSuccessfullyInteractedEvent>(OnObjectInteracted);
            }
            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<ObjectSuccessfullyInteractedEvent>(OnObjectInteracted);
            }
        }
    }
}
