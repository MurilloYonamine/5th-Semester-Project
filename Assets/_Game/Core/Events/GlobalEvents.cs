using UnityEngine;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Events {
    public readonly struct GameStateChangedEvent {
        public readonly GameState PreviousState;
        public readonly GameState CurrentState;

        public GameStateChangedEvent(GameState previous, GameState current) {
            PreviousState = previous;
            CurrentState = current;
        }
    }


    /// ============= Input Events =============
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

    /// ============= Pause Events =============
    public readonly struct PauseToggleRequestedEvent { }

    public readonly struct PauseStateChangedEvent {
        public readonly bool IsPaused;

        public PauseStateChangedEvent(bool isPaused) {
            IsPaused = isPaused;
        }
    }

    /// ============= Dialogue Events =============
    public readonly struct DialogueAdvanceRequestedEvent { }

    public readonly struct DialogueStartedEvent { }

    public readonly struct DialogueEndedEvent { }

    /// ============= Player Events =============
    public readonly struct PlayerSprintChangedEvent {
        public readonly bool IsSprinting;

        public PlayerSprintChangedEvent(bool isSprinting) {
            IsSprinting = isSprinting;
        }
    }

    /// ============= Inventory & Item Events =============
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