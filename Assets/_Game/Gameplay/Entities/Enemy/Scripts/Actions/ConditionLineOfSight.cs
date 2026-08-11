// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay {
    public class ConditionLineOfSight : Node {
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string EYE_TRANSFORM_KEY = "EyeTransform";
        private const string VIEW_DISTANCE_KEY = "ViewDistance";
        private const string FOV_ANGLE_KEY = "FovAngle";
        private const string OBSTACLE_MASK_KEY = "ObstacleMask";
        private const string LOSE_TARGET_DISTANCE_KEY = "LoseTargetDistance";
        private const string HAS_LINE_OF_SIGHT_KEY = "HasLineOfSight";

        private readonly Blackboard _blackboard;

        public ConditionLineOfSight(Blackboard blackboard, string name = "Line of Sight") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            Transform target = _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            Transform eyeTransform = _blackboard.GetData<Transform>(EYE_TRANSFORM_KEY);
            float viewDistance = _blackboard.GetData<float>(VIEW_DISTANCE_KEY);
            float loseDistance = _blackboard.GetData<float>(LOSE_TARGET_DISTANCE_KEY);
            float fovAngle = _blackboard.GetData<float>(FOV_ANGLE_KEY);
            LayerMask obstacleMask = _blackboard.GetData<LayerMask>(OBSTACLE_MASK_KEY);

            if (target == null || eyeTransform == null) {
                _blackboard.SetData(HAS_LINE_OF_SIGHT_KEY, false);
                return Status.Failure;
            }

            Vector3 eyePos = eyeTransform.position;
            Vector3 dirToTarget = target.position - eyePos;
            float distance = dirToTarget.magnitude;

            if (distance > viewDistance) {
                _blackboard.SetData(HAS_LINE_OF_SIGHT_KEY, false);
                return Status.Failure;
            }

            Vector3 dirToTargetNormalized = dirToTarget.normalized;
            float angle = Vector3.Angle(eyeTransform.forward, dirToTargetNormalized);

            if (angle > fovAngle * 0.5f) {
                _blackboard.SetData(HAS_LINE_OF_SIGHT_KEY, false);
                return Status.Failure;
            }

            if (Physics.Raycast(eyePos, dirToTargetNormalized, distance, obstacleMask)) {
                _blackboard.SetData(HAS_LINE_OF_SIGHT_KEY, false);
                return Status.Failure;
            }

            _blackboard.SetData(HAS_LINE_OF_SIGHT_KEY, true);
            return Status.Success;
        }
    }
}
