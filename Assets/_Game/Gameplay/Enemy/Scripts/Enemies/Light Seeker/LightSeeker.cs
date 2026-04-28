using UnityEngine;
using FifthSemester.Framework.BehaviourTrees;
using UnityEngine.AI;

namespace FifthSemester.Gameplay.Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class LightSeeker : MonoBehaviour {
        [SerializeField] private Transform _target;
        [SerializeField] private Transform[] _patrolWaypoints;
        public bool isIlluminated = false;

        private BehaviourTree _tree;
        private Blackboard _blackboard;

        private NavMeshAgent _agent;
        private Animator _animator;

        private readonly int _speedHash = Animator.StringToHash("Speed");

        private void Awake() {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Start() {
            _blackboard = new Blackboard();
            _blackboard.SetData("PlayerTarget", _target);

            var chaseNode = new ActionChase(_agent);
            var actionStop = new ActionStop(_agent);

            var patrolNode = new ActionPatrol(
                _agent,
                _patrolWaypoints,
                _agent.stoppingDistance
            );

            chaseNode.Blackboard = this._blackboard;

            var chaseWithAbort = new Abort(() => isIlluminated = true);
            chaseWithAbort.AddChild(chaseNode);

            var root = new Selector("LightSeeker Root");
            root.AddChild(patrolNode);

            _tree = new BehaviourTree("LightSeeker Behaviour Tree", root);
        }

        private void Update() {
            _tree.Process();
            _animator.SetFloat(_speedHash, _agent.velocity.magnitude);
        }

        public void SetIllumination(bool status) {
            isIlluminated = status;
        }
    }
}
