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

        private void EnsureInitialized() {
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>("NavAgent");
            if (_animator == null) _animator = _blackboard.GetData<Animator>("Animator");
            if (_target == null) _target = _blackboard.GetData<Transform>("PlayerTarget");
        }

        public override Status Process() {
            EnsureInitialized();

            if (_agent == null || _target == null) return Status.Failure;

            // animator transitions are driven by Speed in LightSeeker.Update

            _agent.SetDestination(_target.position);

            return Status.Running;
        }

        public override void Reset() {
            base.Reset();
        }
    }
}

