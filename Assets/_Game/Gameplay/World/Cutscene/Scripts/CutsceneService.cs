// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace FifthSemester.Gameplay {

    public class CutsceneService : MonoBehaviour, ICutsceneService {
        private const float SKIP_FADE_DURATION = 1f;

        [SerializeField]
        private List<CutsceneController> _cutscenesInMap;

        private Dictionary<CutsceneType, CutsceneController> _cutsceneDictionary;

        private CutsceneController _activeCutscene;

        [SerializeField]
        private CinemachineCamera _playerCamera;

        private IEventBus _eventBus;
        private IFadeService _fadeService;

        private void Awake() {

            ServiceLocator.Register<ICutsceneService>(this);

            _cutsceneDictionary =
                new Dictionary<CutsceneType, CutsceneController>();

            foreach (var cutscene in _cutscenesInMap) {

                if (cutscene == null)
                    continue;

                if (cutscene.CutsceneID == CutsceneType.None)
                    continue;

                if (!_cutsceneDictionary.ContainsKey(cutscene.CutsceneID)) {

                    _cutsceneDictionary.Add(
                        cutscene.CutsceneID,
                        cutscene
                    );
                }
            }
        }

        private void Start() {

            _eventBus = ServiceLocator.Get<IEventBus>();
            ServiceLocator.TryGet<IFadeService>(out _fadeService);

            _eventBus?.Subscribe<SkipCutsceneRequestedEvent>(
                OnSkipRequested
            );
        }

        private void OnDestroy() {

            ServiceLocator.Unregister<ICutsceneService>();

            _eventBus?.Unsubscribe<SkipCutsceneRequestedEvent>(
                OnSkipRequested
            );
        }

        private void OnSkipRequested(SkipCutsceneRequestedEvent evt) {

            SkipActiveCutscene();
        }

        public void PlayCutscene(CutsceneType type) {

            if (_cutsceneDictionary.TryGetValue(type, out var cutscene)) {

                _activeCutscene = cutscene;

                cutscene.SetPlayerCamera(_playerCamera);

                cutscene.PlayCutscene();
            }
            else {

                Debug.LogError(
                    $"Cutscene {type} não encontrada na cena atual!"
                );
            }
        }

        public void SkipActiveCutscene() {

            if (_activeCutscene == null)
                return;

            if (!_activeCutscene.IsPlaying)
                return;

            if (_fadeService == null) {
                ServiceLocator.TryGet<IFadeService>(out _fadeService);
            }

            if (_fadeService == null) {
                _activeCutscene.SkipCutscene();
                _activeCutscene = null;
                return;
            }

            _fadeService.FadeOut(SKIP_FADE_DURATION, () => {
                _activeCutscene.SkipCutscene();

                var dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
                dialogueService?.ForceEndDialogueImmediate();

                _fadeService.FadeIn(SKIP_FADE_DURATION);
                _activeCutscene = null;
            });
        }
    }
}
