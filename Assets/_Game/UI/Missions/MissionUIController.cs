// Autor: Generated / Updated
// Data: 05/05/2026

using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using TMPro;

namespace FifthSemester.UI {
    [RequireComponent(typeof(Canvas))]
    public class MissionUIController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;

        private IEventBus _eventBus;

        private void Awake() {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void OnEnable() {
            _eventBus?.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
            _eventBus?.Subscribe<MissionProgressEvent>(OnMissionProgress);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
            _eventBus?.Unsubscribe<MissionProgressEvent>(OnMissionProgress);
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            if (_titleText != null) _titleText.text = evt.Title;
            if (_descriptionText != null) _descriptionText.text = evt.Description;
        }

        private void OnMissionProgress(MissionProgressEvent evt) {
            if (_progressText != null) {
                _progressText.text = $"{evt.Current}/{evt.Required}";
            }
        }
    }
}
