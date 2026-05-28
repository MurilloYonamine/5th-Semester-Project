# Triggers

World interaction entry points for the dialogue system.

- `TextTriggerBase` handles trigger enter/exit, interaction hints, and `IInteractable` compatibility.
- `DialogueTrigger` resolves localized dialogue text, forwards it to the dialogue service, and falls back to an `Animator` `IsTalking` flag when no `PlayableDirector` is used.
- `DocumentTrigger` parses localized document text and renders it through `DocumentView`.
- `CaptionTrigger` shows short caption text through the caption view.