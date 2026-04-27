// Autor: Murillo Gomes Yonamine
// Data: 24/04/2026

using UnityEngine;
using UnityEngine.Events;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Shared;
using ThirdParty.QuickOutline;

namespace FifthSemester.Gameplay.Interactables {
    [RequireComponent(typeof(Outline))]
    public class DeliveryPoint : MonoBehaviour, IInteractable {
        private const string TAG = "<color=blue>[DeliveryPoint]</color>";
        [field: SerializeField] public string Id { get; private set; }

        [Tooltip("ID ou nome do item necessário para concluir a entrega.")]
        [SerializeField] private string _requiredItemId;

        [Tooltip("O item deve ser removido do inventário após o uso?")]
        [SerializeField] private bool _consumeItemOnDelivery = true;

        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private bool _isCompleted = false;

        public bool IsInteractable => !_isCompleted;

        private Outline _outline;

        private void Awake() {
            _outline = GetComponent<Outline>();
            Highlight(false);
        }

        private void Start() {
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            Highlight(false);
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }

        public void Interact() {
            if (!IsInteractable) return;

            if (TryDeliverItem()) {
                CompleteDelivery();
                Debug.Log($"{TAG} Item '{_requiredItemId}' entregue com sucesso!");
            }
            else {
                Debug.Log($"{TAG} O item necessário '{_requiredItemId}' não está no inventário.");
            }
        }

        public void StopInteract() {

        }

        private bool TryDeliverItem() {
            if (_inventoryService == null) {
                Debug.LogError($"{TAG} IInventoryService não encontrado no Service Locator.");
                return false;
            }

            var items = _inventoryService.GetItems();
            foreach (var item in items) {
                if (item.Id == _requiredItemId) {

                    if (_consumeItemOnDelivery) {
                        _inventoryService.RemoveItem(item);
                    }
                    return true;
                }
            }

            return false;
        }

        private void CompleteDelivery() {
            _isCompleted = true;

            _eventBus?.Publish(new ItemDeliveredEvent(Id, _requiredItemId));
        }
    }
}