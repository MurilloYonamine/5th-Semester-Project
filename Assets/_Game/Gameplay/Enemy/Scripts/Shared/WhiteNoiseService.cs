// Autor: Murillo Gomes Yonamine
// Data: 09/05/2026

using FifthSemester.Core.Audio;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public class WhiteNoiseService : MonoBehaviour, IWhiteNoiseService {
        [Header("Audio")]
        [SerializeField] private AudioClip _whiteNoiseClip;

        [SerializeField, Range(0f, 1f)]
        private float _maxVolume = 0.5f;

        [Header("Visual")]
        [SerializeField, Range(0f, 1f)]
        private float _maxOpacity = 0.4f;

        [Header("Behaviour")]
        [SerializeField]
        private float _fadeSpeed = 3f;

        private IAudioService _audioService;

        private AudioTrack _track;

        private float _currentIntensity;
        private float _requestedIntensity;

        private void Awake() {
            ServiceLocator.Register<IWhiteNoiseService>(this);

            _audioService = ServiceLocator.Get<IAudioService>();

            Shader.SetGlobalFloat("_NoiseOpacity", 0f);
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<IWhiteNoiseService>();

            Shader.SetGlobalFloat("_NoiseOpacity", 0f);
        }

        private void Update() {
            _currentIntensity = Mathf.MoveTowards(
                _currentIntensity,
                _requestedIntensity,
                Time.deltaTime * _fadeSpeed
            );

            UpdateShader();
            UpdateAudio();

            _requestedIntensity = 0f;
        }

        public void RequestIntensity(float intensity) {
            if (intensity > _requestedIntensity) {
                _requestedIntensity = intensity;
            }
        }

        public void ResetIntensity() {
            _requestedIntensity = 0f;
        }

        private void UpdateShader() {
            Shader.SetGlobalFloat(
                "_NoiseOpacity",
                _currentIntensity * _maxOpacity
            );
        }

        private void UpdateAudio() {
            if (_currentIntensity <= 0.01f) {
                StopAudio();
                return;
            }

            if (_track == null) {
                _track = _audioService.PlayAmbience(
                    _whiteNoiseClip,
                    loop: true,
                    startingVolume: 0f,
                    volumeCap: _maxVolume
                );
            }

            _track.Volume = _currentIntensity * _maxVolume;
        }

        private void StopAudio() {
            if (_track == null) {
                return;
            }

            _audioService.StopAmbience(_whiteNoiseClip);

            _track = null;
        }
    }
}