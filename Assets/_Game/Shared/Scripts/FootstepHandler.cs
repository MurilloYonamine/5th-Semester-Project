using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Shared
{
    public class FootstepHandler : MonoBehaviour
    {
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField, Range(0f, 2f)] private float _footstepVolume = 1.5f;
        private IAudioService _audioService;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
        }
        public void PlayFootsteps() {
            if (_footstepClips.Length == 0) return;

            AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            _audioService?.PlaySFX(clip, volume: _footstepVolume, spatialBlend: 1f);
        }
    }
}
