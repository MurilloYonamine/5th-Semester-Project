using UnityEngine;

namespace FifthSemester.Core.Events {
    public readonly struct MoveInputEvent {
        public readonly Vector2 Value;

        public MoveInputEvent(Vector2 value) {
            Value = value;
        }
    }

    public readonly struct LookInputEvent {
        public readonly Vector2 Value;

        public LookInputEvent(Vector2 value) {
            Value = value;
        }
    }

    public readonly struct JumpInputEvent { }

    public readonly struct CrouchInputEvent {
        public readonly bool IsPressed;

        public CrouchInputEvent(bool isPressed) {
            IsPressed = isPressed;
        }
    }

    public readonly struct SprintInputEvent {
        public readonly bool IsPressed;

        public SprintInputEvent(bool isPressed) {
            IsPressed = isPressed;
        }
    }

    public readonly struct InteractInputEvent { }

    public readonly struct ZoomInputEvent {
        public readonly bool IsPressed;

        public ZoomInputEvent(bool isPressed) {
            IsPressed = isPressed;
        }
    }

    public readonly struct NextInputEvent { }

    public readonly struct PreviousInputEvent { }

    public readonly struct PauseToggleRequestedEvent { }

    public readonly struct DialogueAdvanceRequestedEvent { }

    public readonly struct PlayerSprintChangedEvent {
        public readonly bool IsSprinting;

        public PlayerSprintChangedEvent(bool isSprinting) {
            IsSprinting = isSprinting;
        }
    }

    public readonly struct InventoryToggledEvent {
        public readonly bool IsOpen;

        public InventoryToggledEvent(bool isOpen) {
            IsOpen = isOpen;
        }
    }

    public readonly struct InventoryItemAddedEvent {
        public readonly int Index;
        public readonly int TotalItems;

        public InventoryItemAddedEvent(int index, int totalItems) {
            Index = index;
            TotalItems = totalItems;
        }
    }

    public readonly struct InventoryItemRemovedEvent {
        public readonly int Index;
        public readonly int TotalItems;

        public InventoryItemRemovedEvent(int index, int totalItems) {
            Index = index;
            TotalItems = totalItems;
        }
    }

    public readonly struct ItemPickedUpEvent {
        public readonly string ItemName;
        public readonly GameObject ItemGameObject;

        public ItemPickedUpEvent(string itemName, GameObject itemGameObject) {
            ItemName = itemName;
            ItemGameObject = itemGameObject;
        }
    }
}