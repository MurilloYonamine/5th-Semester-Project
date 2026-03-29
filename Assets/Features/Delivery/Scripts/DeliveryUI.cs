// Author: Murillo Gomes Yonamine
// Date: 29/03/2026

using UnityEngine;
using FifthSemester.Delivery;
using FifthSemester.Items;

namespace FifthSemester.UI {
    public class DeliveryUI : MonoBehaviour {
        [SerializeField] private DeliveryPoint _deliveryPoint;

        private void Awake() {
            if (_deliveryPoint != null) {
                _deliveryPoint.OnItemReceived += HandleItemReceived;
            }
        }

        private void OnDestroy() {
            if (_deliveryPoint != null) {
                _deliveryPoint.OnItemReceived -= HandleItemReceived;
            }
        }

        private void HandleItemReceived(IInteractable item) {
            Debug.Log($"Item received: {item}");
        }
    }
}
