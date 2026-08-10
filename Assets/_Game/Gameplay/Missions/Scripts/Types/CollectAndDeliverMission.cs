using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public class CollectAndDeliverMission : MissionBase {
        private bool _itemsCollected;
        private bool _subscribed;
<<<<<<< HEAD
        private IMissionService _missionService;
        private bool _awaitingCompletionDialogue;
=======
>>>>>>> origin/main

        // NOVO: Contador específico para as entregas
        private int _deliveredCount;

        private int RequiredCollectCount => _definition != null && _definition.CollectCount > 0 ? _definition.CollectCount : 1;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);
<<<<<<< HEAD
            ServiceLocator.TryGet<IMissionService>(out _missionService);

            int current = GetProgressCount();
            _itemsCollected = current >= RequiredCollectCount;
            _deliveredCount = _itemsCollected ? current : 0;
=======

            int current = GetProgressCount();
            _itemsCollected = current >= RequiredCollectCount;
>>>>>>> origin/main

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
<<<<<<< HEAD
            // Always initialize door state to the default (only the first delivery door unlocked),
            // then apply delivered count if there are already delivered items recorded.
            _missionService?.UpdateCollectAndDeliverDoorState(_definition, -1);
            if (_itemsCollected) {
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);
            }
=======
>>>>>>> origin/main

            if (_eventBus != null && !_subscribed) {
                _eventBus.Subscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Subscribe<ItemDeliveredEvent>(OnItemDelivered);
<<<<<<< HEAD
                _eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
=======
>>>>>>> origin/main
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

<<<<<<< HEAD
=======
            Debug.Log($"Item coletado: {evt.ItemName}. Verificando progresso...");
>>>>>>> origin/main
            Debug.Log($"Texto Progress: {_progress}");

            int current = GetProgressCount() + 1;
            _itemsCollected = current >= RequiredCollectCount;

            if (_itemsCollected) {
<<<<<<< HEAD
                _deliveredCount = 0;
                _progress = $"Entregues: 0/{RequiredCollectCount}";
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);
                _missionService?.PlayMissionCompleteSFX();
=======
                _progress = $"Entregues: 0/{RequiredCollectCount}";
>>>>>>> origin/main
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

<<<<<<< HEAD
            bool deliveryPointMatches = System.Array.Exists(_definition.DeliveryPointIds, id => {
                if (string.IsNullOrWhiteSpace(id)) return false;
                string[] parts = id.Split(new char[] { ',', ';', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string partRaw in parts) {
                    string part = partRaw.Trim();
                    if (string.IsNullOrEmpty(part)) continue;

                    // direct match
                    if (string.Equals(part, evt.DeliveryPointId, System.StringComparison.OrdinalIgnoreCase)) return true;

                    // match by suffix (e.g., DeliveryPointId 'Delivery_A' vs token 'A')
                    int us = evt.DeliveryPointId.LastIndexOf('_');
                    string evtSuffix = us >= 0 && us < evt.DeliveryPointId.Length - 1 ? evt.DeliveryPointId.Substring(us + 1) : evt.DeliveryPointId;
                    if (string.Equals(part, evtSuffix, System.StringComparison.OrdinalIgnoreCase)) return true;
                }

                return false;
            });
=======
            bool deliveryPointMatches = System.Array.Exists(_definition.DeliveryPointIds, id => id == evt.DeliveryPointId);
>>>>>>> origin/main
            bool itemMatches = evt.DeliveredItemId == _definition.CollectItemName;

            if (deliveryPointMatches && itemMatches) {
                _deliveredCount++; 
                _progress = $"Entregues: {_deliveredCount}/{RequiredCollectCount}";
<<<<<<< HEAD
                _missionService?.UpdateCollectAndDeliverDoorState(_definition, _deliveredCount);
=======
>>>>>>> origin/main

                SaveProgress();
                PublishProgress();

                if (_deliveredCount >= RequiredCollectCount) {
<<<<<<< HEAD
                    _awaitingCompletionDialogue = true;
=======
                    Complete();
>>>>>>> origin/main
                }
            }
        }

<<<<<<< HEAD
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

=======
>>>>>>> origin/main
        public override void Cleanup() {
            if (_eventBus != null && _subscribed) {
                _eventBus.Unsubscribe<ItemPickedUpEvent>(OnItemAdded);
                _eventBus.Unsubscribe<ItemDeliveredEvent>(OnItemDelivered);
<<<<<<< HEAD
                _eventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
=======
>>>>>>> origin/main
                _subscribed = false;
            }

            base.Cleanup();
        }
    }
}
