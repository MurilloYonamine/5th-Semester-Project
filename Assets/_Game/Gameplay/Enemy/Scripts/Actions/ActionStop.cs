using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionStop : Node {
        private readonly NavMeshAgent _agent;

        public ActionStop(NavMeshAgent agent, string name = "Stop Movement") : base(name) {
            _agent = agent;
        }

        public override Status Process() {
            if (_agent == null || !_agent.isOnNavMesh) {
                return Status.Failure;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            return Status.Success;
        }
    }
}