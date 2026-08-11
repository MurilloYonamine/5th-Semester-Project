// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using System;
using UnityEngine;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay {
    public class PuzzleMission : MissionBase {
        [Header("Configurações do Puzzle")]
        [SerializeField] private string _puzzleId;
        [SerializeField] private int _totalPartsRequired;

        private int _currentPartsPlaced;

        public override void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService) {
            base.Initialize(definition, eventBus, saveService);

            _currentPartsPlaced = 0;
            UpdateProgressText();

            _eventBus.Subscribe<PuzzlePartPlacedEvent>(OnPuzzlePartPlaced);
        }

        private void OnPuzzlePartPlaced(PuzzlePartPlacedEvent evt) {
            if (string.IsNullOrEmpty(evt.PuzzleId) ||
                !string.Equals(evt.PuzzleId, _puzzleId, StringComparison.Ordinal)) {
                return;
            }

            _currentPartsPlaced++;
            UpdateProgressText();

            if (_currentPartsPlaced >= _totalPartsRequired) {
                Complete();
            }
        }

        private void UpdateProgressText() {
            _progress = $"{_currentPartsPlaced}/{_totalPartsRequired}";
            PublishProgress();
        }

        public override void Cleanup() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<PuzzlePartPlacedEvent>(OnPuzzlePartPlaced);
            }

            base.Cleanup();
        }

        private void OnDestroy() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<PuzzlePartPlacedEvent>(OnPuzzlePartPlaced);
            }
        }
    }
}
