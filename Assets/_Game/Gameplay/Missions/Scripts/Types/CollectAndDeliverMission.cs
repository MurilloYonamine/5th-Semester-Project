// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class CollectAndDeliverMission : MissionBase {
        private bool _itemsCollected;
        private bool _subscribed;

        private int RequiredCollectCount => _definition != null && _definition.CollectCount > 0 ? _definition.CollectCount : 1;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            int current = GetProgressCount();
            _progress = $"{current}/{RequiredCollectCount}";
            _itemsCollected = current >= RequiredCollectCount;
        }

        public override void StartMission() {
            base.StartMission();
            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Subscribe<ItemDeliveredEvent>(OnItemDelivered);
                _subscribed = true;
            }
        }

        private void OnItemAdded(ItemPickedUpEvent evt) {
            if (_itemsCollected || _isComplete) return;
            if (evt.ItemName != _definition.CollectItemName) return;

            int current = GetProgressCount() + 1;
            _progress = $"{current}/{RequiredCollectCount}";
            _itemsCollected = current >= RequiredCollectCount;

            SaveProgress();
            PublishProgress();
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
                _eventBus.Unsubscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Unsubscribe<ItemDeliveredEvent>(OnItemDelivered);
                _subscribed = false;
            }
            base.Cleanup();
        }
    }
}
