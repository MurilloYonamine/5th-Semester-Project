using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour {
        private NavMeshAgent _navMeshAgent;
        private Transform _playerTransform;

        [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;

        private void Awake() {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = _speed;
        }

        private void Start() {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update() {
            SetAgentDestination();
        }

        private void SetAgentDestination() {
            if (_playerTransform != null) {
                _navMeshAgent.SetDestination(_playerTransform.position);
            }
        }
    }
}
