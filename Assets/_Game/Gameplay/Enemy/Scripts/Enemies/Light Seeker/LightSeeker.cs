// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using UnityEngine;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine.AI;


namespace FifthSemester.Gameplay.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class LightSeeker : MonoBehaviour {
        [SerializeField] private Transform _target;


        [Header("Patrol Settings")] [SerializeField]
        private Transform[] _patrolWaypoints;

        [SerializeField] private float _patrolWaitTime = 2f;

        private float PatrolStoppingDistance => _agent.stoppingDistance;

        [Header("Vision Settings")] [SerializeField]
        private Transform _eyeTransform;

        [SerializeField, Range(0, 120)] private float _viewDistance = 15f;
        [SerializeField, Range(0, 360)] private float _fovAngle = 90f;
        [SerializeField] private LayerMask _obstacleMask;

        private BehaviourTree _tree;
        private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Animator _animator;

        [Header("Speed Settings")] [SerializeField, Range(0f, 10f)]
        private float _speed = 1.5f;

        [SerializeField, Range(0f, 10f)] private float _sprint = 3f;


        private readonly int _speedHash = Animator.StringToHash("Speed");

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
            _agent.speed = _speed;
        }

        private void OnEnable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<PlayerSprintChangedEvent>(HandleSprint);
        }

        private void OnDisable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Unsubscribe<PlayerSprintChangedEvent>(HandleSprint);
        }

        private void HandleSprint(PlayerSprintChangedEvent evt) {
            if (_agent != null) {
                _agent.speed = evt.IsSprinting ? _sprint : _speed;
            }
        }

        private void Start() {
            _blackboard = new Blackboard();
            _blackboard.SetData("PlayerTarget", _target);
            _blackboard.SetData("NavAgent", _agent);
            _blackboard.SetData("PatrolWaypoints", _patrolWaypoints);
            _blackboard.SetData("Animator", _animator);
            _blackboard.SetData("PatrolWaitTime", _patrolWaitTime);

            var abort = new Abort(() => IsPlayerInFOV(), "FOVAbort");
            abort.AddChild(new ActionPatrol(_blackboard));

            var chase = new ActionChase(_blackboard);

            var root = new Selector("RootSelector");
            root.AddChild(abort);
            root.AddChild(chase);

            _tree = new BehaviourTree("LightSeeker Behaviour Tree", root);
        }

        private void Update() {
            _tree?.Process();
            _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
        }

        private bool IsPlayerInFOV() {
            if (_target == null || _eyeTransform == null) return false;

            Vector3 eyePos = _eyeTransform.position;
            Vector3 dirToTarget = _target.position - eyePos;
            float dist = dirToTarget.magnitude;

            if (dist > _viewDistance) return false;

            float angle = Vector3.Angle(_eyeTransform.forward, dirToTarget.normalized);
            if (angle > _fovAngle * 0.5f) return false;

            if (Physics.Raycast(eyePos, dirToTarget.normalized, dist, _obstacleMask)) {
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

            // target
            if (_target != null) {
                Gizmos.color = inFov ? Color.red : Color.green;
                Gizmos.DrawLine(pos, _target.position);
            }
        }
    }
}
