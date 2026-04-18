using UnityEngine;

namespace FifthSemester.Core.Utils {
    public class FPSLock : MonoBehaviour {
        [SerializeField, Range(1, 30)] private int _targetFPS = 24;

        private void Awake() {
            Application.targetFrameRate = _targetFPS;
        }
    }
}