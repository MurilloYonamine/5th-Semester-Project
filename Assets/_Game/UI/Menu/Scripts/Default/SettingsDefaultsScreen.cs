using UnityEngine;

namespace FifthSemester.UI {
    [CreateAssetMenu(fileName = "SettingsDefaultsScreen", menuName = "Settings/Defaults/Screen")]
    public class SettingsDefaultsScreen : ScriptableObject {
        [Header("Screen")]
        public int FrameRate = 60;
        public bool IsFullscreen = true;
        public int ResolutionIndex = 0;
    }
}