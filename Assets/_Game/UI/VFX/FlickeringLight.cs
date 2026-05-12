using UnityEngine;

namespace FifthSemester.UI.VFX {
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour {
        [SerializeField] private float _minOffDuration = 0.8f;
        [SerializeField] private float _maxOffDuration = 1.5f;
        [SerializeField] private float _minOnDuration = 1f;
        [SerializeField] private float _maxOnDuration = 2f;
        [SerializeField] private GameObject _sparkVFXPrefab;

        private Light _light;
        private float _flickerTimer;
        private bool _isLightOn;

        private void Awake() {
            _light = GetComponent<Light>();
            _isLightOn = _light.enabled;
            ResetTimer();
        }

        private void Update() {
            _flickerTimer -= Time.deltaTime;

            if (_flickerTimer <= 0) {
                ToggleLight();
            }
        }

        private void ToggleLight() {
            _isLightOn = !_isLightOn;
            _light.enabled = _isLightOn;

            if (!_isLightOn) {
                TriggerSparkVFX();
            }

            ResetTimer();
        }

        private void ResetTimer() {
            if (_isLightOn) {
                _flickerTimer = Random.Range(_minOnDuration, _maxOnDuration);
            } else {
                _flickerTimer = Random.Range(_minOffDuration, _maxOffDuration);
            }
        }

        private void TriggerSparkVFX() {
            if (_sparkVFXPrefab == null) return;

            Instantiate(_sparkVFXPrefab, transform.position, Quaternion.identity);
        }
    }
}
