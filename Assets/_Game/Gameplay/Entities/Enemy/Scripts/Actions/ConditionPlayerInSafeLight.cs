// Autor: Murillo Gomes Yonamine
// Data: 11/05/2026

using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay {
    public class ConditionPlayerInSafeLight : Node {
        private const string PLAYER_IN_LIGHT_KEY = "IsPlayerInSafeLight";
        private readonly Blackboard _blackboard;

        public ConditionPlayerInSafeLight(Blackboard blackboard, string name = "Player In Safe Light") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            bool inLight = _blackboard.HasKey(PLAYER_IN_LIGHT_KEY) && _blackboard.GetData<bool>(PLAYER_IN_LIGHT_KEY);
            return inLight ? Status.Success : Status.Failure;
        }
    }
}
