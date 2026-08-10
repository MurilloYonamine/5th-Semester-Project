# The `Delivery` Directory: Objectives & Delivery System

The **Delivery** directory implements **delivery points and objective tracking**—players deliver items to specific locations to complete missions.

---

## Purpose

Delivery System provides:
- **Delivery points**: Designated locations where items are delivered
- **Objective tracking**: Monitor delivery progress
- **Completion detection**: Check if player has delivered correct items
- **Reward system**: Grant rewards upon successful delivery
- **Quest integration**: Connect to larger quest/objective system

---

## Architecture

```
DeliveryService.cs (orchestrator)
├── DeliveryPoint (MonoBehaviour)
│   ├── requiredItems[]
│   ├── location
│   └── rewardData
├── Objective (data class)
│   ├── id
│   ├── requiredItems
│   ├── isComplete
│   └── reward
└── ObjectiveUI
    ├── objective list
    └── progress display
```

---

## Key Files

### `DeliveryPoint.cs`

Interactive location where player delivers items.

<<<<<<< HEAD
If the same GameObject also has a `DialogueTrigger`, the delivery point can forward the interaction to it after a successful delivery or when the delivery is already completed.

`DeliveryPoint` also exposes whether that delivery cutscene has already played, so other systems can react only after the cutscene starts.

=======
>>>>>>> origin/main
```csharp
public class DeliveryPoint : MonoBehaviour, IInteractable {
    [SerializeField] private int _objectiveId;
    [SerializeField] private Item[] _requiredItems;
    [SerializeField] private int[] _requiredQuantities;
    [SerializeField] private Reward _reward;

    [Header("Visual")]
    [SerializeField] private Outline _outline;
    [SerializeField] private GameObject _completedMarker;

    private bool _isComplete = false;
    private IEventBus _eventBus;

    public bool IsInteractable => !_isComplete;

    private void Start() {
        _eventBus = ServiceLocator.Get<IEventBus>();
    }

    public void Interact() {
        if (_isComplete) {
            Debug.Log("Objective already complete");
            return;
        }

        if (CanCompleteDelivery()) {
            CompleteDelivery();
        } else {
            ShowMissingItems();
        }
    }

    private bool CanCompleteDelivery() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        var slots = inventory.GetAllSlots();

        for (int i = 0; i < _requiredItems.Length; i++) {
            int required = _requiredQuantities[i];
            int have = 0;

            // Count items in inventory
            foreach (var slot in slots) {
                if (slot.item != null && slot.item.id == _requiredItems[i].id) {
                    have += slot.quantity;
                }
            }

            if (have < required) return false;
        }

        return true;
    }

    private void CompleteDelivery() {
        // Remove items from inventory
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        
        for (int i = 0; i < _requiredItems.Length; i++) {
            int toRemove = _requiredQuantities[i];
            var slots = inventory.GetAllSlots();

            for (int j = 0; j < slots.Count && toRemove > 0; j++) {
                if (slots[j].item != null && slots[j].item.id == _requiredItems[i].id) {
                    int removeAmount = Mathf.Min(toRemove, slots[j].quantity);
                    inventory.TryRemove(j, removeAmount);
                    toRemove -= removeAmount;
                }
            }
        }

        // Mark complete
        _isComplete = true;
        _completedMarker.SetActive(true);
        _outline.enabled = false;

        // Give reward
        ApplyReward();

        // Publish event
        _eventBus.Publish<ObjectiveCompletedEvent>(new(_objectiveId, _reward));

        Debug.Log($"Delivery {_objectiveId} complete!");
    }

    private void ApplyReward() {
        // Give reward to player
        if (_reward.healthRestore > 0) {
            PlayerController.Instance.Heal(_reward.healthRestore);
        }

        if (_reward.staminaRestore > 0) {
            // Restore stamina
        }

        // Play sound/animation
        var audio = ServiceLocator.Get<IAudioService>();
        audio.PlaySFX("UI/Notification");
    }

    private void ShowMissingItems() {
        // Show UI saying what items are needed
        Debug.Log("Missing required items for delivery");
    }

    public void EnableOutline(bool enable) {
        _outline.enabled = enable;
    }
}

[System.Serializable]
public struct Reward {
    public int healthRestore;
    public int staminaRestore;
    public int experiencePoints;
}
```

---

### `DeliveryService.cs`

Tracks all active and completed objectives.

```csharp
public class DeliveryService : MonoBehaviour {
    private Dictionary<int, Objective> _objectives = new();
    private HashSet<int> _completedObjectives = new();
    private IEventBus _eventBus;

    [System.Serializable]
    public struct Objective {
        public int id;
        public string description;
        public Item[] requiredItems;
        public int[] requiredQuantities;
        public bool isComplete;
    }

    private void Start() {
        ServiceLocator.Register<IDeliveryService>(this);
        _eventBus = ServiceLocator.Get<IEventBus>();
        
        _eventBus.Subscribe<ObjectiveCompletedEvent>(OnObjectiveCompleted);
        
        // Load objectives
        LoadObjectives();
    }

    private void LoadObjectives() {
        // Load from ScriptableObject or JSON
        var objectives = Resources.Load<ObjectiveList>("Objectives/MainObjectives");
        
        foreach (var objective in objectives.objectives) {
            _objectives[objective.id] = objective;
        }
    }

    private void OnObjectiveCompleted(ObjectiveCompletedEvent evt) {
        if (_objectives.TryGetValue(evt.ObjectiveId, out var objective)) {
            _completedObjectives.Add(evt.ObjectiveId);
            Debug.Log($"Objective {objective.description} completed!");
        }
    }

    public IReadOnlyList<Objective> GetActiveObjectives() {
        return _objectives.Values
            .Where(o => !_completedObjectives.Contains(o.id))
            .ToList();
    }

    public bool IsObjectiveComplete(int objectiveId) {
        return _completedObjectives.Contains(objectiveId);
    }

    public Objective GetObjective(int objectiveId) {
        return _objectives[objectiveId];
    }
}
```

---

### `ObjectiveUI.cs`

Displays objective progress to player.

```csharp
public class ObjectiveUI : MonoBehaviour {
    [SerializeField] private Transform _objectiveList;
    [SerializeField] private ObjectiveItemUI _objectiveItemPrefab;

    private IDeliveryService _deliveryService;
    private IEventBus _eventBus;
    private Dictionary<int, ObjectiveItemUI> _objectiveUIs = new();

    private void Start() {
        _deliveryService = ServiceLocator.Get<IDeliveryService>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        _eventBus.Subscribe<ObjectiveCompletedEvent>(OnObjectiveCompleted);
        
        RefreshObjectives();
    }

    private void RefreshObjectives() {
        var objectives = _deliveryService.GetActiveObjectives();

        foreach (var objective in objectives) {
            if (!_objectiveUIs.ContainsKey(objective.id)) {
                var ui = Instantiate(_objectiveItemPrefab, _objectiveList);
                ui.SetObjective(objective);
                _objectiveUIs[objective.id] = ui;
            }
        }
    }

    private void OnObjectiveCompleted(ObjectiveCompletedEvent evt) {
        if (_objectiveUIs.TryGetValue(evt.ObjectiveId, out var ui)) {
            ui.MarkComplete();
        }

        RefreshObjectives();
    }
}
```

---

### `ObjectiveItemUI.cs`

Individual objective display in list.

```csharp
public class ObjectiveItemUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Image _completionImage;

    private IDeliveryService.Objective _objective;

    public void SetObjective(IDeliveryService.Objective objective) {
        _objective = objective;
        _descriptionText.text = objective.description;

        // Show required items
        string progress = "Required items: ";
        for (int i = 0; i < objective.requiredItems.Length; i++) {
            progress += $"{objective.requiredItems[i].itemName}({objective.requiredQuantities[i]}x) ";
        }
        _progressText.text = progress;

        _completionImage.enabled = false;
    }

    public void MarkComplete() {
        _descriptionText.text = "✓ " + _objective.description;
        _completionImage.enabled = true;
    }
}
```

---

## Setup

### Creating Objectives

1. Create ScriptableObject for objectives:
```csharp
[CreateAssetMenu(fileName = "Objectives", menuName = "Objectives/List")]
public class ObjectiveList : ScriptableObject {
    public Objective[] objectives;
}
```

2. In Editor:
   - Create asset at `Resources/Objectives/MainObjectives`
   - Set objectives array
   - For each objective:
     - Set ID, description, required items, quantities

### Placing Delivery Points

1. Create GameObject in scene
2. Add `DeliveryPoint.cs` component
3. In Inspector:
   - Set Objective ID (must match in ObjectiveList)
   - Drag required items into array
   - Set required quantities
   - Drag outline component
   - Set completed marker (visual indicator when done)

---

## Objective Flow

```
Game starts
    ↓
Objectives loaded from resources
    ↓
Objectives displayed in UI
    ↓
Player collects required items
    ↓
Player approaches delivery point
    ↓
Player interacts with delivery point
    ├─ Check: Has required items?
    │  ├─ YES → Remove items, mark complete, give reward
    │  └─ NO → Show message, return
    ↓
ObjectiveCompletedEvent published
    ↓
UI updates, objective marked complete
    ↓
New objectives appear (if chain quest)
```

---

## Best Practices

### 1. Use ScriptableObjects for Objectives

✅ **Good:**
```csharp
// Load from resources, editable in Inspector
var objectives = Resources.Load<ObjectiveList>("Objectives/Main");
```

### 2. Validate Item Counts

✅ **Good:**
```csharp
private bool CanCompleteDelivery() {
    // Check each required item
    for (int i = 0; i < _requiredItems.Length; i++) {
        if (!HasEnoughItems(_requiredItems[i], _requiredQuantities[i])) {
            return false;
        }
    }
    return true;
}
```

### 3. Publish Events on Completion

✅ **Good:**
```csharp
private void CompleteDelivery() {
    // ... complete logic ...
    _eventBus.Publish<ObjectiveCompletedEvent>(new(_objectiveId, _reward));
}
```

### 4. Provide Feedback to Player

✅ **Good:**
```csharp
// Show what items are needed
private void ShowMissingItems() {
    var missing = GetMissingItems();
    UI.ShowNotification($"Missing: {string.Join(", ", missing)}");
}
```

### 5. Track Progress Visually

✅ **Good:**
```
[✓] Deliver Medicine to Hospital
[ ] Find Food for Refugee
[ ] Return Key to Guard
```

---

## Common Patterns

### Simple Delivery Quest

```csharp
// Player needs to deliver 1 Bandage to Hospital

// 1. Create objective
Objective quest = new() {
    id = 1,
    description = "Deliver Medicine",
    requiredItems = new[] { bandageItem },
    requiredQuantities = new[] { 1 }
};

// 2. Create delivery point at hospital
// 3. Player gets bandage
// 4. Player goes to hospital and interacts
// 5. Delivery completes
```

### Multi-Item Delivery

```csharp
// Player must deliver multiple different items

requiredItems = new[] { food, water, medicine };
requiredQuantities = new[] { 3, 2, 1 };
// Player must have all three item types with correct quantities
```

### Chained Quests

```csharp
private void OnObjectiveCompleted(ObjectiveCompletedEvent evt) {
    if (evt.ObjectiveId == 1) {
        // Unlock objective 2
        _deliveryService.UnlockObjective(2);
    }
}
```

---

## Extending Delivery

### Delivery Restrictions

```csharp
public class RestrictedDeliveryPoint : DeliveryPoint {
    [SerializeField] private float _timeLimit;
    
    public override bool CanDeliver() {
        if (Time.timeSinceLevelLoad > _timeLimit) {
            return false;  // Time expired
        }
        return base.CanDeliver();
    }
}
```

### Objective Rewards

```csharp
private void ApplyReward() {
    var eventBus = ServiceLocator.Get<IEventBus>();
    
    // Give experience
    eventBus.Publish<ExperienceGainedEvent>(new(_reward.exp));
    
    // Give currency
    eventBus.Publish<CurrencyGainedEvent>(new(_reward.money));
    
    // Unlock next quest
    eventBus.Publish<QuestUnlockedEvent>(new(_nextQuestId));
}
```

---

## Summary

The **Delivery** system provides:
- **Objective tracking**: Monitor delivery progress
- **Delivery points**: Interactive locations for deliveries
- **Item verification**: Check player has required items
- **Reward system**: Grant rewards upon completion
- **Quest chains**: Support multiple connected objectives

By using Delivery system:
- **Players** have clear goals
- **Objectives** drive gameplay progression
- **Items** have purpose and value
- **Quests** can be complex and interconnected

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Gameplay/Inventory/README.md](../Inventory/README.md) - Item system
