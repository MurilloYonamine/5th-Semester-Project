# The `Environment` Directory: World Layout & Maps

The **Environment** directory contains **map prefabs, world layout, and environmental assets**—the physical structure of the game world where gameplay occurs.

---

## Purpose

Environment System provides:
- **Map/scene layout**: Organized level design
- **Prefab organization**: Reusable environment sections
- **NavMesh data**: AI pathfinding for enemies
- **Visual consistency**: Shared materials and visual themes
- **Performance**: Optimized scene structure

---

## Directory Structure

```
Environment/
├── Maps/
│   ├── Hospital.prefab
│   ├── Hallway.prefab
│   └── Lab.prefab
├── Materials/
│   ├── Floor.mat
│   ├── Walls.mat
│   └── Effects.mat
├── Models/
│   ├── Hospital/ (FBX files)
│   ├── Furniture/
│   └── Decorations/
├── Prefabs/
│   ├── RoomSection.prefab
│   ├── CorridorSection.prefab
│   └── Transition.prefab
└── NavMesh/
    ├── Hospital.asset
    └── Lab.asset
```

---

## Map Organization

### Hospital Map
```
Hospital/
├── Ground Floor
│   ├── Reception
│   ├── Hallway Main
│   ├── Emergency Room
│   ├── Pharmacy
│   └── Storage
└── Upper Floor
    ├── Patient Ward A
    ├── Patient Ward B
    ├── Office
    └── Roof Access
```

### Navigation Flow

```
Reception → Hallway Main
    ├─ Emergency Room
    ├─ Pharmacy
    └─ Stairs → Upper Floor
        ├─ Patient Ward A
        ├─ Patient Ward B
        └─ Office
```

---

## Map Sections

Each map area is organized as a prefab:

### `Hospital.prefab`

Main map container.

```
Hospital (GameObject)
├── Lighting
│   ├── Main Light (Directional)
│   ├── Ambient Light settings
│   └── ReflectionProbe
├── Ground Floor (child)
│   ├── Reception (child prefab)
│   ├── HallwayMain (child prefab)
│   ├── EmergencyRoom (child prefab)
│   └── (more rooms)
├── Upper Floor (child)
│   ├── PatientWardA (child prefab)
│   ├── Office (child prefab)
│   └── (more rooms)
├── Navigation
│   ├── NavMesh.asset
│   └── NavMeshObstacles (for dynamic objects)
└── Fog
    └── Volume settings
```

### Room Section Pattern

```
RoomSection (GameObject)
├── Colliders (walls, floor)
├── Visuals
│   ├── Mesh (walls, floor, ceiling)
│   ├── Materials
│   └── Lighting
├── Interactive Objects
│   ├── Door
│   ├── Furniture
│   └── Collectibles
└── Spawn Points
    ├── PlayerSpawn
    └── EnemySpawn
```

---

## NavMesh Setup

### Baking NavMesh

1. Select all environment objects
2. Mark static: Inspector → Static → Baked Only
3. Window → AI → Navigation
4. Click "Bake"
5. Verify agents can pathfind

### NavMesh Obstacles

For dynamic objects (doors, moving platforms):

```csharp
public class DynamicObstacle : MonoBehaviour {
    private NavMeshObstacle _obstacle;

    private void Start() {
        _obstacle = GetComponent<NavMeshObstacle>();
    }

    public void OpenDoor() {
        _obstacle.enabled = false;  // NavMesh updates
    }
}
```

---

## Material Organization

### Shared Materials

```
Materials/
├── Hospital/
│   ├── Floor_Tile.mat
│   ├── Wall_Plaster.mat
│   ├── Door_Metal.mat
│   └── Glass_Window.mat
└── Effects/
    ├── Fog.mat
    ├── Damage.mat
    └── Glow.mat
```

### Material Properties

```csharp
// Set material properties at runtime
var renderer = GetComponent<Renderer>();
renderer.material.SetColor("_Color", Color.white);
renderer.material.SetFloat("_Metallic", 0.8f);
```

---

## Lighting Setup

### Baked Lighting

- Lightmaps for static geometry
- Faster runtime performance
- Pre-computed shadows

### Real-Time Lighting

- Dynamic lights for interactive areas
- Realtime shadows for moving objects
- Post-processing effects

### Light Probes

For characters moving through levels:
```
1. Place Light Probes in scene
2. Bake lighting with probes
3. Characters sample probe colors
```

---

## Optimization

### Culling

```csharp
// Cameras only render visible area
Camera.main.cullingMask = LayerMask.GetMask("Environment");
```

### LOD Groups

For distant objects:
```
ModelHigh (close distance)
ModelMedium (medium distance)
ModelLow (far distance)
```

### Prefab Variants

```
RoomSection.prefab (base)
├── RoomSection_Hospital.prefab (variant)
├── RoomSection_Lab.prefab (variant)
└── RoomSection_Abandoned.prefab (variant)
```

---

## Best Practices

### 1. Organize by Room/Zone

✅ **Good:**
```
Environment/
├── Hospital/
│   ├── Emergency Room.prefab
│   ├── Pharmacy.prefab
│   └── Hallway.prefab
└── Lab/
    ├── Main Lab.prefab
    └── Storage.prefab
```

### 2. Use Consistent Scale

✅ **Good:**
```
1 unit = 1 meter
Doors: 2m tall, 1m wide
Player height: 1.8m
```

### 3. Bake NavMesh

✅ **Good:**
```
1. Mark geometry as "Baked Only"
2. Window → AI → Navigation → Bake
3. Verify enemies can pathfind
```

### 4. Name Clearly

✅ **Good:**
```
Hospital_Ground_Reception
Hospital_Ground_Hallway_Main
Hospital_Upper_PatientWard_A
```

### 5. Use Layers

✅ **Good:**
```
Layers:
- Environment
- Interactive
- Enemy
- Player
- UI
```

---

## Common Patterns

### Transition Between Areas

```csharp
public class AreaTransition : MonoBehaviour {
    [SerializeField] private string _targetScene;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            SceneManager.LoadScene(_targetScene);
        }
    }
}
```

### Dynamic Area Loading

```csharp
public class AreaLoader : MonoBehaviour {
    [SerializeField] private GameObject _areaPrefab;
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Instantiate(_areaPrefab);  // Load area on demand
        }
    }
}
```

### Fog of War

```csharp
public class FogVolume : MonoBehaviour {
    [SerializeField] private float _fogDensity = 0.05f;

    private void Start() {
        RenderSettings.fogDensity = _fogDensity;
    }
}
```

---

## Extending Environment

### Environmental Hazards

```csharp
public class HazardArea : MonoBehaviour {
    [SerializeField] private float _damagePerSecond = 10f;

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController.Instance.TakeDamage(_damagePerSecond * Time.deltaTime);
        }
    }
}
```

### Collectible Placement

```
Each room section includes:
- 0-3 collectibles
- 0-1 healing items
- 0-1 ammo caches
```

### Spawn Point Management

```csharp
public class SpawnManager : MonoBehaviour {
    [SerializeField] private Transform[] _playerSpawns;
    [SerializeField] private Transform[] _enemySpawns;

    public Transform GetRandomPlayerSpawn() => _playerSpawns[Random.Range(0, _playerSpawns.Length)];
    public Transform GetRandomEnemySpawn() => _enemySpawns[Random.Range(0, _enemySpawns.Length)];
}
```

---

## Asset References

### Typical Hospital Map

```
Ground Floor (500m²)
├── Reception (50m²)
├── Main Hallway (200m²)
├── Emergency Room (150m²)
├── Pharmacy (50m²)
└── Storage (50m²)

Upper Floor (500m²)
├── Patient Wards (300m²)
├── Office (100m²)
├── Stairwell (50m²)
└── Roof Access (50m²)

Total: ~1000m² explorable area
```

---

## Summary

The **Environment** directory provides:
- **Map prefabs**: Reusable level sections
- **Visual consistency**: Shared materials and lighting
- **NavMesh data**: AI pathfinding information
- **Organized structure**: Logical room/zone hierarchy
- **Performance optimization**: LOD, culling, baking

By organizing Environment:
- **Level designers** work efficiently
- **Performance** is optimized via baking
- **Navigation** is reliable for enemies
- **Visual style** is consistent
- **Assets** are reusable and organized

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Gameplay/Door/README.md](../Door/README.md) - Interactive doors
