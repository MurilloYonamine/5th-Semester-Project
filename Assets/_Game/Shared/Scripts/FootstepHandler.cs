using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Shared
{
    public class FootstepHandler : MonoBehaviour
    {
        [SerializeField] private AudioClip[] _footstepClips;
        private IAudioService _audioService;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
        }
        public void PlayFootsteps() {
            if (_footstepClips.Length == 0) return;

            AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            _audioService?.PlaySFX(clip, volume: 0.5f, spatialBlend: 1f);
        }
    }
}
