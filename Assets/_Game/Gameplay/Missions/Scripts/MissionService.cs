// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class MissionService : MonoBehaviour, IMissionService {
        [SerializeField] private MissionDefinition[] _missionDefinitions;

        private IEventBus _eventBus;
        private ISaveService _saveService;
        private IMission _currentMission;

        public MissionDefinition[] Missions => _missionDefinitions;
        public int CurrentIndex { get; private set; } = -1;

        private void Awake() {
            ServiceLocator.Register<IMissionService>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();
            _saveService = ServiceLocator.Get<ISaveService>();
        }

        private void Start() {
            SaveData saveData = _saveService?.LoadFromSlot("default");
            int startIndex = saveData?.CurrentMissionIndex ?? 0;

            if (_missionDefinitions != null && _missionDefinitions.Length > 0) {
                SetCurrentMission(startIndex);
            }
        }

        private void OnDestroy() {
            CleanupCurrentMission();
        }

        public MissionDefinition GetCurrentMission() {
            if (_missionDefinitions == null || CurrentIndex < 0 || CurrentIndex >= _missionDefinitions.Length) return null;
            return _missionDefinitions[CurrentIndex];
        }

        private void SetCurrentMission(int index) {
            if (_missionDefinitions == null || index < 0 || index >= _missionDefinitions.Length) return;

            CleanupCurrentMission();

            CurrentIndex = index;
            MissionDefinition def = _missionDefinitions[index];
            _currentMission = MissionFactory.CreateMission(def);

            if (_currentMission != null) {
                _currentMission.Initialize(def, _eventBus, _saveService);
                if (_currentMission is MissionBase missionBase) {
                    missionBase.OnMissionComplete += OnMissionComplete;
                }
                _currentMission.StartMission();
                PublishMissionUpdate();
            }
        }

        private void CleanupCurrentMission() {
            if (_currentMission == null) return;

            UnityEngine.Object missionObject = _currentMission as UnityEngine.Object;
            if (missionObject == null) {
                _currentMission = null;
                return;
            }

            if (_currentMission is MissionBase missionBase) {
                missionBase.OnMissionComplete -= OnMissionComplete;
            }

            _currentMission.Cleanup();
            _currentMission = null;
        }

        private void OnMissionComplete() {
            CompleteCurrentMission();
        }

        public void CompleteCurrentMission() {
            int next = CurrentIndex + 1;
            if (_missionDefinitions == null) return;

            if (next >= _missionDefinitions.Length) {
                Debug.Log("[MissionService] All missions completed.");
                SaveGameState();
                return;
            }

            SaveGameState();
            SetCurrentMission(next);
        }

        public void SkipToMission(int missionIndex) {
            if (_missionDefinitions == null || missionIndex < 0 || missionIndex >= _missionDefinitions.Length) {
                Debug.LogWarning($"[MissionService] Invalid mission index: {missionIndex}");
                return;
            }

            for (int i = 0; i <= missionIndex; i++) {
                MissionDefinition def = _missionDefinitions[i];
                if (def == null || def.DebugSetupEvents == null) continue;

                foreach (string debugEvent in def.DebugSetupEvents) {
                    if (string.IsNullOrWhiteSpace(debugEvent)) continue;

                    if (debugEvent.StartsWith("Item:", System.StringComparison.OrdinalIgnoreCase)) {
                        string itemName = debugEvent.Substring(5);
                        _eventBus?.Publish(new ItemPickedUpEvent(itemName, null));
                    } else {
                        _eventBus?.Publish(new GenericGameEvent(debugEvent));
                    }
                }
            }

            SetCurrentMission(missionIndex);
        }

        private void PublishMissionUpdate() {
            MissionDefinition current = GetCurrentMission();
            if (current != null) {
                _eventBus?.Publish(new MissionUpdatedEvent(current.MissionId, current.Title, current.Description));
            }
        }

        private void SaveGameState() {
            if (_saveService == null) return;

            SaveData saveData = _saveService.LoadFromSlot("default") ?? new SaveData();
            saveData.CurrentMissionIndex = CurrentIndex;
            _saveService.SaveToSlot("default", saveData);
        }
    }
}
