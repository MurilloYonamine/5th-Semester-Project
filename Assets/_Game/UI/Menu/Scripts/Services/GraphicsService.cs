using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FifthSemester.Core.Services;
using FifthSemester.UI;

namespace FifthSemester.UI
{
    public class GraphicsService : IGraphicsService
    {
        private const string TAG = "<color=yellow><b>[GraphicsService]</b></color>";

        private UniversalRendererData _rendererData;

        [Header("Feature Names")]
        private const string PIXELATE = "PixelatePassRendererFeature";
        private const string DITHER = "DitherPassRendererFeature";
        private const string BARREL_DISTORTION = "BarrelDistortionPassRendererFeature";
        private const string ROLLING_BANDS = "RollingBandsPassRendererFeature";
        private const string SCANLINES = "ScanlinesPassRendererFeature";
        private const string VHS_EFFECT = "VHSEffectPassRendererFeature";

        public GraphicsService(UniversalRendererData rendererData)
        {
            _rendererData = rendererData;
            ServiceLocator.Register<IGraphicsService>(this);
        }

        public void SetBarrelDistortion(bool isEnabled)
        {
            SetFeatureActive(BARREL_DISTORTION, isEnabled);
            UpdatePSXVolume(vol => vol.enableBarrelDistortion.value = isEnabled);
        }

        public void SetDithering(bool isEnabled)
        {
            SetFeatureActive(DITHER, isEnabled);
            UpdatePSXVolume(vol => vol.enableDithering.value = isEnabled);
        }

        public void SetPixelation(bool isEnabled)
        {
            SetFeatureActive(PIXELATE, isEnabled);
            UpdatePSXVolume(vol => vol.enablePixelation.value = isEnabled);
        }

        public void SetRollingBands(bool isEnabled)
        {
            SetFeatureActive(ROLLING_BANDS, isEnabled);
            UpdatePSXVolume(vol => vol.enableScanlines.value = isEnabled);
        }

        public void SetScanlines(bool isEnabled)
        {
            SetFeatureActive(SCANLINES, isEnabled);
            UpdatePSXVolume(vol => vol.enableScanlines.value = isEnabled);
        }

        public void SetVHSEffect(bool isEnabled)
        {
            SetFeatureActive(VHS_EFFECT, isEnabled);
            UpdatePSXVolume(vol => vol.enableGlitch.value = isEnabled);
        }

        private void SetFeatureActive(string featureName, bool isEnabled)
        {
            if (_rendererData != null && _rendererData.rendererFeatures != null)
            {
                var feature = _rendererData.rendererFeatures.Find(f => f != null && f.name == featureName);
                if (feature != null)
                {
                    feature.SetActive(isEnabled);
                }
            }
        }

        private void UpdatePSXVolume(System.Action<PSXVolume> updateAction)
        {
            if (VolumeManager.instance != null && VolumeManager.instance.stack != null)
            {
                var psxVolume = VolumeManager.instance.stack.GetComponent<PSXVolume>();
                if (psxVolume != null)
                {
                    updateAction(psxVolume);
                }
            }
        }
    }
}