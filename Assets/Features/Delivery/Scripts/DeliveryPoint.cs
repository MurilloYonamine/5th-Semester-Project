// Author: Murillo Gomes Yonamine
// Date: 29/03/2026

using System;
using UnityEngine;
using FifthSemester.Items;
using FifthSemester.Core.Managers;
using System.Collections.Generic;

namespace FifthSemester.Delivery
{
    public class DeliveryPoint : MonoBehaviour, IItemReceiver
    {
        [SerializeField] private bool _isActive = true;
        [SerializeField] private AudioClip _deliverySound;
        [SerializeField] private List<string> _requiredItemNames = new List<string>();

        public event Action<IInteractable> OnItemReceived;

        public void ReceiveItem(IInteractable item)
        {
            if (!_isActive) return;
            if (item == null) return;

            if (_requiredItemNames.Contains(item.ToString()))
            {
                OnItemReceived?.Invoke(item);
                Debug.Log($"Item '{item}' delivered to {gameObject.name}.");
                _requiredItemNames.Remove(item.ToString());
                if (_requiredItemNames.Count == 0) _isActive = false;

                if (_deliverySound != null)
                {
                    AudioManager.Instance.PlaySFX(_deliverySound);
                }
            }
        }

        public bool TryDeliverFromInventory(Inventory.InventoryController inventory)
        {
            if (!_isActive || inventory == null) return false;
            foreach (var item in inventory.Items)
            {
                if (_requiredItemNames.Contains(item.ToString()))
                {
                    ReceiveItem(item);
                    inventory.RemoveItem(item);
                    return true;
                }
            }
            return false;
        }
    }
}
