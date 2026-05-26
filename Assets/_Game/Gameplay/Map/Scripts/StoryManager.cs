// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using System.Collections;
using FifthSemester.Doors;
using FifthSemester.Gameplay.Missions;
using FifthSemester.Gameplay.Props;
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    public class StoryManager : MonoBehaviour {
        [SerializeField] private MissionSequenceSO _storySequence;
        private IMissionService _missionService;
        private IMapService _registry;
        private IEventBus _eventBus;

        private void Start() {
            _missionService = ServiceLocator.Get<IMissionService>();
            _registry = ServiceLocator.Get<IMapService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
                _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            }
            if (_missionService != null) {
                _missionService.StartSequence(_storySequence);
            }
            ApplyMissionEffects();
            StartCoroutine(ApplyMissionEffectsNextFrame());
        }

        private void OnDestroy() {
            if (_eventBus == null) return;
            _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            ApplyMissionEffects();
        }

        private IEnumerator ApplyMissionEffectsNextFrame() {
            yield return null;
            ApplyMissionEffects();
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
