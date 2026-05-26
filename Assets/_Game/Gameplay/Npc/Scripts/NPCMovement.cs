using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

namespace FifthSemester.Gameplay.NPC {
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCMovement : MonoBehaviour {
        [SerializeField] private float walkRadius = 20f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;

        private NavMeshAgent _agent;
        private Animator _animator;
        private IGameStateService _gameStateService;
        private float _waitTimer;
        private bool _waiting;

        private readonly int _speedParameter = Animator.StringToHash("Speed");

        private void Start() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
            _gameStateService = ServiceLocator.Get<IGameStateService>();
            GoToRandomPoint();
        }

        private void Update() {
            if (_gameStateService == null || _gameStateService.CurrentState != GameState.Gameplay) {
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

        private void GoToRandomPoint() {
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
