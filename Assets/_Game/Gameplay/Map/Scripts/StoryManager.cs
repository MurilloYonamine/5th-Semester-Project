// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Doors;
using FifthSemester.Gameplay.Missions;
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    public class StoryManager : MonoBehaviour {
        [SerializeField] private MissionSequenceSO _storySequence;

        private IMissionService _missionService;
        private IMapService _registry;
        private IEventBus _eventBus;

        private int _currentSequenceIndex = 0;

        private void Start() {
            _missionService = ServiceLocator.Get<IMissionService>();
            _registry = ServiceLocator.Get<IMapService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
                _eventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
                _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            }

            StartNextMission();
            ApplyMissionEffects();
        }

        private void OnDestroy() {
            if (_eventBus == null) return;

            _eventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
            _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            ApplyMissionEffects();
        }

        private void OnMissionCompleted(MissionCompletedEvent evt) {
            _currentSequenceIndex++;
            StartNextMission();
            ApplyMissionEffects();
        }

        private void StartNextMission() {
            if (_storySequence == null || _storySequence.Sequence == null) return;

            if (_currentSequenceIndex < _storySequence.Sequence.Count) {
                var nextMission = _storySequence.Sequence[_currentSequenceIndex];
                if (nextMission == null) return;

                _missionService.StartMission(nextMission);
            }
        }

        private void ApplyMissionEffects() {
            if (_missionService == null || _registry == null) return;

            var mission = _missionService.GetCurrentMission();
            if (mission == null || mission.MapActions == null) return;

            bool skipDoorLockForTalkToNpc = string.Equals(mission.Type.ToString(), "TalkToNpc", System.StringComparison.Ordinal);

            foreach (var action in mission.MapActions) {
                if (skipDoorLockForTalkToNpc &&
                    (action.Type == MapAction.ActionType.LockDoor || action.Type == MapAction.ActionType.LockAllDoorsExcept)) {
                    continue;
                }

                if (action.Type == MapAction.ActionType.LockAllDoorsExcept) {
                    LockAllDoorsExcept(action.DoorsToKeepUnlocked);
                    continue;
                }

                GameObject targetObj = null;

                if (action.Type == MapAction.ActionType.LockDoor || action.Type == MapAction.ActionType.UnlockDoor) {
                    targetObj = _registry.Get(action.TargetDoor);
                }
                else {
                    targetObj = _registry.Get(action.TargetObjectId);
                }

                if (targetObj == null) continue;

                switch (action.Type) {
                    case MapAction.ActionType.Activate:
                        targetObj.SetActive(true);
                        break;
                    case MapAction.ActionType.Deactivate:
                        targetObj.SetActive(false);
                        break;
                    case MapAction.ActionType.LockDoor:
                        targetObj.GetComponent<Door>()?.Lock();
                        break;
                    case MapAction.ActionType.UnlockDoor:
                        targetObj.GetComponent<Door>()?.Unlock();
                        break;
                }
            }
        }

        private void LockAllDoorsExcept(DoorType[] doorsToKeepUnlocked) {
            foreach (DoorType doorType in System.Enum.GetValues(typeof(DoorType))) {
                if (doorType == DoorType.None) continue;

                GameObject doorObj = _registry.Get(doorType);
                if (doorObj == null) continue;
                doorObj.GetComponent<Door>()?.Lock();
            }

            if (doorsToKeepUnlocked == null || doorsToKeepUnlocked.Length == 0) return;

            for (int i = 0; i < doorsToKeepUnlocked.Length; i++) {
                if (doorsToKeepUnlocked[i] == DoorType.None) continue;

                GameObject doorObj = _registry.Get(doorsToKeepUnlocked[i]);
                if (doorObj == null) continue;

                doorObj.GetComponent<Door>()?.Unlock();
            }
        }
    }
}
