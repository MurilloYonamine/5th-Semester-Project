// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class CollectAndDeliverMission : MissionBase {
        private bool _itemsCollected;
        private bool _subscribed;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
            _itemsCollected = GetProgressCount() >= _definition.RequiredCount;
        }

        public override void StartMission() {
            base.StartMission();
            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<InventoryItemAddedEvent>(OnItemAdded);
                _eventBus.Subscribe<ItemDeliveredEvent>(OnItemDelivered);
                _subscribed = true;
            }
        }

        private void OnItemAdded(InventoryItemAddedEvent evt) {
            if (_itemsCollected || _isComplete) return;
            IncrementProgress();
        }

        private void OnItemDelivered(ItemDeliveredEvent evt) {
            if (!_itemsCollected || _isComplete) return;

            bool deliveryPointMatches = System.Array.Exists(_definition.DeliveryPointIds, id => id == evt.DeliveryPointId);
            bool itemMatches = evt.DeliveredItemId == _definition.CollectItemName;

            if (deliveryPointMatches && itemMatches) {
                Complete();
            }
        }

        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<InventoryItemAddedEvent>(OnItemAdded);
                _eventBus.Unsubscribe<ItemDeliveredEvent>(OnItemDelivered);
                _subscribed = false;
            }
            base.Cleanup();
        }
    }
}
