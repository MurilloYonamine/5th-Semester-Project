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
        private Light _light;
        private Coroutine _toggleCoroutine;
        private Coroutine _sparkLoopCoroutine;
        private GameObject _sparkInstance;

        private void Awake() {
            _sparkPrefab = GetComponentInChildren<VisualEffect>()?.gameObject;
            _light = GetComponent<Light>();
            SpawnSparkPrefab();
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
            float randomDelay = _toggleDelay + Random.Range(-_toggleDelayVariation, _toggleDelayVariation);
            yield return new WaitForSeconds(randomDelay);
            _light.enabled = true;
            
            if (_sparkInstance != null) {
                _sparkInstance.SetActive(false);
            }
        }
    }
}
