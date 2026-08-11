using UnityEngine;
using TMPro;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;


namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour {
        [SerializeField] private string _localizationKey;

        private TextMeshProUGUI _textMesh;
        private IEventBus _eventBus;
        private ILocalizationService _localizationService;

        private void Awake() {
            _textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _localizationService = ServiceLocator.Get<ILocalizationService>();

            if (_eventBus != null) {
                _eventBus.Subscribe<LanguageChangedEvent>(OnLanguageChanged);
            }

            UpdateText();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<LanguageChangedEvent>(OnLanguageChanged);
            }
        }

        private void OnLanguageChanged(LanguageChangedEvent evt) {
            UpdateText();
        }

        private void UpdateText() {
            if (_localizationService == null || string.IsNullOrEmpty(_localizationKey)) return;

            _textMesh.text = _localizationService.GetText(_localizationKey);
        }
    }
}
