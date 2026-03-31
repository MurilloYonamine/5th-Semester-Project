using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Core.Managers
{
    /// <summary>
    /// Represents an audio channel that manages multiple tracks (AudioTrack), allowing playing, stopping, and smooth transitions between tracks.
    /// Responsible for creating the track container, controlling the volume, activating/deactivating tracks, and destroying unused tracks.
    /// </summary>
    public class AudioChannel
    {
        private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";

        public int ChannelIndex { get; private set; }
        public Transform TrackContainer { get; private set; } = null;
        public AudioTrack ActiveTrack { get; private set; } = null;
        
        private List<AudioTrack> _tracks = new List<AudioTrack>();

        bool IsLevelingVolume => co_volumeLeveling != null;
        Coroutine co_volumeLeveling = null;

        /// <summary>
        /// Initializes a new audio channel, creating its track container and setting its index.
        /// </summary>
        /// <param name="channel">The index of the channel.</param>
        public AudioChannel(int channel)
        {
            ChannelIndex = channel;

            TrackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
            TrackContainer.SetParent(AudioManager.Instance.transform);
        }

        /// <summary>
        /// Plays an audio track on this channel. If the track already exists, it resumes playback; otherwise, it creates and plays a new track.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="loop">Whether the track should loop.</param>
        /// <param name="startingVolume">Initial volume of the track.</param>
        /// <param name="volumeCap">Maximum volume for the track.</param>
        /// <param name="pitch">Pitch of the track.</param>
        /// <param name="filePath">File path of the audio clip.</param>
        /// <returns>The AudioTrack being played.</returns>
        public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, string filePath)
        {
            if (TryGetTrack(clip.name, out AudioTrack existingTrack))
            {
                if (!existingTrack.IsPlaying)
                {
                    existingTrack.Play();
                }

                SetAsActiveTrack(existingTrack);

                return existingTrack;
            }

            AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, AudioManager.Instance.MusicMixer, filePath);
            track.Play();

            SetAsActiveTrack(track);

            return track;
        }

        /// <summary>
        /// Tries to find a track by name in this channel.
        /// </summary>
        /// <param name="trackName">The name of the track to search for.</param>
        /// <param name="value">The found AudioTrack, or null if not found.</param>
        /// <returns>True if the track exists, false otherwise.</returns>
        public bool TryGetTrack(string trackName, out AudioTrack value)
        {
            trackName = trackName.ToLower();

            foreach (var track in _tracks)
            {
                if (track.Name.ToLower() == trackName)
                {
                    value = track;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Sets the given track as the active track for this channel and starts volume leveling if needed.
        /// </summary>
        /// <param name="track">The track to set as active.</param>
        private void SetAsActiveTrack(AudioTrack track)
        {
            if (!_tracks.Contains(track))
            {
                _tracks.Add(track);
            }

            ActiveTrack = track;

            TryStartVolumeLeveling();
        }

        /// <summary>
        /// Starts the volume leveling coroutine if it is not already running.
        /// </summary>
        private void TryStartVolumeLeveling()
        {
            if (!IsLevelingVolume)
            {
                co_volumeLeveling = AudioManager.Instance.StartCoroutine(VolumeLeveling());
            }
        }

        /// <summary>
        /// Coroutine that smoothly transitions track volumes, fading out inactive tracks and fading in the active one.
        /// </summary>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator VolumeLeveling()
        {
            while (ShouldContinueVolumeLeveling())
            {
                for (int i = _tracks.Count - 1; i >= 0; i--)
                {
                    AudioTrack track = _tracks[i];

                    float targetVol = ActiveTrack == track ? track.VolumeCap : 0;

                    if (track == ActiveTrack && track.Volume == targetVol) continue;

                    track.Volume = Mathf.MoveTowards(track.Volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);

                    if (track != ActiveTrack && track.Volume == 0)
                    {
                        DestroyTrack(track);
                    }
                }
                yield return null;
            }

            co_volumeLeveling = null;
        }
        /// <summary>
        /// Determines if the volume leveling coroutine should continue running.
        /// </summary>
        /// <returns>True if leveling should continue, false otherwise.</returns>
        private bool ShouldContinueVolumeLeveling()
        {
            bool hasActiveTrack = ActiveTrack != null;
            bool multipleTracksOrVolumeNotMax = hasActiveTrack && (_tracks.Count > 1 || ActiveTrack.Volume != ActiveTrack.VolumeCap);
            bool noActiveTrackButHasTracks = !hasActiveTrack && _tracks.Count > 0;

            bool shouldContinue = multipleTracksOrVolumeNotMax || noActiveTrackButHasTracks;

            return shouldContinue;
        }
        /// <summary>
        /// Removes and destroys the specified track from this channel.
        /// </summary>
        /// <param name="track">The track to destroy.</param>
        private void DestroyTrack(AudioTrack track)
        {
            if (_tracks.Contains(track))
            {
                _tracks.Remove(track);
            }

            Object.Destroy(track.Root);
        }

        /// <summary>
        /// Stops the currently active track. If immediate is true, destroys the track instantly; otherwise, fades it out.
        /// </summary>
        /// <param name="immediate">If true, stops and destroys the track immediately.</param>
        public void StopTrack(bool immediate = false)
        {
            if (ActiveTrack == null) return;

            if (immediate)
            {
                DestroyTrack(ActiveTrack);
                ActiveTrack = null;
                return;
            }
            ActiveTrack = null;
            TryStartVolumeLeveling();
        }
    }
}
