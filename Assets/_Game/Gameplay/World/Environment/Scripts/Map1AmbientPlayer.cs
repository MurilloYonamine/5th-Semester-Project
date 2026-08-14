using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class Map1AmbientPlayer : MonoBehaviour {
        [Header("Ambient Settings")]
        [Tooltip("Ambient audio clip for Map 1.")]
        [SerializeField] private AudioClip _ambientClip;

        [Tooltip("Playback volume (0-1).")]
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _playOnStart = true;

        private IAudioService _audioService;

        private void Awake() {
            ServiceLocator.TryGet<IAudioService>(out _audioService);
        }

        private void Start() {
            if (_playOnStart) Play();
        }

        public void Play() {
            if (_ambientClip == null) return;

            if (_audioService == null && !ServiceLocator.TryGet<IAudioService>(out _audioService)) {
                return;
            }

            _audioService.PlayAmbience(_ambientClip, startingVolume: _volume, loop: _loop);
        }

        public void OnDestroy() {
            if (_ambientClip == null) return;

            if (_audioService is Object unityObj && unityObj == null) {
                return;
            }

            if (_audioService != null) {
                _audioService.StopAmbience(_ambientClip);
            }
        }
    }
}
