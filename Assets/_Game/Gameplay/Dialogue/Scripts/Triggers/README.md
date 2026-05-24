# Triggers

World interaction entry points for the dialogue system.

- `TextTriggerBase` handles trigger enter/exit, interaction hints, and `IInteractable` compatibility.
- `DialogueTrigger` resolves localized dialogue text and forwards it to the dialogue service.
- `DocumentTrigger` parses localized document text and renders it through `DocumentView`.
- `CaptionTrigger` shows short caption text through the caption view.