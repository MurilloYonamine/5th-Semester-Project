using FifthSemester.Core.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace FifthSemester.Core.Services {
    public interface IAudioService {
        AudioSource PlaySFX(string filePath, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false, int spatialBlend = 1);
        AudioSource PlaySFX(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1f, float pitch = 1f, bool loop = false, int spatialBlend = 1, string filePath = "");
        AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "");
        
        AudioTrack PlayAmbience(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "");
        

        void StopTrack(int channelNumber);
        void StopTrack(string trackName);
        void StopAllTracks();

        void StopSFX(AudioClip clip);
        void StopSFX(string sfxName);
        void StopAllSFX();

        void StopAmbience(AudioClip clip);
        void StopAmbience(string ambienceName);
        void StopAllAmbience();

        void SetMasterVolume(float volume, bool muted = false);
        void SetMusicVolume(float volume, bool muted = false);
        void SetSFXVolume(float volume, bool muted = false);
        void SetAmbienceVolume(float volume, bool muted = false);
    }
}