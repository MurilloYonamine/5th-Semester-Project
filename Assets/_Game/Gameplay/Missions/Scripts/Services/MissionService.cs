// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Doors;
using FifthSemester.Gameplay.Map;

namespace FifthSemester.Gameplay.Missions {
    public class MissionService : MonoBehaviour, IMissionService {
        private const float START_FADE_DURATION = 1f;
        [SerializeField] private MissionSequenceSO _defaultSequence;
        [SerializeField] private AudioClip _missionCompleteSFX;

        private IEventBus _eventBus;
        private IFadeService _fadeService;
        private ISaveService _saveService;
        private IAudioService _audioService;
        private IMapService _mapService;
        private IMission _currentMission;
        private MissionSequenceSO _activeSequence;
        private int _sequenceIndex = -1;
        private MissionDefinition _currentDefinition;
        public int CurrentIndex { get; private set; } = -1;
        private void Awake() {
            ServiceLocator.Register<IMissionService>(this);
            
            _eventBus = ServiceLocator.Get<IEventBus>();
            _saveService = ServiceLocator.Get<ISaveService>();
            ServiceLocator.TryGet<IAudioService>(out _audioService);
            ServiceLocator.TryGet<IMapService>(out _mapService);
        }

        private void Start() {
    #if UNITY_EDITOR
                int startIndex = 0;
    #else
                SaveData saveData = _saveService?.LoadFromSlot("default");
                int startIndex = saveData?.CurrentMissionIndex ?? 0;
    #endif
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
                PlayStartFadeIfNeeded(mission);
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
                PlayStartFadeIfNeeded(def);
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
            PlayMissionCompleteSFX();
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

        private void PlayMissionCompleteSFX() {
            if (_missionCompleteSFX == null) return;
            if (_audioService == null) ServiceLocator.TryGet<IAudioService>(out _audioService);
            _audioService?.PlaySFX(clip: _missionCompleteSFX);
        }

        void IMissionService.PlayMissionCompleteSFX() {
            PlayMissionCompleteSFX();
        }

        void IMissionService.UpdateCollectAndDeliverDoorState(MissionDefinition definition, int deliveredCount) {
            UpdateCollectAndDeliverDoorState(definition, deliveredCount);
        }

        private void UpdateCollectAndDeliverDoorState(MissionDefinition definition, int deliveredCount) {
            if (definition == null || definition.DeliveryPointIds == null || definition.DeliveryPointIds.Length == 0) {
                return;
            }

            if (_mapService == null) {
                ServiceLocator.TryGet<IMapService>(out _mapService);
            }

            if (_mapService == null) {
                return;
            }

            List<DoorType> orderedDoors = new List<DoorType>();

            for (int i = 0; i < definition.DeliveryPointIds.Length; i++) {
                DoorType doorType = GetDoorTypeForDeliveryPoint(definition.DeliveryPointIds[i]);
                if (doorType == DoorType.None || orderedDoors.Contains(doorType)) {
                    continue;
                }

                orderedDoors.Add(doorType);
            }

            if (orderedDoors.Count == 0) {
                return;
            }

            if (orderedDoors.Count == 0) {
                return;
            }

            if (deliveredCount < 0) {
                for (int i = 0; i < orderedDoors.Count; i++) {
                    GameObject doorObject = _mapService.Get(orderedDoors[i]);
                    Door door = doorObject != null ? doorObject.GetComponent<Door>() : null;
                    door?.Lock();
                }

                GameObject firstDoorObj = _mapService.Get(orderedDoors[0]);
                Door firstDoor = firstDoorObj != null ? firstDoorObj.GetComponent<Door>() : null;
                firstDoor?.Unlock();

                GameObject corredorObjInit = _mapService.Get(DoorType.Door_Corredor);
                Door corredorInit = corredorObjInit != null ? corredorObjInit.GetComponent<Door>() : null;
                corredorInit?.Lock();

                return;
            }

            for (int i = 0; i < orderedDoors.Count; i++) {
                GameObject doorObject = _mapService.Get(orderedDoors[i]);
                Door door = doorObject != null ? doorObject.GetComponent<Door>() : null;
                if (door == null) {
                    continue;
                }

                if (i <= deliveredCount) {
                    door.Unlock();
                }
                else {
                    door.Lock();
                }
            }

            GameObject corredorObj = _mapService.Get(DoorType.Door_Corredor);
            Door corredor = corredorObj != null ? corredorObj.GetComponent<Door>() : null;
            if (deliveredCount >= orderedDoors.Count) {
                corredor?.Unlock();
            }
            else {
                corredor?.Lock();
            }
        }

        private static DoorType GetDoorTypeForDeliveryPoint(string deliveryPointId) {
            if (string.IsNullOrWhiteSpace(deliveryPointId)) {
                return DoorType.None;
            }

            string suffix = deliveryPointId;
            int underscoreIndex = deliveryPointId.LastIndexOf('_');
            if (underscoreIndex >= 0 && underscoreIndex < deliveryPointId.Length - 1) {
                suffix = deliveryPointId.Substring(underscoreIndex + 1);
            }

            return suffix.ToUpperInvariant() switch {
                "A" => DoorType.Door_RoomA,
                "B" => DoorType.Door_RoomB,
                "C" => DoorType.Door_RoomC,
                "E" => DoorType.Door_RoomE,
                "P" => DoorType.Door_RoomP,
                _ => DoorType.None
            };
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
                    }
                    else {
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

        private void PlayStartFadeIfNeeded(MissionDefinition mission) {
            if (mission == null || !mission.UseFadeOnStart) return;

            if (_fadeService == null) {
                ServiceLocator.TryGet<IFadeService>(out _fadeService);
            }

            if (_fadeService == null) return;

            _fadeService.FadeIn(START_FADE_DURATION);
        }

        public void StartSequence(MissionSequenceSO sequence) {
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
