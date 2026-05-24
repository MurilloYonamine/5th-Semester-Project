// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.Utilities;
using FifthSemester.Core.Enums;
using UnityEngine.InputSystem.LowLevel;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Services {
    [RequireComponent(typeof(CanvasGroup))]
    public class InputDeviceService : MonoBehaviour, IInputDeviceService {
        public DeviceDisplayType CurrentDevice { get; private set; } = DeviceDisplayType.Keyboard;
        public event Action<DeviceDisplayType> OnDeviceChanged;

        private CanvasGroup _canvasGroup;
        private IEventBus _eventBus;

        private Action<InputEventPtr, InputDevice> _onEventHandler;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _eventBus = ServiceLocator.Get<IEventBus>();
            ServiceLocator.Register<IInputDeviceService>(this);
            _onEventHandler = HandleInputEvent;
            InputSystem.onEvent += _onEventHandler;

            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            ApplyGameState(ServiceLocator.Get<IGameStateService>()?.CurrentState ?? GameState.Gameplay);
        }

        private void OnDestroy() {
            if (_onEventHandler != null) {
                InputSystem.onEvent -= _onEventHandler;
                _onEventHandler = null;
            }

            _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void HandleInputEvent(InputEventPtr inputEvent, InputDevice device) {
            UpdateDevice(device);
        }

        private void UpdateDevice(InputDevice device) {
            DeviceDisplayType newDevice = DetectDevice(device);
            if (newDevice != CurrentDevice) {
                CurrentDevice = newDevice;
                OnDeviceChanged?.Invoke(CurrentDevice);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            ApplyGameState(evt.CurrentState);
        }

        private void ApplyGameState(GameState currentState) {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = currentState == GameState.Gameplay ? 1f : 0f;
        }

        private DeviceDisplayType DetectDevice(InputDevice device) {
            string name = device.name;
            if (name.Contains("DualShock") || name.Contains("DualSense")) return DeviceDisplayType.PlayStation;
            if (name.Contains("XInput") || name.Contains("Xbox") || device is Gamepad) return DeviceDisplayType.Xbox;
            return DeviceDisplayType.Keyboard;
        }
    }
}