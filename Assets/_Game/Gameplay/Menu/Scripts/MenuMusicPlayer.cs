using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class MenuMusicPlayer : MonoBehaviour {
        [Header("Music")]
        [SerializeField] private string _musicFilePath = "Audio/menu_musica";
        [SerializeField] private int _channel = 0;
        [SerializeField] private bool _loop = true;
        [SerializeField] private float _startingVolume = 0.35f;
        [SerializeField] private float _volumeCap = 1f;
        [SerializeField] private float _pitch = 1f;
        [SerializeField] private bool _playOnStart = true;

        private IAudioService _audioService;

        private void Start() {
            ServiceLocator.TryGet<IAudioService>(out _audioService);

            if (_playOnStart) {
                PlayMusic();
            }
        }

        public void PlayMusic() {
            if (_audioService == null) {
                ServiceLocator.TryGet<IAudioService>(out _audioService);
            }

            if (_audioService == null || string.IsNullOrWhiteSpace(_musicFilePath)) {
                return;
            }

            _audioService.PlayTrack(_musicFilePath, _channel, _loop, _startingVolume, _volumeCap, _pitch);
        }

        public void StopMusic() {
            if (_audioService == null) {
                ServiceLocator.TryGet<IAudioService>(out _audioService);
            }

            if (_audioService != null && !string.IsNullOrWhiteSpace(_musicFilePath)) {
                _audioService.StopTrack(_musicFilePath);
            }
        }

        private void OnDestroy() {
            StopMusic();
        }
    }
}