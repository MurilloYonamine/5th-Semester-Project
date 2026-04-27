using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    [CreateAssetMenu(fileName = "SettingsDefaults", menuName = "Settings/Defaults")]
    public class SettingsDefaults : ScriptableObject {
        [Header("Audio")]
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SFXVolume = 1f;
        public float AmbienceVolume = 1f;
        public bool ForceMonoAudio = false;

        [Header("PSX Shaders")]
        public bool BarrelDistortion = true;
        public bool Dithering = true;
        public bool Pixelation = true;
        public bool RollingBands = false;
        public bool Scanlines = false;
        public bool VHSEffect = false;

        [Header("Screen & Window")]
        public int FrameRate = 60;
        public bool IsFullscreen = true;
        public int ResolutionIndex = 0;
        public Vector2Int[] AvailableResolutions = new Vector2Int[0];

        [Header("Gameplay")]
        public Language Language = Language.English;
        public bool InvertYAxis = false;
        public float Sensibility = 1f;
    }
}
