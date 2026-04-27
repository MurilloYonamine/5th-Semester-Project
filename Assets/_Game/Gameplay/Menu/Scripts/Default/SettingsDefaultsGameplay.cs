using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    [CreateAssetMenu(fileName = "SettingsDefaultsGameplay", menuName = "Settings/Defaults/Gameplay")]
    public class SettingsDefaultsGameplay : ScriptableObject {
        [Header("Gameplay")]
        public Language Language = Language.English;
        public bool InvertYAxis = false;
        public float Sensibility = 1f;
    }
}