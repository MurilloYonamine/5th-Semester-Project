// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public class MissionService : MonoBehaviour, IMissionService {
        [SerializeField] private MissionSequenceSO _defaultSequence;

        private IEventBus _eventBus;
        private ISaveService _saveService;
        private IMission _currentMission;
        private MissionSequenceSO _activeSequence;
        private int _sequenceIndex = -1;
        private MissionDefinition _currentDefinition;
        public int CurrentIndex { get; private set; } = -1; 
        private void Awake() {
            ServiceLocator.Register<IMissionService>(this);
            EnsureServices();
        }

        private void Start() {
            EnsureServices();

            SaveData saveData = _saveService?.LoadFromSlot("default");
            int startIndex = saveData?.CurrentMissionIndex ?? 0;
            if (_defaultSequence != null) {
                StartSequence(_defaultSequence);
                if (startIndex > 0) {
                    SkipToMission(startIndex);
                }
            }
        }

        private void OnDestroy() {
            CleanupCurrentMission();
        }

        public void StartMission(MissionDefinition mission) {
            EnsureServices();

            if (mission == null) {
                Debug.LogError("[MissionService] Tentativa de iniciar uma missão nula.");
                return;
            }

            if (_activeSequence != null && _activeSequence.Sequence != null) {
                int idx = _activeSequence.Sequence.IndexOf(mission);
                if (idx == -1) {
                    Debug.LogWarning("[MissionService] Mission not found in active sequence, starting standalone.");
                    StartStandaloneMission(mission);
                    return;
                }

                SetCurrentMission(idx);
                return;
            }

            StartStandaloneMission(mission);
        }

        private void StartStandaloneMission(MissionDefinition mission) {
            CleanupCurrentMission();
            CurrentIndex = -1;
            _currentDefinition = mission;
            _currentMission = MissionFactory.CreateMission(mission);
            if (_currentMission != null) {
                _currentMission.Initialize(mission, _eventBus, _saveService);
                if (_currentMission is MissionBase missionBase) missionBase.OnMissionComplete += OnMissionComplete;
                _currentMission.StartMission();
                PublishMissionUpdate();
            }
        }

        public MissionDefinition GetCurrentMission() {
            if (_currentDefinition != null) return _currentDefinition;
            if (_activeSequence != null && _sequenceIndex >= 0 && _sequenceIndex < _activeSequence.Sequence.Count) {
                return _activeSequence.Sequence[_sequenceIndex];
            }
            return null;
        }

        private void SetCurrentMission(int index) {
            EnsureServices();

            if (_eventBus == null) return;
            if (_activeSequence == null || _activeSequence.Sequence == null) return;
            if (index < 0 || index >= _activeSequence.Sequence.Count) return;

            CleanupCurrentMission();

            _sequenceIndex = index;
            CurrentIndex = index;
            MissionDefinition def = _activeSequence.Sequence[index];
            _currentDefinition = def;
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
            if (_activeSequence != null && _activeSequence.Sequence != null) {
                _sequenceIndex++;
                if (_sequenceIndex >= _activeSequence.Sequence.Count) {
                    Debug.Log("[MissionService] Sequence completed.");
                    SaveGameState();
                    _activeSequence = null;
                    _sequenceIndex = -1;
                    CurrentIndex = -1;
                    _currentDefinition = null;
                    return;
                }

                SaveGameState();
                SetCurrentMission(_sequenceIndex);
                return;
            }

            Debug.Log("[MissionService] Standalone mission completed.");
            SaveGameState();
            CleanupCurrentMission();
            CurrentIndex = -1;
            _currentDefinition = null;
        }

        public void SkipToMission(int missionIndex) {
            if (_activeSequence == null || _activeSequence.Sequence == null) {
                Debug.LogWarning("[MissionService] No active sequence to skip within.");
                return;
            }

            if (missionIndex < 0 || missionIndex >= _activeSequence.Sequence.Count) {
                Debug.LogWarning($"[MissionService] Invalid mission index: {missionIndex}");
                return;
            }

            for (int i = 0; i <= missionIndex; i++) {
                MissionDefinition def = _activeSequence.Sequence[i];
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

        private void EnsureServices() {
            if (_eventBus == null) {
                _eventBus = ServiceLocator.Get<IEventBus>();
            }

            if (_saveService == null) {
                _saveService = ServiceLocator.Get<ISaveService>();
            }
        }

        public void StartSequence(MissionSequenceSO sequence) {
            EnsureServices();

            if (sequence == null || sequence.Sequence == null || sequence.Sequence.Count == 0) {
                Debug.LogWarning("[MissionService] Attempted to start an empty or null sequence.");
                return;
            }

            _activeSequence = sequence;
            _sequenceIndex = 0;
            StartMission(_activeSequence.Sequence[_sequenceIndex]);
        }

    }
}
