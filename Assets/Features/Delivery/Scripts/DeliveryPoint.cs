// Author: Murillo Gomes Yonamine
// Date: 29/03/2026

using System;
using UnityEngine;
using FifthSemester.Items;
using FifthSemester.Core.Managers;
using System.Collections.Generic;
using FifthSemester.DialogueSystem;
using ThirdParty.QuickOutline;
namespace FifthSemester.Delivery {
    public class DeliveryPoint : MonoBehaviour, IItemReceiver {
        [SerializeField] private bool _isActive = true;
        [SerializeField] private AudioClip _deliverySound;
        [SerializeField] private int _requiredAmount = 1;
        private int _receivedAmount = 0;
        [SerializeField] private DialogueSO _onDeliveryDialogue;

        public event Action<DeliveryPoint, IInteractable> OnItemReceived;

        [SerializeField] private Outline _outline;

        public void ReceiveItem(IInteractable item) {
            if (!_isActive) return;
            if (item == null) return;

            if (item is not MedicineItem) return;

            _receivedAmount++;

            if (_onDeliveryDialogue != null) {
                DialogueManager.Instance.StartDialogue(_onDeliveryDialogue);
            }

            if (_receivedAmount >= _requiredAmount) {
                _isActive = false;
                OnItemReceived?.Invoke(this, item);
            }

            if (_deliverySound != null) {
                AudioManager.Instance.PlaySFX(_deliverySound);
            }
        }
        public bool TryDeliverFromInventory(Inventory.InventoryController inventory) {
            if (!_isActive || inventory == null) return false;

            for (int i = 0; i < inventory.Items.Count; i++) {
                var item = inventory.Items[i];

                if (item is MedicineItem) {
                    inventory.RemoveItem(item);
                    ReceiveItem(item);
                    return true;
                }
            }

            return false;
        }
        public void EnableOutline(bool enable) {
            if (_outline != null) {
                _outline.enabled = enable;
            }
        }
    }
}
