# The `Dialogue` Directory: Conversation System

The **Dialogue** directory manages **character conversations**—dialogue playback, character data, line-by-line display, and state transitions during conversations.

---

## Purpose

Dialogue System provides:
- **Sequential line display**: Show dialogue one line at a time
- **Character data**: Speaker names, colors, voice info
- **ScriptableObject storage**: Design dialogues in Editor without code
- **State control**: Pause game during dialogue, resume after
- **Extensibility**: Easy to add voice acting, animations, branching

---

## Architecture

```
DialogueSO (ScriptableObject)
├── DialogueLine[] (list of lines)
│   ├─ speaker: CharacterSO
│   └─ text: string

CharacterSO (ScriptableObject)
├── characterName: string
├── nameColor: Color
└── textColor: Color

DialogueTrigger (MonoBehaviour)
├─ Detects player interaction
└─ Tells DialogueService to start dialogue

DialogueService (MonoBehaviour)
├─ Manages active dialogue
├─ Displays lines one by one
└─ Publishes DialogueStartedEvent / DialogueEndedEvent
```

---

## Key Files

### `DialogueSO.cs`

ScriptableObject that stores dialogue data.

```csharp
[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject {
    public DialogueLine[] lines;  // Array of dialogue lines
}

[System.Serializable]
public class DialogueLine {
    public CharacterSO speaker;   // Who is speaking
    public string text;            // What they say
}
```

**Usage:**
- Create in Editor: Right-click → Create → Dialogue → Dialogue
- Fill with lines, assign characters, write text
- Reference in features via `Resources.Load<DialogueSO>("Dialogue/NpcGreeting")`

---

### `CharacterSO.cs`

ScriptableObject storing character info.

```csharp
[CreateAssetMenu(fileName = "Character", menuName = "Dialogue/Character")]
public class CharacterSO : ScriptableObject {
    public string characterName;   // "Nurse", "Patient", etc.
    public Color nameColor;        // Color for name text
    public Color textColor;        // Color for dialogue text
}
```

**Usage:**
- Create characters: Right-click → Create → Dialogue → Character
- Set colors for visual distinction in dialogue UI
- Reuse characters across multiple dialogues

---

### `DialogueService.cs`

Main component managing active dialogue.

```csharp
public class DialogueService : MonoBehaviour, IDialogueService<DialogueSO> {
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    public bool IsDialogueActive { get; private set; }

    public void StartDialogue(DialogueSO dialogue) {
        ToggleDialogue(true);
        _eventBus.Publish(new DialogueStartedEvent());
        _linesQueue = new Queue<DialogueLine>(dialogue.lines);
        DisplayNextLine();
    }

    public void DisplayNextLine() {
        if (_linesQueue.Count == 0) {
            EndDialogue();
            return;
        }

        DialogueLine line = _linesQueue.Dequeue();
        _nameText.text = line.speaker.characterName;
        _nameText.color = line.speaker.nameColor;
        _dialogueText.text = line.text;
        _dialogueText.color = line.speaker.textColor;
    }

    public void EndDialogue() {
        ToggleDialogue(false);
        _eventBus.Publish(new DialogueEndedEvent());
    }
}
```

**Key Features:**
- Manages dialogue panel visibility
- Queue-based line progression
- Color-coded speaker names
- Publishes events for state management

---

### `DialogueTrigger.cs`

Detects player interaction and starts dialogue.

```csharp
public class DialogueTrigger : MonoBehaviour {
    [SerializeField] private DialogueSO _dialogue;

    public void Interact() {
        var dialogueService = ServiceLocator.Get<IDialogueService<DialogueSO>>();
        dialogueService.StartDialogue(_dialogue);
    }
}
```

**Attached to:** NPCs, interactive objects that have dialogue

---

## Data Organization

### Character Data
```
Assets/_Game/Gameplay/Dialogue/Data/Characters/
├── Ellie.asset
├── EnfermeiraA.asset
├── PacienteA.asset
└── ...
```

Create these in Editor:
1. Right-click folder → Create → Dialogue → Character
2. Fill in name and colors
3. Save as asset

### Dialogue Data
```
Assets/_Game/Gameplay/Dialogue/Data/Dialogue/
├── Dialogue 1.asset
├── NPCGreeting.asset
└── ...
```

Create these in Editor:
1. Right-click folder → Create → Dialogue → Dialogue
2. Set array size for lines
3. For each line:
   - Drag character into Speaker field
   - Write text in Text field

---

## Usage

### Starting Dialogue

```csharp
// In an NPC or interactive object
public void OnPlayerInteract() {
    var dialogueService = ServiceLocator.Get<IDialogueService<DialogueSO>>();
    dialogueService.StartDialogue(_myDialogue);
}
```

### Advancing Dialogue

The player clicks/presses button to advance:

```csharp
// In UI or InputService
public void OnDialogueAdvance() {
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus.Publish<DialogueAdvanceRequestedEvent>(new());
    
    // DialogueService listens and calls DisplayNextLine()
}
```

### Responding to Dialogue Events

```csharp
private void Start() {
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
    eventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
}

private void OnDialogueStarted(DialogueStartedEvent evt) {
    Debug.Log("Dialogue started");
}

private void OnDialogueEnded(DialogueEndedEvent evt) {
    Debug.Log("Dialogue ended");
}
```

---

## Best Practices

### 1. Use ScriptableObjects for Data

✅ **Good:**
```csharp
[SerializeField] private DialogueSO _dialogue;
// Edit in Inspector, no code changes needed
```

❌ **Bad:**
```csharp
// Hard-coded dialogue—not flexible
public string GetDialogue() {
    return "Hello, how are you?";
}
```

### 2. Organize Characters Consistently

✅ **Good:**
```
Characters/
├── NPCs/
│   ├── Nurse.asset
│   └── Doctor.asset
└── Players/
    ├── Protagonist.asset
    └── Companion.asset
```

### 3. Color-Code Speakers

✅ **Good:**
- Each character has distinct colors
- Players can see speaker at a glance
- Example: Nurse in blue, Doctor in white, Player in green

### 4. Make Dialogue Reusable

✅ **Good:**
```csharp
// Same dialogue can be triggered from multiple places
[SerializeField] private DialogueSO _greetingDialogue;

public void OnMorningInteraction() {
    StartDialogue(_greetingDialogue);
}

public void OnEveningInteraction() {
    StartDialogue(_greetingDialogue);  // Reuse same dialogue
}
```

### 5. Validate Dialogue Data

✅ **Good:**
```csharp
public void StartDialogue(DialogueSO dialogue) {
    if (dialogue == null || dialogue.lines.Length == 0) {
        Debug.LogError("Invalid dialogue");
        return;
    }
    // ... proceed
}
```

---

## Common Patterns

### Simple NPC Greeting

1. Create `CharacterSO` (e.g., "Nurse.asset")
2. Create `DialogueSO` (e.g., "NurseGreeting.asset") with 3 lines
3. Attach to NPC:

```csharp
public class NPC : MonoBehaviour, IInteractable {
    [SerializeField] private DialogueSO _greetingDialogue;

    public void Interact() {
        var service = ServiceLocator.Get<IDialogueService<DialogueSO>>();
        service.StartDialogue(_greetingDialogue);
    }
}
```

### Multi-Speaker Conversation

Create `DialogueSO` with alternating speakers:

```
Line 1: Nurse says "Hello, patient!"
Line 2: Patient says "Hi, how are you?"
Line 3: Nurse says "I'm doing well, thank you!"
```

Each line references its speaker's `CharacterSO`.

### Dialogue After Event

```csharp
public class DoctorVisit : MonoBehaviour {
    [SerializeField] private DialogueSO _diagnoseDialogue;

    public void OnDiagnosisComplete() {
        // After some gameplay event
        var service = ServiceLocator.Get<IDialogueService<DialogueSO>>();
        service.StartDialogue(_diagnoseDialogue);
    }
}
```

---

## Extending Dialogue

### Adding Voice Acting

```csharp
[CreateAssetMenu(fileName = "Character", menuName = "Dialogue/Character")]
public class CharacterSO : ScriptableObject {
    public string characterName;
    public Color nameColor;
    public Color textColor;
    public AudioClip[] voiceLines;  // NEW: Voice clips
}

// In DialogueService
private void DisplayNextLine() {
    DialogueLine line = _linesQueue.Dequeue();
    _nameText.text = line.speaker.characterName;
    _dialogueText.text = line.text;
    
    // Play voice
    if (line.speaker.voiceLines.Length > lineIndex) {
        AudioSource.PlayClipAtPoint(
            line.speaker.voiceLines[lineIndex],
            transform.position
        );
    }
}
```

### Adding Branching Dialogue

```csharp
[System.Serializable]
public class DialogueOption {
    public string text;
    public DialogueSO nextDialogue;
}

[System.Serializable]
public class DialogueLine {
    public CharacterSO speaker;
    public string text;
    public DialogueOption[] choices;  // Player can choose
}
```

### Adding Animations

```csharp
private void DisplayNextLine() {
    DialogueLine line = _linesQueue.Dequeue();
    
    // Play speaker animation
    if (line.speaker.animator != null) {
        line.speaker.animator.SetTrigger("Talk");
    }
    
    _dialogueText.text = line.text;
}
```

---

## Debug Display

```csharp
private void OnGUI() {
    if (IsDialogueActive) {
        GUI.Box(new Rect(10, 10, 300, 100), _dialogueText.text);
    }
}
```

---

## Summary

The **Dialogue** system provides:
- **Data-driven dialogue**: ScriptableObjects for easy editing
- **Character management**: Reusable character definitions
- **Sequential playback**: Line-by-line display
- **Event-driven**: Integrates with state management
- **Extensible**: Easy to add voice, animations, branching

By using Dialogue system:
- **Designers** can write dialogue without coding
- **Programmers** don't hardcode text
- **Conversations** are reusable across scenes
- **NPCs** are easy to add and configure

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Core/Events/README.md](../../Core/Events/README.md) - Event system
