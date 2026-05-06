// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ConditionIsIlluminated : Node {
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";

        private readonly Blackboard _blackboard;

        public ConditionIsIlluminated(Blackboard blackboard, string name = "Is Illuminated") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            bool isIlluminated = _blackboard.GetData<bool>(IS_STUNNED_KEY);
            return isIlluminated ? Status.Success : Status.Failure;
        }
    }
}
