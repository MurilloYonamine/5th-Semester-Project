using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FifthSemester.Items;

namespace FifthSemester.Inventory {
    public class InventoryUI : MonoBehaviour {
        [Header("References")]
        private InventoryController _inventory;
        [SerializeField] private InventorySlot[] _slots;
        [SerializeField] private CanvasGroup _inventoryCanvasGroup;
        [SerializeField] private Animator _animator;

        [Header("Display Settings")]
        [SerializeField] private float _displayDuration = 3f;
        [SerializeField] private float _highlightScale = 1.2f;
        [SerializeField] private float _animationDuration = 1f;

        private int _currentIndex = 0;
        private Coroutine _hideCoroutine;
        private Vector3[] _originalScales;

        private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");
        private static readonly int IsCloseHash = Animator.StringToHash("IsClose");

        public void SetInventory(InventoryController inventory) {
            if (_inventory != null) {
                _inventory.OnInventoryToggled -= HandleInventoryToggled;
                _inventory.OnItemAdded -= HandleItemAdded;
                _inventory.OnItemRemoved -= HandleItemRemoved;
            }

            _inventory = inventory;

            if (_inventory != null) {
                _inventory.OnInventoryToggled += HandleInventoryToggled;
                _inventory.OnItemAdded += HandleItemAdded;
                _inventory.OnItemRemoved += HandleItemRemoved;
            }
        }

        private void Start() {
            if (_animator == null) {
                _animator = _inventoryCanvasGroup.GetComponent<Animator>();
            }
            InitializeSlots();
        }

        private void OnDisable() {
            if (_inventory != null) {
                _inventory.OnInventoryToggled -= HandleInventoryToggled;
                _inventory.OnItemAdded -= HandleItemAdded;
                _inventory.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void InitializeSlots() {
            if (_slots == null || _slots.Length == 0) {
                Debug.LogWarning("InventoryUI: No slots assigned!");
                return;
            }

            _originalScales = new Vector3[_slots.Length];
            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i] != null) {
                    _originalScales[i] = _slots[i].transform.localScale;
                }
            }
        }

        private void HandleInventoryToggled(bool isOpen) {
            if (isOpen) {
                ShowInventory();
            }
        }

        private void HandleItemAdded(IInteractable item, int index) {
            if (index >= 0 && index < _slots.Length) {
                _slots[index].SetItem(item);
            }
        }

        private void HandleItemRemoved(int index) {
            if (index >= 0 && index < _slots.Length) {
                _slots[index].ClearItem();
            }
        }

        public void NavigateNext() {
            if (_slots == null || _slots.Length == 0) return;

            ResetSlotScale(_currentIndex);
            _currentIndex = (_currentIndex + 1) % _slots.Length;
            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        public void NavigatePrevious() {
            if (_slots == null || _slots.Length == 0) return;

            ResetSlotScale(_currentIndex);
            _currentIndex = (_currentIndex - 1 + _slots.Length) % _slots.Length;
            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        private void ShowInventory() {
            if (_inventoryCanvasGroup == null) return;

            _inventoryCanvasGroup.alpha = 1f;
            _inventoryCanvasGroup.interactable = true;
            _inventoryCanvasGroup.blocksRaycasts = true;

            if (_animator != null) {
                _animator.SetBool(IsOpenHash, true);
                _animator.SetBool(IsCloseHash, false);
            }

            HighlightSlot(_currentIndex);
            ResetHideTimer();
        }

        private void HideInventory() {
            if (_inventoryCanvasGroup == null) return;

            if (_animator != null) {
                _animator.SetBool(IsOpenHash, false);
                _animator.SetBool(IsCloseHash, true);
            }

            StartCoroutine(HideAfterAnimation());
        }

        private IEnumerator HideAfterAnimation() {
            yield return new WaitForSeconds(_animationDuration);

            _inventoryCanvasGroup.alpha = 0f;
            _inventoryCanvasGroup.interactable = false;
            _inventoryCanvasGroup.blocksRaycasts = false;

            ResetAllSlots();
        }

        private void ResetHideTimer() {
            if (_hideCoroutine != null) {
                StopCoroutine(_hideCoroutine);
            }
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay() {
            yield return new WaitForSeconds(_displayDuration);
            HideInventory();
            _inventory?.CloseInventory();
        }

        private void HighlightSlot(int index) {
            if (_slots == null || index < 0 || index >= _slots.Length || _slots[index] == null) return;

            _slots[index].transform.localScale = _originalScales[index] * _highlightScale;
        }

        private void ResetSlotScale(int index) {
            if (_slots == null || index < 0 || index >= _slots.Length || _slots[index] == null) return;

            _slots[index].transform.localScale = _originalScales[index];
        }

        private void ResetAllSlots() {
            if (_slots == null || _originalScales == null) return;

            for (int i = 0; i < _slots.Length; i++) {
                if (_slots[i] != null) {
                    _slots[i].transform.localScale = _originalScales[i];
                }
            }
        }
    }
}
