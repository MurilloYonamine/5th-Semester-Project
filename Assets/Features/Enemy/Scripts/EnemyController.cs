using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour {
        private NavMeshAgent _navMeshAgent;
        private Transform _playerTransform;

        [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;
        [SerializeField, Range(0f, 10f)] private float _stoppingDistance = 1f;
        [SerializeField, Range(0f, 25f)] private float _range = 5f;


        private void Awake() {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = _speed;
            _navMeshAgent.stoppingDistance = _stoppingDistance;
        }

        private void Start() {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update() {
            if (IsPlayerInRange()) {
                SetAgentDestination();
            }
            else {
                _navMeshAgent.ResetPath();
            }
        }

        private void SetAgentDestination() {
            if (_playerTransform != null) {
                _navMeshAgent.SetDestination(_playerTransform.position);
            }
        }

        private bool IsPlayerInRange() {
            if (_playerTransform == null) return false;

            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            return distanceToPlayer <= _range;
        }

        private void OnDrawGizmos() {
            if (_playerTransform == null) return;

            GizmosPlayerDistance();
            GizmosEnemyRange();
        }
        private void GizmosPlayerDistance() {
            if (!IsPlayerInRange()) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
        private void GizmosEnemyRange() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}
