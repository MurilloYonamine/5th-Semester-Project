using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public class CollectAndDeliverMission : MissionBase {
        private bool _itemsCollected;
        private bool _subscribed;

        // NOVO: Contador específico para as entregas
        private int _deliveredCount;

        private int RequiredCollectCount => _definition != null && _definition.CollectCount > 0 ? _definition.CollectCount : 1;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            int current = GetProgressCount();
            _itemsCollected = current >= RequiredCollectCount;

            // Define o texto inicial dependendo se ainda está a recolher ou se já está a entregar
            if (!_itemsCollected) {
                _progress = $"Coletados: {current}/{RequiredCollectCount}";
            }
            else {
                _progress = $"Entregues: {_deliveredCount}/{RequiredCollectCount}";
            }
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
            if (_itemsCollected || _isComplete) {
                return;
            }

            if (_definition == null || evt.ItemName != _definition.CollectItemName) {
                return;
            }

            Debug.Log($"Texto Progress: {_progress}");

            int current = GetProgressCount() + 1;
            _itemsCollected = current >= RequiredCollectCount;

            if (_itemsCollected) {
                _progress = $"Entregues: 0/{RequiredCollectCount}";
            }
            else {
                _progress = $"Coletados: {current}/{RequiredCollectCount}";
            }

            SaveProgress();
            PublishProgress();
        }

        private void OnItemDelivered(ItemDeliveredEvent evt) {
            if (!_itemsCollected || _isComplete) {
                return;
            }

            if (_definition == null) {
                return;
            }

            bool deliveryPointMatches = System.Array.Exists(_definition.DeliveryPointIds, id => id == evt.DeliveryPointId);
            bool itemMatches = evt.DeliveredItemId == _definition.CollectItemName;

            if (deliveryPointMatches && itemMatches) {
                _deliveredCount++; 
                _progress = $"Entregues: {_deliveredCount}/{RequiredCollectCount}";

                SaveProgress();
                PublishProgress();

                if (_deliveredCount >= RequiredCollectCount) {
                    Complete();
                }
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
