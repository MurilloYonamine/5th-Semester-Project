using UnityEngine.Rendering.Universal;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class GraphicsService : IGraphicsService {
        private UniversalRendererData _rendererData;

        [Header("Feature Names")]
        private const string PIXELATE = "PixelatePassRendererFeature";
        private const string DITHER = "DitherPassRendererFeature";
        private const string BARREL_DISTORTION = "BarrelDistortionPassRendererFeature";
        private const string ROLLING_BANDS = "RollingBandsPassRendererFeature";
        private const string SCANLINES = "ScanlinesPassRendererFeature";
        private const string VHS_EFFECT = "VHSEffectPassRendererFeature";

        public GraphicsService(UniversalRendererData rendererData) {
            _rendererData = rendererData;
            ServiceLocator.Register<IGraphicsService>(this);
        }

        public void SetBarrelDistortion(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == BARREL_DISTORTION).SetActive(isEnabled);
        }

        public void SetDithering(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == DITHER).SetActive(isEnabled);
        }

        public void SetPixelation(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == PIXELATE).SetActive(isEnabled);
        }

        public void SetRollingBands(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == ROLLING_BANDS).SetActive(isEnabled);
        }

        public void SetScanlines(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == SCANLINES).SetActive(isEnabled);
        }

        public void SetVHSEffect(bool isEnabled) {
            _rendererData.rendererFeatures.Find(feature => feature.name == VHS_EFFECT).SetActive(isEnabled);
        }
    }
}