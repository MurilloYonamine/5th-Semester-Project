// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.Utilities;
using FifthSemester.Core.Enums;

namespace FifthSemester.Core.Services
{
    public class InputDeviceService : MonoBehaviour, IInputDeviceService
    {
        public DeviceDisplayType CurrentDevice { get; private set; } = DeviceDisplayType.Keyboard;
        public event Action<DeviceDisplayType> OnDeviceChanged;

        private void Awake()
        {
            ServiceLocator.Register<IInputDeviceService>(this);
            InputSystem.onEvent += (inputEvent, device) => UpdateDevice(device);
        }

        private void UpdateDevice(InputDevice device)
        {
            DeviceDisplayType newDevice = DetectDevice(device);
            if (newDevice != CurrentDevice)
            {
                CurrentDevice = newDevice;
                OnDeviceChanged?.Invoke(CurrentDevice);
            }
        }

        private DeviceDisplayType DetectDevice(InputDevice device)
        {
            string name = device.name;
            if (name.Contains("DualShock") || name.Contains("DualSense")) return DeviceDisplayType.PlayStation;
            if (name.Contains("XInput") || name.Contains("Xbox") || device is Gamepad) return DeviceDisplayType.Xbox;
            return DeviceDisplayType.Keyboard;
        }
    }
}