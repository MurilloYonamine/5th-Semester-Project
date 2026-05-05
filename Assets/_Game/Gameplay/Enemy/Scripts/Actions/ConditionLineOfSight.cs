// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ConditionLineOfSight : Node {
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string EYE_TRANSFORM_KEY = "EyeTransform";
        private const string VIEW_DISTANCE_KEY = "ViewDistance";
        private const string FOV_ANGLE_KEY = "FovAngle";
        private const string OBSTACLE_MASK_KEY = "ObstacleMask";

        private readonly Blackboard _blackboard;

        public ConditionLineOfSight(Blackboard blackboard, string name = "Line of Sight") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            Transform target = _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            Transform eyeTransform = _blackboard.GetData<Transform>(EYE_TRANSFORM_KEY);
            float viewDistance = _blackboard.GetData<float>(VIEW_DISTANCE_KEY);
            float fovAngle = _blackboard.GetData<float>(FOV_ANGLE_KEY);
            LayerMask obstacleMask = _blackboard.GetData<LayerMask>(OBSTACLE_MASK_KEY);

            if (target == null || eyeTransform == null) {
                return Status.Failure;
            }

            // Check distance
            Vector3 eyePos = eyeTransform.position;
            Vector3 dirToTarget = target.position - eyePos;
            float distance = dirToTarget.magnitude;

            if (distance > viewDistance) {
                return Status.Failure;
            }

            // Check angle
            Vector3 dirToTargetNormalized = dirToTarget.normalized;
            float angle = Vector3.Angle(eyeTransform.forward, dirToTargetNormalized);

            if (angle > fovAngle * 0.5f) {
                return Status.Failure;
            }

            // Check line of sight
            if (Physics.Raycast(eyePos, dirToTargetNormalized, distance, obstacleMask)) {
                return Status.Failure;
            }

            return Status.Success;
        }
    }
}
