// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using UnityEngine;
using UnityEngine.AI;
using FifthSemester.Framework.BehaviourTrees;

namespace FifthSemester.Gameplay.Enemy {
    public class ActionPatrol : Node {
        private const string NAV_AGENT_KEY = "NavAgent";
        private const string PLAYER_TRANSFORM_KEY = "PlayerTarget";
        private const string PATROL_WAIT_TIME_KEY = "PatrolWaitTime";
        private const string IS_STUNNED_KEY = "IsStunnedByFlashlight";
        private const string EYE_TRANSFORM_KEY = "EyeTransform";
        private const string VIEW_DISTANCE_KEY = "ViewDistance";
        private const string FOV_ANGLE_KEY = "FovAngle";
        private const string OBSTACLE_MASK_KEY = "ObstacleMask";

        private readonly Blackboard _blackboard;
        private NavMeshAgent _agent;
        private Transform _playerTransform;

        private bool _isWaiting = false;
        private bool _hasDestination = false;
        private ActionWait _waitNode;
        private float _waitTime = 1f;

        private float _roamRadius = 15f; // Quão longe ele pode ir na patrulha aleatória
        private float _approachDistance = 8f; // Quantos metros ele anda na direção do jogador quando sorteado
        private int _approachThreshold = 50; // Chance (0 a 100). Ex: < 50 Aleatório, >= 50 Vai pro Player.

        public ActionPatrol(Blackboard blackboard, string name = "Patrol") : base(name, blackboard) {
            _blackboard = blackboard;
            _waitNode = new ActionWait(0f);
        }

        public override Status Process() {
            RefreshDataFromBlackboard();

            // Se a Enfermeira for agressiva e o jogador NÃO estiver em sala segura ou luz segura,
            // cancela a patrulha e retoma a perseguição ativa imediatamente (wallhack Weeping Angel)
            bool isAggressive = _blackboard.HasKey("IsAggressive") && _blackboard.GetData<bool>("IsAggressive");
            bool isPlayerInRoom = _blackboard.HasKey("IsPlayerInRoom") && _blackboard.GetData<bool>("IsPlayerInRoom");
            bool isPlayerInSafeLight = _blackboard.HasKey("IsPlayerInSafeLight") && _blackboard.GetData<bool>("IsPlayerInSafeLight");

            if (isAggressive && !isPlayerInRoom && !isPlayerInSafeLight) {
                return Status.Failure;
            }

            if (_blackboard.GetData<bool>(IS_STUNNED_KEY)) {
                return Status.Failure;
            }

            if (_blackboard.HasKey("IsFrozen") && _blackboard.GetData<bool>("IsFrozen")) {
                if (_agent != null && _agent.isOnNavMesh && !_agent.isStopped) {
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                }
                return Status.Running;
            }

            // Garante que o agente seja liberado para andar se saiu do estado congelado
            if (_agent != null && _agent.isOnNavMesh && _agent.isStopped) {
                _agent.isStopped = false;
            }

            if (IsPlayerInLineOfSight()) {
                return Status.Failure;
            }

            if (_agent == null || _playerTransform == null) {
                return Status.Failure;
            }

            // Se está esperando o tempo acabar, processa a espera e retorna
            if (_isWaiting) {
                ProcessWait();
                return Status.Running;
            }

            // Se não tem um destino atual, sorteia um novo e começa a andar
            if (!_hasDestination) {
                SetNewProceduralDestination();
            }

            // Verifica se chegou ao destino (ou bem perto dele)
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) {
                BeginWait();
            }

            return Status.Running;
        }

        private void RefreshDataFromBlackboard() {
            _agent = _blackboard.GetData<NavMeshAgent>(NAV_AGENT_KEY);
            _playerTransform = _blackboard.GetData<Transform>(PLAYER_TRANSFORM_KEY);
            _waitTime = _blackboard.GetData<float>(PATROL_WAIT_TIME_KEY);
        }

        private bool IsPlayerInLineOfSight() {
            Transform eyeTransform = _blackboard.GetData<Transform>(EYE_TRANSFORM_KEY);

            if (_playerTransform == null || eyeTransform == null) {
                return false;
            }

            Vector3 eyePosition = eyeTransform.position;
            Vector3 directionToPlayer = _playerTransform.position - eyePosition;
            float distanceToPlayer = directionToPlayer.magnitude;
            float viewDistance = _blackboard.GetData<float>(VIEW_DISTANCE_KEY);

            if (distanceToPlayer > viewDistance) {
                return false;
            }

            Vector3 directionToPlayerNormalized = directionToPlayer.normalized;
            float fovAngle = _blackboard.GetData<float>(FOV_ANGLE_KEY);
            float angle = Vector3.Angle(eyeTransform.forward, directionToPlayerNormalized);

            if (angle > fovAngle * 0.5f) {
                return false;
            }

            LayerMask obstacleMask = _blackboard.GetData<LayerMask>(OBSTACLE_MASK_KEY);
            if (Physics.Raycast(eyePosition, directionToPlayerNormalized, distanceToPlayer, obstacleMask)) {
                return false;
            }

            return true;
        }

        private void SetNewProceduralDestination() {
            int roll = Random.Range(0, 101);
            Vector3 targetPosition = _agent.transform.position;

            if (roll >= _approachThreshold) {
                // Pega a direção do inimigo para o jogador
                Vector3 directionToPlayer = (_playerTransform.position - _agent.transform.position).normalized;

                // Define o ponto alvo andando X metros naquela direção
                targetPosition = _agent.transform.position + (directionToPlayer * _approachDistance);
            }
            else {
                Vector2 random2D = Random.insideUnitCircle * _roamRadius;

                Vector3 randomDirection = new Vector3(
                    x: random2D.x,
                    y: 0,
                    z: random2D.y
                );

                targetPosition = _agent.transform.position + randomDirection;
            }

            Debug.DrawLine(_agent.transform.position, targetPosition, Color.red, 2f);

            // Valida o ponto no NavMesh para garantir que ele não tente andar para dentro de uma parede
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, _roamRadius, NavMesh.AllAreas)) {

                NavMeshHit edgeHit;

                if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas)) {

                    float minDistanceFromWall = 1.5f;

                    if (edgeHit.distance < minDistanceFromWall) {
                        return;
                    }
                }

                _agent.SetDestination(hit.position);
                _hasDestination = true;
            }
        }

        private void BeginWait() {
            _isWaiting = true;
            _hasDestination = false;
            _waitNode = new ActionWait(_waitTime);
        }

        private void ProcessWait() {
            if (_waitNode.Process() != Status.Success) {
                return;
            }

            _isWaiting = false;
        }

        public override void Reset() {
            base.Reset();
            _isWaiting = false;
            _hasDestination = false;
            _waitNode?.Reset();
        }
    }
}