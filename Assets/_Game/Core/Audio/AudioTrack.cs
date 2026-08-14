using UnityEngine;
using UnityEngine.Audio;

namespace FifthSemester.Core.Audio {
    /// <summary>
    /// Represents an audio track instance, managing playback, volume, pitch, and AudioSource configuration for a single audio clip in a specific channel.
    /// </summary>
    public class AudioTrack {
        private const string TRACK_NAME_FORMAT = "Track - [{0}]";
        public string Name { get; private set; }
        public string Path { get; private set; }

        public GameObject Root => source != null ? source.gameObject : null;
        public AudioSource Source => source;

        private readonly AudioChannel channel;
        private readonly AudioSource source;

        public float VolumeCap { get; private set; }
        public float Pitch { get { return source != null ? source.pitch : 1f; } set { if (source != null) source.pitch = value; } }
        public float Volume { get { return source != null ? source.volume : 0f; } set { if (source != null) source.volume = value; } }

        public bool Loop => source != null && source.loop;
        public bool IsPlaying => source != null && source.isPlaying;

        /// <summary>
        /// Initializes a new AudioTrack, creating its AudioSource and setting all playback parameters.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="loop">Whether the track should loop.</param>
        /// <param name="startingVolume">Initial volume of the track.</param>
        /// <param name="volumeCap">Maximum volume for the track.</param>
        /// <param name="pitch">Pitch of the track.</param>
        /// <param name="channel">The audio channel this track belongs to.</param>
        /// <param name="mixer">The AudioMixerGroup for output.</param>
        /// <param name="filePath">File path of the audio clip.</param>
        public AudioTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, AudioChannel channel, AudioMixerGroup mixer, string filePath) {
            Name = clip != null ? clip.name : string.Empty;
            Path = filePath;

            this.channel = channel;
            this.VolumeCap = volumeCap;

            source = CreateSource();
            if (source != null) {
                source.clip = clip;
                source.loop = loop;
                source.volume = startingVolume;
                source.pitch = pitch;
                source.outputAudioMixerGroup = mixer;
            }
        }

        /// <summary>
        /// Creates and configures the AudioSource for this track, attaching it to the channel's container.
        /// </summary>
        /// <returns>The created AudioSource component.</returns>
        private AudioSource CreateSource() {
            if (channel == null || channel.TrackContainer == null) return null;

            GameObject sourceObject = new GameObject(string.Format(TRACK_NAME_FORMAT, Name));
            sourceObject.transform.SetParent(channel.TrackContainer);
            AudioSource source = sourceObject.AddComponent<AudioSource>();

            return source;
        }

        /// <summary>
        /// Starts playback of the audio track.
        /// </summary>
        public void Play() {
            if (source != null) {
                source.Play();
            }
        }

        /// <summary>
        /// Stops playback of the audio track.
        /// </summary>
        public void Stop() {
            if (source != null) {
                source.Stop();
            }
        }
    }
}
