# The `Inventory` Directory: Item Management System

The **Inventory** directory implements **item storage, equipment management, and UI display**—inventory slots, item capacity, equipment system, and inventory updates.

---

## Purpose

Inventory System provides:
- **Item storage**: Slots with limited capacity
- **Item management**: Add, remove, equip items
- **Inventory UI**: Display items and slots
- **Equipment system**: Equipped items separate from stored items
- **Persistence**: Save/load inventory state

---

## Architecture

```
InventoryService.cs (core)
├── Item (abstract class)
│   ├── PickupItem
│   ├── EquipmentItem
│   └── ConsumableItem
├── InventorySlot
│   ├── itemReference
│   ├── quantity
│   └── isEquipped
└── IInventoryService<Item>

InventoryUI.cs (display)
├── SlotUI[] (display slots)
├── EquipmentSlotUI[] (show equipped items)
└── ItemDetailsPanel
```

---

## Key Files

### `InventoryService.cs`

Core inventory management logic.

```csharp
public class InventoryService : MonoBehaviour, IInventoryService<Item> {
    [SerializeField] private int _maxCapacity = 6;

    private List<InventorySlot> _slots = new();
    private EquipmentSlot _equippedItem;
    private IEventBus _eventBus;

    [System.Serializable]
    public struct InventorySlot {
        public Item item;
        public int quantity;
    }

    private void Start() {
        ServiceLocator.Register<IInventoryService<Item>>(this);
        _eventBus = ServiceLocator.Get<IEventBus>();

        // Initialize empty slots
        for (int i = 0; i < _maxCapacity; i++) {
            _slots.Add(new InventorySlot());
        }
    }

    public bool TryAdd(Item item, int quantity = 1) {
        // Try to stack if stackable
        if (item.isStackable) {
            for (int i = 0; i < _slots.Count; i++) {
                if (_slots[i].item != null && _slots[i].item.id == item.id) {
                    _slots[i].quantity += quantity;
                    _eventBus.Publish<InventoryChangedEvent>(new());
                    return true;
                }
            }
        }

        // Find empty slot
        for (int i = 0; i < _slots.Count; i++) {
            if (_slots[i].item == null) {
                _slots[i] = new InventorySlot { item = item, quantity = quantity };
                _eventBus.Publish<InventoryChangedEvent>(new());
                return true;
            }
        }

        return false;  // Inventory full
    }

    public bool TryRemove(int slotIndex, int quantity = 1) {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return false;

        var slot = _slots[slotIndex];
        if (slot.item == null) return false;

        slot.quantity -= quantity;

        if (slot.quantity <= 0) {
            _slots[slotIndex] = new InventorySlot();  // Empty slot
        } else {
            _slots[slotIndex] = slot;
        }

        _eventBus.Publish<InventoryChangedEvent>(new());
        return true;
    }

    public bool TryEquip(int slotIndex) {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return false;

        var slot = _slots[slotIndex];
        if (slot.item == null || !slot.item.isEquippable) return false;

        _equippedItem = new EquipmentSlot { item = slot.item, slotIndex = slotIndex };
        _eventBus.Publish<ItemEquippedEvent>(new(slot.item));
        return true;
    }

    public InventorySlot GetSlot(int index) => _slots[index];
    public IReadOnlyList<InventorySlot> GetAllSlots() => _slots.AsReadOnly();
    public int GetCapacity() => _maxCapacity;
}
```

---

### `Item.cs`

Base item class.

```csharp
public abstract class Item : ScriptableObject {
    public int id;
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    public bool isEquippable;
    public int maxStack = 1;

    public abstract void Use();
}
```

---

### Item Types

#### `PickupItem.cs`
```csharp
[CreateAssetMenu(fileName = "Pickup", menuName = "Items/Pickup")]
public class PickupItem : Item {
    public override void Use() {
        Debug.Log($"Using {itemName}");
    }
}
```

#### `EquipmentItem.cs`
```csharp
[CreateAssetMenu(fileName = "Equipment", menuName = "Items/Equipment")]
public class EquipmentItem : Item {
    [SerializeField] private int _defenseBonus;

    private void Awake() {
        isEquippable = true;
    }

    public override void Use() {
        Debug.Log($"Equipped {itemName} (+{_defenseBonus} defense)");
    }
}
```

#### `ConsumableItem.cs`
```csharp
[CreateAssetMenu(fileName = "Consumable", menuName = "Items/Consumable")]
public class ConsumableItem : Item {
    [SerializeField] private int _healthRestore;

    private void Awake() {
        isStackable = true;
        maxStack = 5;
    }

    public override void Use() {
        var player = PlayerController.Instance;
        player.Heal(_healthRestore);
        Debug.Log($"Used {itemName} (+{_healthRestore} health)");
    }
}
```

---

### `InventoryUI.cs`

Displays inventory on screen.

```csharp
public class InventoryUI : MonoBehaviour {
    [SerializeField] private Canvas _inventoryCanvas;
    [SerializeField] private InventorySlotUI[] _slotUIs;
    [SerializeField] private ItemDetailsPanel _detailsPanel;

    private IInventoryService<Item> _inventory;
    private IEventBus _eventBus;

    private void Start() {
        _inventory = ServiceLocator.Get<IInventoryService<Item>>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        _eventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

        // Setup slot UI
        for (int i = 0; i < _slotUIs.Length; i++) {
            int slotIndex = i;
            _slotUIs[i].OnClicked += () => OnSlotClicked(slotIndex);
        }

        UpdateDisplay();
    }

    private void OnInventoryChanged(InventoryChangedEvent evt) {
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        var slots = _inventory.GetAllSlots();

        for (int i = 0; i < _slotUIs.Length; i++) {
            if (slots[i].item != null) {
                _slotUIs[i].SetItem(slots[i].item, slots[i].quantity);
            } else {
                _slotUIs[i].SetEmpty();
            }
        }
    }

    private void OnSlotClicked(int slotIndex) {
        var slot = _inventory.GetSlot(slotIndex);
        if (slot.item != null) {
            _detailsPanel.Show(slot.item, slot.quantity);
        }
    }
}
```

---

### `InventorySlotUI.cs`

Individual slot visual representation.

```csharp
public class InventorySlotUI : MonoBehaviour {
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _quantityText;

    public System.Action OnClicked { get; set; }

    private void Start() {
        GetComponent<Button>().onClick.AddListener(() => OnClicked?.Invoke());
    }

    public void SetItem(Item item, int quantity) {
        _itemIcon.sprite = item.icon;
        _itemIcon.enabled = true;

        if (quantity > 1) {
            _quantityText.text = quantity.ToString();
        } else {
            _quantityText.text = "";
        }
    }

    public void SetEmpty() {
        _itemIcon.enabled = false;
        _quantityText.text = "";
    }
}
```

---

## Item Creation

### Creating Items in Editor

1. Right-click folder → Create → Items → [Item Type]
2. Fill in properties:
   - Name: "Bandage"
   - Icon: Select sprite
   - Stackable: true (for consumables)
   - Equippable: false
3. Save as asset

### Item Data Organization
```
Assets/_Game/Gameplay/Inventory/Items/
├── Consumables/
│   ├── Bandage.asset
│   └── Antidote.asset
├── Equipment/
│   ├── Vest.asset
│   └── Mask.asset
└── Key/
    ├── KeyCard.asset
    └── DoorKey.asset
```

---

## Usage

### Adding Item to Inventory

```csharp
// When player picks up item
var item = Resources.Load<Item>("Items/Bandage");
var inventory = ServiceLocator.Get<IInventoryService<Item>>();

if (inventory.TryAdd(item)) {
    Debug.Log("Item added");
} else {
    Debug.Log("Inventory full");
}
```

### Using Item

```csharp
// Player clicks item in UI
var item = inventory.GetSlot(0).item;
item.Use();  // Custom effect based on item type

// Remove from inventory if consumable
if (item is ConsumableItem) {
    inventory.TryRemove(0);
}
```

### Equipping Item

```csharp
if (inventory.TryEquip(0)) {
    Debug.Log("Item equipped");
}
```

---

## Best Practices

### 1. Use Scriptable Objects for Items

✅ **Good:**
```csharp
var bandage = Resources.Load<Item>("Items/Bandage");
// Easily reusable, editable in Inspector
```

### 2. Separate Item Types

✅ **Good:**
```
Items/
├── Consumables/
├── Equipment/
└── Keys/
```

### 3. Stackable vs. Unique

✅ **Good:**
```csharp
// Consumables are stackable
public class ConsumableItem : Item {
    private void Awake() {
        isStackable = true;
        maxStack = 5;
    }
}

// Equipment is unique
public class EquipmentItem : Item {
    private void Awake() {
        isStackable = false;
    }
}
```

### 4. Persist Inventory

✅ **Good:**
```csharp
public void SaveInventory() {
    var slots = _inventory.GetAllSlots();
    string json = JsonConvert.SerializeObject(slots);
    File.WriteAllText("inventory.json", json);
}
```

### 5. Publish Events on Changes

✅ **Good:**
```csharp
public bool TryAdd(Item item) {
    // ... add logic ...
    _eventBus.Publish<InventoryChangedEvent>(new());
    return true;
}
```

---

## Common Patterns

### Item Pickup

```csharp
public class Pickup : MonoBehaviour, IInteractable {
    [SerializeField] private Item _itemToPickup;

    public void Interact() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        
        if (inventory.TryAdd(_itemToPickup)) {
            Destroy(gameObject);  // Remove pickup
        } else {
            Debug.Log("Inventory full");
        }
    }
}
```

### Inventory Display

```
┌─────────────────────┐
│ Inventory (6/6)     │
├─────────────────────┤
│ □ □ □ □ □ □ │
│ □ □ □ □ □ □ │
├─────────────────────┤
│ [Item Details Panel]│
└─────────────────────┘
```

### Quick Use

```csharp
// Player presses number key 1-6
if (Input.GetKeyDown(KeyCode.Alpha1)) {
    var item = _inventory.GetSlot(0).item;
    item.Use();
}
```

---

## Extending Inventory

### Quest Items

```csharp
[CreateAssetMenu(fileName = "Quest", menuName = "Items/Quest")]
public class QuestItem : Item {
    [SerializeField] private int _questId;
    
    public override void Use() {
        // Trigger quest event
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus.Publish<QuestItemUsedEvent>(new(_questId));
    }
}
```

### Crafting System

```csharp
public bool TryCraft(Recipe recipe) {
    // Check if inventory has all materials
    foreach (var material in recipe.materials) {
        if (!HasItem(material.item, material.quantity)) {
            return false;
        }
    }

    // Remove materials
    // Add crafted item
    return true;
}
```

---

## Summary

The **Inventory** system provides:
- **Item storage**: Slots with capacity limits
- **Item types**: Consumables, equipment, quest items
- **UI integration**: Visual inventory display
- **Stackable items**: Combine identical items
- **Equipment system**: Equip and track equipped items

By using Inventory system:
- **Players** can collect and manage items
- **Items** are flexible and extensible
- **UI** updates automatically
- **Game design** is data-driven

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Gameplay/Delivery/README.md](../Delivery/README.md) - Quest items
