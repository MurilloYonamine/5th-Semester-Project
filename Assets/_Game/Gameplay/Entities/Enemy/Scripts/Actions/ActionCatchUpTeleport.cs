using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay {
    public class ActionCatchUpTeleport : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string ANIMATOR_KEY = "Animator";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;
        private Animator _animator;
        private IAudioService _audioService;

        // --- Timers e Configurações ---
        private readonly float _jumpUpAnimDuration = 2f; 
        private readonly float _timeInAir = 1.5f; 
        private readonly float _landAnimDuration = 2f; 

        private enum CatchUpState { JumpingUp, HoveringInAir, Landing }
        private CatchUpState _currentState = CatchUpState.JumpingUp;
        private float _currentTimer = 0f;

        private AudioClip _jumpSound;
        private AudioClip _landSound;

        public ActionCatchUpTeleport(Blackboard blackboard, string name = "Catch Up Teleport") : base(name, blackboard) {
            _blackboard = blackboard;
            ServiceLocator.TryGet<IAudioService>(out _audioService);
        }

        public override Status Process() {
            if (_agent == null) _agent = _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            if (_target == null) _target = _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            if (_animator == null) _animator = _blackboard.GetData<Animator>(ANIMATOR_KEY);

            if (_agent == null || _target == null) return Status.Failure;

            // Load sounds from blackboard if available
            if (_jumpSound == null && _blackboard.HasKey("JumpSound")) {
                _jumpSound = _blackboard.GetData<AudioClip>("JumpSound");
            }
            if (_landSound == null && _blackboard.HasKey("LandSound")) {
                _landSound = _blackboard.GetData<AudioClip>("LandSound");
            }

            switch (_currentState) {
                case CatchUpState.JumpingUp:
                    return ProcessJumpingUp();
                case CatchUpState.HoveringInAir:
                    return ProcessHoveringInAir();
                case CatchUpState.Landing:
                    return ProcessLanding();
            }

            return Status.Running;
        }

        private Status ProcessJumpingUp() {
            if (_currentTimer == 0f) {
                _agent.isStopped = true;
                _agent.enabled = false;

                if (_animator != null) _animator.SetTrigger("Jump");
                PlaySfx(_jumpSound);
            }

            _currentTimer += Time.deltaTime;

            if (_currentTimer >= _jumpUpAnimDuration) {
                _currentState = CatchUpState.HoveringInAir;
                _currentTimer = 0f;
            }
            return Status.Running;
        }

        private Status ProcessHoveringInAir() {
            _currentTimer += Time.deltaTime;

            if (_currentTimer >= _timeInAir) {
                _currentState = CatchUpState.Landing;
                _currentTimer = 0f;

                // Encontra um ponto de aterrissagem seguro no NavMesh próximo ao player (ex: entre 5 e 8 metros)
                Vector3 landingTarget = GetValidLandingPosition(_target.position);

                if (_agent != null) {
                    _agent.transform.position = landingTarget;
                    _agent.enabled = true;
                    _agent.isStopped = true;
                    _agent.ResetPath();
                    _agent.Warp(landingTarget);
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

            // Aguarda a animação de queda terminar
            if (_currentTimer < _landAnimDuration) {
                return Status.Running;
            }

            // Aterrissagem concluída com sucesso
            if (_agent != null) {
                _agent.enabled = true;
                _agent.isStopped = false;
            }
            return Status.Success;
        }

        public override void Reset() {
            base.Reset();
            _currentState = CatchUpState.JumpingUp;
            _currentTimer = 0f;
            if (_agent != null && !_agent.enabled) {
                _agent.enabled = true;
            }
        }

        private Vector3 GetValidLandingPosition(Vector3 playerPos) {
            for (int i = 0; i < 10; i++) {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(5f, 8f);
                Vector3 candidatePos = playerPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

                if (NavMesh.SamplePosition(candidatePos, out NavMeshHit navHit, 4f, NavMesh.AllAreas)) {
                    return navHit.position;
                }
            }
            return playerPos;
        }

        private void PlaySfx(AudioClip clip) {
            if (clip == null || _audioService == null) {
                return;
            }
            _audioService.PlaySFX(clip);
        }
    }
}
