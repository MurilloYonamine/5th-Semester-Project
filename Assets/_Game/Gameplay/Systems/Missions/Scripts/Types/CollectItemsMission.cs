// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay {
    public class CollectItemsMission : MissionBase {
        private bool _subscribed;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            int current = GetProgressCount();
            _progress = $"{current}/{_definition.RequiredCount}";
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

            int current = GetProgressCount() + 1;
            int total = _definition.RequiredCount;
            _progress = $"{current}/{total}";
            SaveProgress();
            PublishProgress();

            if (current >= total) {
                Complete();
            }
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
