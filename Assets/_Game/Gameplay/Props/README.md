# The `Props` Directory: Environmental Objects

The **Props** directory contains **environmental decorations and small interactive objects**—furniture, decorations, breakables, and non-critical interactive items that populate the world.

---

## Purpose

Props System provides:
- **Environmental decoration**: Furniture and visual elements
- **Breakable objects**: Destructible props for gameplay
- **Interactive props**: Objects with simple interactions
- **Atmosphere**: Visual storytelling through prop placement
- **Reusability**: Prefab-based prop library

---

## Directory Structure

```
Props/
├── Furniture/
│   ├── Chair.prefab
│   ├── Table.prefab
│   ├── Cabinet.prefab
│   ├── Shelf.prefab
│   └── Bed.prefab
├── Decorations/
│   ├── Picture.prefab
│   ├── Plant.prefab
│   ├── Lamp.prefab
│   ├── Poster.prefab
│   └── Vase.prefab
├── Breakables/
│   ├── Bottle.prefab
│   ├── Plate.prefab
│   ├── Window.prefab
│   └── Mirror.prefab
├── Interactive/
│   ├── Locker.prefab
│   ├── Cabinet.prefab
│   ├── Drawer.prefab
│   └── Crate.prefab
├── Models/
│   └── [FBX files organized by category]
└── Materials/
    └── [Shared materials]
```

---

## Prop Types

### Decorative Props

```csharp
public class DecorativeProp : MonoBehaviour {
    // No functionality, just visual
    [SerializeField] private MeshFilter _mesh;
    [SerializeField] private Material _material;
    
    // Optionally: particle effects, animations
    [SerializeField] private ParticleSystem _ambientParticles;
}
```

### Breakable Props

```csharp
public class BreakableProp : MonoBehaviour {
    [SerializeField] private float _health = 10f;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] private ParticleSystem _breakEffect;
    [SerializeField] private AudioClip _breakSound;

    public void Break() {
        // Disable visual mesh
        GetComponent<MeshRenderer>().enabled = false;
        
        // Enable physics
        _rigidbody.isKinematic = false;
        
        // Play effects
        Instantiate(_breakEffect, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(_breakSound, transform.position);
        
        // Clean up after delay
        Destroy(gameObject, 5f);
    }

    public void TakeDamage(float amount) {
        _health -= amount;
        if (_health <= 0) {
            Break();
        }
    }
}
```

### Interactive Props

```csharp
public class LockerProp : MonoBehaviour, IInteractable {
    [SerializeField] private bool _isOpen = false;
    [SerializeField] private Item _containedItem;
    [SerializeField] private Animator _animator;

    public bool IsInteractable => !_isOpen;

    public void Interact() {
        if (_isOpen) return;

        _isOpen = true;
        _animator.SetTrigger("Open");
        
        // Spawn item if has one
        if (_containedItem != null) {
            var inventory = ServiceLocator.Get<IInventoryService<Item>>();
            inventory.TryAdd(_containedItem);
        }
    }

    public void OnLookAt() {
        // Show interaction hint
    }

    public void OnLookAway() {
        // Hide hint
    }
}
```

---

## Common Prop Examples

### Chair

```
Chair
├── Mesh (visual)
├── Collider (for physics)
└── Material
```

### Breakable Bottle

```
Bottle
├── MeshRenderer (visual)
├── Rigidbody (physics - initially kinematic)
├── Collider (trigger for breaking)
├── BreakableProp.cs
├── ParticleSystem (break effect)
└── AudioSource (break sound)
```

### Locker with Item

```
Locker
├── LockerProp.cs
├── Animator (open animation)
├── Collider
├── Rigidbody
├── MeshRenderer
└── (references contained item)
```

---

## Prop Prefab Template

### Decorative Prop Template

```
PropName
├── Mesh (visual)
├── Material
├── Collider (physics)
└── (optional) ParticleSystem
```

### Breakable Prop Template

```
PropName (Breakable)
├── Mesh (visual)
├── Rigidbody (initially kinematic)
├── Collider
├── BreakableProp.cs
├── ParticleSystem (break effect)
└── AudioClip (break sound)
```

### Interactive Prop Template

```
PropName (Interactive)
├── Mesh (visual)
├── Collider (trigger)
├── Rigidbody
├── [PropType].cs (LockerProp, etc.)
├── Animator (optional)
└── Outline (interaction hint)
```

---

## Setup Guidelines

### Creating Breakable Props

1. Import model (FBX)
2. Add Rigidbody (set kinematic = true initially)
3. Add Collider
4. Add BreakableProp.cs component
5. Configure break effects and sounds
6. Test physics when broken

### Creating Interactive Props

1. Import model (FBX)
2. Add Collider (trigger)
3. Add specific prop script (LockerProp, CrateProp, etc.)
4. Add Animator if animation needed
5. Configure contained items/effects
6. Test interaction

### Performance Optimization

```csharp
// Use object pooling for frequently spawned props
public class PropPooler : MonoBehaviour {
    [SerializeField] private GameObject _propPrefab;
    [SerializeField] private int _poolSize = 10;

    private Queue<GameObject> _pool = new();

    private void Start() {
        for (int i = 0; i < _poolSize; i++) {
            var prop = Instantiate(_propPrefab);
            prop.SetActive(false);
            _pool.Enqueue(prop);
        }
    }

    public GameObject Spawn(Vector3 position) {
        var prop = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_propPrefab);
        prop.transform.position = position;
        prop.SetActive(true);
        return prop;
    }

    public void Despawn(GameObject prop) {
        prop.SetActive(false);
        _pool.Enqueue(prop);
    }
}
```

---

## Best Practices

### 1. Simple, Focused Props

✅ **Good:**
```csharp
// Each prop does one thing
public class Table : MonoBehaviour {
    // Just a visual table
}
```

### 2. Reusable Prefabs

✅ **Good:**
```
Prefabs/
├── Chair_Office.prefab
├── Chair_Waiting.prefab
└── (variations of same chair)
```

### 3. Consistent Naming

✅ **Good:**
```
Prop_[Category]_[Type]_[Variant]
Prop_Furniture_Chair_Office
Prop_Breakable_Bottle_Glass
Prop_Interactive_Locker_Red
```

### 4. Collision Optimization

✅ **Good:**
```csharp
// Use simple colliders
// Box colliders for furniture
// Sphere colliders for breakables
// Avoid mesh colliders for performance
```

### 5. Visual Consistency

✅ **Good:**
```
All props in same location use consistent materials
Lighting matches environment
Shadows properly baked
```

---

## Common Patterns

### Prop Spawner

```csharp
public class PropSpawner : MonoBehaviour {
    [SerializeField] private GameObject[] _propPrefabs;
    [SerializeField] private Transform[] _spawnPoints;

    private void Start() {
        foreach (var point in _spawnPoints) {
            var prefab = _propPrefabs[Random.Range(0, _propPrefabs.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }
    }
}
```

### Loot Container

```csharp
public class LootContainer : MonoBehaviour, IInteractable {
    [SerializeField] private Item[] _possibleLoot;
    [SerializeField] private int _lootCount = 3;

    public void Interact() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        
        for (int i = 0; i < _lootCount; i++) {
            var item = _possibleLoot[Random.Range(0, _possibleLoot.Length)];
            inventory.TryAdd(item);
        }

        Destroy(gameObject);
    }
}
```

### Trap Prop

```csharp
public class TrapProp : MonoBehaviour {
    [SerializeField] private float _damageOnTrigger = 20f;
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController.Instance.TakeDamage(_damageOnTrigger);
            // Play trap effect
        }
    }
}
```

---

## Extending Props

### Animated Props

```csharp
public class AnimatedProp : MonoBehaviour {
    [SerializeField] private Animator _animator;

    private void Start() {
        _animator.SetBool("IsActive", true);
    }
}
```

### Prop with Particle Effects

```csharp
public class EffectProp : MonoBehaviour {
    [SerializeField] private ParticleSystem[] _particles;

    public void Activate() {
        foreach (var ps in _particles) {
            ps.Play();
        }
    }
}
```

### Conditional Props

```csharp
public class ConditionalProp : MonoBehaviour {
    [SerializeField] private bool _showIfQuestComplete = false;
    [SerializeField] private int _questId = 0;

    private void Start() {
        var questService = ServiceLocator.Get<IQuestService>();
        
        if (_showIfQuestComplete && questService.IsQuestComplete(_questId)) {
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }
}
```

---

## Asset Library

### Furniture Set
```
Chair, Table, Cabinet, Bed, Shelf, Desk, Lamp
```

### Decorations Set
```
Picture, Plant, Poster, Vase, Clock, Mirror, Carpet
```

### Breakables Set
```
Bottle, Plate, Window, Mirror, Crate, Box
```

### Interactive Set
```
Locker, Cabinet, Drawer, Crate, Chest, Shelf
```

---

## Summary

The **Props** directory provides:
- **Environmental decoration**: Visual atmosphere
- **Breakable objects**: Interactive destruction
- **Loot containers**: Item discovery
- **Reusable library**: Prefab-based organization
- **Visual consistency**: Shared materials and styling

By using Props:
- **Level design** is faster with prefab library
- **World feels alive** with varied objects
- **Gameplay depth** via breakables and secrets
- **Atmosphere** is enhanced through decoration
- **Performance** is optimized with simple geometry

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Gameplay/Environment/README.md](../Environment/README.md) - World layout
