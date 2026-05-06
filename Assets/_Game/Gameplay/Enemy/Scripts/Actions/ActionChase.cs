// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionChase : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;

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

            EnsureAgentIsMoving();
            _agent.SetDestination(_target.position);

            return HasReachedTarget() ? Status.Success : Status.Running;
        }

        private void CacheReferences() {
            _agent ??= _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _target ??= _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
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

