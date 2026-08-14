// Autor: Murillo Gomes Yonamine
// Data: 14/08/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class HUDService : IHUDService {
        private const string TAG = "<color=yellow><b>[HUDService]</b></color>";

        private bool _isHUDVisible = true;

        public bool IsHUDVisible {
            get => _isHUDVisible;
            set {
                if (_isHUDVisible != value) {
                    _isHUDVisible = value;
                    ServiceLocator.Get<IEventBus>()?.Publish(new HUDVisibilityChangedEvent(_isHUDVisible));
                    Debug.Log($"{TAG} HUD visibility changed to: {_isHUDVisible}");
                }
            }
        }

        public void ToggleHUD() {
            IsHUDVisible = !IsHUDVisible;
        }

        public void SetHUDVisible(bool visible) {
            IsHUDVisible = visible;
        }
    }
}
