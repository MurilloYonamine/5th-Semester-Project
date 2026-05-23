// Autor: Murillo Gomes Yonamine
// Data: 09/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class Nurse : MonoBehaviour {
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string ANIMATOR_KEY = "Animator";
        private const string JUMPSCARE_DIRECTOR_KEY = "JumpscareDirector";
        private const string IS_FROZEN_KEY = "IsFrozen";
        private const string IS_OBSERVED_KEY = "HasLineOfSight";

        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private Camera _playerCamera;

        [Header("Game Design Rules")]
        [Tooltip("Ative para a Fase 3, onde ela não ataca, apenas bloqueia o caminho.")]
        [SerializeField] private bool _isPhase3Passive = false;

        [Header("Vision (Weeping Angel Settings)")]
        [SerializeField] private Transform _eyeTransform;
        [SerializeField] private LayerMask _obstacleMask;

        [Header("Speed & Rotation Settings")]
        [SerializeField, Range(0f, 15f)] private float _rotationSpeed = 8f;
        [SerializeField] private float _patrolWaitTime = 2f;

        private BehaviourTree _tree;
        private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Animator _animator;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector _jumpscareDirector;

        private readonly int _speedHash = Animator.StringToHash("Speed");

        [Header("Observation Speed Settings")]
        [SerializeField] private float _normalSpeed = 2.5f;
        [SerializeField] private float _observedSpeed = 0.6f;
        [SerializeField] private float _speedLerp = 5f;
        private bool _isObserved = false;

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            _agent.speed = _normalSpeed;
            _agent.updateRotation = false;

            SetupBlackboard();
        }
        private void Start() {
            if (_playerCamera == null) _playerCamera = Camera.main;
            BuildBehaviourTree();
        }
        private void SetupBlackboard() {
            _blackboard = new Blackboard();
            _blackboard.SetData(PLAYER_TARGET_KEY, _target);
            _blackboard.SetData(NAV_AGENT_KEY, _agent);
            _blackboard.SetData(ANIMATOR_KEY, _animator);
            _blackboard.SetData(JUMPSCARE_DIRECTOR_KEY, _jumpscareDirector);
            _blackboard.SetData(IS_FROZEN_KEY, false);
            _blackboard.SetData(IS_OBSERVED_KEY, false);

            _blackboard.SetData("PatrolWaitTime", _patrolWaitTime);
        }

        private void BuildBehaviourTree() {
            var root = new Selector("NurseRootBehavior");

            if (!_isPhase3Passive) {
                var chaseSequence = new Sequence("AggressiveChase");
                chaseSequence.AddChild(new ActionChase(_blackboard, "Chase Player"));
                chaseSequence.AddChild(new ActionPlayJumpscare(_blackboard, "Jumpscare"));

                root.AddChild(chaseSequence);
            }

            var patrolSequence = new Sequence("SearchPatrol");
            patrolSequence.AddChild(new ActionPatrol(_blackboard, "Patrol Waypoints"));

            root.AddChild(patrolSequence);

            _tree = new BehaviourTree("Nurse Behaviour Tree", root);
        }

        private void Update() {
            CheckIfObservedByPlayer();
            UpdateState();

            _tree?.Process();
            HandleRotation();

            if (_animator != null && _agent != null && !_isObserved) {
                _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
            }
        }

        private void CheckIfObservedByPlayer() {
            if (_playerCamera == null || _eyeTransform == null) return;

            Vector3 viewportPoint = _playerCamera.WorldToViewportPoint(_eyeTransform.position);
            bool inViewport = viewportPoint.x > 0 && viewportPoint.x < 1 &&
                              viewportPoint.y > 0 && viewportPoint.y < 1 &&
                              viewportPoint.z > 0;

            if (inViewport) {
                Vector3 dirToCamera = (_playerCamera.transform.position - _eyeTransform.position).normalized;
                float distToCamera = Vector3.Distance(_playerCamera.transform.position, _eyeTransform.position);

                if (!Physics.Raycast(_eyeTransform.position, dirToCamera, distToCamera, _obstacleMask)) {
                    _isObserved = true;

                    _blackboard.SetData(IS_FROZEN_KEY, true);
                    _blackboard.SetData(IS_OBSERVED_KEY, true);

                    return;
                }
            }

            _isObserved = false;

            _blackboard.SetData(IS_FROZEN_KEY, false);
            _blackboard.SetData(IS_OBSERVED_KEY, false);
        }

        private void UpdateState() {
            if (_agent == null || !_agent.isOnNavMesh) {
                return;
            }

            float targetSpeed = _isObserved
                ? _observedSpeed
                : _normalSpeed;

            _agent.speed = Mathf.Lerp(
                _agent.speed,
                targetSpeed,
                Time.deltaTime * _speedLerp
            );

            _agent.isStopped = false;

            if (_animator != null) {
                _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
            }
        }

        private void HandleRotation() {
            if (_target == null) return;

            Vector3 direction;

            if (_isObserved) {
                direction = (_target.position - transform.position).normalized;
            }
            else {
                direction = _agent.desiredVelocity.normalized;
            }

            direction.y = 0;

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * _rotationSpeed
            );
        }
        private void OnDrawGizmos() {
            if (_eyeTransform == null || _playerCamera == null) return;

            Gizmos.color = _isObserved ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(_eyeTransform.position, 0.3f);

            if (_isObserved) {
                Gizmos.DrawLine(_eyeTransform.position, _playerCamera.transform.position);
            }
        }
    }
}