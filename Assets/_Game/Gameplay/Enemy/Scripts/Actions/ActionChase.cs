using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionChase : Node {
        private NavMeshAgent _agent;
        private float _stoppingDistance;

        public ActionChase(NavMeshAgent agent, float stoppingDistance = 1.5f, string name = "Chase Player") : base(name) {
            this._agent = agent;
            this._stoppingDistance = stoppingDistance;
        }

        public override Status Process() {
            // Recupera a referência do alvo no Blackboard!
            Transform target = Blackboard.GetData<Transform>("PlayerTarget");

            if (target == null) {
                return Status.Failure; // Se por algum motivo não achar o alvo, a ação falha
            }

            // Manda o NavMeshAgent ir até o jogador
            _agent.isStopped = false;
            _agent.SetDestination(target.position);

            // Verifica se o inimigo já chegou perto o suficiente
            if (!_agent.pathPending && _agent.remainingDistance <= _stoppingDistance) {
                return Status.Success; // Chegou no alvo!
            }

            // Se ainda não chegou, continua rodando no próximo frame
            return Status.Running;
        }
    }
}
