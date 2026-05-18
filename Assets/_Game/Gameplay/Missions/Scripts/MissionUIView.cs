// Autor: Murillo Gomes Yonamine
// Data: Atualizado para UI de Missões

using UnityEngine;
using TMPro;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay.Missions {
    public class MissionUIView : MonoBehaviour {
        [Header("Referências de UI")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private IEventBus _eventBus;
        private IMissionService _missionService;

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _missionService = ServiceLocator.Get<IMissionService>();

            _eventBus.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            _eventBus.Subscribe<MissionProgressEvent>(OnMissionProgressed);

            UpdateUI();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
                _eventBus.Unsubscribe<MissionProgressEvent>(OnMissionProgressed);
            }
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            UpdateUI();
        }

        private void OnMissionProgressed(MissionProgressEvent evt) {
            if (_progressText.gameObject.activeSelf) {
                _progressText.text = $"{evt.Current} / {evt.Required}";
            }
            UpdateUI();
        }

        private void UpdateUI() {
            var currentMission = _missionService.GetCurrentMission();

            if (currentMission != null) {
                _canvasGroup.alpha = 1f;

                _titleText.text = currentMission.Title;
                _descriptionText.text = currentMission.Description;

                if (currentMission.Type == MissionType.CollectItems || currentMission.Type == MissionType.CollectAndDeliver) {
                    _progressText.gameObject.SetActive(true);

                }
                else {
                    _progressText.gameObject.SetActive(false);
                }
            }
            else {
                _canvasGroup.alpha = 0f;
            }
        }
    }
}
