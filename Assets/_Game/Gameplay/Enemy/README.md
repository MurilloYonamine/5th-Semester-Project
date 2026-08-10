# The `Enemy` Directory: AI with Behaviour Trees

The **Enemy** directory implements **intelligent enemies** using the custom Behaviour Tree system. Enemies patrol, chase, attack, and jumpscare the player based on dynamic decision-making.

---

## Purpose

Enemy System provides:
- **Behaviour Tree AI**: Non-scripted, reactive enemy decisions
- **NavMesh integration**: Pathfinding and movement
- **Custom actions**: Chase, Patrol, Attack, Jumpscare nodes
- **Responsive AI**: Reacts to player sprint, positions, and events
- **Reusable architecture**: Easy to create new enemy types

---

## Architecture

```
EnemyController.cs (base class)
├── NavMeshAgent (movement)
├── Animator (animations)
├── BehaviourTree (decision-making)
│   └── Root (composite node)
│       ├── Selector (OR logic)
│       │   ├── Sequence (AND logic)
│       │   │   ├── CanChaseTarget? (Condition)
│       │   │   └── Chase (Action)
│       │   └── Patrol (Action)
│       └── Blackboard (shared memory)
└── EventSubscriptions (reactions)
    └── PlayerSprintChangedEvent

Specific Enemies
├── LightSeeker
├── PacientEnemy
<<<<<<< HEAD
├── Nurse
=======
>>>>>>> origin/main
└── ...
```

---

## Key Files

### `EnemyController.cs`

Base class for all enemies.

```csharp
public class EnemyController : MonoBehaviour {
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;

    [Header("AI")]
    [SerializeField] private Node _behaviour;
    [SerializeField] private float _viewDistance = 50f;

    protected Blackboard _blackboard;
    public static List<EnemyController> AllEnemies { get; private set; } = new();

    protected virtual void Start() {
        AllEnemies.Add(this);
        
        // Initialize AI memory
        _blackboard = new Blackboard();
        _blackboard.SetData("agent", _agent);
        _blackboard.SetData("animator", _animator);
        _blackboard.SetData("controller", this);
        
        // Subscribe to events
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus.Subscribe<PlayerSprintChangedEvent>(OnPlayerSprint);
    }

    protected virtual void Update() {
        // Behaviour tree evaluates each frame
        _behaviour.Process(_blackboard);
        
        // Update animations
        _animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

    public Transform GetTargetTransform() => Player.Instance?.transform;

    private void OnPlayerSprint(PlayerSprintChangedEvent evt) {
        // Increase AI difficulty when player sprints
        _blackboard.SetData("isSprinting", evt.IsSprinting);
    }
}
```

**Key Features:**
- Manages NavMeshAgent movement
- Maintains Blackboard for AI memory
- Responds to events (player sprint, etc.)
- Static list of all enemies for global queries

---

### `Scripts/Actions/ChaseTarget.cs`

Custom Behaviour Tree action that pursues the player.

```csharp
public class ChaseTarget : Node {
    private float _chaseSpeed = 5.5f;
    private float _stoppingDistance = 2f;

    public ChaseTarget(Node[] children = null) : base(children) { }

    public override Status Process(Blackboard blackboard) {
        var agent = blackboard.GetData<NavMeshAgent>("agent");
        var controller = blackboard.GetData<EnemyController>("controller");
        var target = controller.GetTargetTransform();

        if (target == null) return Status.Failure;

        // Move toward player
        agent.speed = _chaseSpeed;
        agent.SetDestination(target.position);

        // Check if reached player
        if (!agent.hasPath || agent.remainingDistance > _stoppingDistance) {
            return Status.Running;
        }

        // Reached target
        return Status.Success;
    }
}
```

**Behavior:**
- Sets NavMeshAgent destination to player
- Returns Running while moving
- Returns Success when reached
- Returns Failure if no target

---

### `Scripts/Actions/PatrolAction.cs`

Custom action that patrols waypoints.

```csharp
public class PatrolAction : Node {
    private Vector3[] _waypoints;
    private int _currentWaypoint = 0;
    private float _waypointDistance = 1f;

    public PatrolAction(Vector3[] waypoints) : base(null) {
        _waypoints = waypoints;
    }

    public override Status Process(Blackboard blackboard) {
        var agent = blackboard.GetData<NavMeshAgent>("agent");

        if (_waypoints.Length == 0) return Status.Failure;

        // Move to current waypoint
        agent.SetDestination(_waypoints[_currentWaypoint]);
        agent.speed = 2f;  // Patrol is slower

        // Check if reached waypoint
        if (agent.remainingDistance < _waypointDistance) {
            _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
        }

        return Status.Running;  // Always running (infinite patrol)
    }
}
```

---

### `Scripts/Enemies/LightSeeker.cs`

Specific enemy type that hunts based on light/noise and drives a proximity-based white noise effect as it closes in on the player.

```csharp
public class LightSeeker : EnemyController {
    [SerializeField] private float _lightSensitivity = 1f;
    [SerializeField] private float _noiseSensitivity = 1f;
    [SerializeField] private AudioClip _whiteNoiseClip;

    protected override void Start() {
        base.Start();

        // Build behavior tree
        var chaseAction = new ChaseTarget();
        var patrolAction = new PatrolAction(_patrolWaypoints);
        
        var canChase = new CanSeeTarget(_viewDistance);
        var chaseSeq = new Sequence(new Node[] { canChase, chaseAction });
        
        _behaviour = new Selector(new Node[] { chaseSeq, patrolAction });
        
        // LightSeeker specific: react to flashlight
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus.Subscribe<FlashlightToggleEvent>(OnFlashlightToggle);
    }

    private void OnFlashlightToggle(FlashlightToggleEvent evt) {
        if (evt.IsOn && CanSeeLight()) {
            // Chase toward light source
            _blackboard.SetData("forceChase", true);
        }
    }

    private bool CanSeeLight() {
        // Check if flashlight is visible to this enemy
        return Vector3.Distance(transform.position, GetLightPosition()) < 30f;
    }
}
```

The chase action reads the white noise clip and max volume from the blackboard, then raises the loop volume as the player gets closer.

---

<<<<<<< HEAD
### `Scripts/Enemies/Nurse/Nurse.cs`

The Nurse can remain blocked until a configured `Map2KeyDefinitionSO` is collected. After the matching key enters the inventory, the AI is released and the existing behaviour tree takes over.

---

=======
>>>>>>> origin/main
## Behaviour Tree Structure

### Example: Standard Chase Behavior

```csharp
var behavior = new Selector(new Node[] {
    // Try to chase if can see target
    new Sequence(new Node[] {
        new CanSeeTarget(50f),
        new ChaseTarget()
    }),
    
    // Otherwise patrol
    new PatrolAction(waypoints)
});
```

**Logic:**
```
Evaluate this tree each frame:
  Selector (pick first successful branch):
    1. Try sequence:
       a. Can we see target? YES → continue
       b. Chase target → Running
       → Sequence returns Running
       → Selector returns Running (don't try patrol)
    
    When player hides:
       a. Can we see target? NO → Sequence fails
    2. Try patrol → returns Running
       → Selector returns Running
```

### Complex Example: Aggressive Chase with Fallback

```csharp
var behavior = new Selector(new Node[] {
    // Highest priority: player in melee range
    new Sequence(new Node[] {
        new IsPlayerClose(2f),
        new AttackAction()
    }),
    
    // Medium priority: player visible, chase
    new Sequence(new Node[] {
        new CanSeeTarget(50f),
        new ChaseTarget()
    }),
    
    // Low priority: heard noise, investigate
    new Sequence(new Node[] {
        new DidHeardNoise(),
        new InvestigateLocation()
    }),
    
    // Fallback: patrol
    new PatrolAction(waypoints)
});
```

---

## Adding Custom Actions

### Create New Action

```csharp
public class AttackAction : Node {
    private float _attackCooldown = 2f;
    private float _nextAttackTime = 0;

    public AttackAction() : base(null) { }

    public override Status Process(Blackboard blackboard) {
        var controller = blackboard.GetData<EnemyController>("controller");
        var animator = blackboard.GetData<Animator>("animator");
        
        if (Time.time < _nextAttackTime) {
            return Status.Running;  // Attack cooldown
        }

        // Play attack animation
        animator.SetTrigger("Attack");
        
        // Damage player
        var player = Player.Instance;
        if (player != null) {
            player.TakeDamage(10);
        }

        _nextAttackTime = Time.time + _attackCooldown;
        return Status.Success;  // Attack complete
    }
}
```

### Use in Tree

```csharp
var attack = new AttackAction();

var tree = new Selector(new Node[] {
    new Sequence(new Node[] {
        new IsPlayerClose(1.5f),  // Within 1.5 units
        attack
    }),
    new Sequence(new Node[] {
        new CanSeeTarget(50f),
        new ChaseTarget()
    })
});
```

---

## Configuration

### AI Difficulty

```csharp
private void OnPlayerSprint(PlayerSprintChangedEvent evt) {
    if (evt.IsSprinting) {
        // Increase enemy difficulty
        _viewDistance = 100f;  // See farther
        agent.speed = 6f;      // Move faster
    } else {
        _viewDistance = 50f;
        agent.speed = 5.5f;
    }
}
```

### Patrol Waypoints

In Inspector:
```
1. Select enemy
2. Set waypoint count
3. Drag each waypoint position in scene
4. Or use Gizmos to visualize path
```

### Chase/Patrol Speed

```csharp
// In action nodes
agent.speed = 5.5f;   // Chase speed
agent.speed = 2f;     // Patrol speed
```

---

## Best Practices

### 1. Keep Actions Simple and Reusable

✅ **Good:**
```csharp
// Generic chase—reuse for all enemy types
public class ChaseTarget : Node {
    public override Status Process(Blackboard bb) {
        var agent = bb.GetData<NavMeshAgent>("agent");
        var target = GetTarget(bb);
        
        agent.SetDestination(target.position);
        return Status.Running;
    }
}
```

### 2. Use Blackboard for Shared Data

✅ **Good:**
```csharp
_blackboard.SetData("agent", _agent);
_blackboard.SetData("targetPlayer", Player.Instance);
_blackboard.SetData("isHunting", true);

// Any action can access
var isHunting = blackboard.GetData<bool>("isHunting");
```

### 3. Respond to Global Events

✅ **Good:**
```csharp
// Enemy can hear player sprint globally
eventBus.Subscribe<PlayerSprintChangedEvent>(OnPlayerSprint);

// Adjust AI based on event data
private void OnPlayerSprint(PlayerSprintChangedEvent evt) {
    _blackboard.SetData("difficultyMultiplier", 1.5f);
}
```

### 4. Debug Behaviour Trees

✅ **Good:**
```csharp
// Add debug in Node.Process
Debug.Log($"{GetType().Name} returning {status}");

// Visualize behaviour state
private void OnGUI() {
    var behavior = _behaviour.GetCurrent();
    GUI.Label(new Rect(10, 10, 200, 20), $"Current: {behavior}");
}
```

### 5. Use Static Enemy List

✅ **Good:**
```csharp
// Find all enemies
var allEnemies = EnemyController.AllEnemies;
foreach (var enemy in allEnemies) {
    enemy.Stun(2f);
}
```

---

## Common Patterns

### Simple Chase & Patrol

```csharp
var chase = new ChaseTarget();
var patrol = new PatrolAction(waypoints);

var canSee = new CanSeeTarget(50f);
var chaseSeq = new Sequence(new[] { canSee, chase });

_behaviour = new Selector(new[] { chaseSeq, patrol });
```

### Three-Tier Aggression

```csharp
// Idle < Hunting < Combat

new Selector(new[] {
    // Attack if close
    new Sequence(new[] {
        new IsPlayerClose(1.5f),
        new AttackAction()
    }),
    
    // Chase if visible
    new Sequence(new[] {
        new CanSeeTarget(50f),
        new ChaseTarget()
    }),
    
    // Hunt if heard
    new Sequence(new[] {
        new DidHeardNoise(),
        new SearchLastKnownPosition()
    }),
    
    // Idle
    new PatrolAction(waypoints)
})
```

### Jumpscare Sequence

```csharp
// Trigger on close proximity
new Sequence(new[] {
    new IsPlayerClose(0.5f),
    new PlayJumpscareAnimation(),
    new PlayScareAudio()
})
```

---

## Extending Enemies

### Scared State

```csharp
public class ScaredEnemy : EnemyController {
    private float _scareEndTime = 0;
    
    protected override void Update() {
        if (Time.time < _scareEndTime) {
            // Flee instead of chase
            // Don't evaluate normal tree
            return;
        }
        
        base.Update();
    }
    
    public void Scare(float duration) {
        _scareEndTime = Time.time + duration;
    }
}
```

### Enemy with Hearing

```csharp
private void OnPlayerMakeNoise(PlayerNoiseEvent evt) {
    float distance = Vector3.Distance(transform.position, evt.NoisePosition);
    
    if (distance < _hearingRange) {
        // Investigate sound location
        _blackboard.SetData("investigateTarget", evt.NoisePosition);
    }
}
```

---

## Debugging

### Print Behaviour Tree State

```csharp
private void OnGUI() {
    GUI.Label(new Rect(10, 50, 300, 20),
        $"Enemy Behavior: {GetBehaviorString()}");
}

private string GetBehaviorString() {
    return _behaviour switch {
        Selector => "Deciding (Selector)",
        Sequence => "Executing (Sequence)",
        _ => "Unknown"
    };
}
```

---

## Summary

The **Enemy** system provides:
- **Behaviour Tree AI**: Flexible, non-scripted decisions
- **Reusable actions**: Chase, patrol, attack, investigate
- **Event responsiveness**: React to player actions
- **Scalable architecture**: Easy to add enemy types
- **Global coordination**: Static list of all enemies

By using Behaviour Trees:
- **AI is declarative**: See the logic in tree structure
- **Easy to modify**: Change behavior by restructuring tree
- **Reusable**: Actions work with any enemy type
- **Extensible**: Add new actions and conditions easily

**See also:**
- [Framework/Behaviour Trees/README.md](../../Framework/Behaviour%20Trees/README.md) - Behaviour tree system
- [Gameplay/README.md](../README.md) - Gameplay features
