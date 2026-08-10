// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
<<<<<<< HEAD
using System.Collections;
using FifthSemester.Doors;
using FifthSemester.Gameplay.Missions;
using FifthSemester.Gameplay.Props;
=======
using FifthSemester.Doors;
using FifthSemester.Gameplay.Missions;
>>>>>>> origin/main
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    public class StoryManager : MonoBehaviour {
        [SerializeField] private MissionSequenceSO _storySequence;
<<<<<<< HEAD
=======

>>>>>>> origin/main
        private IMissionService _missionService;
        private IMapService _registry;
        private IEventBus _eventBus;

<<<<<<< HEAD
=======
        private int _currentSequenceIndex = 0;

>>>>>>> origin/main
        private void Start() {
            _missionService = ServiceLocator.Get<IMissionService>();
            _registry = ServiceLocator.Get<IMapService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
<<<<<<< HEAD
                _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            }
            if (_missionService != null) {
                _missionService.StartSequence(_storySequence);
            }
            ApplyMissionEffects();
            StartCoroutine(ApplyMissionEffectsNextFrame());
=======
                _eventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
                _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            }

            StartNextMission();
            ApplyMissionEffects();
>>>>>>> origin/main
        }

        private void OnDestroy() {
            if (_eventBus == null) return;
<<<<<<< HEAD
=======

            _eventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
>>>>>>> origin/main
            _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            ApplyMissionEffects();
        }

<<<<<<< HEAD
        private IEnumerator ApplyMissionEffectsNextFrame() {
            yield return null;
            ApplyMissionEffects();
        }

        private void ApplyMissionEffects() {
            if (_missionService == null || _registry == null) return;

            int currentIndex = _missionService.CurrentIndex;
            if (_storySequence == null || _storySequence.Sequence == null) return;

            int limit = currentIndex >= 0 ? currentIndex : 0;

            for (int i = 0; i <= limit && i < _storySequence.Sequence.Count; i++) {
                var mission = _storySequence.Sequence[i];
                if (mission == null || mission.MapActions == null) continue;

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

                    Gate gate = targetObj.GetComponent<Gate>();

                    switch (action.Type) {
                        case MapAction.ActionType.Activate:
                            if (gate != null) {
                                gate.Unlock();
                            }
                            else {
                                targetObj.SetActive(true);
                            }
                            break;
                        case MapAction.ActionType.Deactivate:
                            if (gate != null) {
                                gate.Lock();
                            }
                            else {
                                targetObj.SetActive(false);
                            }
                            break;
                        case MapAction.ActionType.LockDoor:
                            targetObj.GetComponent<Door>()?.Lock();
                            break;
                        case MapAction.ActionType.UnlockDoor:
                            targetObj.GetComponent<Door>()?.Unlock();
                            break;
                    }
=======
        private void OnMissionCompleted(MissionCompletedEvent evt) {
            _currentSequenceIndex++;
            StartNextMission();
            ApplyMissionEffects();
        }

        private void StartNextMission() {
            if (_storySequence == null || _storySequence.Sequence == null) return;

            if (_currentSequenceIndex < _storySequence.Sequence.Count) {
                MissionDefinition nextMission = _storySequence.Sequence[_currentSequenceIndex];
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
>>>>>>> origin/main
                }
            }
        }

        private void LockAllDoorsExcept(DoorType[] doorsToKeepUnlocked) {
            foreach (DoorType doorType in System.Enum.GetValues(typeof(DoorType))) {
                if (doorType == DoorType.None) continue;

                GameObject doorObj = _registry.Get(doorType);

                if (doorObj == null) {
                    continue;
                }

                bool shouldUnlock = false;
                foreach (var doorToKeep in doorsToKeepUnlocked) {
                    if (doorType == doorToKeep) {
                        shouldUnlock = true;
                        break;
                    }
                }

                if (shouldUnlock) {
                    doorObj.GetComponent<Door>()?.Unlock();
                }
                else {
                    doorObj.GetComponent<Door>()?.Lock();
                }
            }
        }
    }
}
