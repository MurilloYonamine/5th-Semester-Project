using FifthSemester.Framework.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Gameplay.Enemy {
    public class ConditionPlayerTooFar : Node {
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string NAV_AGENT_KEY = "NavAgent";
        
        private readonly Blackboard _blackboard;
        private readonly float _distanceThreshold;
        private Transform _target;
        private NavMeshAgent _agent;

        public ConditionPlayerTooFar(Blackboard blackboard, string name = "Player Too Far", float distanceThreshold = 25f) : base(name, blackboard) {
            _blackboard = blackboard;
            _distanceThreshold = distanceThreshold;
        }

        public override Status Process() {
            if (_target == null) _target = _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);

            if (_target == null || _agent == null) {
                return Status.Failure;
            }

            float distance = Vector3.Distance(_agent.transform.position, _target.position);
            if (distance >= _distanceThreshold) {
                return Status.Success;
            }

            return Status.Failure;
        }
    }
}
