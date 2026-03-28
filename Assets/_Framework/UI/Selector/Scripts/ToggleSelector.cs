using UnityEngine;

namespace FifthSemester.Framework.UI {
    public enum ToggleState { Off, On }

    public abstract class ToggleSelector : TextSelector<ToggleState> {
        [Header("Toggle Persistence")]
        [SerializeField] protected string _prefsKey;
        [SerializeField] protected int _defaultIndex = 1;

        protected void Awake() {
            _items = new ToggleState[] { ToggleState.Off, ToggleState.On };
        }

        protected override void OnSave() {
            if (string.IsNullOrEmpty(_prefsKey)) return;
            PlayerPrefs.SetInt(_prefsKey, _currentIndex);
            PlayerPrefs.Save();
        }

        protected override void OnLoad() {
            if (string.IsNullOrEmpty(_prefsKey)) return;
            _currentIndex = PlayerPrefs.GetInt(_prefsKey, _defaultIndex);
        }

        protected override void OnItemSelected(ToggleState selectedItem) { }
    }
}
