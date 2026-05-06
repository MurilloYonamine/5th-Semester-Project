// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class CollectItemsMission : MissionBase {
        private bool _subscribed;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
        }

        public override void StartMission() {
            base.StartMission();
            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<InventoryItemAddedEvent>(OnItemAdded);
                _subscribed = true;
            }
        }

        private void OnItemAdded(InventoryItemAddedEvent evt) {
            if (_isComplete) return;
            IncrementProgress();
        }

        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<InventoryItemAddedEvent>(OnItemAdded);
                _subscribed = false;
            }
            base.Cleanup();
        }
    }
}
