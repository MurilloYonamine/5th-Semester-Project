using System.Collections;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2HorrorAmbientPlayer : MonoBehaviour {
        [Header("Ambient Clips")]
        [SerializeField] private AudioClip[] _ambientClips;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float _minDelay = 8f;
        [SerializeField, Min(0f)] private float _maxDelay = 20f;
        [SerializeField] private bool _playOnStart = true;

        [Header("Playback")]
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;
        [SerializeField] private bool _avoidImmediateRepeat = true;

        private Coroutine _ambientRoutine;
        private int _lastClipIndex = -1;
        private IAudioService _audioService;

        private void Awake() {
            ServiceLocator.TryGet<IAudioService>(out _audioService);
        }

        private void Start() {
            if (_playOnStart) {
                StartAmbientLoop();
            }
        }

        public void StartAmbientLoop() {
            if (_ambientRoutine != null) {
                StopCoroutine(_ambientRoutine);
            }


            _ambientRoutine = StartCoroutine(AmbientRoutine());
        }

        public void StopAmbientLoop() {
            if (_ambientRoutine != null) {
                StopCoroutine(_ambientRoutine);
                _ambientRoutine = null;
            }
        }

        private IEnumerator AmbientRoutine() {
            while (true) {
                yield return WaitForNextPlayback();

                if (!TryGetRandomClip(out AudioClip clip)) {
                    yield break;
                }

                PlayClip(clip);
            }
        }

        private IEnumerator WaitForNextPlayback() {
            float delay = Random.Range(_minDelay, Mathf.Max(_minDelay, _maxDelay));
            yield return new WaitForSeconds(delay);
        }

        private bool TryGetRandomClip(out AudioClip clip) {
            clip = null;

            if (_ambientClips == null || _ambientClips.Length == 0) {
                return false;
            }

            int index = Random.Range(0, _ambientClips.Length);
            if (_avoidImmediateRepeat && _ambientClips.Length > 1 && index == _lastClipIndex) {
                index = (index + 1) % _ambientClips.Length;
            }

            clip = _ambientClips[index];
            if (clip == null) {
                return false;
            }

            Debug.Log($"tocando o ${_lastClipIndex}.");

            _lastClipIndex = index;
            return true;
        }

        private void PlayClip(AudioClip clip) {
            if (clip == null) {
                return;
            }

            if (_audioService == null) {
                ServiceLocator.TryGet<IAudioService>(out _audioService);
            }

            _audioService?.PlaySFX(clip, volume: _volume, spatialBlend: 0f);
        }

        private void OnValidate() {
            if (_maxDelay < _minDelay) {
                _maxDelay = _minDelay;
            }
        }

        private void OnDisable() {
            StopAmbientLoop();
        }
    }
}