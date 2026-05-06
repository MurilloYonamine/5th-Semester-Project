// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class LightSeeker : MonoBehaviour {
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PATROL_WAYPOINTS_KEY = "PatrolWaypoints";
        private const string ANIMATOR_KEY = "Animator";
        private const string PATROL_WAIT_TIME_KEY = "PatrolWaitTime";
        private const string JUMPSCARE_DIRECTOR_KEY = "JumpscareDirector";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";

        [SerializeField] private Transform _target;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] _patrolWaypoints;

        [SerializeField] private float _patrolWaitTime = 2f;

        [Header("Vision Settings")]
        [SerializeField] private Transform _eyeTransform;

        [SerializeField, Range(0, 120)] private float _viewDistance = 15f;
        [SerializeField, Range(0, 360)] private float _fovAngle = 90f;
        [SerializeField] private LayerMask _obstacleMask;

        private BehaviourTree _tree;
        private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Animator _animator;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector _jumpscareDirector;

        [Header("Speed Settings")]
        [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;
        [SerializeField, Range(0f, 10f)] private float _sprint = 3f;

        private readonly int _speedHash = Animator.StringToHash("Speed");

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
            _agent.speed = _speed;

            SetupBlackboard();
        }

        private void SetupBlackboard() {
            _blackboard = new Blackboard();
            _blackboard.SetData(PLAYER_TARGET_KEY, _target);
            _blackboard.SetData(NAV_AGENT_KEY, _agent);
            _blackboard.SetData(PATROL_WAYPOINTS_KEY, _patrolWaypoints);
            _blackboard.SetData(ANIMATOR_KEY, _animator);
            _blackboard.SetData(PATROL_WAIT_TIME_KEY, _patrolWaitTime);
            _blackboard.SetData(JUMPSCARE_DIRECTOR_KEY, _jumpscareDirector);
            _blackboard.SetData(IS_STUNNED_KEY, false);
            
            // Vision parameters
            _blackboard.SetData("EyeTransform", _eyeTransform);
            _blackboard.SetData("ViewDistance", _viewDistance);
            _blackboard.SetData("FovAngle", _fovAngle);
            _blackboard.SetData("ObstacleMask", _obstacleMask);
        }

        private void Start() {
            BuildBehaviourTree();
        }

        private void BuildBehaviourTree() {
            // Priority 1: React to Light (Conditional Abort Pattern)
            // If illuminated, immediately stop all actions and freeze
            var reactionToLightSequence = new Sequence("ReactionToLight");
            reactionToLightSequence.AddChild(new ConditionIsIlluminated(_blackboard, "Illuminated Check"));
            reactionToLightSequence.AddChild(new ActionStop(_agent, "Freeze Movement"));

            // Priority 2: Aggressive Chase
            // If player is in line of sight, chase and attempt jumpscare
            var chaseSequence = new Sequence("AggressiveChase");
            chaseSequence.AddChild(new ConditionLineOfSight(_blackboard, "Line of Sight Check"));
            chaseSequence.AddChild(new ActionChase(_blackboard, "Chase Player"));
            chaseSequence.AddChild(new ActionPlayJumpscare(_blackboard, "Jumpscare"));

            // Priority 3: Fallback Patrol
            // If not illuminated and no line of sight, search by patrolling waypoints
            var patrolSequence = new Sequence("SearchPatrol");
            patrolSequence.AddChild(new ActionPatrol(_blackboard, "Patrol Waypoints"));

            // Root: Selector evaluates priorities left to right
            var root = new Selector("RootBehavior");
            root.AddChild(reactionToLightSequence);
            root.AddChild(chaseSequence);
            root.AddChild(patrolSequence);

            _tree = new BehaviourTree("LightSeeker Behaviour Tree", root);
        }

        private void Update() {
            _tree?.Process();

            if (_animator != null && _agent != null) {
                _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
            }
        }

        private void OnEnable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<PlayerSprintChangedEvent>(HandleSprint);
            eventBus?.Subscribe<FlashlightTargetedEvent>(HandleFlashlightTargeted);
        }

        private void OnDisable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Unsubscribe<PlayerSprintChangedEvent>(HandleSprint);
            eventBus?.Unsubscribe<FlashlightTargetedEvent>(HandleFlashlightTargeted);
        }

        private void HandleSprint(PlayerSprintChangedEvent evt) {
            if (_agent != null) {
                _agent.speed = evt.IsSprinting ? _sprint : _speed;
            }
        }

        private void HandleFlashlightTargeted(FlashlightTargetedEvent evt) {
            if (_agent == null || evt.Target != this.gameObject) {
                return;
            }

            if (evt.IsIlluminated) {
                ApplyStun();
                return;
            }

            RemoveStun();
        }

        private void ApplyStun() {
            if (_agent.isOnNavMesh) {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.ResetPath();
            }

            _blackboard?.SetData(IS_STUNNED_KEY, true);
        }

        private void RemoveStun() {
            if (_agent.isOnNavMesh) {
                _agent.isStopped = false;
            }

            _blackboard?.SetData(IS_STUNNED_KEY, false);
        }

        private bool IsPlayerInFOV() {
            if (_target == null || _eyeTransform == null) {
                return false;
            }

            Vector3 eyePos = _eyeTransform.position;
            Vector3 dirToTarget = _target.position - eyePos;
            float dist = dirToTarget.magnitude;

            if (dist > _viewDistance) {
                return false;
            }

            Vector3 dirToTargetNormalized = dirToTarget.normalized;
            float angle = Vector3.Angle(_eyeTransform.forward, dirToTargetNormalized);

            if (angle > _fovAngle * 0.5f) {
                return false;
            }

            if (Physics.Raycast(eyePos, dirToTargetNormalized, dist, _obstacleMask)) {
                return false;
            }

            return true;
        }

        private void OnDrawGizmosSelected() {
            DrawFOVGizmos();
        }

        private void OnDrawGizmos() {
            DrawFOVGizmos();
        }

        private void DrawFOVGizmos() {
            Transform eye = _eyeTransform != null ? _eyeTransform : transform;
            Vector3 pos = eye.position;

            bool inFov = IsPlayerInFOV();
            Gizmos.color = inFov ? Color.red : Color.yellow;

            Vector3 forward = eye.forward;

            int steps = 10;
            float halfFov = _fovAngle * 0.5f;

            Vector3 prevPoint = Vector3.zero;

            for (int i = 0; i <= steps; i++) {
                float t = i / (float)steps;
                float angle = Mathf.Lerp(-halfFov, halfFov, t);

                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * forward;
                Vector3 point = pos + dir * _viewDistance;

                Gizmos.DrawLine(pos, point);

                if (i > 0) {
                    Gizmos.DrawLine(prevPoint, point);
                }

                prevPoint = point;
            }

            if (_target != null) {
                Gizmos.color = inFov ? Color.red : Color.green;
                Gizmos.DrawLine(pos, _target.position);
            }
        }
    }
}
