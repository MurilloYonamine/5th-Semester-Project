// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using System;
using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class ReachLocationMission : MissionBase {
        [Header("Configuração de Navegação")]
        [SerializeField] private string _destinationZoneId;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            _eventBus.Subscribe<PlayerReachedZoneEvent>(OnPlayerReachedZone);
        }

        private void OnPlayerReachedZone(PlayerReachedZoneEvent evt) {
            if (string.IsNullOrEmpty(evt.ZoneId)) return;

            if (string.Equals(evt.ZoneId, _destinationZoneId, StringComparison.Ordinal)) {
                _progress = "Concluído";
                PublishProgress();
                Complete();
            }
        }

        public override void Cleanup() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<PlayerReachedZoneEvent>(OnPlayerReachedZone);
            }
            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<PlayerReachedZoneEvent>(OnPlayerReachedZone);
            }
        }
    }
}
