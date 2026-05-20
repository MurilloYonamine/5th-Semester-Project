// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public abstract class MissionBase : MonoBehaviour, IMission {
        protected MissionDefinition _definition;
        protected IEventBus _eventBus;
        protected ISaveService _saveService;
        protected string _progress;
        protected bool _isComplete;

        public bool IsComplete => _isComplete;
        public string Progress => _progress;
        public string MissionId => _definition != null ? _definition.MissionId : "";

        public event Action OnMissionComplete;

        public virtual void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            _definition = definition;
            _eventBus = eventBus;
            _saveService = saveService;
            _progress = string.Empty;
            _isComplete = false;

            if (_definition.PersistProgress) {
                LoadProgress();
            }
        }

        public virtual void StartMission() {
            if (_isComplete) return;
            PublishProgress();
        }

        public virtual void Complete() {
            if (_isComplete) return;
            _eventBus?.Publish(new MissionCompletedEvent { MissionId = _definition.MissionId });
            _isComplete = true;
            SaveProgress();
            PublishProgress();
            OnMissionComplete?.Invoke();
            Cleanup();
            Debug.Log($"[{GetType().Name}] Mission completed: {MissionId}");
        }

        public virtual void Cleanup() {
            if (_isComplete) return;

            OnMissionComplete = null;

            if (this == null || gameObject == null) return;

            gameObject.SetActive(false);
        }

        protected virtual void SaveProgress() {
            if (_definition == null || !_definition.PersistProgress || _saveService == null) return;

            SaveData saveData = _saveService.LoadFromSlot("default") ?? new SaveData();
            saveData.MissionProgress[MissionId] = _progress ?? string.Empty;
            _saveService.SaveToSlot("default", saveData);
        }

        protected virtual void LoadProgress() {
            if (_saveService == null) return;

            SaveData saveData = _saveService.LoadFromSlot("default");
            if (saveData != null && saveData.MissionProgress.TryGetValue(MissionId, out string savedProgress)) {
                _progress = savedProgress;
            }
        }

        protected void PublishProgress() {
            _eventBus?.Publish(new MissionProgressEvent(MissionId, _progress));
        }

        protected void IncrementProgress() {
            int progressCount = GetProgressCount();
            progressCount++;
            _progress = progressCount.ToString();
            PublishProgress();

            if (_definition != null && progressCount >= _definition.RequiredCount) {
                Complete();
            }
        }

        protected int GetProgressCount() {
            if (string.IsNullOrWhiteSpace(_progress)) return 0;
            if (int.TryParse(_progress, out int progressCount)) return progressCount;
            return 0;
        }
    }
}
