using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    // Attach to the LightSeeker FBX root. Call PlayJump/PlayLanding from animation events.
    public class LightSeekerAudio : MonoBehaviour {
        [SerializeField] private AudioClip[] _jumpClips;
        [SerializeField] private AudioClip[] _landingClips;
        [SerializeField, Range(0f, 2f)] private float _jumpVolume = 1f;
        [SerializeField, Range(0f, 2f)] private float _landingVolume = 1f;

        private IAudioService _audioService;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        public void PlayJump() {
            if (_jumpClips == null || _jumpClips.Length == 0) return;
            AudioClip clip = _jumpClips[Random.Range(0, _jumpClips.Length)];
            _audioService?.PlaySFX(clip, volume: _jumpVolume, spatialBlend: 1f);
        }

        public void PlayLanding() {
            if (_landingClips == null || _landingClips.Length == 0) return;
            AudioClip clip = _landingClips[Random.Range(0, _landingClips.Length)];
            _audioService?.PlaySFX(clip, volume: _landingVolume, spatialBlend: 1f);
        }
    }
}
