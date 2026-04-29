// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionChase : Node {
        private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;
        private Animator _animator;


        public ActionChase(Blackboard blackboard, string name = "Chase") : base(name, blackboard) {
            _blackboard = blackboard;
        }
        public override Status Process() {
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>("NavAgent");
            if (_animator == null) _animator = _blackboard.GetData<Animator>("Animator");
            if (_target == null) _target = _blackboard.GetData<Transform>("PlayerTarget");

            if (_agent == null || _target == null) return Status.Failure;

            if (_blackboard != null && _blackboard.HasKey("IsStunnedByFlashlight") && _blackboard.GetData<bool>("IsStunnedByFlashlight")) {
                return Status.Failure;
            }

            if (_agent.isStopped) {
                _agent.isStopped = false;
            }

            _agent.SetDestination(_target.position);

            if (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f) {
                return Status.Success;
            }

            return Status.Running;
        }

        public override void Reset() {
            base.Reset();
        }
    }
}

