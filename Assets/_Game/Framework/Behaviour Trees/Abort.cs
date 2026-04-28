// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

using System;

namespace FifthSemester.Framework.BehaviourTrees {
    public class Abort : Node {

        // A condição que, se for verdadeira, causará a interrupção.
        private readonly Func<bool> abortCondition;

        public Abort(Func<bool> abortCondition, string name = "ConditionalAbort") : base(name) {
            this.abortCondition = abortCondition;
        }

        public override Status Process() {
            // Checagem na condição de interrupção
            if (abortCondition.Invoke()) {
                // Se a condição for atendida, precisamos abortar a ação imediatamente!
                Reset();

                // Retornar Failure faz com que o Sequence ou Parallel em que este nó
                // estiver inserido também falhe e interrompa o fluxo.
                return Status.Failure;
            }

            // Se a condição NÃO foi atingida, deixamos o nó filho rodar normalmente.
            if (Children.Count > 0) {
                return Children[0].Process();
            }

            // Se não houver filhos para rodar, apenas retorna Success.
            return Status.Success;
        }
    }
}
