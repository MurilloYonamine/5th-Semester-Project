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
        protected int _progress;
        protected bool _isComplete;

        public int Progress => _progress;
        public bool IsComplete => _isComplete;
        public string MissionId => _definition != null ? _definition.MissionId : "";

        public event Action OnMissionComplete;

        public virtual void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            _definition = definition;
            _eventBus = eventBus;
            _saveService = saveService;
            _progress = 0;
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
            _isComplete = true;
            SaveProgress();
            PublishProgress();
            OnMissionComplete?.Invoke();
            Debug.Log($"[{GetType().Name}] Mission completed: {MissionId}");
        }

        public virtual void Cleanup() {
            gameObject.SetActive(false);
        }

        protected virtual void SaveProgress() {
            if (!_definition.PersistProgress || _saveService == null) return;

            SaveData saveData = _saveService.LoadFromSlot("default") ?? new SaveData();
            saveData.MissionProgress[MissionId] = _progress;
            _saveService.SaveToSlot("default", saveData);
        }

        protected virtual void LoadProgress() {
            if (_saveService == null) return;

            SaveData saveData = _saveService.LoadFromSlot("default");
            if (saveData != null && saveData.MissionProgress.TryGetValue(MissionId, out int savedProgress)) {
                _progress = savedProgress;
            }
        }

        protected void PublishProgress() {
            _eventBus?.Publish(new MissionProgressEvent(MissionId, _progress, _definition.RequiredCount));
        }

        protected void IncrementProgress() {
            _progress++;
            PublishProgress();

            if (_progress >= _definition.RequiredCount) {
                Complete();
            }
        }
    }
}
