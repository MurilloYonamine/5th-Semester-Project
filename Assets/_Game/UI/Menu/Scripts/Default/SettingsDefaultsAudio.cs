using UnityEngine;

namespace FifthSemester.UI {
    [CreateAssetMenu(fileName = "SettingsDefaultsAudio", menuName = "Settings/Defaults/Audio")]
    public class SettingsDefaultsAudio : ScriptableObject {
        [Header("Audio")]
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SFXVolume = 1f;
        public float AmbienceVolume = 1f;
        public bool ForceMonoAudio = false;
    }
}