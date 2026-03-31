// autor: Murillo Gomes Yonamine
// data: 16/03/2026

using FifthSemester.Core.Managers;
using System;
using UnityEngine;

namespace FifthSemester.UI {
    [Serializable]
    public class MenuAudios : MonoBehaviour {
        [Header("Audios")]
        [SerializeField] private AudioClip _navigateClip;
        [SerializeField] private AudioClip _openClip;
        [SerializeField] private AudioClip _closeClip;

        private float _lastOpenTime;
        private const float NAVIGATION_BLOCK_DURATION = 0.1f;

        public void PlayNavigationSFX() {
            if (Time.unscaledTime - _lastOpenTime < NAVIGATION_BLOCK_DURATION) return;
            
            AudioManager.Instance.PlaySFX(_navigateClip, volume: 0.25f);
        }
        
        public void PlayOpenSFX() {
            _lastOpenTime = Time.unscaledTime;
            AudioManager.Instance.StopSFX(_navigateClip);
            
            AudioManager.Instance.PlaySFX(_openClip, volume: 0.25f);
        }
        
        public void PlayCloseSFX() {
            _lastOpenTime = Time.unscaledTime;
            AudioManager.Instance.StopSFX(_navigateClip);

            AudioManager.Instance.PlaySFX(_closeClip, volume: 0.25f);
        }
    }
}
