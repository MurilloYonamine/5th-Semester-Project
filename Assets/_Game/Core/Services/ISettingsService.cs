using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public interface ISettingsService {
        // ====== Audio ======
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SFXVolume { get; set; }
        float AmbienceVolume { get; set; }
        bool ForceMonoAudio { get; set; }

        // ====== PSX Shaders ======
        bool BarrelDistortion { get; set; }
        bool Dithering { get; set; }
        bool Pixelation { get; set; }
        bool RollingBands { get; set; }
        bool Scanlines { get; set; }
        bool VHSEffect { get; set; }

        // ===== Screen & Window ======
        int FrameRate { get; set; }
        bool IsFullscreen { get; set; }
        int ResolutionIndex { get; set; }
        Vector2Int[] AvailableResolutions { get; }

        // ===== Gameplay ======
        Language Language { get; set; }
        bool InvertYAxis { get; set; }
        float Sensibility { get; set; }
    }
}