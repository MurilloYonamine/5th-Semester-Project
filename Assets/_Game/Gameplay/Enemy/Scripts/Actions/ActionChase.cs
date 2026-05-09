// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;
using FifthSemester.Core.Audio;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionChase : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TARGET_KEY = "PlayerTarget";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";
        private const string WHITE_NOISE_CLIP_KEY = "WhiteNoiseClip";
        private const string WHITE_NOISE_MAX_VOLUME_KEY = "WhiteNoiseMaxVolume";
        private const string HAS_LINE_OF_SIGHT_KEY = "HasLineOfSight";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _target;
        private IAudioService _audioService;
        private AudioTrack _whiteNoiseTrack;
        private AudioClip _whiteNoiseClip;

        private float _glitchStartDistance = 15f;
        private float _maxGlitchOpacity = 0.4f;
        private float _maxWhiteNoiseVolume = 0.5f;
        private float _whiteNoiseFadeSpeed = 2f;
        private float _currentWhiteNoiseVolume = 0f;

        private float _loseTargetDistance = 25f;

        public ActionChase(Blackboard blackboard, string name = "Chase") : base(name, blackboard) {
            _blackboard = blackboard;
        }

        public override Status Process() {
            CacheReferences();

            if (_agent == null || _target == null) {
                TurnOffGlitch();
                return Status.Failure;
            }

            if (_blackboard.GetData<bool>(IS_STUNNED_KEY)) {
                TurnOffGlitch();
                return Status.Failure;
            }

            float distance = Vector3.Distance(
                _agent.transform.position,
                _target.position
            );

            if (distance >= _loseTargetDistance) {
                TurnOffGlitch();
                DisposeWhiteNoise();
                return Status.Failure;
            }

            EnsureAgentIsMoving();
            _agent.SetDestination(_target.position);

            UpdateGlitchProximityEffect();

            if (HasReachedTarget()) {
                TurnOffGlitch();
                DisposeWhiteNoise();
                return Status.Success;
            }

            return Status.Running;
        }

        private void CacheReferences() {
            _agent ??= _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _target ??= _blackboard.GetData<Transform>(PLAYER_TARGET_KEY);
            _audioService ??= ServiceLocator.Get<IAudioService>();
            _whiteNoiseClip ??= _blackboard.GetData<AudioClip>(WHITE_NOISE_CLIP_KEY);

            if (_blackboard.HasKey(WHITE_NOISE_MAX_VOLUME_KEY)) {
                _maxWhiteNoiseVolume = _blackboard.GetData<float>(WHITE_NOISE_MAX_VOLUME_KEY);
            }
        }

        private void UpdateGlitchProximityEffect() {
            bool hasLineOfSight = _blackboard.GetData<bool>(HAS_LINE_OF_SIGHT_KEY);

            if (!hasLineOfSight) {
                TurnOffGlitch();
                return;
            }
            
            // Calcula a distância entre o monstro e o jogador
            float currentDistance = Vector3.Distance(_agent.transform.position, _target.position);

            if (currentDistance <= _glitchStartDistance) {
                // Matemática para inverter o valor (mais perto = valor mais alto)
                float intensity = 1f - (currentDistance / _glitchStartDistance);
                float finalOpacity = intensity * _maxGlitchOpacity;

                // Envia o valor diretamente para a memória global dos Shaders
                Shader.SetGlobalFloat("_NoiseOpacity", finalOpacity);
                UpdateWhiteNoise(intensity);
                return;
            }
            TurnOffGlitch();
        }

        private void TurnOffGlitch() {
            Shader.SetGlobalFloat("_NoiseOpacity", 0f);
            StopWhiteNoise();
        }

        private void UpdateWhiteNoise(float intensity) {
            if (_audioService == null || _whiteNoiseClip == null) {
                return;
            }

            float targetVolume = intensity * _maxWhiteNoiseVolume;

            if (targetVolume <= 0f) {
                StopWhiteNoise();
                return;
            }

            if (_whiteNoiseTrack == null) {
                _whiteNoiseTrack = _audioService.PlayAmbience(
                    _whiteNoiseClip,
                    loop: true,
                    startingVolume: 0f,
                    volumeCap: _maxWhiteNoiseVolume
                );
                _currentWhiteNoiseVolume = 0f;
            }

            _currentWhiteNoiseVolume = Mathf.MoveTowards(_currentWhiteNoiseVolume, targetVolume, _whiteNoiseFadeSpeed * Time.deltaTime);
            _whiteNoiseTrack.Volume = _currentWhiteNoiseVolume;
        }

        private void StopWhiteNoise() {
            if (_whiteNoiseTrack != null) {
                _whiteNoiseTrack.Volume = 0f;
                _currentWhiteNoiseVolume = 0f;
            }
        }
        private void DisposeWhiteNoise() {
            if (_whiteNoiseTrack != null) {
                _audioService.StopAmbience(_whiteNoiseClip);
                _whiteNoiseTrack = null;
            }

            _currentWhiteNoiseVolume = 0f;
        }
        private void EnsureAgentIsMoving() {
            if (_agent.isStopped) {
                _agent.isStopped = false;
            }
        }

        private bool HasReachedTarget() {
            if (_agent.pathPending || !_agent.hasPath) {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        public override void Reset() {
            TurnOffGlitch();
            DisposeWhiteNoise();
            base.Reset();
        }
    }
}