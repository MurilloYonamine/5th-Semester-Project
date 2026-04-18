# Audio System Documentation

## Overview
This audio system provides a modular and extensible way to manage music and sound effects (SFX) in a Unity project. It is composed of three main classes: `AudioManager`, `AudioChannel`, and `AudioTrack`. The system supports multiple audio channels, volume control, smooth transitions, and flexible playback of both music and SFX.

## Main Components

### AudioManager
- Singleton responsible for global audio control.
- Manages all audio channels and SFX playback.
- Provides methods to play, stop, and control volume for music and SFX.
- Handles AudioMixer routing for master, music, and SFX groups.

### AudioChannel
- Represents a logical channel for music playback.
- Each channel can play one active track at a time.
- Handles smooth transitions (fade in/out) between tracks.
- Manages a container for all tracks associated with the channel.

### AudioTrack
- Represents a single music track instance.
- Wraps an `AudioSource` and manages its playback, volume, and pitch.
- Can be played, stopped, and queried for its state.

## Features
- Play music tracks on specific channels with custom volume, pitch, and looping.
- Play SFX with optional looping and custom AudioMixer routing.
- Stop tracks or SFX by name, channel, or globally.
- Set master, music, and SFX volumes with mute support.
- Automatic destruction of finished SFX and unused tracks.
- Volume transitions for smooth music changes.

## Usage

### Playing a Music Track
```
AudioManager.Instance.PlayTrack(
    clip: myAudioClip,
    channel: 0, // Channel index
    loop: true,
    startingVolume: 0.5f,
    volumeCap: 1.0f,
    pitch: 1.0f,
    filePath: "Music/MyTrack"
);
```

### Playing a Sound Effect (SFX)
```
AudioManager.Instance.PlaySFX(
    filePath: "SFX/Explosion",
    mixer: null, // Optional AudioMixerGroup
    volume: 1.0f,
    pitch: 1.0f,
    loop: false
);
```

### Stopping Audio
- Stop a track on a channel:
  `AudioManager.Instance.StopTrack(channelNumber);`
- Stop a track by name:
  `AudioManager.Instance.StopTrack("TrackName");`
- Stop all tracks:
  `AudioManager.Instance.StopAllTracks();`
- Stop all SFX:
  `AudioManager.Instance.StopAllSFX();`

### Setting Volumes
```
AudioManager.Instance.SetMasterVolume(0.8f, false);
AudioManager.Instance.SetMusicVolume(0.7f, false);
AudioManager.Instance.SetSFXVolume(1.0f, false);
```

## Extending
- Add new channels by calling `TryGetChannel(channelNumber, true)`.
- Customize volume curves and transition speeds via exposed constants and serialized fields.
- Integrate with Unity's AudioMixer for advanced audio routing and effects.

## Requirements
- UnityEngine
- AudioMixer setup with exposed parameters for master, music, and SFX volumes

## Notes
- All audio files for music and SFX should be placed in the `Resources` folder for dynamic loading.
- The system is designed to persist across scenes and should be initialized automatically on first use.

## License
See LICENSE file for details.
