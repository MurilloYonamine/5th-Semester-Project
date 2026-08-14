// Autor: Murillo Gomes Yonamine
// Data: 14/08/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCMovement : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] private float walkRadius = 20f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;

        [Header("Look at Player")]
        [SerializeField] private float _turnSpeed = 6f;
        [SerializeField] private float _lookAtPlayerRange = 10f;

        private NavMeshAgent _agent;
        private Animator _animator;
        private IGameStateService _gameStateService;
        private IEventBus _eventBus;
        private float _waitTimer;
        private bool _waiting;
        private bool _isTalking;
        private Transform _playerTransform;
        private string _dialogueId;

        private readonly int _speedParameter = Animator.StringToHash("Speed");

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            if (TryGetComponent<DialogueTrigger>(out var trigger)) {
                _dialogueId = string.IsNullOrWhiteSpace(trigger.Id) ? gameObject.name : trigger.Id;
            } else {
                _dialogueId = gameObject.name;
            }
        }

        private void Start() {
            _gameStateService = ServiceLocator.Get<IGameStateService>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<DialogueEndedEvent>(OnDialogueEnded);

            FindPlayerTransform();
            GoToRandomPoint();
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void FindPlayerTransform() {
            if (_playerTransform != null) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) {
                _playerTransform = player.transform;
            } else if (Camera.main != null) {
                _playerTransform = Camera.main.transform;
            }
        }

        private void Update() {
            if (_gameStateService == null || _gameStateService.CurrentState != GameState.Gameplay) {
                return;
            }

            if (_isTalking) {
                HandleTalkingState();
                return;
            }

            UpdateAnimation();

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
                if (!_waiting) {
                    _waiting = true;
                    _waitTimer = Random.Range(minWaitTime, maxWaitTime);
                }

                _waitTimer -= Time.deltaTime;

                if (_waitTimer <= 0f) {
                    _waiting = false;
                    GoToRandomPoint();
                }
            }
        }

        private void HandleTalkingState() {
            if (_agent.enabled && !_agent.isStopped) {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }

            if (_animator != null) {
                _animator.SetFloat(_speedParameter, 0f);
            }

            FindPlayerTransform();
            if (_playerTransform != null) {
                Vector3 direction = _playerTransform.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f) {
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
                }
            }
        }

        public void StartTalking(Transform player = null) {
            _isTalking = true;
            if (player != null) {
                _playerTransform = player;
            } else {
                FindPlayerTransform();
            }

            if (_agent.enabled) {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }

            if (_animator != null) {
                _animator.SetFloat(_speedParameter, 0f);
            }
        }

        public void StopTalking() {
            if (!_isTalking) return;
            _isTalking = false;

            if (_agent.enabled) {
                _agent.isStopped = false;
            }

            _waiting = true;
            _waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (string.IsNullOrWhiteSpace(evt.NpcId) || evt.NpcId == _dialogueId || evt.NpcId == gameObject.name) {
                StopTalking();
            }
        }

        private void GoToRandomPoint() {
            if (_isTalking || !_agent.enabled) return;

            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, UnityEngine.AI.NavMesh.AllAreas)) {
                _agent.SetDestination(hit.position);
            }
        }

        private void UpdateAnimation() {
            if (_animator == null) return;

            float speed = _agent.velocity.magnitude;
            _animator.SetFloat(_speedParameter, speed);
        }
    }
}
