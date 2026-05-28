using System.Collections;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.AI;

namespace FifthSemester.Gameplay.Enemy {
    [DisallowMultipleComponent]
    public class NurseDirector : MonoBehaviour {
        [Header("Retreat Settings")]
        [SerializeField] private float retreatDistance = 8f;
        [SerializeField] private float retreatDuration = 3f;

        [Header("Pursuit Settings")]
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float flashlightPursuitSpeed = 4f;

        private NavMeshAgent _agent;
        private MonoBehaviour _nurseComponent;
        private float _baseSpeed;
        private IEventBus _eventBus;

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _nurseComponent = GetComponent<MonoBehaviour>();
            _baseSpeed = _agent != null ? _agent.speed : 3f;
        }

        private void OnEnable() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus?.Subscribe<PlayerSprintChangedEvent>(OnPlayerSprintChanged);
            _eventBus?.Subscribe<FlashlightTargetedEvent>(OnFlashlightTargeted);
            _eventBus?.Subscribe<PlayerEnteredRoomEvent>(OnPlayerEnteredRoom);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<PlayerSprintChangedEvent>(OnPlayerSprintChanged);
            _eventBus?.Unsubscribe<FlashlightTargetedEvent>(OnFlashlightTargeted);
            _eventBus?.Unsubscribe<PlayerEnteredRoomEvent>(OnPlayerEnteredRoom);
        }

        private void Start() {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_agent != null) _baseSpeed = _agent.speed;
        }

        private void OnPlayerSprintChanged(PlayerSprintChangedEvent evt) {
            if (_agent == null) return;

            if (evt.IsSprinting) {
                _agent.speed = _baseSpeed * sprintMultiplier;
            }
            else {
                _agent.speed = _baseSpeed;
            }
        }

        private void OnFlashlightTargeted(FlashlightTargetedEvent evt) {
            if (_agent == null) return;

            if (evt.Target == gameObject && evt.IsIlluminated) {
                _agent.speed = flashlightPursuitSpeed;
            }
            else if (evt.Target == gameObject && !evt.IsIlluminated) {
                _agent.speed = _baseSpeed;
            }
        }

        private void OnPlayerEnteredRoom(PlayerEnteredRoomEvent evt) {
            if (evt.Player == null || _agent == null) return;

            // calcula o ponto de recuo na direção oposta ao player
            Vector3 dir = (evt.Player.position - transform.position).normalized;
            Vector3 desired = transform.position - dir * retreatDistance;

            // ajusta para o NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desired, out hit, retreatDistance, NavMesh.AllAreas)) {
                _agent.SetDestination(hit.position);
            }
            else {
                _agent.SetDestination(desired);
            }

            // desliga temporariamente o componente Nurse (se existir) para evitar que a BT sobrescreva o destino
            var nurse = GetComponent<Nurse>();
            if (nurse != null) {
                StartCoroutine(TemporarilyDisableNurse(nurse));
            }
        }

        private IEnumerator TemporarilyDisableNurse(Nurse nurse) {
            nurse.enabled = false;
            yield return new WaitForSeconds(retreatDuration);
            if (nurse != null) nurse.enabled = true;
            // restaura velocidade base
            if (_agent != null) _agent.speed = _baseSpeed;
        }
    }
}
