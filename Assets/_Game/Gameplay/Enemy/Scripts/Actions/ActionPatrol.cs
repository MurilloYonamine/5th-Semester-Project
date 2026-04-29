// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionPatrol : Node {
        private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform[] _waypoints;
        private Animator _animator;

        private int _currentWaypoint = 0;
        private bool _isWaiting = false;
        private ActionWait _waitNode;
        private float _waitTime = 1f;

        public ActionPatrol(Blackboard blackboard, string name = "Patrol") : base(name, blackboard) {
            this._blackboard = blackboard;
            _waitNode = new ActionWait(0f);
        }

        public override Status Process() {
            _agent = _blackboard.GetData<NavMeshAgent>("NavAgent");
            _waypoints = _blackboard.GetData<Transform[]>("PatrolWaypoints");
            _animator = _blackboard.GetData<Animator>("Animator");
            _waitTime = _blackboard.GetData<float>("PatrolWaitTime");

            if (_blackboard != null && _blackboard.HasKey("IsStunnedByFlashlight") && _blackboard.GetData<bool>("IsStunnedByFlashlight")) {
                return Status.Failure;
            }

            if ( _waypoints.Length == 0) {
                return Status.Failure;
            }

            Transform target = _waypoints[_currentWaypoint];

            if (!_isWaiting) {
                _agent.SetDestination(target.position);

                if (_agent.pathPending) return Status.Running;

                if (_agent.remainingDistance <= _agent.stoppingDistance) {
                    _isWaiting = true;
                    _waitNode = new ActionWait(_waitTime);
                }

                return Status.Running;
            } else {
                Status waitStatus = _waitNode.Process();
                if (waitStatus == Status.Success) {
                    _isWaiting = false;
                    _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
                }

                return Status.Running;
            }
        }

        public override void Reset() {
            base.Reset();
            _currentWaypoint = 0;
            _isWaiting = false;
            _waitNode?.Reset();
        }
    }
}
