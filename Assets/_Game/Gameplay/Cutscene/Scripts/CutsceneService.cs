// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using System.Collections.Generic;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Enums;
using Unity.Cinemachine;

namespace FifthSemester.Gameplay.Dialogue {
    public class CutsceneService : MonoBehaviour, ICutsceneService {
        [SerializeField] private List<CutsceneController> _cutscenesInMap;
        private Dictionary<CutsceneType, CutsceneController> _cutsceneDictionary;

        private CutsceneController _activeCutscene;
        [SerializeField] private CinemachineCamera _playerCamera;

        private void Awake() {
            ServiceLocator.Register<ICutsceneService>(this);
            _cutsceneDictionary = new Dictionary<CutsceneType, CutsceneController>();

            foreach (var cutscene in _cutscenesInMap) {
                if (cutscene != null && cutscene.CutsceneID != CutsceneType.None) {
                    if (!_cutsceneDictionary.ContainsKey(cutscene.CutsceneID)) {
                        _cutsceneDictionary.Add(cutscene.CutsceneID, cutscene);
                    }
                }
            }
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<ICutsceneService>();
        }

        public void PlayCutscene(CutsceneType type) {
            if (_cutsceneDictionary.TryGetValue(type, out var cutscene)) {
                _activeCutscene = cutscene;
                cutscene.SetPlayerCamera(_playerCamera);
                cutscene.PlayCutscene();
            }
            else {
                Debug.LogError($"Cutscene {type} não encontrada na cena atual!");
            }
        }

        public void SkipActiveCutscene() {
            if (_activeCutscene != null) {
                _activeCutscene.SkipCutscene();
            }
        }
    }
}
