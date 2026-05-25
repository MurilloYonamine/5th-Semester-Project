// Autor: Murillo Gomes Yonamine
// Data: 11/05/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionStareAndPounce : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string LAND_POSITION_KEY = "SafeLightLandPosition";
        private const string ANIMATOR_KEY = "Animator";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;
        private Animator _animator;
        private IAudioService _audioService;

        // --- Timers e Configurações ---
        private float _stareTimeRequired = 4f; // Tempo encarando
        private float _jumpUpAnimDuration = 2f; // Duração exata da animação dele subindo
        private float _timeInAir = 1.5f; // Tempo de suspense lá no teto
        private float _landAnimDuration = 2f; // Duração da animação dele caindo/aterrissando
        private float _postLandDelay = 1f; // Tempo que o jogador tem para reagir após o land
        private float _jumpscareRange = 2f; // Distância máxima para iniciar o jumpscare após o post-land delay

        [Header("Audio")]
        [SerializeField] private AudioClip _jumpSound;
        [SerializeField] private AudioClip _landSound;

        private enum PounceState { Staring, JumpingUp, HoveringInAir, Landing }
        private PounceState _currentState = PounceState.Staring;

        private float _currentTimer = 0f;

        public ActionStareAndPounce(Blackboard blackboard, string name = "Stare And Pounce") : base(name, blackboard) {
            _blackboard = blackboard;
            ServiceLocator.TryGet<IAudioService>(out _audioService);
        }

        public override Status Process() {
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            if (_target == null) _target = _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            if (_animator == null) _animator = _blackboard.GetData<Animator>(ANIMATOR_KEY);

            if (_agent == null || _target == null) return Status.Failure;

            switch (_currentState) {
                case PounceState.Staring:
                    return ProcessStaring();
                case PounceState.JumpingUp:
                    return ProcessJumpingUp();
                case PounceState.HoveringInAir:
                    return ProcessHoveringInAir();
                case PounceState.Landing:
                    return ProcessLanding();
            }

            return Status.Running;
        }

        private Status ProcessStaring() {
            _agent.isStopped = true;

            Vector3 direction = (_target.position - _agent.transform.position).normalized;
            direction.y = 0;
            _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

            _currentTimer += Time.deltaTime;

            if (_currentTimer >= _stareTimeRequired) {
                _currentState = PounceState.JumpingUp;
                _agent.enabled = false; 

                if (_animator != null) _animator.SetTrigger("Jump");
                PlaySfx(_jumpSound);

                _currentTimer = 0f;
            }

            return Status.Running;
        }

        private Status ProcessJumpingUp() {
            _currentTimer += Time.deltaTime;

            if (_currentTimer >= _jumpUpAnimDuration) {
                _currentState = PounceState.HoveringInAir;
                _currentTimer = 0f;
            }
            return Status.Running;
        }

        private Status ProcessHoveringInAir() {
            _currentTimer += Time.deltaTime;

            if (_currentTimer >= _timeInAir) {
                _currentState = PounceState.Landing;
                _currentTimer = 0f;

                Vector3 landingTarget = _blackboard.HasKey(LAND_POSITION_KEY)
                    ? _blackboard.GetData<Vector3>(LAND_POSITION_KEY)
                    : _target.position;

                if (_agent != null) {
                    // Try to safely move agent on navmesh; re-enable agent so subsequent actions can use it
                    if (_agent.isOnNavMesh) {
                        _agent.Warp(landingTarget);
                        if (!_agent.enabled) _agent.enabled = true;
                        _agent.isStopped = true;
                        _agent.ResetPath();
                    }
                    else {
                        _agent.transform.position = landingTarget;
                    }
                }

                Vector3 directionToPlayer = (_target.position - _agent.transform.position).normalized;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero) {
                    _agent.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                }

                if (_animator != null) _animator.SetTrigger("Land");
                PlaySfx(_landSound);
            }
            return Status.Running;
        }

        private Status ProcessLanding() {
            _currentTimer += Time.deltaTime;

            // Wait for landing animation to finish
            if (_currentTimer < _landAnimDuration) {
                return Status.Running;
            }

            // After landing animation, give the player a short reaction window
            if (_currentTimer < _landAnimDuration + _postLandDelay) {
                return Status.Running;
            }

            // After the reaction window, check player's distance. If within range, succeed to allow jumpscare.
            // If the player is still in the safe light at the moment of landing, allow immediate attack.
            if (_blackboard != null && _blackboard.HasKey("IsPlayerInSafeLight") && _blackboard.GetData<bool>("IsPlayerInSafeLight")) {
                 if (_agent != null && !_agent.enabled) _agent.enabled = true;
                if (_agent != null && _agent.isOnNavMesh) {
                    _agent.isStopped = false;
                    _agent.ResetPath();
                    _agent.SetDestination(_target.position);
                }
                return Status.Success;
             }

            if (_target != null && _agent != null) {
                float distanceToPlayer = Vector3.Distance(_agent.transform.position, _target.position);
                if (distanceToPlayer <= _jumpscareRange) {
                    // Ensure agent enabled before proceeding and set a destination so ActionPlayJumpscare can detect arrival
                    if (!_agent.enabled) _agent.enabled = true;
                    if (_agent.isOnNavMesh) {
                        _agent.isStopped = false;
                        _agent.ResetPath();
                        _agent.SetDestination(_target.position);
                    }
                    return Status.Success;
                }
            }

            // Player escaped the reaction window / range
            return Status.Failure;
        }

        public override void Reset() {
            base.Reset();
            _currentState = PounceState.Staring;
            _currentTimer = 0f;
            // Re-enable agent if it was disabled during the jump
            if (_agent != null && !_agent.enabled) {
                _agent.enabled = true;
            }
         }

        private void PlaySfx(AudioClip clip) {
            if (clip == null || _audioService == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }
    }
}
