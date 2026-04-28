using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionStop : Node {
        private NavMeshAgent _agent;

        public ActionStop(NavMeshAgent agent, string name = "Stop Movement") : base(name) {
            this._agent = agent;
        }

        public override Status Process() {
            if (_agent != null && _agent.isOnNavMesh) {
                _agent.isStopped = true; 
                _agent.ResetPath();    
                return Status.Success; 
            }
            return Status.Failure;
        }
    }
}