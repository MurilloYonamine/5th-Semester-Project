// Autor: Murillo Gomes Yonamine
// Data: 24/05/2026

using System;
using FifthSemester.Core.Enums;

namespace FifthSemester.Core.Services {
    public interface IInputDeviceService {
        DeviceDisplayType CurrentDevice { get; }
        event Action<DeviceDisplayType> OnDeviceChanged;
    }
}