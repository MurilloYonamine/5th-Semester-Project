# The `Behaviour Trees` Directory: Composable AI System

The **Behaviour Trees** system is a **custom-built, lightweight AI architecture** that allows designers to compose complex behaviors from simple, reusable nodes. Instead of writing nested if-else statements, you build tree structures that are easy to read, modify, and extend.

---

## Core Concepts

### Status System

Every node returns a **Status** indicating its outcome:

```csharp
public enum Status {
    Success,   // Node completed successfully
    Failure,   // Node failed (condition not met, action interrupted, etc.)
    Running    // Node is still executing (async operations, timers, etc.)
}
```

### Node Types

1. **Composite Nodes**: Contain children, decide how to process them
   - `Sequence`: All children must succeed (AND logic)
   - `Selector`: First child to succeed wins (OR logic)
   - `Parallel`: All children run simultaneously

2. **Decorator Nodes**: Wrap a single child, modify its behavior
   - `Abort`: Interrupt child if condition becomes true

3. **Leaf Nodes**: No children, actual actions/checks
   - `ActionWait`: Wait for a duration
   - Custom nodes: Check player visible, chase, patrol, etc.

### The Blackboard

A **shared dictionary** of AI memory—used to pass data between nodes without tight coupling.

```csharp
// Store
blackboard.SetData("TargetPosition", playerPos);

// Retrieve
Vector3 target = blackboard.GetData<Vector3>("TargetPosition");
```

---

## Architecture

### Base Node Class

```csharp
public class Node {
    public enum Status { Success, Failure, Running }

    public readonly string Name;
    public readonly List<Node> Children = new List<Node>();
    
    protected int _currentChild;  // For Sequence/Selector iteration
    public Blackboard Blackboard;

    public virtual Status Process() { }
    public virtual void Reset() { }
}
```

**Key Properties:**
- `Name`: Identifier for debugging
- `Children`: List of child nodes
- `_currentChild`: Tracks position in iteration (for Sequence/Selector)
- `Blackboard`: Reference to shared memory

---

## Key Files

### `Node.cs` (Base Class)

The foundation of the entire system.

```csharp
// Create a simple action node
var waitNode = new ActionWait(2f, "Wait 2 seconds");

// Add as child to a Sequence
sequence.AddChild(waitNode);
```

**Methods:**
```csharp
// Execute the node
Status Process();

// Reset state (called when node finishes)
void Reset();

// Add a child node
void AddChild(Node child);
```

---

### `BehaviourTree.cs` (Executor)

The main driver that executes your tree every frame.

```csharp
var tree = new BehaviourTree("MyAI", rootNode);

// In Update()
void Update() {
    tree.Process();
    // If Status != Running, tree resets automatically
}
```

**How It Works:**
1. Calls `rootNode.Process()` each frame
2. If result is NOT `Running`, calls `rootNode.Reset()`
3. Returns the status for debugging

---

### `Sequence.cs` (Composite: AND Logic)

Runs children **in order** until one fails.

```
Sequence
├─ Check(PlayerVisible)    → Success
├─ Action(ChasePlayer)     → Running
├─ Action(AttackPlayer)    → (not reached yet)
└─ Result: Running (waiting for Chase to finish)
```

**Logic:**
- ✅ Child Success → move to next child
- ❌ Child Failure → Sequence fails immediately (reset and return Failure)
- ⏳ Child Running → Sequence returns Running (continue next frame)

**Use Case:** "Do A, then B, then C" (all must complete)

```csharp
var patrol = new Sequence("PatrolBehavior",
    new MoveToWaypointNode(),
    new WaitAtWaypointNode(2f),
    new MoveToNextWaypointNode()
);
```

---

### `Selector.cs` (Composite: OR Logic)

Runs children **in order** until one succeeds.

```
Selector
├─ Check(CanAttack)         → Failure
├─ Check(CanChase)          → Success ← STOP! This succeeded
├─ Action(Patrol)           → (not reached)
└─ Result: Success (now reset and wait for next frame)
```

**Logic:**
- ✅ Child Success → Selector succeeds immediately (return Success)
- ❌ Child Failure → move to next child
- ⏳ Child Running → Selector returns Running (continue next frame)

**Use Case:** "Try A, if that fails try B, etc." (first success wins)

```csharp
var behaviorSelection = new Selector("RootBehavior",
    new Sequence("AttackBehavior",
        new CanSeePlayerNode(),
        new AttackPlayerNode()
    ),
    new Sequence("ChaseBehavior",
        new IsPlayerNearNode(),
        new ChasePlayerNode()
    ),
    new PatrolNode()  // Fallback
);
```

---

### `Parallel.cs` (Composite: Concurrent)

Runs **all children simultaneously** every frame.

```
Parallel
├─ Action(Chase)    → Running
├─ Action(Scan)     → Success
├─ Action(Roar)     → Success
└─ Result: Running (waiting for Chase to finish)
```

**Logic:**
- ✅ All Success → Parallel succeeds (return Success)
- ❌ ANY Failure → Parallel fails immediately (return Failure)
- ⏳ Some Running → Parallel returns Running (continue next frame)

**Use Case:** "Do A and B and C all at the same time"

```csharp
var simultaneous = new Parallel("ChasAndSound",
    new ChasePlayerNode(),          // Move toward player
    new PlayAttackSoundNode()        // Play audio simultaneously
);
```

---

### `Abort.cs` (Decorator: Conditional Interrupt)

Wraps a child and interrupts if a condition becomes true.

```csharp
var abortOnPlayerDeath = new Abort(
    abortCondition: () => player.IsDead,
    name: "AbortIfPlayerDead"
);
abortOnPlayerDeath.AddChild(new ChasePlayerNode());
```

**Logic:**
- ✅ Condition NOT met → run child normally
- ❌ Condition MET → interrupt child, return Failure

**Use Case:** "Do this, but stop immediately if X happens"

```csharp
// Chase player, but give up if player enters a forbidden zone
var smartChase = new Abort(
    abortCondition: () => forbiddenZone.Contains(player.position),
    name: "ChasePlayerUntilForbiddenZone"
);
smartChase.AddChild(new ChasePlayerNode());
```

---

### `Blackboard.cs` (Shared Memory)

Dictionary for AI state—allows nodes to share data without coupling.

```csharp
// Store data
blackboard.SetData("TargetPlayer", player);
blackboard.SetData("LastSeenPosition", player.position);
blackboard.SetData("AlertLevel", 0.5f);

// Retrieve data
GameObject target = blackboard.GetData<GameObject>("TargetPlayer");
Vector3 lastSeen = blackboard.GetData<Vector3>("LastSeenPosition");
float alert = blackboard.GetData<float>("AlertLevel");

// Check if key exists
if (blackboard.HasKey("TargetPlayer"))
{
    // Data was set previously
}

// Clear specific data
blackboard.ClearData("TargetPlayer");
```

---

### `ActionWait.cs` (Built-in Action)

A simple example of a leaf node that waits for a duration.

```csharp
public class ActionWait : Node {
    private float waitTime;
    private float startTime;
    private bool isWaiting;

    public ActionWait(float waitTime, string name = "Wait") : base(name) {
        this.waitTime = waitTime;
    }

    public override Status Process() {
        if (!isWaiting) {
            startTime = Time.time;
            isWaiting = true;
        }

        if (Time.time - startTime >= waitTime) {
            return Status.Success;
        }

        return Status.Running;
    }

    public override void Reset() {
        base.Reset();
        isWaiting = false;
    }
}
```

**Usage:**
```csharp
var waitNode = new ActionWait(3f, "Wait 3 seconds");
```

---

## Building Custom Nodes

### Leaf Node (No Children)

```csharp
public class CanSeePlayerNode : Node {
    private Blackboard _blackboard;

    public CanSeePlayerNode(Blackboard blackboard, string name = "CanSeePlayer") 
        : base(name) {
        _blackboard = blackboard;
    }

    public override Status Process() {
        GameObject player = _blackboard.GetData<GameObject>("TargetPlayer");
        
        if (player == null) return Status.Failure;
        
        // Simple line-of-sight check
        Vector3 direction = (player.transform.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit)) {
            if (hit.collider.gameObject == player) {
                return Status.Success;
            }
        }
        
        return Status.Failure;
    }
}
```

### Composite Node

For most uses, the built-in composites (Sequence, Selector, Parallel) are sufficient. Custom composites are rare.

---

## Complete Example: Enemy AI

```csharp
public class EnemyAI : MonoBehaviour {
    private BehaviourTree _tree;
    private Blackboard _memory;

    private void Start() {
        _memory = new Blackboard();
        _memory.SetData("TargetPlayer", FindObjectOfType<Player>().gameObject);

        _tree = new BehaviourTree("EnemyAI", BuildTree());
    }

    private Node BuildTree() {
        return new Selector("Root",
            // Try attacking
            new Sequence("AttackBehavior",
                new CanSeePlayerNode(_memory),
                new IsPlayerCloseNode(_memory, 5f),
                new AttackPlayerNode(_memory)
            ),

            // Try chasing
            new Sequence("ChaseBehavior",
                new CanSeePlayerNode(_memory),
                new Parallel("ChaseAndRotate",
                    new ChasePlayerNode(_memory),
                    new RotateTowardPlayerNode(_memory)
                )
            ),

            // Fallback: patrol
            new PatrolNode(_memory)
        );
    }

    private void Update() {
        _tree.Process();
    }
}
```

---

## Tree Evaluation Examples

### Example 1: Attack Sequence

```
Frame 1:
Selector("Root").Process()
  ├─ Sequence("AttackBehavior").Process()
  │   ├─ CanSeePlayerNode.Process() → Success
  │   ├─ IsPlayerCloseNode.Process() → Success
  │   └─ AttackPlayerNode.Process() → Running
  │   Result: Running ← Propagate up
  └─ (not reached)
Result: Running

Frame 2:
Selector("Root").Process()
  ├─ Sequence("AttackBehavior").Process()
  │   ├─ CanSeePlayerNode.Process() → Success
  │   ├─ IsPlayerCloseNode.Process() → Success
  │   └─ AttackPlayerNode.Process() → Success
  │   Result: Success ← All children succeeded
  │   Reset called
  └─ (not reached)
Result: Success
Reset selector and sequence
```

### Example 2: Fallback to Patrol

```
Selector("Root").Process()
  ├─ Sequence("AttackBehavior").Process()
  │   ├─ CanSeePlayerNode.Process() → Failure ← Player not visible
  │   Result: Failure
  │   Reset called
  ├─ Sequence("ChaseBehavior").Process()
  │   ├─ CanSeePlayerNode.Process() → Failure
  │   Result: Failure
  │   Reset called
  └─ PatrolNode.Process() → Running ← Move to fallback
Result: Running
```

---

## Best Practices

### 1. Keep Nodes Focused

❌ **Bad:**
```csharp
public class AINode : Node {
    public override Status Process() {
        // Too many responsibilities
        CheckPlayer();
        Attack();
        Patrol();
        Evade();
        // ...
    }
}
```

✅ **Good:**
```csharp
public class CanSeePlayerNode : Node { }
public class AttackPlayerNode : Node { }
public class PatrolNode : Node { }
```

### 2. Use Blackboard for State

❌ **Bad:**
```csharp
// Coupling between nodes
public class ChasePlayerNode {
    private Vector3 lastSeenPosition;  // Private state
}
```

✅ **Good:**
```csharp
public class ChasePlayerNode {
    private Blackboard _memory;
    
    public override Status Process() {
        Vector3 target = _memory.GetData<Vector3>("LastSeenPosition");
    }
}
```

### 3. Clear Data When Done

✅ **Good:**
```csharp
// In a failure or reset scenario
public override Status Process() {
    if (playerDead) {
        _blackboard.ClearData("TargetPlayer");
        return Status.Failure;
    }
}
```

### 4. Document Tree Structure

✅ **Good:**
```csharp
private Node BuildTree() {
    // Selector(Root)
    //   ├─ Sequence(Attack)
    //   │   ├─ CanSeePlayer
    //   │   └─ AttackPlayer
    //   └─ PatrolNode(Fallback)

    return new Selector("Root",
        new Sequence("Attack",
            new CanSeePlayerNode(_memory),
            new AttackPlayerNode(_memory)
        ),
        new PatrolNode(_memory)
    );
}
```

### 5. Test Individual Nodes

✅ **Good:**
```csharp
[Test]
public void TestCanSeePlayer() {
    var blackboard = new Blackboard();
    var node = new CanSeePlayerNode(blackboard);
    
    blackboard.SetData("TargetPlayer", testPlayer);
    
    Assert.AreEqual(Node.Status.Success, node.Process());
}
```

---

## Common Patterns

### Conditional Execution

```csharp
new Sequence("ConditionalAttack",
    new Abort(
        () => player.IsInvisible,
        name: "AbortIfInvisible"
    ),
    new AttackPlayerNode()
)
```

### Parallel Actions

```csharp
new Parallel("ChasAndShoot",
    new ChasePlayerNode(),
    new ShootAtPlayerNode()
)
```

### Retry Logic

```csharp
new Sequence("ChaseWithRetry",
    new ChasePlayerNode(),
    new ActionWait(2f),  // Wait, then try again
    new ChasePlayerNode()
)
```

---

## Performance Considerations

- **Frame Rate Stable**: Trees are lightweight, designed to run every frame
- **Lazy Evaluation**: Selector stops at first success (no wasted checks)
- **Memory**: Blackboard is just a Dictionary (minimal overhead)
- **Scale**: Handle hundreds of agents with simple tree structures

---

## Summary

The **Behaviour Trees** system provides:
- **Composable nodes**: Build complex behaviors from simple pieces
- **Clear logic**: Sequence (AND), Selector (OR), Parallel (concurrent)
- **Shared memory**: Blackboard for AI state
- **Interruptible**: Abort conditions for dynamic behavior
- **Lightweight**: Minimal overhead, scales to many agents

By using behavior trees, PHOTOSSYNC achieves:
- **Readable AI**: Tree structures are intuitive
- **Maintainable**: Easy to modify behaviors
- **Reusable**: Nodes work across different enemies/NPCs
- **Extensible**: Add new node types as needed

**See also:**
- [Framework/README.md](../README.md) - Framework overview
- Example AI implementations in Gameplay/Enemy/
