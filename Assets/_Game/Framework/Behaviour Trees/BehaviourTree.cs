// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

namespace FifthSemester.Framework.BehaviourTrees {
    public class BehaviourTree {
        private string _name;
        private readonly Node _rootNode;

        public BehaviourTree(string name, Node rootNode) {
            this._name = name;
            this._rootNode = rootNode;
        }

        public Node.Status Process() {
            if (_rootNode == null) {
                return Node.Status.Failure;
            }

            Node.Status treeStatus = _rootNode.Process();

            if (treeStatus != Node.Status.Running) {
                _rootNode.Reset();
            }

            return treeStatus;
        }
    }
}
