using UnityEngine;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2GasolineItem : Item {
        [Header("Timeline de Transição")]
        [SerializeField] private UnityEngine.Playables.PlayableDirector _transitionTimeline;

        public override void Interact() {
            base.Interact();

            if (_transitionTimeline != null) {
                PlayTransitionTimeline();
            }
        }

        private void PlayTransitionTimeline() {
            if (ServiceLocator.TryGet<IGameStateService>(out var gameStateService)) {
                gameStateService.ChangeState(GameState.Cutscene);
            }

            _transitionTimeline.Play();
            _transitionTimeline.stopped += OnTransitionTimelineStopped;
        }

        private void OnTransitionTimelineStopped(UnityEngine.Playables.PlayableDirector director) {
            if (director == _transitionTimeline) {
                _transitionTimeline.stopped -= OnTransitionTimelineStopped;

                if (ServiceLocator.TryGet<IGameStateService>(out var gameStateService)) {
                    gameStateService.ChangeState(GameState.Gameplay);
                }
            }
        }
    }
}
