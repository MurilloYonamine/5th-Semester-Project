using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

namespace FifthSemester.UI.VFX {
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour {
        private GameObject _sparkPrefab;
        [SerializeField] private float _toggleDelay = 0.5f;
        [SerializeField] private float _toggleDelayVariation = 0.2f;
        [SerializeField] private float _sparkInterval = 3f;
        [SerializeField] private float _sparkIntervalVariation = 1f;
<<<<<<< HEAD
        
        [Header("Configurações de Áudio 3D (Espacializado)")]
        [Tooltip("Som opcional de curto-circuito/piscar de luz.")]
        [SerializeField] private AudioClip _flickerSound;
        [Tooltip("Distância máxima para ouvir o som no volume máximo.")]
        [SerializeField] private float _minDistance = 1.5f;
        [Tooltip("Distância máxima na qual o som ainda pode ser ouvido.")]
        [SerializeField] private float _maxDistance = 10f;
        [Tooltip("Volume geral do som de piscar.")]
        [SerializeField, Range(0f, 1f)] private float _volume = 0.8f;

=======
>>>>>>> origin/main
        private Light _light;
        private Coroutine _toggleCoroutine;
        private Coroutine _sparkLoopCoroutine;
        private GameObject _sparkInstance;
<<<<<<< HEAD
        private AudioSource _audioSource;
=======
>>>>>>> origin/main

        private void Awake() {
            _sparkPrefab = GetComponentInChildren<VisualEffect>()?.gameObject;
            _light = GetComponent<Light>();
            SpawnSparkPrefab();
<<<<<<< HEAD
            Configure3DAudio();
=======
>>>>>>> origin/main
        }

        private void OnEnable() {
            _sparkLoopCoroutine = StartCoroutine(SparkLoopCoroutine());
        }

        private void OnDisable() {
            if (_sparkLoopCoroutine != null) {
                StopCoroutine(_sparkLoopCoroutine);
            }

            if (_toggleCoroutine != null) {
                StopCoroutine(_toggleCoroutine);
            }
        }

<<<<<<< HEAD
        private void Configure3DAudio() {
            if (_flickerSound == null) return;

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.clip = _flickerSound;
            _audioSource.spatialBlend = 1.0f; // 1.0 = Áudio 3D Completo (fica mais baixo conforme se distancia)
            _audioSource.minDistance = _minDistance;
            _audioSource.maxDistance = _maxDistance;
            _audioSource.volume = _volume;
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

=======
>>>>>>> origin/main
        private void SpawnSparkPrefab() {
            if (_sparkPrefab != null && _sparkInstance == null) {
                _sparkInstance = Instantiate(_sparkPrefab, transform.position, Quaternion.identity, transform);
                _sparkInstance.SetActive(false);
                SubscribeToSparkEffect();
            }
        }

        private IEnumerator SparkLoopCoroutine() {
            while (true) {
                float randomInterval = _sparkInterval + Random.Range(-_sparkIntervalVariation, _sparkIntervalVariation);
                yield return new WaitForSeconds(randomInterval);
                PlaySparkEffect();
            }
        }

        private void PlaySparkEffect() {
            if (_sparkInstance != null) {
                _sparkInstance.SetActive(true);
            }
        }

        private void SubscribeToSparkEffect() {
            if (_sparkInstance == null) return;
            
            VisualEffect vfx = _sparkInstance.GetComponent<VisualEffect>();
            if (vfx != null) {
                vfx.outputEventReceived += OnSparkFired;
            }
        }

        private void OnSparkFired(VFXOutputEventArgs eventData) {
            int sparkEventID = Shader.PropertyToID("SparkFired");
            if (eventData.nameId == sparkEventID) {
                if (_toggleCoroutine != null) {
                    StopCoroutine(_toggleCoroutine);
                }
                _toggleCoroutine = StartCoroutine(ToggleLightCoroutine());
            }
        }

        private IEnumerator ToggleLightCoroutine() {
            _light.enabled = false;
<<<<<<< HEAD
            
            // Toca o som 3D perfeitamente sincronizado com o piscar da luz
            if (_audioSource != null && _flickerSound != null) {
                _audioSource.Play();
            }

=======
>>>>>>> origin/main
            float randomDelay = _toggleDelay + Random.Range(-_toggleDelayVariation, _toggleDelayVariation);
            yield return new WaitForSeconds(randomDelay);
            _light.enabled = true;
            
            if (_sparkInstance != null) {
                _sparkInstance.SetActive(false);
            }
        }
    }
}
