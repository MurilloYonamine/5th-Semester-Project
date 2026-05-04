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
            // Cache dos dados do Blackboard
            _agent ??= _blackboard.GetData<NavMeshAgent>("NavAgent");
            _target ??= _blackboard.GetData<Transform>("PlayerTarget");

            if (_agent == null || _target == null) return Status.Failure;

            // Bloqueio se estiver sob efeito da lanterna
            if (_blackboard.GetData<bool>("IsStunnedByFlashlight")) {
                return Status.Failure;
            }

            // Garante que o agente está se movendo
            if (_agent.isStopped) _agent.isStopped = false;

            _agent.SetDestination(_target.position);

            // Checa se chegou no destino
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

