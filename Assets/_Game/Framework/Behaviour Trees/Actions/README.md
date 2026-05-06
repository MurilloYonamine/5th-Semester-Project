# The `Actions` Directory: Leaf Node Implementations

The **Actions** directory contains concrete **leaf node implementations**—actual game behaviors that have no children and perform specific tasks. These are the "doers" in the behavior tree system.

---

## Purpose

Actions are the **executable leaf nodes** of a behavior tree. While Framework provides the structural nodes (Sequence, Selector, Parallel, Abort), each game feature implements its own Actions for:

- Movement (move to target, patrol, flee)
- Combat (attack, dodge, block)
- Animation (play animation, wait for animation)
- Audio (play sound, stop sound)
- State changes (alert, calm down)

---

## Built-in Actions

### `ActionWait.cs`

A simple action that waits for a specified duration.

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

#### Usage

```csharp
// Wait 3 seconds before moving to next action
var sequence = new Sequence("WaitThenAttack",
    new ActionWait(3f, "Wait for cooldown"),
    new AttackPlayerNode()
);
```

#### State Tracking

- `isWaiting`: Indicates this is the first call (sets start time)
- `startTime`: When the wait began (for calculating elapsed time)
- Reset clears state for next execution

---

## Creating Custom Actions

### Pattern 1: Instant Success/Failure

```csharp
public class CheckPlayerVisibleNode : Node {
    private Blackboard _blackboard;

    public CheckPlayerVisibleNode(Blackboard blackboard, string name = "CheckPlayerVisible")
        : base(name) {
        _blackboard = blackboard;
    }

    public override Status Process() {
        GameObject player = _blackboard.GetData<GameObject>("Player");
        
        if (player == null) {
            return Status.Failure;
        }

        // Simple raycast check
        Vector3 direction = (player.transform.position - transform.position).normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, direction, out hit)) {
            if (hit.collider.gameObject == player) {
                _blackboard.SetData("LastSeenPlayerPosition", player.transform.position);
                return Status.Success;
            }
        }

        return Status.Failure;
    }
}
```

**Characteristics:**
- No internal state
- Returns Success or Failure immediately
- Good for conditions/checks

### Pattern 2: Async Over Multiple Frames

```csharp
public class MoveToTargetNode : Node {
    private Blackboard _blackboard;
    private CharacterController _controller;
    private float _moveSpeed = 5f;
    private float _stoppingDistance = 0.5f;

    public MoveToTargetNode(Blackboard blackboard, CharacterController controller, 
                           string name = "MoveToTarget") 
        : base(name) {
        _blackboard = blackboard;
        _controller = controller;
    }

    public override Status Process() {
        Vector3 target = _blackboard.GetData<Vector3>("TargetPosition");
        
        // Calculate direction
        Vector3 direction = target - _controller.transform.position;
        float distance = direction.magnitude;

        // Check if reached target
        if (distance < _stoppingDistance) {
            return Status.Success;
        }

        // Move toward target
        Vector3 movement = direction.normalized * _moveSpeed * Time.deltaTime;
        _controller.Move(movement);

        return Status.Running;  // Still moving
    }
}
```

**Characteristics:**
- Maintains internal state (movement direction, elapsed time, etc.)
- Returns Running while executing
- Returns Success when goal achieved
- Returns Failure on errors

### Pattern 3: Animation-Based Action

```csharp
public class PlayAttackAnimationNode : Node {
    private Animator _animator;
    private string _animationTrigger = "Attack";
    private float _animationLength;
    private float _startTime;
    private bool _animationStarted;

    public PlayAttackAnimationNode(Animator animator, string name = "PlayAttack")
        : base(name) {
        _animator = animator;
    }

    public override Status Process() {
        if (!_animationStarted) {
            _animator.SetTrigger(_animationTrigger);
            _animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
            _startTime = Time.time;
            _animationStarted = true;
        }

        // Check if animation finished
        if (Time.time - _startTime >= _animationLength) {
            return Status.Success;
        }

        return Status.Running;
    }

    public override void Reset() {
        base.Reset();
        _animationStarted = false;
    }
}
```

**Characteristics:**
- Integrates with Unity Animation System
- Waits for animation to complete
- Returns Running while animating

### Pattern 4: Action with Sound

```csharp
public class PlaySoundNode : Node {
    private AudioSource _audioSource;
    private float _startTime;
    private float _soundDuration;
    private bool _soundPlaying;

    public PlaySoundNode(AudioSource audioSource, AudioClip clip, string name = "PlaySound")
        : base(name) {
        _audioSource = audioSource;
        _soundDuration = clip.length;
    }

    public override Status Process() {
        if (!_soundPlaying) {
            _audioSource.Play();
            _startTime = Time.time;
            _soundPlaying = true;
        }

        // Wait for sound to finish
        if (Time.time - _startTime >= _soundDuration) {
            return Status.Success;
        }

        return Status.Running;
    }

    public override void Reset() {
        base.Reset();
        _soundPlaying = false;
    }
}
```

---

## Common Action Categories

### Movement Actions

```csharp
// Move in a direction
public class MoveForwardNode : Node { }

// Move to a specific position
public class MoveToPositionNode : Node { }

// Patrol between waypoints
public class PatrolNode : Node { }

// Flee from target
public class FleeNode : Node { }
```

### Combat Actions

```csharp
// Perform attack
public class AttackNode : Node { }

// Charge attack (wind-up)
public class ChargeAttackNode : Node { }

// Block/Defend
public class BlockNode : Node { }

// Dodge/Evade
public class DodgeNode : Node { }
```

### State Actions

```csharp
// Become alert
public class AlertNode : Node { }

// Calm down/relax
public class CalmNode : Node { }

// Sleep/Inactive
public class SleepNode : Node { }

// Investigate (look around)
public class InvestigateNode : Node { }
```

### Animation Actions

```csharp
// Play specific animation
public class PlayAnimationNode : Node { }

// Wait for animation to finish
public class WaitForAnimationNode : Node { }

// Blend between animations
public class BlendAnimationNode : Node { }
```

### Audio Actions

```csharp
// Play sound effect
public class PlaySoundNode : Node { }

// Play dialogue
public class PlayDialogueNode : Node { }

// Loop ambient sound
public class PlayAmbienceNode : Node { }
```

---

## Best Practices

### 1. Keep Actions Focused

❌ **Bad:**
```csharp
public class DoEverythingNode : Node {
    public override Status Process() {
        Move();
        Attack();
        PlaySound();
        UpdateUI();
        // Too many responsibilities!
    }
}
```

✅ **Good:**
```csharp
public class MoveToTargetNode : Node { }
public class AttackPlayerNode : Node { }
public class PlayAttackSoundNode : Node { }

// Compose them together
var attackSequence = new Sequence("Attack",
    new MoveToTargetNode(),
    new AttackPlayerNode(),
    new PlayAttackSoundNode()
);
```

### 2. Use Blackboard for Data

✅ **Good:**
```csharp
public class MoveToTargetNode : Node {
    private Blackboard _blackboard;
    
    public override Status Process() {
        // Get target from blackboard
        Vector3 target = _blackboard.GetData<Vector3>("TargetPosition");
        // Move there
    }
}
```

### 3. Handle State Properly

✅ **Good:**
```csharp
public class MoveNode : Node {
    private bool _started = false;

    public override Status Process() {
        if (!_started) {
            // Initialize
            _started = true;
        }
        
        // Execute movement
        
        if (reachedTarget) {
            return Status.Success;
        }
        
        return Status.Running;
    }

    public override void Reset() {
        base.Reset();
        _started = false;
    }
}
```

### 4. Provide Meaningful Names

❌ **Bad:**
```csharp
new Node("A");  // What is "A"?
new Node("DoThing");  // Vague
```

✅ **Good:**
```csharp
new MoveToTargetNode("MoveToPlayerPosition");
new AttackPlayerNode("AttackWithMelee");
```

### 5. Document Return Values

```csharp
/// <returns>
/// Success: Attack animation completed
/// Running: Attack in progress
/// Failure: No target or out of range
/// </returns>
public override Status Process() { }
```

---

## Template for New Actions

```csharp
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine;

namespace FifthSemester.Gameplay.Enemy.AI {
    public class MyActionNode : Node {
        private Blackboard _blackboard;
        private Component _component;  // Reference to required component

        // State tracking (if action spans multiple frames)
        private bool _started = false;

        public MyActionNode(Blackboard blackboard, Component component, 
                           string name = "MyAction") 
            : base(name) {
            _blackboard = blackboard;
            _component = component;
        }

        public override Status Process() {
            // Initialize on first call
            if (!_started) {
                OnStart();
                _started = true;
            }

            // Execute action
            Status result = OnUpdate();

            return result;
        }

        private void OnStart() {
            // Setup (play sound, set animation, etc.)
        }

        private Status OnUpdate() {
            // Return Success, Failure, or Running
            return Status.Running;
        }

        public override void Reset() {
            base.Reset();
            _started = false;
        }
    }
}
```

---

## Where to Place Custom Actions

Custom actions belong in the **Feature** that uses them:

```
Gameplay/
├── Enemy/
│   └── AI/
│       ├── CheckPlayerVisibleNode.cs
│       ├── ChasePlayerNode.cs
│       ├── AttackPlayerNode.cs
│       └── PatrolNode.cs
├── Player/
│   └── AI/
│       ├── MovePlayerNode.cs
│       └── AttackEnemyNode.cs
└── NPC/
    └── AI/
        ├── TalkToPlayerNode.cs
        └── FollowPlayerNode.cs
```

**NOT in Framework/Actions/**—Framework stays generic.

---

## Summary

The **Actions** directory provides:
- **Built-in utility actions**: ActionWait for basic delays
- **Template for custom actions**: Standardized implementation pattern
- **Clear return value semantics**: Success/Failure/Running
- **State management**: Proper initialization and reset

By implementing Actions, PHOTOSSYNC enables:
- **Reusable behaviors**: Actions can be composed into different trees
- **Clean separation**: Action logic separate from tree structure
- **Easy debugging**: Each action has a clear purpose
- **Extensibility**: Add new actions as features grow

**See also:**
- [Behaviour Trees/README.md](../README.md) - How nodes compose
- Enemy AI implementations in Gameplay/Enemy/
