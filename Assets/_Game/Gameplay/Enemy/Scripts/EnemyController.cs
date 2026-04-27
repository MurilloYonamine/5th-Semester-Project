// Autor: Murillo Gomes Yonamine
// Data: 28/03/2026

using FifthSemester.Core;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour {
        private NavMeshAgent _navMeshAgent;
        private Transform _playerTransform;

        [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;
        [SerializeField, Range(0f, 10f)] private float _sprint = 3f;

        [SerializeField, Range(0f, 10f)] private float _stoppingDistance = 1f;
        [SerializeField, Range(0f, 25f)] private float _range = 5f;

        public static List<EnemyController> AllEnemies = new List<EnemyController>();

        private void OnEnable() {
            if (!AllEnemies.Contains(this)) {
                AllEnemies.Add(this);
            }

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<PlayerSprintChangedEvent>(HandleSprint);
        }
        private void OnDisable() {
            if (AllEnemies.Contains(this)) {
                AllEnemies.Remove(this);
            }

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Unsubscribe<PlayerSprintChangedEvent>(HandleSprint);
        }


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
        private void HandleSprint(PlayerSprintChangedEvent evt) {
            _navMeshAgent.speed = evt.IsSprinting ? _sprint : _speed;
        }

        public void Freeze(bool isFrozen) {
            _navMeshAgent.isStopped = isFrozen;
        }

        #region Gizmos
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
        #endregion
    }
}
