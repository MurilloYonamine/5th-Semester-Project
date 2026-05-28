# The `Map2` Directory: Passwords, Keys & Doors

The **Map2** directory contains the standalone puzzle for map 2: digit sprites, persistent password state, reusable keys, and key-locked doors.

---

## Purpose

Map2 provides:
- **Password reveal**: Each digit sprite reveals one fixed position in the code
- **Password delivery**: A dedicated interactable can accept the completed password and trigger the next step
- **Persistent progress**: The revealed code survives scene changes and restarts
- **Reusable keys**: Key items stay in the inventory and can unlock multiple doors
- **Key-locked doors**: Doors open only when the matching key is present
- **Local intro sequence**: The Map 2 scene can fade in and then start its own timeline without using the global cutscene system
- **Horror ambience**: Map 2 can play random ambient clips at irregular intervals to create tension

---

## Directory Structure

```
Map2/
├── README.md
└── Scripts/
    ├── README.md
    ├── Map2KeyDefinitionSO.cs
    ├── Map2PasswordState.cs
    ├── Map2PasswordController.cs
    ├── Map2PasswordDigitInteractable.cs
    ├── Map2PasswordDeliveryPoint.cs
    ├── Map2KeyItem.cs
    ├── Map2KeyDoor.cs
    └── Map2HorrorAmbientPlayer.cs
```

---

## Flow

```
Digit Sprite Interactables
    ↓
Map2PasswordController
    ↓
Map2PasswordState (PlayerPrefs)
    ↓
Map2PasswordView

Map2PasswordDeliveryPoint checks whether the password is complete and invokes the configured next-step event.

Map2KeyItem + Map2KeyDoor use a shared `Map2KeyDefinitionSO` reference instead of string IDs.
Map2IntroSequence drives the Map 2 opening flow locally through `FadeService` and a scene `PlayableDirector`.
Map2HorrorAmbientPlayer plays random `AudioClip` entries with a configurable delay range between sounds.
```
