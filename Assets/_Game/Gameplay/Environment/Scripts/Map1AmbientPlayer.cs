using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Environment {
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
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        private void Start() {
            if (_playOnStart) Play();
        }

        public void Play() {
            if (_ambientClip == null) return;

            _audioService.PlayAmbience(_ambientClip, startingVolume: 1f, loop: _loop);
        }

        public void OnDestroy() {
            if (_audioService != null) {
                _audioService.StopAmbience(_ambientClip);
            }
        }
    }
}
