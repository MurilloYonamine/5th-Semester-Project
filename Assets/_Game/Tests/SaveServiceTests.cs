using NUnit.Framework;
using FifthSemester.Gameplay.Save;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Tests
{
    public class SaveServiceTests
    {
        private SaveService _saveService;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
            _saveService = new SaveService();
        }

        [Test]
        public void SaveToSlot_CreatesPlayerPref()
        {
            var data = new TestSaveData();
            _saveService.SaveToSlot("test", data);

            Assert.IsTrue(PlayerPrefs.HasKey("save_test"));
        }

        [Test]
        public void LoadFromSlot_ReturnsSavedData()
        {
            var data = new TestSaveData { LastCheckpointId = "cp1" };
            _saveService.SaveToSlot("slot1", data);

            var loaded = _saveService.LoadFromSlot("slot1");

            Assert.IsNotNull(loaded);
            Assert.AreEqual("cp1", loaded.LastCheckpointId);
        }

        [Test]
        public void DeleteSlot_RemovesPlayerPref()
        {
            var data = new TestSaveData();
            _saveService.SaveToSlot("todelete", data);
            Assert.IsTrue(PlayerPrefs.HasKey("save_todelete"));

            _saveService.DeleteSlot("todelete");
            Assert.IsFalse(PlayerPrefs.HasKey("save_todelete"));
        }

        private class TestSaveData : SaveData { }
    }
}
