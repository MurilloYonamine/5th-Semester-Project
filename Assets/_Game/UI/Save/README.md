# Save UI

This folder contains UI feedback for the autosave flow.

## Contents
- `SaveToastView.cs` - top-right toast that shows "Salvando o jogo..." only while an autosave is running.

## Behavior
- Listens to `AutosaveStartedEvent` and `AutosaveCompletedEvent`.
- Uses a `CanvasGroup` so it can fade in/out without blocking input.
- Should be placed on a small HUD panel anchored to the top-right corner.
