using FifthSemester.Gameplay.Enemy;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Collider))]
    public class Map2DoorAiTrigger : MonoBehaviour {
        [SerializeField] private Map2KeyDoor _door;

        private void Reset() {
            Collider triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null) {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (_door == null) {
                return;
            }

            Nurse nurse = other.GetComponentInParent<Nurse>();
            if (nurse == null) {
                return;
            }

            _door.TryOpenByAI();
        }
    }
}
