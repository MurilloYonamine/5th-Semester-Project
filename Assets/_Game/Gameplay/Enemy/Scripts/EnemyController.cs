// Autor: Murillo Gomes Yonamine
// Data: 28/03/2026

using FifthSemester.Core;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Framework.BehaviourTrees;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Gameplay.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour {
        private NavMeshAgent _navMeshAgent;

        [Header("Player Settings")]
        private GameObject _player;
        [SerializeField] private string _playerTag = "Player";

        [Header("Enemy Settings")]
        [SerializeField, Range(0f, 10f)] private float _speed = 1.5f;
        [SerializeField, Range(0f, 10f)] private float _sprint = 3f;
        [SerializeField, Range(0f, 10f)] private float _stoppingDistance = 1f;
        [SerializeField, Range(0f, 25f)] private float _range = 5f;

        public static List<EnemyController> AllEnemies = new List<EnemyController>();

        [SerializeField] private List<Transform> _patrolWaypoints = new List<Transform>();

        [SerializeField] private Animator animator;
        private readonly int _speedHash = Animator.StringToHash("Speed");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

        private void Awake() {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.speed = _speed;
            _navMeshAgent.stoppingDistance = _stoppingDistance;
        }
        private void Start() {
            _player = GameObject.FindGameObjectWithTag(_playerTag);
        }
        private void Update() {
            animator.SetFloat(_speedHash, _navMeshAgent.velocity.magnitude);
            animator.SetBool(_isGroundedHash, _navMeshAgent.isOnOffMeshLink == false);
        }
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


        private void HandleSprint(PlayerSprintChangedEvent evt) {
            _navMeshAgent.speed = evt.IsSprinting ? _sprint : _speed;
        }

        public void GetPlayerPosition(out GameObject position) {
            position = _player;
        }
    }
}
