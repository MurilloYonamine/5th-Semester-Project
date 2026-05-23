// Autor: Murillo Gomes Yonamine
// Data: 11/05/2026

using UnityEngine;
using FifthSemester.Gameplay.Enemy;

namespace FifthSemester.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class SafeLightZone : MonoBehaviour
    {
        [SerializeField] private LightSeeker _lightSeeker;
        [SerializeField] private Transform _landTransform;
        private void Start()
        {
            if (_lightSeeker == null)
                _lightSeeker = FindFirstObjectByType<LightSeeker>();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player")) {
                if (_lightSeeker != null && _lightSeeker.Blackboard != null) {
                    _lightSeeker.Blackboard.SetData("IsPlayerInSafeLight", true);

                    if (_landTransform != null) {
                        _lightSeeker.Blackboard.SetData("SafeLightLandPosition", _landTransform.position);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("Player")) {
                if (_lightSeeker != null && _lightSeeker.Blackboard != null) {
                    _lightSeeker.Blackboard.SetData("IsPlayerInSafeLight", false);
                }
            }
        }
    }
}
