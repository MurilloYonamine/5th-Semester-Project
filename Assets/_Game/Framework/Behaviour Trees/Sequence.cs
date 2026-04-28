namespace FifthSemester.Framework.BehaviourTrees {
    public class Sequence : Node {
        public Sequence(string name = "Sequence") : base(name) { }

        public override Status Process() {
            // Percorre a lista de filhos a partir do currentChild atual
            while (_currentChild < Children.Count) {
                Status childStatus = Children[_currentChild].Process();

                // Se o filho ainda está rodando, a sequência também fica rodando
                if (childStatus == Status.Running) return Status.Running;

                // Se o filho falhou, a sequência falha imediatamente
                if (childStatus == Status.Failure) {
                    Reset();
                    return Status.Failure;
                }

                // Se o filho teve sucesso, avança para o próximo
                _currentChild++;
            }

            // Se o loop terminou, significa que todos os filhos retornaram Success
            Reset();
            return Status.Success;
        }
    }
}