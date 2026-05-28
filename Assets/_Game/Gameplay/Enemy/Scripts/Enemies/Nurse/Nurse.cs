// Autor: Murillo Gomes Yonamine
// Data: 09/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Framework.BehaviourTrees;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Map2;
using System.Collections.Generic;
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
        private const string CUTSCENE_ACTIVE_KEY = "CutsceneActive";
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

        [Header("Jumpscare Settings")]
        [SerializeField, Range(0.1f, 5f)] private float _jumpscareTriggerDistance = 1.25f;

        private BehaviourTree _tree;
            private Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Animator _animator;
        private IAudioService _audioService;

        private float _footstepTimer;
        private bool _lastObservedState = false;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector _jumpscareDirector;

        [Header("Unlock Gate")]
        [SerializeField] private Map2KeyDefinitionSO _unlockKeyDefinition;

        private readonly int _speedHash = Animator.StringToHash("Speed");

        [Header("Observation Speed Settings")]
        [SerializeField] private float _normalSpeed = 2.5f;
        [SerializeField] private float _observedSpeed = 0.6f;
        [SerializeField] private float _speedLerp = 5f;
        private bool _isObserved = false;
        private bool _isLockedByKey = false;
        private bool _isAggressive = false;
        private IInventoryService<Item> _inventoryService;

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            // Try to cache inventory service early so RefreshUnlockState (called in OnEnable)
            // has a chance to validate lock state before first Update.
            ServiceLocator.TryGet<IInventoryService<Item>>(out _inventoryService);

            Debug.Log($"[Nurse] Awake: agent={( _agent!=null )}, animator={( _animator!=null )}, inventoryCached={( _inventoryService!=null )}");

            if(_target == null)
                _target = GameObject.FindGameObjectWithTag("Player")?.transform;

            if(_playerCamera == null)
                _playerCamera = Camera.main;

            if(_animator == null)
                _animator = GetComponentInChildren<Animator>();

            _agent.speed = _normalSpeed;
            _agent.updateRotation = false;
            _agent.stoppingDistance = _jumpscareTriggerDistance;

            SetupBlackboard();
        }
        private void Start() {
            if (_playerCamera == null) _playerCamera = Camera.main;
            ServiceLocator.TryGet<IInventoryService<Item>>(out _inventoryService);
            ServiceLocator.TryGet<IAudioService>(out _audioService);
            BuildBehaviourTree();
            RefreshUnlockState();

            Debug.Log($"[Nurse] Start: isLockedByKey={_isLockedByKey}, isAggressive={_isAggressive}");
        }

        private void OnEnable() {
            IEventBus eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<InventoryItemAddedEvent>(OnInventoryItemAdded);
            RefreshUnlockState();

            Debug.Log("[Nurse] OnEnable: subscribed to InventoryItemAddedEvent and refreshed lock state");
        }

        private void OnDisable() {
            IEventBus eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Unsubscribe<InventoryItemAddedEvent>(OnInventoryItemAdded);
        }

        private void SetupBlackboard() {
            _blackboard = new Blackboard();
            _blackboard.SetData(PLAYER_TARGET_KEY, _target);
            _blackboard.SetData(NAV_AGENT_KEY, _agent);
            _blackboard.SetData(ANIMATOR_KEY, _animator);
            _blackboard.SetData(JUMPSCARE_DIRECTOR_KEY, _jumpscareDirector);
            _blackboard.SetData(CUTSCENE_ACTIVE_KEY, false);
            _blackboard.SetData(IS_FROZEN_KEY, false);
            _blackboard.SetData(IS_OBSERVED_KEY, false);

            _blackboard.SetData("PatrolWaitTime", _patrolWaitTime);
        }

        private void BuildBehaviourTree() {
            RebuildBehaviourTree(includeChase: _isAggressive);
        }

        private void RebuildBehaviourTree(bool includeChase) {
            var root = new Selector("NurseRootBehavior");

            if (includeChase && !_isPhase3Passive) {
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
            if (_isLockedByKey) {
                if (_agent != null && _agent.isOnNavMesh) {
                    _agent.isStopped = true;
                    _agent.ResetPath();
                }

                return;
            }

            bool isCutsceneActive = _blackboard != null && _blackboard.HasKey(CUTSCENE_ACTIVE_KEY) && _blackboard.GetData<bool>(CUTSCENE_ACTIVE_KEY);

            if (isCutsceneActive) {
                _isObserved = false;
                _lastObservedState = false;
                _blackboard.SetData(IS_FROZEN_KEY, false);
                _blackboard.SetData(IS_OBSERVED_KEY, false);
            }
            else {
                CheckIfObservedByPlayer();
                UpdateState();
            }

            _tree?.Process();
            if (!isCutsceneActive) {
                HandleRotation();
            }

            if (_animator != null && _agent != null && !_isObserved && !isCutsceneActive) {
                _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
            }

        }

        private void OnInventoryItemAdded(InventoryItemAddedEvent evt) {
            Debug.Log($"[Nurse] OnInventoryItemAdded: item added event received: {evt}");
            RefreshUnlockState();
        }

        private void RefreshUnlockState() {
            Debug.Log("[Nurse] RefreshUnlockState: checking inventory for unlock key");
            if (_unlockKeyDefinition == null) {
                _isLockedByKey = false;
                _isAggressive = false;
                Debug.Log("[Nurse] No unlock key defined - nurse remains unlocked and passive.");
                return;
            }
            if (_inventoryService == null) {
                ServiceLocator.TryGet<IInventoryService<Item>>(out _inventoryService);
            }

            if (_inventoryService == null) {
                Debug.Log("[Nurse] Inventory service not available yet - nurse remains unlocked and passive.");
                _isLockedByKey = false;
                _isAggressive = false;
                return;
            }

            IReadOnlyList<Item> items = _inventoryService.GetItems();
            if (items == null) {
                Debug.Log("[Nurse] Inventory returned null items - nurse remains unlocked and passive.");
                _isLockedByKey = false;
                _isAggressive = false;
                return;
            }

            for (int i = 0; i < items.Count; i++) {
                if (items[i] is Map2KeyItem keyItem && keyItem.KeyDefinition == _unlockKeyDefinition) {
                    // When the player obtains the key, nurse becomes aggressive (chase + jumpscare)
                    _isAggressive = true;
                    _isLockedByKey = false;
                    Debug.Log("[Nurse] Unlock key found in inventory - enabling aggression.");
                    RebuildBehaviourTree(includeChase: true);
                    Debug.Log("[Nurse] Behaviour tree rebuilt to include chase sequence.");
                    return;
                }
            }

            // Key not found - remain unlocked (patrolling), not aggressive
            _isLockedByKey = false;
            _isAggressive = false;
            Debug.Log("[Nurse] Unlock key not found - nurse remains unlocked and passive.");
        }

        private void CheckIfObservedByPlayer() {
            if (_playerCamera == null || _eyeTransform == null) return;
            Vector3 viewportPoint = _playerCamera.WorldToViewportPoint(_eyeTransform.position);
            bool inViewport = viewportPoint.x > 0 && viewportPoint.x < 1 &&
                              viewportPoint.y > 0 && viewportPoint.y < 1 &&
                              viewportPoint.z > 0;

            if (inViewport) {
                Vector3 origin = _eyeTransform.position;
                Vector3 dirToCamera = (_playerCamera.transform.position - origin).normalized;
                float distToCamera = Vector3.Distance(_playerCamera.transform.position, origin);

                RaycastHit hitInfo;

                // Primary check: raycast against configured obstacle mask
                bool blockedByObstacle = Physics.Raycast(origin, dirToCamera, out hitInfo, distToCamera, _obstacleMask);

                if (blockedByObstacle) {
                    Debug.Log($"[Nurse] Vision blocked by {hitInfo.collider.gameObject.name} (layer={hitInfo.collider.gameObject.layer})");
                    // obstructed -> not observed
                    _isObserved = false;
                    _blackboard.SetData(IS_FROZEN_KEY, false);
                    _blackboard.SetData(IS_OBSERVED_KEY, false);
                    if (_lastObservedState) Debug.Log("[Nurse] Player no longer observing nurse - resuming behavior.");
                    _lastObservedState = false;
                    return;
                }

                // Fallback: spherecast to catch thin obstacles or gaps the ray misses
                float sphereRadius = 0.18f;
                bool sphereBlocked = Physics.SphereCast(origin, sphereRadius, dirToCamera, out hitInfo, distToCamera, _obstacleMask);
                if (sphereBlocked) {
                    Debug.Log($"[Nurse] Vision spherecast blocked by {hitInfo.collider.gameObject.name} (layer={hitInfo.collider.gameObject.layer})");
                    _isObserved = false;
                    _blackboard.SetData(IS_FROZEN_KEY, false);
                    _blackboard.SetData(IS_OBSERVED_KEY, false);
                    if (_lastObservedState) Debug.Log("[Nurse] Player no longer observing nurse - resuming behavior.");
                    _lastObservedState = false;
                    return;
                }

                // No obstacle detected -> observed
                _isObserved = true;
                _blackboard.SetData(IS_FROZEN_KEY, true);
                _blackboard.SetData(IS_OBSERVED_KEY, true);
                if (!_lastObservedState) Debug.Log("[Nurse] Player observed nurse - freezing behavior.");
                _lastObservedState = true;

                return;
            }

            _isObserved = false;
            _blackboard.SetData(IS_FROZEN_KEY, false);
            _blackboard.SetData(IS_OBSERVED_KEY, false);
            if (_lastObservedState) Debug.Log("[Nurse] Player no longer observing nurse - resuming behavior.");
            _lastObservedState = false;
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