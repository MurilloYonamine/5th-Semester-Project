// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionPatrol : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PATROL_WAYPOINTS_KEY = "PatrolWaypoints";
        private const string PATROL_WAIT_TIME_KEY = "PatrolWaitTime";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform[] _waypoints;

        private int _currentWaypoint = 0;
        private bool _isWaiting = false;
        private ActionWait _waitNode;
        private float _waitTime = 1f;

        public ActionPatrol(Blackboard blackboard, string name = "Patrol") : base(name, blackboard) {
            _blackboard = blackboard;
            _waitNode = new ActionWait(0f);
        }

        public override Status Process() {
            RefreshDataFromBlackboard();

            if (_blackboard.GetData<bool>(IS_STUNNED_KEY)) {
                return Status.Failure;
            }

            if (_agent == null || _waypoints == null || _waypoints.Length == 0) {
                return Status.Failure;
            }

            if (!_isWaiting) {
                MoveToCurrentWaypoint();

                if (_agent.pathPending) {
                    return Status.Running;
                }

                BeginWaitIfReachedWaypoint();
                return Status.Running;
            }

            ProcessWait();
            return Status.Running;
        }

        private void RefreshDataFromBlackboard() {
            _agent = _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _waypoints = _blackboard.GetData<Transform[]>(PATROL_WAYPOINTS_KEY);
            _waitTime = _blackboard.GetData<float>(PATROL_WAIT_TIME_KEY);
        }

        private void MoveToCurrentWaypoint() {
            _agent.SetDestination(_waypoints[_currentWaypoint].position);
        }

        private void BeginWaitIfReachedWaypoint() {
            if (_agent.remainingDistance > _agent.stoppingDistance) {
                return;
            }

            _isWaiting = true;
            _waitNode = new ActionWait(_waitTime);
        }

        private void ProcessWait() {
            if (_waitNode.Process() != Status.Success) {
                return;
            }

            _isWaiting = false;
            _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
        }

        public override void Reset() {
            base.Reset();
            _currentWaypoint = 0;
            _isWaiting = false;
            _waitNode?.Reset();
        }
    }
}
