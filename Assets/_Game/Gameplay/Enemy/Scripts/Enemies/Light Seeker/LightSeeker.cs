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
        }

        private void Start() {
            BuildBehaviourTree();
        }

        private void BuildBehaviourTree() {
            var isStunned = new Abort(() => _blackboard.GetData<bool>(IS_STUNNED_KEY), "StunCheck");
            isStunned.AddChild(new ActionStop(_agent));

            var patrolBranch = new Abort(() => IsPlayerInFOV(), "FOVAbort");
            patrolBranch.AddChild(new ActionPatrol(_blackboard));

            var chaseSequence = new Sequence("ChaseSequence");
            chaseSequence.AddChild(new ActionChase(_blackboard));
            chaseSequence.AddChild(new ActionPlayJumpscare(_blackboard));

            var root = new Selector("RootSelector");
            root.AddChild(isStunned);
            root.AddChild(patrolBranch);
            root.AddChild(chaseSequence);

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
