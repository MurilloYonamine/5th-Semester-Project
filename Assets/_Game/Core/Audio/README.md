# The `Audio` Directory: Centralized Audio Management

The **Audio** layer provides a unified, channel-based audio system for PHOTOSSYNC. Instead of scattering `AudioSource` components throughout the scene, all audio playback is managed through a single `AudioService` that routes to organized channels (Music, SFX, Ambience) with independent volume control and mixer integration.

---

## Architecture Overview

The audio system is built on three core concepts:

1. **AudioService**: Central hub that manages all audio playback and channel management
2. **AudioChannel**: Manages multiple tracks within a specific audio category (Music, SFX, etc.)
3. **AudioTrack**: Individual audio clip instance with volume, pitch, and playback control
4. **AudioMixer**: Unity's AudioMixer for hierarchical volume control and effects

---

## Key Files

### `AudioService.cs`

The main implementation of `IAudioService`. It provides:

- **Multi-channel playback**: Maintains separate channels (indexed by integer)
- **SFX playback**: Fire-and-forget sound effects with optional pooling
- **Track management**: Named tracks for music and ambience with smooth transitions
- **Volume control**: Per-channel mixer parameter control (Master, Music, SFX, Ambience)
- **Mixer integration**: Routes audio through `AudioMixerGroup` for effects and dynamics

#### Key Methods

```csharp
// Play a one-shot SFX from file path
AudioSource source = audioService.PlaySFX("SFX/Jump", volume: 0.8f, pitch: 1f);

// Play a one-shot SFX from AudioClip
AudioSource source = audioService.PlaySFX(jumpClip, mixer: sfxMixer, volume: 1f);

// Play looping music track
AudioTrack track = audioService.PlayTrack(
    clip: menuMusicClip,
    channel: 0,           // Channel index
    loop: true,
    startingVolume: 0f,   // Fade in from 0
    volumeCap: 1f,        // Max volume
    pitch: 1f
);

// Play ambience
AudioTrack ambience = audioService.PlayAmbience(
    clip: forestAmbienceClip,
    channel: 1,
    loop: true,
    startingVolume: 0.5f,
    volumeCap: 0.7f
);

// Stop audio
audioService.StopTrack(0);           // Stop channel 0
audioService.StopTrack("TrackName"); // Stop by name
audioService.StopAllSFX();           // Clear all SFX

// Volume control
audioService.SetMasterVolume(0.8f);
audioService.SetMusicVolume(0.6f);
audioService.SetSFXVolume(1f);
audioService.SetAmbienceVolume(0.5f);
```

#### Mixer Parameter Names (Constants)

- `MASTER_VOLUME_PARAMETER_NAME = "MasterVolume"`
- `MUSIC_VOLUME_PARAMETER_NAME = "MusicVolume"`
- `SFX_VOLUME_PARAMETER_NAME = "SFXVolume"`
- `AMBIENCE_VOLUME_PARAMETER_NAME = "AmbienceVolume"`

#### Configuration Constants

- `TRACK_TRANSITION_SPEED = 1f`: Fade-in/out duration for track transitions
- `MUTED_VOLUME_LEVEL = -80f`: dB value for muted audio (AudioMixer standard)

---

### `AudioChannel.cs`

Represents a single audio channel that can hold multiple tracks (e.g., Music channel can have multiple songs queued).

#### Purpose

- Manages a list of `AudioTrack` instances
- Handles smooth volume transitions between tracks
- Cleans up inactive tracks
- Provides active track state tracking

#### Key Properties

```csharp
public int ChannelIndex { get; }           // 0, 1, 2, etc.
public Transform TrackContainer { get; }   // Hierarchy organization
public AudioTrack ActiveTrack { get; }     // Currently playing track
```

#### Key Methods

```csharp
// Play a track on this channel
AudioTrack playedTrack = channel.PlayTrack(
    clip: musicClip,
    loop: true,
    startingVolume: 0f,
    volumeCap: 1f,
    pitch: 1f,
    filePath: "Music/MenuTheme"
);

// Stop playback
channel.StopTrack(trackName);

// Fade volume
channel.SetVolume(0.5f, fadeSpeed: 1f);
```

#### Architecture

Channels are organized hierarchically in the scene:

```
AudioService
├── Channel - [0]          (Music)
│   ├── Track - [MenuTheme]
│   └── Track - [Gameplay]
├── Channel - [1]          (Ambience)
│   └── Track - [ForestLoop]
└── SFX
    ├── SFX - [Jump]
    └── SFX - [EnemyAttack]
```

---

### `AudioTrack.cs`

Represents a single audio clip instance being played in a specific channel.

#### Purpose

- Wraps an `AudioSource` component
- Stores metadata (name, file path)
- Provides volume/pitch control
- Tracks playback state (playing, volume cap, pitch)

#### Key Properties

```csharp
public string Name { get; }        // Audio clip name
public string Path { get; }        // File path (for debugging)
public float Volume { get; set; }  // Current volume (0-1)
public float VolumeCap { get; }    // Maximum allowed volume
public float Pitch { get; set; }   // Playback speed
public bool Loop { get; }          // Is looping enabled?
public bool IsPlaying { get; }     // Is currently playing?
public GameObject Root { get; }    // GameObject containing AudioSource
```

#### Key Methods

```csharp
// Playback control
track.Play();
track.Stop();
track.Pause();
track.Resume();

// Volume/Pitch control
track.Volume = 0.8f;
track.Pitch = 1.5f;  // Speed up
```

---

### `MainMixer.mixer`

Unity AudioMixer asset defining the hierarchical volume control structure.

#### Structure

- **Master**: Root volume group
  - **Music**: Music channel volume
  - **SFX**: Sound effects channel volume
  - **Ambience**: Background ambience channel volume

#### Usage

Each mixer group receives audio from its corresponding `AudioMixerGroup` in AudioService:

```csharp
[field: SerializeField] public AudioMixerGroup MasterMixer { get; }
[field: SerializeField] public AudioMixerGroup MusicMixer { get; }
[field: SerializeField] public AudioMixerGroup SFXMixer { get; }
[field: SerializeField] public AudioMixerGroup AmbienceMixer { get; }
```

---

## Usage Patterns

### 1. Play Menu Music (with fade-in)

```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

AudioTrack menuMusic = audio.PlayTrack(
    clip: menuMusicClip,
    channel: 0,
    loop: true,
    startingVolume: 0f,      // Start silent
    volumeCap: 0.8f,         // Fade in to 80%
    pitch: 1f
);
```

### 2. Play Sound Effect

```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

// One-shot SFX (no waiting for return)
audio.PlaySFX("SFX/PlayerJump", volume: 0.9f);

// Or from AudioClip
audio.PlaySFX(jumpClip, sfxMixer, volume: 1f, pitch: 1f);
```

### 3. Cross-fade Music

```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

// Stop old music (fades out)
audio.StopTrack(0);

// Play new music (fades in from 0)
audio.PlayTrack(
    clip: combatMusicClip,
    channel: 0,
    loop: true,
    startingVolume: 0f,
    volumeCap: 1f
);
```

### 4. Control Volume Dynamically

```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

// Player opens settings
audio.SetMusicVolume(0.5f);
audio.SetSFXVolume(0.7f);
audio.SetMasterVolume(0.9f);
```

### 5. Mute All Audio

```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

audio.SetMasterVolume(0f, muted: true);
```

---

## Best Practices

### 1. Always Request AudioService via ServiceLocator
❌ **Bad:**
```csharp
public class Player : MonoBehaviour
{
    private AudioService _audio = FindObjectOfType<AudioService>();
}
```

✅ **Good:**
```csharp
public class Player : MonoBehaviour
{
    private IAudioService _audio = ServiceLocator.Get<IAudioService>();
}
```

### 2. Use File Paths for SFX, AudioClips for Music
❌ **Bad:**
```csharp
// Direct serialization causes tight coupling
[SerializeField] private AudioClip jumpSFX;
audio.PlaySFX(jumpSFX);
```

✅ **Good:**
```csharp
// File path based; can be updated without code changes
audio.PlaySFX("SFX/Jump", volume: 0.8f);
```

### 3. Fade Music on Transitions, Not SFX
✅ **Good:**
```csharp
// Music fades gracefully
audio.PlayTrack(newMusic, channel: 0, startingVolume: 0f, volumeCap: 1f);

// SFX plays immediately
audio.PlaySFX("SFX/UIClick", volume: 1f);
```

### 4. Set Volume Caps to Control Blend
✅ **Good:**
```csharp
// Music quieter when combat active
audio.PlayTrack(
    clip: combatMusic,
    channel: 0,
    startingVolume: 0.5f,  // Start at 50%
    volumeCap: 0.6f        // Never louder than 60%
);
```

### 5. Stop Tracks Explicitly Before Playing New Ones
✅ **Good:**
```csharp
audio.StopTrack(0);  // Clear old music
audio.PlayTrack(newMusic, channel: 0, startingVolume: 0f, volumeCap: 1f);
```

---

## Setup Checklist

- [ ] Assign **MasterMixer** in AudioService Inspector
- [ ] Assign **MusicMixer** group from MainMixer
- [ ] Assign **SFXMixer** group from MainMixer
- [ ] Assign **AmbienceMixer** group from MainMixer
- [ ] Create Resources folder structure: `Resources/SFX/`, `Resources/Music/`, `Resources/Ambience/`
- [ ] Register AudioService in Core initialization
- [ ] Subscribe to `ISettingsService` for volume persistence

---

## Summary

The **Audio** subsystem provides:
- **Centralized management** of all audio playback
- **Channel-based organization** (Music, SFX, Ambience)
- **Mixer integration** for professional audio control
- **Fire-and-forget SFX** alongside managed tracks
- **Smooth transitions** between music tracks
- **Volume persistence** via `ISettingsService`

By using `IAudioService` consistently, the game maintains clean separation between gameplay logic and audio concerns.
