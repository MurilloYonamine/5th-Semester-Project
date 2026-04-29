// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine.SceneManagement;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionPlayJumpscare : Node {
        private Blackboard _blackboard;
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
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>("NavAgent");
            if (_director == null) _director = _blackboard.GetData<PlayableDirector>("JumpscareDirector");
            if (_target == null) _target = _blackboard.GetData<Transform>("PlayerTarget");

            if (!_started) {
                float distanceToPlayer = Vector3.Distance(_agent.transform.position, _target.position);

                if (distanceToPlayer <= _agent.stoppingDistance + 0.5f) {

                    if (_agent.isOnNavMesh) {
                        _agent.isStopped = true;
                        _agent.ResetPath();
                    }

                    _director.stopped += OnDirectorStopped;

                    _gameStateService.ChangeState(GameState.Cutscene);
                    _director.Play();
                    _started = true;
                    return Status.Running;
                }

                return Status.Running;
            }

            if (_finished) {
                _director.stopped -= OnDirectorStopped;
                _gameStateService.ChangeState(GameState.Gameplay);
                return Status.Success;
            }

            return Status.Running;
        }

        private void OnDirectorStopped(PlayableDirector pd) {
            _finished = true;
            SceneManager.LoadScene("MainMenu");
        }

        public override void Reset() {
            base.Reset();
            if (_director != null) _director.stopped -= OnDirectorStopped;
            _started = false;
            _finished = false;
        }
    }
}

