using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionPatrol : Node {
        private NavMeshAgent _agent;
        private Transform[] _waypoints;
        private int _currentWaypointIndex = 0;
        private float _stoppingDistance;

        public ActionPatrol(NavMeshAgent agent, Transform[] waypoints, float stoppingDistance = 1.0f, string name = "Patrol") : base(name) {
            this._agent = agent;
            this._waypoints = waypoints;
            this._stoppingDistance = stoppingDistance;
        }

        public override Status Process() {
            // Prevenção de erros caso não existam pontos de patrulha configurados
            if (_waypoints == null || _waypoints.Length == 0) {
                return Status.Failure;
            }

            Transform target = _waypoints[_currentWaypointIndex];

            // Garante que o agente está livre para andar
            _agent.isStopped = false;
            _agent.SetDestination(target.position);
            _agent.transform.LookAt(target);

            // Verifica se o agente chegou perto o suficiente do waypoint atual
            if (!_agent.pathPending && _agent.remainingDistance <= _stoppingDistance) {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
            }

            // Retorna Running para que a árvore continue executando a patrulha no próximo frame
            return Status.Running;
        }
    }
}
