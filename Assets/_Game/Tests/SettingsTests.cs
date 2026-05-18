// Autor: Murillo Gomes Yonamine
// Data: 18/05/2026

using NUnit.Framework;
using UnityEngine;
using FifthSemester.Gameplay.Menu;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;

namespace FifthSemester.Tests {
    public class SettingsTests {
        private SettingsService _settingsService;

        [SetUp]
        public void Setup() {
            PlayerPrefs.DeleteAll();
            _settingsService = new SettingsService();
        }

        [Test]
        public void MasterVolume_SetAndGet_ReturnsCorrectValue() {
            float testValue = 75f;

            _settingsService.MasterVolume = testValue;

            Assert.AreEqual(0.75f, _settingsService.MasterVolume, 0.01f);
        }

        [Test]
        public void Language_Change_UpdatesPlayerPrefs() {
            _settingsService.Language = Language.English;

            int savedValue = PlayerPrefs.GetInt("Settings_Language");
            Assert.AreEqual((int)Language.English, savedValue);
        }

        [Test]
        public void InvertYAxis_Toggle_PersistsCorrectly() {
            _settingsService.InvertYAxis = true;

            Assert.IsTrue(_settingsService.InvertYAxis);
            Assert.AreEqual(1, PlayerPrefs.GetInt("Settings_InvertY"));
        }
    }
}
