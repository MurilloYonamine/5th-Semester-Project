using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    [CreateAssetMenu(fileName = "SettingsDefaultsShaders", menuName = "Settings/Defaults/Shaders")]
    public class SettingsDefaultsShaders : ScriptableObject {
        [Header("PSX Shaders")]
        public bool BarrelDistortion = true;
        public bool Dithering = true;
        public bool Pixelation = true;
        public bool RollingBands = false;
        public bool Scanlines = false;
        public bool VHSEffect = false;
    }
}