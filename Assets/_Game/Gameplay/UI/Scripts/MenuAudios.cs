// autor: Murillo Gomes Yonamine
// data: 16/03/2026

using System;
using UnityEngine;
using FifthSemester.Core.Services;

namespace FifthSemester.UI {
    [Serializable]
    public class MenuAudios : MonoBehaviour {
        [Header("Audios")]
        [SerializeField] private AudioClip _navigateClip;
        [SerializeField] private AudioClip _openClip;
        [SerializeField] private AudioClip _closeClip;

        private float _lastOpenTime;
        private const float NAVIGATION_BLOCK_DURATION = 0.1f;
        private IAudioService _audioService;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        public void PlayNavigationSFX() {
            if (Time.unscaledTime - _lastOpenTime < NAVIGATION_BLOCK_DURATION) return;
            
            _audioService?.PlaySFX(_navigateClip, volume: 0.25f);
        }
        
        public void PlayOpenSFX() {
            _lastOpenTime = Time.unscaledTime;
            _audioService?.StopSFX(_navigateClip);
            
            _audioService?.PlaySFX(_openClip, volume: 0.25f);
        }
        
        public void PlayCloseSFX() {
            _lastOpenTime = Time.unscaledTime;
            _audioService?.StopSFX(_navigateClip);

            _audioService?.PlaySFX(_closeClip, volume: 0.25f);
        }
    }
}
