// Autor: Generated
// Data: 05/05/2026

using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.UI {
    [RequireComponent(typeof(Canvas))]
    public class MissionUIController : MonoBehaviour {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;

        private IEventBus _eventBus;

        private void Awake() {
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void OnEnable() {
            _eventBus?.Subscribe<MissionUpdatedEvent>(OnMissionUpdated);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<MissionUpdatedEvent>(OnMissionUpdated);
        }

        private void OnMissionUpdated(MissionUpdatedEvent evt) {
            if (_titleText != null) _titleText.text = evt.Title;
            if (_descriptionText != null) _descriptionText.text = evt.Description;
        }
    }
}
