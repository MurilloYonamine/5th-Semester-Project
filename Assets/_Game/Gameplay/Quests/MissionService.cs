// Autor: Generated
// Data: 05/05/2026

using System;
using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Quests {
    public class MissionService : MonoBehaviour, IMissionService {
        [SerializeField] private MissionSO[] _missions;

        private IEventBus _eventBus;
        public MissionSO[] Missions => _missions;

        public int CurrentIndex { get; private set; } = -1;

        private void Awake() {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void Start() {
            ServiceLocator.Register<IMissionService>(this);
            // subscribe to generic events
            _eventBus?.Subscribe<GenericGameEvent>(OnGenericGameEvent);

            if (_missions != null && _missions.Length > 0) {
                SetCurrentMission(0);
            }
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<GenericGameEvent>(OnGenericGameEvent);
            // Unregister not necessary depending on ServiceLocator implementation
        }

        private void OnGenericGameEvent(GenericGameEvent evt) {
            var current = GetCurrentMission();
            if (current == null) return;

            if (!string.IsNullOrEmpty(current.CompletionEventId) && string.Equals(current.CompletionEventId, evt.Name, StringComparison.Ordinal)) {
                CompleteCurrentMission();
            }
        }

        public MissionSO GetCurrentMission() {
            if (_missions == null || CurrentIndex < 0 || CurrentIndex >= _missions.Length) return null;
            return _missions[CurrentIndex];
        }

        private void SetCurrentMission(int index) {
            CurrentIndex = index;
            var m = GetCurrentMission();
            _eventBus?.Publish(new MissionUpdatedEvent(m != null ? m.MissionId : string.Empty, m != null ? m.Title : string.Empty, m != null ? m.Description : string.Empty));
        }

        public void CompleteCurrentMission() {
            int next = CurrentIndex + 1;
            if (_missions == null) return;

            if (next >= _missions.Length) {
                // no more missions, set to end
                SetCurrentMission(_missions.Length - 1);
                Debug.Log("MissionService: All missions completed.");
                return;
            }

            SetCurrentMission(next);
        }

        public void SkipToMission(int missionIndex) {
            if (_missions == null || missionIndex < 0 || missionIndex >= _missions.Length) {
                Debug.LogWarning($"MissionService: SkipToMission index out of range: {missionIndex}");
                return;
            }

            // Apply debug setups for all missions up to and including target
            for (int i = 0; i <= missionIndex; i++) {
                var m = _missions[i];
                if (m == null || m.DebugSetupEvents == null) continue;

                foreach (var debugEvent in m.DebugSetupEvents) {
                    if (string.IsNullOrWhiteSpace(debugEvent)) continue;

                    // Simple parsing for common debug actions: Item:<name>
                    if (debugEvent.StartsWith("Item:", StringComparison.OrdinalIgnoreCase)) {
                        var itemName = debugEvent.Substring(5);
                        // publish ItemPickedUpEvent so inventory systems can react
                        _eventBus?.Publish(new Core.Events.ItemPickedUpEvent(itemName, null));
                    } else {
                        // Generic fallback: publish as generic game event
                        _eventBus?.Publish(new GenericGameEvent(debugEvent));
                    }
                }
            }

            SetCurrentMission(missionIndex);
        }
    }
}
