using UnityEngine;
using UnityEngine.Audio;

namespace FifthSemester.Systems.Audio {
    /// <summary>
    /// Represents an audio track instance, managing playback, volume, pitch, and AudioSource configuration for a single audio clip in a specific channel.
    /// </summary>
    public class AudioTrack {
        private const string TRACK_NAME_FORMAT = "Track - [{0}]";
        public string Name { get; private set; }
        public string Path { get; private set; }

        public GameObject Root => source.gameObject;

        private readonly AudioChannel channel;
        private readonly AudioSource source;

        public float VolumeCap { get; private set; }
        public float Pitch { get { return source.pitch; } set { source.pitch = value; } }
        public float Volume { get { return source.volume; } set { source.volume = value; } }

        public bool Loop => source.loop;
        public bool IsPlaying => source.isPlaying;

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
            Name = clip.name;
            Path = filePath;

            this.channel = channel;
            this.VolumeCap = volumeCap;

            source = CreateSource();
            source.clip = clip;
            source.loop = loop;
            source.volume = startingVolume;
            source.pitch = pitch;

            source.outputAudioMixerGroup = mixer;
        }

        /// <summary>
        /// Creates and configures the AudioSource for this track, attaching it to the channel's container.
        /// </summary>
        /// <returns>The created AudioSource component.</returns>
        private AudioSource CreateSource() {
            GameObject sourceObject = new GameObject(string.Format(TRACK_NAME_FORMAT, Name));
            sourceObject.transform.SetParent(channel.TrackContainer);
            AudioSource source = sourceObject.AddComponent<AudioSource>();

            return source;
        }

        /// <summary>
        /// Starts playback of the audio track.
        /// </summary>
        public void Play() {
            source.Play();
        }

        /// <summary>
        /// Stops playback of the audio track.
        /// </summary>
        public void Stop() {
            source.Stop();
        }
    }
}
