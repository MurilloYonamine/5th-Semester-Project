using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class SettingsService : ISettingsService {

        // Funções auxiliares para salvar e ler booleanos no PlayerPrefs
        private bool GetBool(string key, bool defaultValue = false) {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        private void SetBool(string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public SettingsService() {
            Application.targetFrameRate = FrameRate;
        }

        // ====== General ======
        public Language Language {
            get => (Language)PlayerPrefs.GetInt("Settings_Language", 0);
            set { PlayerPrefs.SetInt("Settings_Language", (int)value); PlayerPrefs.Save(); }
        }

        // ====== Audio ======
        public float MasterVolume {
            get => PlayerPrefs.GetFloat("Settings_MasterVolume", 1f);
            set { PlayerPrefs.SetFloat("Settings_MasterVolume", value); PlayerPrefs.Save(); }
        }
        public float MusicVolume {
            get => PlayerPrefs.GetFloat("Settings_MusicVolume", 1f);
            set { PlayerPrefs.SetFloat("Settings_MusicVolume", value); PlayerPrefs.Save(); }
        }
        public float SFXVolume {
            get => PlayerPrefs.GetFloat("Settings_SFXVolume", 1f);
            set { PlayerPrefs.SetFloat("Settings_SFXVolume", value); PlayerPrefs.Save(); }
        }
        public float AmbienceVolume {
            get => PlayerPrefs.GetFloat("Settings_AmbienceVolume", 1f);
            set { PlayerPrefs.SetFloat("Settings_AmbienceVolume", value); PlayerPrefs.Save(); }
        }
        public bool ForceMonoAudio {
            get => GetBool("Settings_ForceMonoAudio", false);
            set => SetBool("Settings_ForceMonoAudio", value);
        }

        // ====== PSX Shaders ======
        public bool BarrelDistortion {
            get => GetBool("Settings_BarrelDistortion", true);
            set => SetBool("Settings_BarrelDistortion", value);
        }
        public bool Dithering {
            get => GetBool("Settings_Dithering", true);
            set => SetBool("Settings_Dithering", value);
        }
        public bool Pixelation {
            get => GetBool("Settings_Pixelation", true);
            set => SetBool("Settings_Pixelation", value);
        }
        public bool RollingBands {
            get => GetBool("Settings_RollingBands", true);
            set => SetBool("Settings_RollingBands", value);
        }
        public bool Scanlines {
            get => GetBool("Settings_Scanlines", true);
            set => SetBool("Settings_Scanlines", value);
        }
        public bool VHSEffect {
            get => GetBool("Settings_VHSEffect", true);
            set => SetBool("Settings_VHSEffect", value);
        }

        // ===== Screen & Window ======
        public int FrameRate {
            get => PlayerPrefs.GetInt("Settings_FrameRate", 60);
            set {
                PlayerPrefs.SetInt("Settings_FrameRate", value);
                Application.targetFrameRate = value;
                PlayerPrefs.Save();
            }
        }
        public bool IsFullscreen {
            get => GetBool("Settings_Fullscreen", true);
            set {
                SetBool("Settings_Fullscreen", value);
                Screen.fullScreen = value;
            }
        }
        public int ResolutionIndex {
            get => PlayerPrefs.GetInt("Settings_ResolutionIndex", AvailableResolutions.Length > 0 ? AvailableResolutions.Length - 1 : 0);
            set {
                PlayerPrefs.SetInt("Settings_ResolutionIndex", value);
                if (value >= 0 && value < AvailableResolutions.Length) {
                    Vector2Int res = AvailableResolutions[value];
                    Screen.SetResolution(res.x, res.y, IsFullscreen); // Aplica a resolução dinamicamente
                }
                PlayerPrefs.Save();
            }
        }
        public Vector2Int[] AvailableResolutions { get; private set; } = new Vector2Int[] {
            new Vector2Int(320, 240),
            new Vector2Int(640, 480),
            new Vector2Int(800, 600),
            new Vector2Int(1024, 768),
            new Vector2Int(1280, 720)
        };

        // ===== Gameplay ======
        public bool InvertYAxis {
            get => GetBool("Settings_InvertY", false);
            set => SetBool("Settings_InvertY", value);
        }
        public bool Sensibility {
            get => GetBool("Settings_Sensibility", false);
            set => SetBool("Settings_Sensibility", value);
        }
    }
}