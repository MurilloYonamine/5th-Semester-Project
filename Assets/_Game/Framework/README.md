# The `Framework` Directory: Reusable Systems & Architecture

The **Framework** layer contains **generic, game-agnostic systems** that could theoretically be ported to an entirely different project. These are foundational architectures and UI components that provide structure without depending on specific game logic.

This layer is built on the principle: **Framework never depends on Features or Gameplay.**

---

## Architecture Overview

Framework is divided into two main domains:

1. **`Behaviour Trees/`**: Custom-built AI decision-making system (Sequences, Selectors, Parallels, Blackboard)
2. **`UI/`**: Generic UI components and templates (OptionSelector, etc.)

---

## 1. Behaviour Trees (`Framework/Behaviour Trees`)

A **lightweight, custom AI system** for creating intelligent agent behaviors without external plugins.

### Purpose

Instead of writing complex `if-else` statements or massive switch statements for AI decisions, designers can visually compose behavior trees:

```
┌─ Selector (Try options until one succeeds)
│  ├─ Sequence (Do all steps in order)
│  │  ├─ Can see player?
│  │  └─ Chase player
│  └─ Sequence (Fallback)
│     ├─ Patrol waypoints
│     └─ Wait
```

### Core Concepts

- **Nodes**: Individual decision or action units
- **Status**: `Success`, `Failure`, or `Running` (ongoing)
- **Composition**: Nodes combine via Sequence, Selector, and Parallel
- **Blackboard**: Shared memory for AI state (target position, enemy health, etc.)

### Key Files

See [Behaviour Trees/README.md](./Behaviour%20Trees/README.md) for detailed documentation.

### Quick Example

```csharp
// Build a simple AI tree
var blackboard = new Blackboard();
var tree = new BehaviourTree("EnemyAI",
    new Selector("Root",
        new Sequence("ChaseBehavior",
            new CanSeePlayerNode("CanSeePlayer", blackboard),
            new ChasePlayerNode("Chase", blackboard)
        ),
        new PatrolNode("Patrol", blackboard)
    )
);

// Run the tree every frame
void Update()
{
    tree.Process();
}
```

---

## 2. UI (`Framework/UI`)

Generic, reusable UI components that don't depend on specific game mechanics.

### Purpose

Provide **drop-in UI widgets** for common interaction patterns (selectors, dialogs, menus) that can be customized per-feature.

### Current Components

#### OptionSelector

A **dropdown-style selector** for cycling through options with keyboard/gamepad input.

```csharp
var selector = GetComponent<OptionSelector>();
selector.Initialize(new List<string> { "Easy", "Normal", "Hard" }, startIndex: 1);
selector.OnValueChanged += (selectedIndex) => ApplyDifficulty(selectedIndex);
```

**Features:**
- Keyboard & gamepad support (left/right arrows or analog stick)
- Visual button feedback (flash on selection)
- Extensible—add custom logic for any option type

See [UI/README.md](./UI/README.md) for detailed documentation.

---

## Design Principles

### 1. Framework is Game-Agnostic

✅ **Good:**
```csharp
// Behaviour Tree system works for ANY AI
public class ChasePlayerNode : Node { }  // Can be used by Enemies, NPCs, Drones, etc.
```

❌ **Bad:**
```csharp
// Specific to one game
public class EnemyEyeChasePlayerNode : Node { }  // Too specific
```

### 2. Framework Provides Structure, Not Content

✅ **Good:**
```csharp
// Framework provides the system
public class BehaviourTree { }

// Features implement the logic
public class ZombieAI : MonoBehaviour
{
    private BehaviourTree _tree;
    
    void Start()
    {
        _tree = BuildZombieTree();
    }
}
```

### 3. Framework Respects Dependency Rules

```
┌──────────────────────────┐
│ Gameplay Features        │
│ (depend on Framework)    │
└──────────────────────────┘
          ↑
┌──────────────────────────┐
│ Framework                │
│ (NEVER depends on        │
│  Gameplay or Features)   │
└──────────────────────────┘
```

---

## Common Usage Patterns

### 1. Building AI with Behaviour Trees

```csharp
// In Gameplay/Enemy/EnemyAI.cs
public class EnemyAI : MonoBehaviour
{
    private BehaviourTree _tree;
    private Blackboard _memory;

    private void Start()
    {
        _memory = new Blackboard();
        _tree = BuildBehaviourTree();
    }

    private BehaviourTree BuildBehaviourTree()
    {
        return new BehaviourTree("ZombieAI",
            new Selector("Root",
                new Sequence("ChaseBehavior",
                    new CheckPlayerVisibleNode(_memory),
                    new ChasePlayerNode(_memory)
                ),
                new PatrolNode(_memory)
            )
        );
    }

    private void Update()
    {
        _tree.Process();
    }
}
```

### 2. Using OptionSelector for Game Settings

```csharp
// In UI/SettingsPanel.cs
public class SettingsPanel : MonoBehaviour
{
    private OptionSelector _difficultySelector;

    private void Start()
    {
        _difficultySelector = GetComponentInChildren<OptionSelector>();
        _difficultySelector.Initialize(
            new List<string> { "Easy", "Normal", "Hard", "Nightmare" },
            startIndex: 1
        );
        _difficultySelector.OnValueChanged += OnDifficultyChanged;
    }

    private void OnDifficultyChanged(int selectedIndex)
    {
        Debug.Log($"Difficulty set to: {selectedIndex}");
    }
}
```

---

## Best Practices

### 1. Extend Framework for Game-Specific Logic

❌ **Bad:**
```csharp
// Polluting Framework with game logic
public class EnemyChaseNode : Node { }  // Framework shouldn't know about Enemies
```

✅ **Good:**
```csharp
// Framework provides the structure
public class Node { }

// Features implement specific nodes
// In Gameplay/Enemy/AI/EnemyChaseNode.cs
public class EnemyChaseNode : Node { }
```

### 2. Use Blackboard for AI Memory

✅ **Good:**
```csharp
// Store AI state in Blackboard
_blackboard.SetData("LastSeenPlayerPosition", transform.position);
_blackboard.SetData("IsAlerted", true);

// Retrieve when needed
Vector3 lastPos = _blackboard.GetData<Vector3>("LastSeenPlayerPosition");
```

### 3. Keep Nodes Focused

✅ **Good:**
```csharp
// One responsibility per node
public class CanSeePlayerNode : Node { }
public class ChasePlayerNode : Node { }
public class PatrolNode : Node { }
```

❌ **Bad:**
```csharp
// Too many responsibilities
public class AINode : Node
{
    // Handles seeing, chasing, patrolling, attacking...
}
```

### 4. Document Tree Structure

✅ **Good:**
```csharp
private BehaviourTree BuildEnemyTree()
{
    // Structure:
    // Selector
    //   ├─ Sequence (Attack if can see)
    //   │   ├─ CanSeePlayer
    //   │   └─ AttackPlayer
    //   └─ Sequence (Patrol fallback)
    //       ├─ HasPatrolPath
    //       └─ Patrol

    return new BehaviourTree("EnemyAI",
        new Selector("Root",
            new Sequence("AttackBehavior",
                new CanSeePlayerNode(_memory),
                new AttackPlayerNode(_memory)
            ),
            new PatrolNode(_memory)
        )
    );
}
```

---

## Summary

The **Framework** layer provides:
- **Behaviour Tree System**: Flexible, composable AI architecture
- **Generic UI Components**: Reusable widgets (OptionSelector, etc.)
- **Game-agnostic design**: Can be ported to other projects
- **Clean separation**: No feature-specific logic in Framework

By using Framework instead of rolling custom AI systems, PHOTOSSYNC gains:
- **Scalability**: Easy to add new nodes and behaviors
- **Maintainability**: Clear structure and composition
- **Reusability**: Framework systems work across different game projects
- **Designer-friendly**: Behavior trees are intuitive to visualize and modify

**See also:**
- [Behaviour Trees/README.md](./Behaviour%20Trees/README.md) - Detailed AI system documentation
- [UI/README.md](./UI/README.md) - UI component documentation
