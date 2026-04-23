using System.Collections;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySlot[] _slots;
        [SerializeField] private CanvasGroup _inventoryCanvasGroup;
        [SerializeField] private Animator _animator;

        [Header("Display Settings")]
        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _highlightScale = 1.2f;
        [SerializeField] private float _animationDuration = 1f;

        private IEventBus _eventBus;
        private IInventoryService<Item> _inventoryService;

        private int _currentIndex = 0;
        private Coroutine _hideDelayCoroutine;
        private Coroutine _animationCoroutine;
        private Vector3[] _originalScales;

        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");
        private static readonly int IsCloseHash = Animator.StringToHash("IsClose");

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();

            if (_animator == null && _inventoryCanvasGroup != null)
            {
                _animator = _inventoryCanvasGroup.GetComponent<Animator>();
            }

            InitializeSlots();

            if (_eventBus != null)
            {
                _eventBus.Subscribe<InventoryToggledEvent>(HandleInventoryToggled);
                _eventBus.Subscribe<InventoryItemAddedEvent>(HandleItemAdded);
                _eventBus.Subscribe<InventoryItemRemovedEvent>(HandleItemRemoved);
                _eventBus.Subscribe<NextInputEvent>(HandleNextInput);
                _eventBus.Subscribe<PreviousInputEvent>(HandlePreviousInput);
            }
            
            if (_inventoryCanvasGroup != null)
            {
                _inventoryCanvasGroup.alpha = 0f;
                _inventoryCanvasGroup.interactable = false;
                _inventoryCanvasGroup.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<InventoryToggledEvent>(HandleInventoryToggled);
                _eventBus.Unsubscribe<InventoryItemAddedEvent>(HandleItemAdded);
                _eventBus.Unsubscribe<InventoryItemRemovedEvent>(HandleItemRemoved);
                _eventBus.Unsubscribe<NextInputEvent>(HandleNextInput);
                _eventBus.Unsubscribe<PreviousInputEvent>(HandlePreviousInput);
            }
        }

        private void InitializeSlots()
        {
            if (_slots == null || _slots.Length == 0) return;

            _originalScales = new Vector3[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null)
                {
                    _originalScales[i] = _slots[i].transform.localScale;
                }
            }
        }

        private void HandleInventoryToggled(InventoryToggledEvent inventory)
        {
            if (inventory.IsOpen) ShowInventory();
            else HideInventory();
        }

        private void HandleItemAdded(InventoryItemAddedEvent item)
        {
            if (_slots != null && item.Index >= 0 && item.Index < _slots.Length)
            {
                var items = _inventoryService?.GetItems();
                if (items != null && item.Index < items.Count)
                {
                    _slots[item.Index].SetItem(items[item.Index]);
                    ShowInventory();
                }
            }
        }

        private void HandleItemRemoved(InventoryItemRemovedEvent item)
        {
            if (_slots != null && item.Index >= 0 && item.Index < _slots.Length)
            {
                ShowInventory();
                HighlightSlot(item.Index);
                _slots[item.Index].ClearItem();
            }
        }

        private void HandleNextInput(NextInputEvent evt)
        {
            if (_inventoryCanvasGroup != null && _inventoryCanvasGroup.alpha > 0)
                NavigateNext();
        }

        private void HandlePreviousInput(PreviousInputEvent evt)
        {
            if (_inventoryCanvasGroup != null && _inventoryCanvasGroup.alpha > 0)
                NavigatePrevious();
        }

        public void NavigateNext()
        {
            if (_slots == null || _slots.Length == 0) return;

            ResetSlotScale(_currentIndex);
            _currentIndex = (_currentIndex + 1) % _slots.Length;
            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        public void NavigatePrevious()
        {
            if (_slots == null || _slots.Length == 0) return;

            ResetSlotScale(_currentIndex);
            _currentIndex = (_currentIndex - 1 + _slots.Length) % _slots.Length;
            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        private void ShowInventory()
        {
            if (_inventoryCanvasGroup == null) return;

            if (_hideDelayCoroutine != null) StopCoroutine(_hideDelayCoroutine);
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);

            _inventoryCanvasGroup.alpha = 1f;
            _inventoryCanvasGroup.interactable = true;
            _inventoryCanvasGroup.blocksRaycasts = true;

            if (_animator != null)
            {
                _animator.SetBool(IsOpenHash, true);
                _animator.SetBool(IsCloseHash, false);
            }

            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        private void HideInventory()
        {
            if (_inventoryCanvasGroup == null) return;

            if (_animator != null)
            {
                _animator.SetBool(IsOpenHash, false);
                _animator.SetBool(IsCloseHash, true);
            }

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(HideAfterAnimation());
        }

        private IEnumerator HideAfterAnimation()
        {
            yield return new WaitForSeconds(_animationDuration);

            _inventoryCanvasGroup.alpha = 0f;
            _inventoryCanvasGroup.interactable = false;
            _inventoryCanvasGroup.blocksRaycasts = false;

            ResetAllSlots();
        }

        private void ResetHideTimer()
        {
            if (_hideDelayCoroutine != null) StopCoroutine(_hideDelayCoroutine);
            _hideDelayCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_displayDuration);
            HideInventory();
            
            _eventBus?.Publish(new InventoryToggledEvent(false));
        }

        private void HighlightSlot(int index)
        {
            if (_slots == null || index < 0 || index >= _slots.Length || _slots[index] == null) return;
            _slots[index].transform.localScale = _originalScales[index] * _highlightScale;
        }

        private void ResetSlotScale(int index)
        {
            if (_slots == null || index < 0 || index >= _slots.Length || _slots[index] == null) return;
            _slots[index].transform.localScale = _originalScales[index];
        }

        private void ResetAllSlots()
        {
            if (_slots == null || _originalScales == null) return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null)
                {
                    _slots[i].transform.localScale = _originalScales[i];
                }
            }
        }
    }
}