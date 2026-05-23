// Autor: Murillo Gomes Yonamine
// Data: 18/05/2026


using NUnit.Framework;
using UnityEngine;
using FifthSemester.Features.Localization;
using FifthSemester.Core.Services;
using FifthSemester.Core.Enums;

namespace FifthSemester.Tests
{
    public class LocalizationTests
    {
        private LocalizationService _localizationService;

        [SetUp]
        public void Setup()
        {
            GameObject go = new GameObject();
            _localizationService = go.AddComponent<LocalizationService>();

            ServiceLocator.Register<ILocalizationService>(_localizationService);
        }

        [TearDown]
        public void Teardown()
        {
            ServiceLocator.Unregister<ILocalizationService>();
            Object.DestroyImmediate(_localizationService.gameObject);
        }

        [Test]
        public void GetText_WithMissingKey_ReturnsFormattedKey()
        {
            string result = _localizationService.GetText("chave_inexistente");

            Assert.AreEqual("[chave_inexistente]", result);
        }

        [Test]
        public void SetLanguage_ChangesCurrentLanguage()
        {
            Assert.DoesNotThrow(() => _localizationService.SetLanguage(Language.English));
        }
    }
}
