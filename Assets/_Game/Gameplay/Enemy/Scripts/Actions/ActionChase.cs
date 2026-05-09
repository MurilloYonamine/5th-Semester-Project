// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionChase : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";
        private const string HAS_LINE_OF_SIGHT_KEY = "HasLineOfSight";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;

        private float _glitchStartDistance = 15f;
        private float _loseTargetDistance = 25f;

        private IWhiteNoiseService _whiteNoiseService;

        public ActionChase(Blackboard blackboard, string name = "Chase") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            CacheReferences();

            if (_agent == null || _target == null) {
                return Status.Failure;
            }

            if (_blackboard.GetData<bool>(IS_STUNNED_KEY)) {
                return Status.Failure;
            }

            float distance = Vector3.Distance(
                _agent.transform.position,
                _target.position
            );

            if (distance >= _loseTargetDistance) {
                _agent.ResetPath();
                return Status.Failure;
            }

            EnsureAgentIsMoving();
            _agent.SetDestination(_target.position);

            UpdateGlitchProximityEffect();

            if (HasReachedTarget()) {
                return Status.Success;
            }

            return Status.Running;
        }

        private void CacheReferences() {
            _agent ??= _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _target ??= _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);

            _whiteNoiseService ??= ServiceLocator.Get<IWhiteNoiseService>();
        }

        private void UpdateGlitchProximityEffect() {
            bool hasLineOfSight = _blackboard.GetData<bool>(HAS_LINE_OF_SIGHT_KEY);

            if (!hasLineOfSight) {
                return;
            }

            float currentDistance = Vector3.Distance(
                _agent.transform.position,
                _target.position
            );

            if (currentDistance <= _glitchStartDistance) {
                float intensity = 1f - (currentDistance / _glitchStartDistance);

                _whiteNoiseService?.RequestIntensity(intensity);
            }
        }
        private void EnsureAgentIsMoving() {
            if (_agent.isStopped) {
                _agent.isStopped = false;
            }
        }

        private bool HasReachedTarget() {
            if (_agent.pathPending || !_agent.hasPath) {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        public override void Reset() {
            base.Reset();
        }
    }
}