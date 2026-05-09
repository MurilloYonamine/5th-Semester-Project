// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine.SceneManagement;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionPlayJumpscare : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string JUMPSCARE_DIRECTOR_KEY = "JumpscareDirector";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string MAIN_MENU_SCENE_NAME = "MainMenu";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private PlayableDirector _director;
        private Transform _target;

        private bool _started = false;
        private bool _finished = false;

        private IGameStateService _gameStateService;

        public ActionPlayJumpscare(Blackboard blackboard, string name = "PlayJumpscare") : base(name, blackboard) {
            _blackboard = blackboard;
            _gameStateService = ServiceLocator.Get<IGameStateService>();
        }

        public override Status Process() {
            CacheReferences();

            if (_agent == null || _director == null || _target == null) {
                return Status.Failure;
            }

            if (!_started) {
                if (CanStartJumpscare()) {
                    StartJumpscare();
                    return Status.Running;
                }

                return Status.Running;
            }

            if (_finished) {
                FinalizeJumpscare();
                return Status.Success;
            }

            return Status.Running;
        }

        private void CacheReferences() {
            _agent ??= _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _director ??= _blackboard.GetData<PlayableDirector>(JUMPSCARE_DIRECTOR_KEY);
            _target ??= _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
        }

        private bool CanStartJumpscare() {
            if (_agent.pathPending || !_agent.hasPath) {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance;
        }

        private void StartJumpscare() {
            if (_agent.isOnNavMesh) {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _director.stopped += OnDirectorStopped;
            _gameStateService?.ChangeState(GameState.Cutscene);
            _director.Play();
            _started = true;
        }

        private void FinalizeJumpscare() {
            _director.stopped -= OnDirectorStopped;
            _gameStateService?.ChangeState(GameState.Gameplay);
        }

        private void OnDirectorStopped(PlayableDirector pd) {
            if (_director != null && pd != _director) {
                return;
            }

            _finished = true;
            SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }

        public override void Reset() {
            base.Reset();
            if (_director != null) _director.stopped -= OnDirectorStopped;
            _started = false;
            _finished = false;
        }
    }
}

