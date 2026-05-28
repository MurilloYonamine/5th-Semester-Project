# Map2 Scripts

Scripts for the standalone map 2 puzzle.

## Contents

- `Map2KeyDefinitionSO.cs` — key asset definition used by item and door
- `Map2PasswordState.cs` — persistent password data and PlayerPrefs storage
- `Map2PasswordController.cs` — orchestrates password updates and UI refresh
- `Map2PasswordDigitInteractable.cs` — one-shot digit sprite interaction
- `Map2PasswordDeliveryPoint.cs` — interaction point that accepts the completed password
- `Map2KeyItem.cs` — reusable inventory key item with key-definition reference
- `Map2KeyDoor.cs` — door variant that checks the matching key-definition reference
- `Map2IntroSequence.cs` — local Map 2 startup flow that fades in and then plays the intro timeline
- `Map2HorrorAmbientPlayer.cs` — random horror ambient player that fires clips at irregular intervals
