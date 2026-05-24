# The `Dialogue` Directory: Conversation System

> Runtime update: the dialogue system is being split into `Views/`, `Triggers/`, and `Services/`.
>
> Current runtime flow:
> - `DialogueTrigger` still loads localized `TextAsset` dialogue files
> - `DialogueService` parses `TextAsset` content through `DialogueParser.Parse(TextAsset)`
> - `DialogueStartedEvent` / `DialogueEndedEvent` still drive game state transitions
> - UI rendering is moving into the new `Views/` layer

The **Dialogue** directory manages **text-driven interactions**—dialogue playback, documents, captions, character data, and the transition toward a separated view/trigger/service architecture.

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

Views/
├─ TextViewBase
├─ DialogueView
├─ DocumentView
└─ CaptionView

Triggers/
├─ TextTriggerBase
├─ DialogueTrigger
├─ DocumentTrigger
└─ CaptionTrigger

DialogueService (MonoBehaviour)
├─ Coordinates dialogue playback
├─ Still publishes DialogueStartedEvent / DialogueEndedEvent
└─ Is being refactored away from direct UI ownership
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
