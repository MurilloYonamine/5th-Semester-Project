
using UnityEngine;

namespace FifthSemester.Gameplay {
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
            Debug.Log($"[Map2DoorAiTrigger] OnTriggerEnter fired! Collider: '{other.name}' in parent '{other.transform.parent?.name}'");

            if (_door == null) {
                Debug.LogWarning("[Map2DoorAiTrigger] OnTriggerEnter: No door reference is set on this trigger!");
                return;
            }

            Nurse nurse = other.GetComponentInParent<Nurse>();
            if (nurse == null) {
                Debug.Log($"[Map2DoorAiTrigger] OnTriggerEnter: Collider '{other.name}' does not belong to the Nurse. Ignoring.");
                return;
            }

            Debug.Log($"[Map2DoorAiTrigger] Nurse detected! Calling TryOpenByAI() on door: '{_door.gameObject.name}'");
            _door.TryOpenByAI();
        }
    }
}
