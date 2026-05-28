using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Enemy;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2KeyService : MonoBehaviour, IMap2KeyService {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector _allKeysCollectedTimeline;

        [Header("Trigger Key Configuration")]
        [Tooltip("A chave específica que, ao ser coletada, dispara a cutscene e libera o estado das chaves.")]
        [SerializeField] private Map2KeyDefinitionSO _triggerKeyDefinition;

        private List<Map2KeyItem> _registeredKeys = new List<Map2KeyItem>(); // Mantido para compatibilidade de assinatura, mas não utilizado
        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private bool _played;

        public bool HasCollectedAllKeys { get; private set; }

        private void Awake() {
            ServiceLocator.Register<IMap2KeyService>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
        }

        private void Start() {
            _eventBus?.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
            ServiceLocator.TryGet<IMap2KeyService>(out var dummy);
        }

        public void RegisterKey(Map2KeyItem key) {
            // Ignorado intencionalmente para evitar bugs com GameObjects inativos
        }

        public void UnregisterKey(Map2KeyItem key) {
            // Ignorado intencionalmente
        }

        private void OnItemPickedUp(ItemPickedUpEvent evt) {
            if (_played) {
                Debug.Log("[Map2KeyService] OnItemPickedUp: Event ignored because cutscene/all keys completed was already triggered (_played is true).");
                return;
            }
            if (evt.ItemGameObject == null) {
                Debug.LogWarning("[Map2KeyService] OnItemPickedUp: Event received but ItemGameObject is null!");
                return;
            }

            Map2KeyItem picked = evt.ItemGameObject.GetComponent<Map2KeyItem>();
            if (picked == null) {
                Debug.Log($"[Map2KeyService] OnItemPickedUp: Item '{evt.ItemGameObject.name}' is not a Map2KeyItem. Ignoring.");
                return;
            }

            Debug.Log($"[Map2KeyService] OnItemPickedUp: Key '{picked.name}' (Definition: {(picked.KeyDefinition != null ? picked.KeyDefinition.name : "None")}) was picked up.");

            // Se uma chave gatilho foi configurada e esta chave corresponde a ela
            if (_triggerKeyDefinition != null) {
                if (picked.KeyDefinition == _triggerKeyDefinition) {
                    Debug.Log($"[Map2KeyService] MATCH DETECTED! Key '{picked.name}' matches Trigger Key Definition '{_triggerKeyDefinition.name}'. Starting cutscene sequence.");
                    TriggerKeysCompleted(picked);
                } else {
                    Debug.Log($"[Map2KeyService] Key '{picked.name}' does NOT match Trigger Key Definition '{_triggerKeyDefinition.name}'. Waiting for the correct trigger key.");
                }
            } else {
                Debug.LogWarning("[Map2KeyService] OnItemPickedUp: Trigger Key Definition is NOT configured in the inspector! Cannot match keys.");
            }
        }

        public bool TryPrepareForLastKey(Map2KeyItem lastKey) {
            if (_played) {
                Debug.Log("[Map2KeyService] TryPrepareForLastKey: Ignored because _played is true.");
                return false;
            }
            if (_inventoryService == null) {
                Debug.LogWarning("[Map2KeyService] TryPrepareForLastKey: Inventory service is null!");
                return false;
            }
            if (lastKey == null) {
                Debug.LogWarning("[Map2KeyService] TryPrepareForLastKey: Provided lastKey is null!");
                return false;
            }

            Debug.Log($"[Map2KeyService] TryPrepareForLastKey: Checking key '{lastKey.name}' (Definition: {(lastKey.KeyDefinition != null ? lastKey.KeyDefinition.name : "None")}).");

            // Só preparamos se for a chave gatilho configurada
            if (_triggerKeyDefinition != null && lastKey.KeyDefinition == _triggerKeyDefinition) {
                Debug.Log($"[Map2KeyService] TryPrepareForLastKey: MATCH! Key '{lastKey.name}' is the trigger key. Cleaning up inventory of other keys.");
                
                // Limpa as chaves anteriores do inventário
                var items = _inventoryService.GetItems();
                if (items != null) {
                    List<Map2KeyItem> keysInInventory = new List<Map2KeyItem>();
                    for (int i = 0; i < items.Count; i++) {
                        if (items[i] is Map2KeyItem keyItem && keyItem != lastKey) {
                            keysInInventory.Add(keyItem);
                        }
                    }

                    Debug.Log($"[Map2KeyService] TryPrepareForLastKey: Found {keysInInventory.Count} other keys in inventory to clean up.");
                    for (int i = 0; i < keysInInventory.Count; i++) {
                        Debug.Log($"[Map2KeyService] TryPrepareForLastKey: Removing and disabling auxiliary key '{keysInInventory[i].name}' from inventory.");
                        _inventoryService.RemoveItem(keysInInventory[i]);
                        if (keysInInventory[i] != null) {
                            keysInInventory[i].gameObject.SetActive(false);
                        }
                    }
                }

                _played = true;
                HasCollectedAllKeys = true;
                DeactivateNurse();

                if (_allKeysCollectedTimeline != null) {
                    Debug.Log("[Map2KeyService] TryPrepareForLastKey: Playing 'all keys collected' cutscene timeline.");
                    try { _allKeysCollectedTimeline.Play(); } catch (System.Exception e) { Debug.LogError($"[Map2KeyService] Error playing timeline: {e}"); }
                } else {
                    Debug.LogWarning("[Map2KeyService] TryPrepareForLastKey: 'All Keys Collected Timeline' PlayableDirector is NOT assigned in the inspector!");
                }
                
                return true;
            }

            return false;
        }

        private void TriggerKeysCompleted(Map2KeyItem pickedKey) {
            Debug.Log($"[Map2KeyService] TriggerKeysCompleted: Initializing key collection completion sequence with key '{pickedKey.name}'.");
            
            if (_inventoryService == null) {
                Debug.LogWarning("[Map2KeyService] TriggerKeysCompleted: Inventory service is null, skipping inventory cleanup.");
            } else {
                var items = _inventoryService.GetItems();
                if (items != null) {
                    // Limpa as chaves anteriores, mantendo apenas a chave gatilho recém-adquirida
                    List<Map2KeyItem> keysToRemove = new List<Map2KeyItem>();
                    for (int i = 0; i < items.Count; i++) {
                        if (items[i] is Map2KeyItem keyItem && keyItem != pickedKey) {
                            keysToRemove.Add(keyItem);
                        }
                    }

                    Debug.Log($"[Map2KeyService] TriggerKeysCompleted: Found {keysToRemove.Count} auxiliary keys to clean up from inventory.");
                    for (int i = 0; i < keysToRemove.Count; i++) {
                        Map2KeyItem keyToRemove = keysToRemove[i];
                        Debug.Log($"[Map2KeyService] TriggerKeysCompleted: Removing and disabling auxiliary key '{keyToRemove.name}' from inventory.");
                        _inventoryService.RemoveItem(keyToRemove);
                        if (keyToRemove != null) {
                            keyToRemove.gameObject.SetActive(false);
                        }
                    }
                }
            }

            _played = true;
            HasCollectedAllKeys = true;
            DeactivateNurse();

            if (_allKeysCollectedTimeline != null) {
                Debug.Log("[Map2KeyService] TriggerKeysCompleted: Playing 'all keys collected' cutscene timeline.");
                try { _allKeysCollectedTimeline.Play(); } catch (System.Exception e) { Debug.LogError($"[Map2KeyService] Error playing timeline: {e}"); }
            } else {
                Debug.LogWarning("[Map2KeyService] TriggerKeysCompleted: 'All Keys Collected Timeline' PlayableDirector is NOT assigned in the inspector!");
            }
        }

        private void DeactivateNurse() {
            var nurses = FindObjectsOfType<Nurse>();
            Debug.Log($"[Map2KeyService] DeactivateNurse: Searching for nurses in scene. Found {nurses.Length} nurse instances.");
            foreach (var nurse in nurses) {
                if (nurse != null) {
                    nurse.gameObject.SetActive(false);
                    Debug.Log($"[Map2KeyService] Enfermeira '{nurse.name}' desativada com sucesso após a coleta da chave gatilho.");
                }
            }
        }
    }
}
