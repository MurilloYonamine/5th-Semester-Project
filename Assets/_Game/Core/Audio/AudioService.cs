using FifthSemester.Core.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace FifthSemester.Core.Audio {
    /// <summary>
    /// Central audio manager for the project. Handles music and SFX playback, channel management, volume control, and AudioMixer routing.
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService {
        private const string TAG = "<color=yellow><b>[AudioService]</b></color>";

        [Header("Audio Mixer Parameter Names")]
        public const string MASTER_VOLUME_PARAMETER_NAME = "MasterVolume";
        public const string MUSIC_VOLUME_PARAMETER_NAME = "MusicVolume";
        public const string SFX_VOLUME_PARAMETER_NAME = "SFXVolume";
        public const string AMBIENCE_VOLUME_PARAMETER_NAME = "AmbienceVolume";

        [Header("Audio Mixers")]
        [field: SerializeField] public AudioMixerGroup MasterMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup MusicMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup SFXMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup AmbienceMixer { get; private set; }

        [Header("Audio Settings")]
        public const float TRACK_TRANSITION_SPEED = 1f;
        public const float MUTED_VOLUME_LEVEL = -80f;

        [Header("SFX Settings")]
        private const string SFX_PARENT_NAME = "SFX";
        private const string SFX_NAME_FORMAT = "SFX - [{0}]";
        private Transform _sfxRoot;

        public Dictionary<int, AudioChannel> channels = new Dictionary<int, AudioChannel>();

        private void Awake() {
            ServiceLocator.Register<IAudioService>(this);

            _sfxRoot = new GameObject(SFX_PARENT_NAME).transform;
            _sfxRoot.SetParent(transform);
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<IAudioService>();
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            StopAllAmbience();
            StopAllSFX();
        }

        private void Start() {
            ISettingsService settings = ServiceLocator.Get<ISettingsService>();
            if (settings != null) {
                SetMasterVolume(settings.MasterVolume);
                SetMusicVolume(settings.MusicVolume);
                SetSFXVolume(settings.SFXVolume);
                SetAmbienceVolume(settings.AmbienceVolume);
            }
        }

        #region Play Audio
        /// <summary>
        /// Plays a sound effect (SFX) from a file path in the Resources folder.
        /// </summary>
        public AudioSource PlaySFX(string filePath, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false, float spatialBlend = 0.5f, float maxDistance = 500f) {
            AudioClip clip = Resources.Load<AudioClip>(filePath);

            if (clip == null) {
                Debug.LogError($"{TAG} Could not load audio file '{filePath}'. Please make sure this exists in the Resources directory!");
                return null;
            }

            return PlaySFX(clip, mixer, volume, pitch, loop, spatialBlend, filePath, maxDistance);
        }

        /// <summary>
        /// Plays a sound effect (SFX) from an AudioClip.
        /// </summary>
        public AudioSource PlaySFX(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false, float spatialBlend = 0.5f, string filePath = "", float maxDistance = 500f) {
            if (clip == null || this == null || !isActiveAndEnabled || _sfxRoot == null) return null;

            AudioMixerGroup targetMixer = mixer != null ? mixer : SFXMixer;

            GameObject effectObject = new GameObject(string.Format(SFX_NAME_FORMAT, clip.name));
            effectObject.transform.SetParent(_sfxRoot);

            AudioSource source = effectObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.maxDistance = maxDistance;
            source.outputAudioMixerGroup = targetMixer;

            source.Play();

            if (!loop) {
                Destroy(effectObject, clip.length / ((pitch <= 0) ? 1 : pitch));
            }

            return source;
        }

        /// <summary>
        /// Plays an audio track on the specified channel, creating it if needed.
        /// </summary>
        public AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f) {
            AudioClip clip = Resources.Load<AudioClip>(filePath);

            if (clip == null) {
                Debug.LogError($"{TAG} Could not load audio file '{filePath}'. Please make sure this exists in the Resources directory!");
                return null;
            }

            return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch, filePath: filePath);
        }

        /// <summary>
        /// Plays an audio track on the specified channel using an AudioClip.
        /// </summary>
        public AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "") {
            return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch, MusicMixer, filePath);
        }

        public AudioTrack PlayTrack(AudioClip clip, int channel, bool loop, float startingVolume, float volumeCap, float pitch, AudioMixerGroup mixer, string filePath = "") {
            if (clip == null || this == null || !isActiveAndEnabled) return null;

            AudioMixerGroup targetMixer = mixer != null ? mixer : MusicMixer;

            AudioChannel audioChannel = TryGetChannel(channel, createIfDoesNotExist: true);

            if (audioChannel == null) return null;

            AudioTrack track = audioChannel.PlayTrack(
                clip: clip,
                loop: loop,
                startingVolume: startingVolume,
                volumeCap: volumeCap,
                pitch: pitch,
                mixer: targetMixer,
                filePath: filePath
            );

            return track;
        }

        public AudioTrack PlayAmbience(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "") {
            int channelIndex = channel == 0 ? 1 : channel;
            return PlayTrack(
                clip: clip,
                channel: channelIndex,
                loop: loop,
                startingVolume: startingVolume,
                volumeCap: volumeCap,
                pitch: pitch,
                mixer: AmbienceMixer,
                filePath: filePath
            );
        }
        #endregion

        #region Stop Audio
        /// <summary>
        /// Stops the audio track on the specified channel.
        /// </summary>
        public void StopTrack(int channelNumber) {
            if (this == null) return;

            AudioChannel channel = TryGetChannel(
                channelNumber: channelNumber,
                createIfDoesNotExist: false
            );

            channel?.StopTrack();
        }

        /// <summary>
        /// Stops the audio track with the specified name.
        /// </summary>
        public void StopTrack(string trackName) {
            if (string.IsNullOrEmpty(trackName) || this == null || channels == null) return;

            trackName = trackName.ToLower();

            foreach (var channel in channels.Values) {
                if (channel != null && channel.TryGetTrack(trackName, out AudioTrack track)) {
                    channel.StopTrack();
                    return;
                }
            }
        }

        /// <summary>
        /// Stops all audio tracks on all channels.
        /// </summary>
        public void StopAllTracks() {
            if (this == null || channels == null) return;

            foreach (var channel in channels.Values) {
                channel?.StopTrack();
            }
        }

        /// <summary>
        /// Stops a specific sound effect (SFX) by AudioClip.
        /// </summary>
        public void StopSFX(AudioClip clip) {
            if (clip == null) return;

            StopSFX(clip.name);
        }

        /// <summary>
        /// Stops a specific sound effect (SFX) by name.
        /// </summary>
        public void StopSFX(string sfxName) {
            if (string.IsNullOrEmpty(sfxName) || this == null || _sfxRoot == null) return;

            sfxName = sfxName.ToLower();

            AudioSource[] sources = _sfxRoot.GetComponentsInChildren<AudioSource>();
            foreach (var source in sources) {
                if (source != null && source.clip != null && source.clip.name.ToLower() == sfxName) {
                    Destroy(source.gameObject);
                    return;
                }
            }
        }

        /// <summary>
        /// Stops all currently playing sound effects (SFX).
        /// </summary>
        public void StopAllSFX() {
            if (this == null || _sfxRoot == null) return;

            foreach (Transform child in _sfxRoot) {
                if (child != null) {
                    Destroy(child.gameObject);
                }
            }
        }

        public void StopAmbience(AudioClip clip) {
            if (clip == null) return;

            StopAmbience(clip.name);
        }

        public void StopAmbience(string ambienceName) {
            if (string.IsNullOrEmpty(ambienceName) || this == null || channels == null) return;

            ambienceName = ambienceName.ToLower();

            foreach (var channel in channels.Values) {
                if (channel != null && channel.TryGetTrack(ambienceName, out AudioTrack track)) {
                    channel.StopTrack(); 
                    return;
                }
            }
        }

        public void StopAllAmbience() {
            if (this == null || channels == null) return;

            foreach (var channel in channels.Values) {
                if (channel != null && channel.ActiveTrack != null && channel.ActiveTrack.Source != null && channel.ActiveTrack.Source.outputAudioMixerGroup == AmbienceMixer) {
                    channel.StopTrack(immediate: true);
                }
            }
        }
        #endregion

        #region Set Volumes
        /// <summary>
        /// Sets the master volume of the mixer.
        /// </summary>
        public void SetMasterVolume(float volume, bool muted = false) {
            if (MasterMixer == null || MasterMixer.audioMixer == null) return;
            float dbVolume = (volume <= 0) ? -80f : Mathf.Log10(volume / 100f) * 20f;
            MasterMixer.audioMixer.SetFloat(MASTER_VOLUME_PARAMETER_NAME, dbVolume);
        }

        /// <summary>
        /// Sets the music volume of the mixer.
        /// </summary>
        public void SetMusicVolume(float volume, bool muted = false) {
            if (MusicMixer == null || MusicMixer.audioMixer == null) return;
            float dbVolume = (volume <= 0) ? -80f : Mathf.Log10(volume / 100f) * 20f;
            MusicMixer.audioMixer.SetFloat(MUSIC_VOLUME_PARAMETER_NAME, dbVolume);
        }

        /// <summary>
        /// Sets the sound effects (SFX) volume of the mixer.
        /// </summary>
        public void SetSFXVolume(float volume, bool muted = false) {
            if (SFXMixer == null || SFXMixer.audioMixer == null) return;
            float dbVolume = (volume <= 0) ? -80f : Mathf.Log10(volume / 100f) * 20f;
            SFXMixer.audioMixer.SetFloat(SFX_VOLUME_PARAMETER_NAME, dbVolume);
        }

        /// <summary>
        /// Sets the ambience volume of the mixer.
        /// </summary>
        public void SetAmbienceVolume(float volume, bool muted = false) {
            if (AmbienceMixer == null || AmbienceMixer.audioMixer == null) return;
            float dbVolume = (volume <= 0) ? -80f : Mathf.Log10(volume / 100f) * 20f;
            AmbienceMixer.audioMixer.SetFloat(AMBIENCE_VOLUME_PARAMETER_NAME, dbVolume);
        }
        #endregion

        /// <summary>
        /// Tries to get an audio channel by number. Creates a new channel if it doesn't exist and createIfDoesNotExist is true.
        /// </summary>
        public AudioChannel TryGetChannel(int channelNumber, bool createIfDoesNotExist = false) {
            if (channels.TryGetValue(channelNumber, out AudioChannel channel)) {
                return channel;
            }
            else if (createIfDoesNotExist && this != null && isActiveAndEnabled) {
                channel = new AudioChannel(channelNumber, this);
                channels.Add(channelNumber, channel);
                return channel;
            }

            return null;
        }
    }
}
