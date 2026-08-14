using FifthSemester.Shared;
// Autor: Murillo Gomes Yonamine
// Data: 24/04/2026

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;



using ThirdParty.QuickOutline;

namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(Outline))]
    public class DeliveryPoint : MonoBehaviour, IInteractable {
        private const string TAG = "<color=blue>[DeliveryPoint]</color>";
        [field: SerializeField] public string Id { get; private set; }

        [Tooltip("ID ou nome do item necessário para concluir a entrega.")]
        [SerializeField] private string _requiredItemId;

        [Tooltip("O item deve ser removido do inventário após o uso?")]
        [SerializeField] private bool _consumeItemOnDelivery = true;

        [Header("UI")]
        [SerializeField] private TextMeshPro _interactionPromptText;
        [SerializeField] private string _deliverPromptText = "entregar";
        [SerializeField] private string _talkPromptText = "conversar";

        [Header("Audio")]
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private IAudioService _audioService;
        private DialogueTrigger _dialogueTrigger;
        private bool _isCompleted = false;

        public bool HasPlayedDeliveryCutscene { get; private set; }

        public bool IsInteractable => !_isCompleted;

        private Outline _outline;

        private void Awake() {
            _outline = GetComponent<Outline>();
            TryGetComponent(out _dialogueTrigger);
            Highlight(false);
        }

        private void Start() {
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            ServiceLocator.TryGet<IAudioService>(out _audioService);

            UpdateInteractionPrompt();
            Highlight(false);
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }

        public void Interact() {
            Debug.Log($"{TAG} Interact triggered on DeliveryPoint '{Id}'. _isCompleted={_isCompleted}, _requiredItemId='{_requiredItemId}'");

            if (_isCompleted) {
                TryPlayDialogueTrigger();
                return;
            }

            if (TryDeliverItem()) {
                Debug.Log($"{TAG} Delivery SUCCESS on '{Id}' with item '{_requiredItemId}'.");
                CompleteDelivery();
                UpdateInteractionPrompt();
                PlayFeedback(_successSound);
                TryPlayDialogueTrigger();
            }
            else {
                Debug.LogWarning($"{TAG} Delivery FAILED on '{Id}'. Required item '{_requiredItemId}' not found in inventory.");
                PlayFeedback(_failureSound);
            }
        }

        public void StopInteract() {

        }

        private bool TryDeliverItem() {
            if (_inventoryService == null) {
                Debug.LogError($"{TAG} IInventoryService not found in Service Locator.");
                return false;
            }

            var items = _inventoryService.GetItems();
            if (items == null) {
                Debug.LogWarning($"{TAG} Inventory items list is null.");
                return false;
            }

            foreach (var item in items) {
                if (item != null && string.Equals(item.Id, _requiredItemId, System.StringComparison.OrdinalIgnoreCase)) {
                    if (_consumeItemOnDelivery) {
                        _inventoryService.RemoveItem(item);
                        Debug.Log($"{TAG} Consumed item '{item.Id}' from inventory.");
                    }
                    return true;
                }
            }

            return false;
        }

        private void CompleteDelivery() {
            _isCompleted = true;
            Debug.Log($"{TAG} Publishing ItemDeliveredEvent: DeliveryPointId='{Id}', DeliveredItemId='{_requiredItemId}'");
            _eventBus?.Publish(new ItemDeliveredEvent(Id, _requiredItemId));
        }

        private void UpdateInteractionPrompt() {
            if (_interactionPromptText == null) {
                return;
            }

            _interactionPromptText.text = _isCompleted ? _talkPromptText : _deliverPromptText;
        }

        private void PlayFeedback(AudioClip clip) {
            if (clip == null || _audioService == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }

        private void TryPlayDialogueTrigger() {
            if (_dialogueTrigger == null || !_dialogueTrigger.IsInteractable) {
                return;
            }

            HasPlayedDeliveryCutscene = true;
            _dialogueTrigger.Interact();
        }
    }
}
