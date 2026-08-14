using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class CollectAndDeliverMission : MissionBase {
        private bool _itemsCollected;
        private bool _subscribed;
        private IMissionService _missionService;
        private bool _awaitingCompletionDialogue;

        private int _deliveredCount;

        private int RequiredCollectCount => _definition != null && _definition.CollectCount > 0 ? _definition.CollectCount : 1;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
            ServiceLocator.TryGet<IMissionService>(out _missionService);

            int current = GetProgressCount();
            _itemsCollected = current >= RequiredCollectCount;
            _deliveredCount = _itemsCollected ? current : 0;

            if (!_itemsCollected) {
                _progress = $"Coletados: {current}/{RequiredCollectCount}";
            }
            else {
                _progress = $"Entregues: {_deliveredCount}/{RequiredCollectCount}";
            }

            Debug.Log($"[CollectAndDeliverMission] Initialize: MissionId='{definition?.MissionId}', _itemsCollected={_itemsCollected}, _deliveredCount={_deliveredCount}, _progress='{_progress}'");
        }

        public override void StartMission() {
            base.StartMission();
            Debug.Log($"[CollectAndDeliverMission] StartMission: Initializing door state for '{_definition?.MissionId}'...");
            _missionService?.UpdateCollectAndDeliverDoorState(_definition, -1);
            if (_itemsCollected) {
                Debug.Log($"[CollectAndDeliverMission] StartMission: Items already collected, applying deliveredCount={_deliveredCount} to doors.");
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);
            }

            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Subscribe<ItemDeliveredEvent>(OnItemDelivered);
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
                _subscribed = true;
            }
        }

        private void OnItemAdded(ItemPickedUpEvent evt) {
            if (_itemsCollected || _isComplete) {
                return;
            }

            if (_definition == null || !string.Equals(evt.ItemName, _definition.CollectItemName, System.StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            int current = GetProgressCount() + 1;
            _itemsCollected = current >= RequiredCollectCount;

            Debug.Log($"[CollectAndDeliverMission] OnItemAdded: Picked up '{evt.ItemName}'. Collected {current}/{RequiredCollectCount}. _itemsCollected is now {_itemsCollected}.");

            if (_itemsCollected) {
                _deliveredCount = 0;
                _progress = $"Entregues: 0/{RequiredCollectCount}";
                Debug.Log("[CollectAndDeliverMission] All items collected! Updating doors to initial delivery state (deliveredCount=0)...");
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);
                _missionService?.PlayMissionCompleteSFX();
            }
            else {
                _progress = $"Coletados: {current}/{RequiredCollectCount}";
            }

            SaveProgress();
            PublishProgress();
        }

        private void OnItemDelivered(ItemDeliveredEvent evt) {
            Debug.Log($"[CollectAndDeliverMission] OnItemDelivered received! Event DeliveryPointId='{evt.DeliveryPointId}', DeliveredItemId='{evt.DeliveredItemId}'. _itemsCollected={_itemsCollected}, _isComplete={_isComplete}, _deliveredCount={_deliveredCount}");

            if (!_itemsCollected || _isComplete) {
                Debug.LogWarning($"[CollectAndDeliverMission] OnItemDelivered ignored: _itemsCollected={_itemsCollected}, _isComplete={_isComplete}");
                return;
            }

            if (_definition == null) {
                Debug.LogWarning("[CollectAndDeliverMission] OnItemDelivered ignored: _definition is null");
                return;
            }

            bool deliveryPointMatches = System.Array.Exists(_definition.DeliveryPointIds, id => {
                if (string.IsNullOrWhiteSpace(id)) return false;
                string[] parts = id.Split(new char[] { ',', ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string partRaw in parts) {
                    string part = partRaw.Trim();
                    if (string.IsNullOrEmpty(part)) continue;

                    if (string.Equals(part, evt.DeliveryPointId, System.StringComparison.OrdinalIgnoreCase)) return true;

                    int us = evt.DeliveryPointId.LastIndexOf('_');
                    string evtSuffix = us >= 0 && us < evt.DeliveryPointId.Length - 1 ? evt.DeliveryPointId.Substring(us + 1) : evt.DeliveryPointId;
                    if (string.Equals(part, evtSuffix, System.StringComparison.OrdinalIgnoreCase)) return true;
                }

                return false;
            });
            bool itemMatches = string.Equals(evt.DeliveredItemId, _definition.CollectItemName, System.StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[CollectAndDeliverMission] deliveryPointMatches={deliveryPointMatches}, itemMatches={itemMatches} (expected item: '{_definition.CollectItemName}', expected DeliveryPointIds: [{string.Join(", ", _definition.DeliveryPointIds ?? System.Array.Empty<string>())}])");

            if (deliveryPointMatches && itemMatches) {
                _deliveredCount++; 
                _progress = $"Entregues: {_deliveredCount}/{RequiredCollectCount}";
                Debug.Log($"[CollectAndDeliverMission] Delivery SUCCESS! Updated progress: '{_progress}'. Updating doors with deliveredCount={_deliveredCount}...");
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);

                SaveProgress();
                PublishProgress();

                if (_deliveredCount >= RequiredCollectCount) {
                    _awaitingCompletionDialogue = true;
                    Debug.Log("[CollectAndDeliverMission] All items delivered! Awaiting completion dialogue...");
                }
            }
            else {
                Debug.LogWarning($"[CollectAndDeliverMission] Delivery rejected! deliveryPointMatches={deliveryPointMatches}, itemMatches={itemMatches}");
            }
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (_isComplete || !_awaitingCompletionDialogue) {
                return;
            }

            if (_definition == null || string.IsNullOrWhiteSpace(evt.NpcId)) {
                return;
            }

            if (!MatchesDeliveryPoint(evt.NpcId)) {
                return;
            }

            Debug.Log($"[CollectAndDeliverMission] Completion dialogue finished with '{evt.NpcId}'. Completing mission!");
            _awaitingCompletionDialogue = false;
            Complete();
        }

        private bool MatchesDeliveryPoint(string npcId) {
            if (_definition == null || _definition.DeliveryPointIds == null) {
                return false;
            }

            for (int i = 0; i < _definition.DeliveryPointIds.Length; i++) {
                string deliveryPointId = _definition.DeliveryPointIds[i];
                if (string.IsNullOrWhiteSpace(deliveryPointId)) {
                    continue;
                }

                string[] parts = deliveryPointId.Split(new char[] { ',', ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string partRaw in parts) {
                    string part = partRaw.Trim();
                    if (string.IsNullOrEmpty(part)) {
                        continue;
                    }

                    if (string.Equals(part, npcId, System.StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }

                    int deliverySuffixIndex = npcId.LastIndexOf('_');
                    string npcSuffix = deliverySuffixIndex >= 0 && deliverySuffixIndex < npcId.Length - 1 ? npcId.Substring(deliverySuffixIndex + 1) : npcId;
                    if (string.Equals(part, npcSuffix, System.StringComparison.OrdinalIgnoreCase)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Unsubscribe<ItemDeliveredEvent>(OnItemDelivered);
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
                _subscribed = false;
            }

            base.Cleanup();
        }
    }
}
