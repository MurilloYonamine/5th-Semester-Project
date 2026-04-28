// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using System.Collections.Generic;

namespace FifthSemester.Framework.BehaviourTrees {
    public class Node {
        public enum Status { Success, Failure, Running }

        public readonly string Name;

        public readonly List<Node> Children = new List<Node>();
        protected int _currentChild;

        public Blackboard Blackboard;

        public Node(string name = "Node", Blackboard blackboard = null) {
            this.Name = name;
            this.Blackboard = blackboard;
        }

        public void AddChild(Node child) {
            Children.Add(child);
        }

        public virtual Status Process() {
            return Children[_currentChild].Process();
        }

        public virtual void Reset() {
            _currentChild = 0;
            foreach (var child in Children) {
                child.Reset();
            }
        }
    }
}
