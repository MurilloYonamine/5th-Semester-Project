// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

namespace FifthSemester.Framework.BehaviourTrees {
    public class Selector : Node {

        public Selector(string name = "Selector") : base(name) { }

        public override Status Process() {
            while (_currentChild < Children.Count) {
                Status childStatus = Children[_currentChild].Process();

                if (childStatus == Status.Running) return Status.Running;

                // Se o filho teve sucesso, o seletor para e retorna sucesso (já encontrou uma ação válida!)
                if (childStatus == Status.Success) {
                    Reset();
                    return Status.Success;
                }

                // Se o filho falhou, avança para o próximo (tenta a próxima opção)
                _currentChild++;
            }

            // Se o loop terminou, significa que TODOS os filhos retornaram Failure
            Reset();
            return Status.Failure;
        }
    }
}
