using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace FifthSemester.Systems.Audio {
    /// <summary>
    /// Central audio manager for the project. Handles music and SFX playback, channel management, volume control, and AudioMixer routing.
    /// Singleton pattern ensures a single instance persists across scenes.
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer Parameter Names")]
        public const string MASTER_VOLUME_PARAMETER_NAME = "MasterVolume";
        public const string MUSIC_VOLUME_PARAMETER_NAME = "MusicVolume";
        public const string SFX_VOLUME_PARAMETER_NAME = "SFXVolume";

        [Header("Audio Mixers")]
        [field: SerializeField] public AudioMixerGroup MasterMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup MusicMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup SFXMixer { get; private set; }

        [Header("Audio Settings")]
        public const float TRACK_TRANSITION_SPEED = 1f;
        public const float MUTED_VOLUME_LEVEL = -80f;
        [SerializeField] private AnimationCurve audioFalloffCurve = AnimationCurve.Linear(0, -80, 1, 0);

        [Header("SFX Settings")]
        private const string SFX_PARENT_NAME = "SFX";
        private const string SFX_NAME_FORMAT = "SFX - [{0}]";
        private Transform _sfxRoot;

        public Dictionary<int, AudioChannel> channels = new Dictionary<int, AudioChannel>();

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }

            _sfxRoot = new GameObject(SFX_PARENT_NAME).transform;
            _sfxRoot.SetParent(transform);
        }

        private void Start() {
            int masterLevel = PlayerPrefs.GetInt("MasterVolume", 4);
            int musicLevel = PlayerPrefs.GetInt("MusicVolume", 4);
            int sfxLevel = PlayerPrefs.GetInt("SFXVolume", 4);

            SetMasterVolume(masterLevel * 0.25f);
            SetMusicVolume(musicLevel * 0.25f);
            SetSFXVolume(sfxLevel * 0.25f);
        }
        #region Play Audio
        /// <summary>
        /// Plays a sound effect (SFX) from a file path in the Resources folder.
        /// </summary>
        /// <param name="filePath">Audio file path in the Resources folder.</param>
        /// <param name="mixer">Audio mixer to use (optional).</param>
        /// <param name="volume">Audio volume.</param>
        /// <param name="pitch">Audio pitch.</param>
        /// <param name="loop">Whether the audio should loop.</param>
        /// <returns>AudioSource created for the SFX.</returns>
        public AudioSource PlaySFX(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false, int spatialBlend = 1) {
            AudioClip clip = Resources.Load<AudioClip>(filePath);

            if (clip == null) {
                Debug.LogError($"Could not load audio file '{filePath}'. Please make sure this exists in the Resources directory!");
                return null;
            }

            return PlaySFX(clip, mixer, volume, pitch, loop, spatialBlend, filePath);
        }
        /// <summary>
        /// Plays a sound effect (SFX) from an AudioClip.
        /// </summary>
        /// <param name="clip">AudioClip to play.</param>
        /// <param name="mixer">Audio mixer to use (optional).</param>
        /// <param name="volume">Audio volume.</param>
        /// <param name="pitch">Audio pitch.</param>
        /// <param name="loop">Whether the audio should loop.</param>
        /// <param name="filePath">File name or path (optional).</param>
        /// <returns>AudioSource created for the SFX.</returns>
        public AudioSource PlaySFX(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false, int spatialBlend = 1, string filePath = "") {
            string fileName = clip != null ? clip.name : "NULL_CLIP";

            if (filePath != string.Empty) {
                fileName = filePath;
            }

            AudioSource audioSource = new GameObject(string.Format(SFX_NAME_FORMAT, fileName)).AddComponent<AudioSource>();
            audioSource.transform.SetParent(_sfxRoot);
            audioSource.transform.position = _sfxRoot.position;

            audioSource.clip = clip;

            if (mixer == null) {
                mixer = SFXMixer;
            }

            audioSource.outputAudioMixerGroup = mixer;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = spatialBlend;
            audioSource.loop = loop;

            audioSource.Play();

            if (!loop) {
                Destroy(audioSource.gameObject, (clip.length / pitch) + 1);
            }

            return audioSource;
        }
        /// <summary>
        /// Plays an audio track (music) from a file path in the Resources folder on a specific channel.
        /// </summary>
        /// <param name="filePath">Audio file path in the Resources folder.</param>
        /// <param name="channel">Channel number.</param>
        /// <param name="loop">Whether the track should loop.</param>
        /// <param name="startingVolume">Initial track volume.</param>
        /// <param name="volumeCap">Maximum allowed volume.</param>
        /// <param name="pitch">Track pitch.</param>
        /// <returns>AudioTrack created for the track.</returns>
        private AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f) {
            AudioClip clip = Resources.Load<AudioClip>(filePath);

            if (clip == null) {
                Debug.LogError($"Could not load audio file '{filePath}'. Please make sure this exists in the Resources directory!");
                return null;
            }

            return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch, filePath);

        }
        /// <summary>
        /// Plays an audio track (music) from an AudioClip on a specific channel.
        /// </summary>
        /// <param name="clip">AudioClip to play.</param>
        /// <param name="channel">Channel number.</param>
        /// <param name="loop">Whether the track should loop.</param>
        /// <param name="startingVolume">Initial track volume.</param>
        /// <param name="volumeCap">Maximum allowed volume.</param>
        /// <param name="pitch">Track pitch.</param>
        /// <param name="filePath">File name or path (optional).</param>
        /// <returns>AudioTrack created for the track.</returns>
        public AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "") {
            AudioChannel audioChannel = TryGetChannel(
                channelNumber: channel,
                createIfDoesNotExist: true
            );

            AudioTrack audioTrack = audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, pitch, filePath);

            return audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, pitch, filePath);
        }
        #endregion
        #region Stop Audio
        /// <summary>
        /// Stops the audio track on a specific channel.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        public void StopTrack(int channelNumber) {
            AudioChannel channel = TryGetChannel(
                channelNumber: channelNumber,
                createIfDoesNotExist: false
            );

            channel?.StopTrack();
        }
        /// <summary>
        /// Stops the audio track with the specified name.
        /// </summary>
        /// <param name="trackName">Track name.</param>
        public void StopTrack(string trackName) {
            trackName = trackName.ToLower();

            foreach (var channel in channels.Values) {
                if (channel.TryGetTrack(trackName, out AudioTrack track)) {
                    channel.StopTrack();
                    return;
                }
            }
        }
        /// <summary>
        /// Stops all audio tracks on all channels.
        /// </summary>
        public void StopAllTracks() {
            foreach (var channel in channels.Values) {
                channel.StopTrack();
            }
        }
        /// <summary>
        /// Stops a specific sound effect (SFX) by AudioClip.
        /// </summary>
        /// <param name="clip">AudioClip of the SFX to stop.</param>
        public void StopSFX(AudioClip clip) {
            if (clip == null) return;

            StopSFX(clip.name);
        }
        /// <summary>
        /// Stops a specific sound effect (SFX) by name.
        /// </summary>
        /// <param name="sfxName">SFX name.</param>
        public void StopSFX(string sfxName) {
            sfxName = sfxName.ToLower();

            AudioSource[] sources = _sfxRoot.GetComponentsInChildren<AudioSource>();
            foreach (var source in sources) {
                if (source.clip != null && source.clip.name.ToLower() == sfxName) {
                    Destroy(source.gameObject);
                    return;
                }
            }
        }
        /// <summary>
        /// Stops all currently playing sound effects (SFX).
        /// </summary>
        public void StopAllSFX() {
            foreach (Transform child in _sfxRoot) {
                Destroy(child.gameObject);
            }
        }
        #endregion
        #region Set Volumes
        /// <summary>
        /// Sets the master volume of the mixer.
        /// </summary>
        /// <param name="volume">Volume value (0 to 1).</param>
        /// <param name="muted">Whether to mute the audio.</param>
        public void SetMasterVolume(float volume, bool muted = false) {
            float dbVolume = (muted || volume <= 0f) ? MUTED_VOLUME_LEVEL : Mathf.Log10(volume) * 20f;
            MasterMixer.audioMixer.SetFloat(MASTER_VOLUME_PARAMETER_NAME, dbVolume);
        }

        /// <summary>
        /// Sets the music volume of the mixer.
        /// </summary>
        /// <param name="volume">Volume value (0 to 1).</param>
        /// <param name="muted">Whether to mute the audio.</param>
        public void SetMusicVolume(float volume, bool muted = false) {
            float dbVolume = (muted || volume <= 0f) ? MUTED_VOLUME_LEVEL : Mathf.Log10(volume) * 20f;
            MusicMixer.audioMixer.SetFloat(MUSIC_VOLUME_PARAMETER_NAME, dbVolume);
        }

        /// <summary>
        /// Sets the sound effects (SFX) volume of the mixer.
        /// </summary>
        /// <param name="volume">Volume value (0 to 1).</param>
        /// <param name="muted">Whether to mute the audio.</param>
        public void SetSFXVolume(float volume, bool muted = false) {
            float dbVolume = (muted || volume <= 0f) ? MUTED_VOLUME_LEVEL : Mathf.Log10(volume) * 20f;
            SFXMixer.audioMixer.SetFloat(SFX_VOLUME_PARAMETER_NAME, dbVolume);
        }
        #endregion

        /// <summary>
        /// Tries to get an audio channel by number. Creates a new channel if it doesn't exist and createIfDoesNotExist is true.
        /// </summary>
        /// <param name="channelNumber">Channel number.</param>
        /// <param name="createIfDoesNotExist">Whether to create the channel if it doesn't exist.</param>
        /// <returns>Corresponding AudioChannel or null.</returns>
        public AudioChannel TryGetChannel(int channelNumber, bool createIfDoesNotExist = false) {
            if (channels.TryGetValue(channelNumber, out AudioChannel channel)) {
                return channel;
            }
            else if (createIfDoesNotExist) {
                channel = new AudioChannel(channelNumber);
                channels.Add(channelNumber, channel);
                return channel;
            }

            return null;
        }
    }
}
