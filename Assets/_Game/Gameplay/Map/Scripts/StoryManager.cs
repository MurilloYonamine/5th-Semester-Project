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

        private int _currentSequenceIndex = 0;

        private void Start() {
            _missionService = ServiceLocator.Get<IMissionService>();
            _registry = ServiceLocator.Get<IMapService>();

            ServiceLocator.Get<IEventBus>().Subscribe<MissionCompletedEvent>(OnMissionCompleted);

            StartNextMission();
        }

        private void OnMissionCompleted(MissionCompletedEvent evt) {
            _currentSequenceIndex++;
            StartNextMission();
        }

        private void StartNextMission() {
            if (_currentSequenceIndex < _storySequence.Sequence.Count) {
                var nextMission = _storySequence.Sequence[_currentSequenceIndex];
                _missionService.StartMission(nextMission);
            }
            else {
                Debug.Log("História concluída!");
            }
        }
        private void ApplyMissionEffects() {
            var mission = _missionService.GetCurrentMission();
            if (mission == null || mission.MapActions == null) return;

            foreach (var action in mission.MapActions) {
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
                doorObj?.GetComponent<Door>()?.Lock();
            }

            if (doorsToKeepUnlocked == null || doorsToKeepUnlocked.Length == 0) return;

            for (int i = 0; i < doorsToKeepUnlocked.Length; i++) {
                if (doorsToKeepUnlocked[i] == DoorType.None) continue;

                GameObject doorObj = _registry.Get(doorsToKeepUnlocked[i]);
                doorObj?.GetComponent<Door>()?.Unlock();
            }
        }
    }
}
