// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using System.Collections;
using FifthSemester.Doors;


using UnityEngine;

namespace FifthSemester.Gameplay {
    public class StoryManager : MonoBehaviour {
        private const string TAG = "<color=yellow><b>[StoryManager]</b></color>";
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

            if (_missionService != null && _missionService.CurrentIndex < 0 && !SaveLoader.IsPendingSave) {
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

            int currentIndex = _missionService.CurrentIndex;
            if (_storySequence == null || _storySequence.Sequence == null) return;

            int limit = currentIndex >= 0 ? currentIndex : 0;

            for (int i = 0; i <= limit && i < _storySequence.Sequence.Count; i++) {
                var mission = _storySequence.Sequence[i];
                if (mission == null || mission.MapActions == null) continue;

                bool skipDoorLock = string.Equals(mission.Type.ToString(), "TalkToNpc", System.StringComparison.Ordinal)
                                 || string.Equals(mission.Type.ToString(), "CollectAndDeliver", System.StringComparison.Ordinal);

                foreach (var action in mission.MapActions) {
                    if (skipDoorLock &&
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
