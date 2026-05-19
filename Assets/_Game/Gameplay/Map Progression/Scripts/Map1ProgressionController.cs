using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Doors;
using FifthSemester.Gameplay.Missions;
using FifthSemester.Gameplay.Dialogue; 
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class Map1ProgressionController : MonoBehaviour {
        [Header("Referências do Mapa")]
        [SerializeField] private OpeningCutscene _openingCutscene; 
        [SerializeField] private GameObject _medsOnTable;
        [SerializeField] private Door _doorMedsRoom;
        [SerializeField] private Door[] _patientDoors;
        [SerializeField] private GameObject _jumpscareTriggerEvent5;

        [Header("Referências de Missão (SOs)")]
        [SerializeField] private MissionDefinition _mission01_TalkToNurse;
        [SerializeField] private MissionDefinition _mission02_CollectMeds;
        [SerializeField] private MissionDefinition _mission03_MedicatePatients;

        private IEventBus _eventBus;
        private IMissionService _missionService;
        private bool _openingCutscenePlayed;

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _missionService = ServiceLocator.Get<IMissionService>();

            _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            _eventBus.Subscribe<ItemDeliveredEvent>(OnItemDelivered);

            _doorMedsRoom.Lock();
            LockAllPatientDoors();
            _medsOnTable.SetActive(false);
            _jumpscareTriggerEvent5.SetActive(false);

            SyncMapWithCurrentMission();
            TryPlayOpeningCutscene();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
                _eventBus.Unsubscribe<ItemDeliveredEvent>(OnItemDelivered);
            }
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            SyncMapWithCurrentMission();
            TryPlayOpeningCutscene();
        }

        private void SyncMapWithCurrentMission() {
            MissionDefinition currentMission = _missionService.GetCurrentMission();
            if (currentMission != null) {
                ApplyMapState(currentMission);
            }
        }

        private void TryPlayOpeningCutscene() {
            if (_openingCutscenePlayed || _openingCutscene == null || _missionService == null) return;
            if (_missionService.GetCurrentMission() != _mission01_TalkToNurse) return;

            _openingCutscenePlayed = true;
            _openingCutscene.PlayCutscene();
        }

        private void ApplyMapState(MissionDefinition currentDef) {
            if (currentDef == _mission01_TalkToNurse) {
                _doorMedsRoom.Lock();
                _medsOnTable.SetActive(false);
                LockAllPatientDoors();
                _jumpscareTriggerEvent5.SetActive(false);
            }
            else if (currentDef == _mission02_CollectMeds) {
                _doorMedsRoom.Unlock();
                _medsOnTable.SetActive(true);
            }
            else if (currentDef == _mission03_MedicatePatients) {
                UnlockAllPatientDoors();
            }
        }

        private void OnItemDelivered(ItemDeliveredEvent evt) {
            if (evt.DeliveryPointId == "Paciente_B") {
                _jumpscareTriggerEvent5.SetActive(true);
            }
        }

        private void LockAllPatientDoors() {
            foreach (var door in _patientDoors) door.Lock();
        }

        private void UnlockAllPatientDoors() {
            foreach (var door in _patientDoors) door.Unlock();
        }
    }
}
