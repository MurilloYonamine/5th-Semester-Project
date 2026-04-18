// Author: Murillo Gomes Yonamine
// Date: 29/03/2026

using UnityEngine;
using FifthSemester.Items;
using FifthSemester.Gameplay.Delivery;

namespace FifthSemester.Gameplay.UI {
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

        private void HandleItemReceived(DeliveryPoint deliveryPoint, IInteractable item) {
            Debug.Log($"Item received: {item} at {deliveryPoint.name}");
        }
    }
}
