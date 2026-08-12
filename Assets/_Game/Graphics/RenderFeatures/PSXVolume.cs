using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FifthSemester.UI
{
    [Serializable, VolumeComponentMenu("Retro PSX/CRT Post-Process")]
    public class PSXVolume : VolumeComponent, IPostProcessComponent
    {
        [Header("Master Controls")]
        public BoolParameter activeEffect = new BoolParameter(true);

        [Header("Pixelation")]
        public BoolParameter enablePixelation = new BoolParameter(true);
        public ClampedFloatParameter pixelResolutionX = new ClampedFloatParameter(320f, 64f, 1920f);
        public ClampedFloatParameter pixelResolutionY = new ClampedFloatParameter(240f, 48f, 1080f);

        [Header("CRT Barrel Distortion")]
        public BoolParameter enableBarrelDistortion = new BoolParameter(true);
        public ClampedFloatParameter barrelStrength = new ClampedFloatParameter(0.12f, -0.5f, 0.5f);
        public ClampedFloatParameter barrelTightness = new ClampedFloatParameter(3.0f, 0.1f, 8.0f);
        public ClampedFloatParameter barrelZoom = new ClampedFloatParameter(0.98f, 0.5f, 1.5f);
        public ClampedFloatParameter cornerVignette = new ClampedFloatParameter(0.4f, 0f, 1f);

        [Header("Bayer Dithering")]
        public BoolParameter enableDithering = new BoolParameter(true);
        public ClampedFloatParameter ditherSpread = new ClampedFloatParameter(16f, 2f, 64f);
        public ClampedFloatParameter ditherStrength = new ClampedFloatParameter(0.7f, 0f, 1f);

        [Header("Chroma Bleed Analogo NTSC")]
        public BoolParameter enableChromaBleed = new BoolParameter(true);
        public ClampedFloatParameter bleedSpread = new ClampedFloatParameter(0.005f, 0f, 0.02f);

        [Header("Scanlines and Rolling Bands")]
        public BoolParameter enableScanlines = new BoolParameter(true);
        public ClampedFloatParameter scanlineCount = new ClampedFloatParameter(240f, 50f, 800f);
        public ClampedFloatParameter scanlineIntensity = new ClampedFloatParameter(0.3f, 0f, 1f);
        public ClampedFloatParameter rollingBandSpeed = new ClampedFloatParameter(1.0f, -5f, 5f);
        public ClampedFloatParameter rollingBandIntensity = new ClampedFloatParameter(0.1f, 0f, 1f);

        [Header("Glitch and VHS Tape Noise")]
        public BoolParameter enableGlitch = new BoolParameter(false);
        public ClampedFloatParameter glitchAmount = new ClampedFloatParameter(0.01f, 0f, 0.1f);
        public ClampedFloatParameter vhsTapeGrain = new ClampedFloatParameter(0.1f, 0f, 1f);

        [Header("Distance Fog (World Shaders)")]
        public BoolParameter enableFog = new BoolParameter(false);
        public ColorParameter fogColor = new ColorParameter(new Color(0.1f, 0.05f, 0.15f, 1f), true, false, false);
        public ClampedFloatParameter fogStart = new ClampedFloatParameter(5f, 0f, 100f);
        public ClampedFloatParameter fogEnd = new ClampedFloatParameter(30f, 1f, 500f);
        public BoolParameter fogExponential = new BoolParameter(false);
        public ClampedFloatParameter fogDensity = new ClampedFloatParameter(0.02f, 0f, 0.1f);

        public bool IsActive() => activeEffect.value;

        public bool IsTileCompatible() => false;
    }
}
