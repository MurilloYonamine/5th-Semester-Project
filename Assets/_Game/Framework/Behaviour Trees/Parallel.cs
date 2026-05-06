// Autor: Murillo Gomes Yonamine
// Data: 28/04/2026

namespace FifthSemester.Framework.BehaviourTrees {
    public class Parallel : Node {

        public Parallel(string name = "Parallel") : base(name) {}

        public override Status Process() {
            int successCount = 0;

            // Precisa rodar TODOS os filhos a cada chamada.
            foreach (var child in Children) {
                Status childStatus = child.Process();

                // Se qualquer filho falhar, o nó inteiro falha e aborta os outros
                if (childStatus == Status.Failure) {
                    Reset();
                    return Status.Failure;
                }

                // Contabiliza quantos filhos já terminaram com sucesso
                if (childStatus == Status.Success) {
                    successCount++;
                }
            }

            // Se a quantidade de sucessos for igual ao total de filhos, o nó teve sucesso total
            if (successCount == Children.Count) {
                Reset();
                return Status.Success;
            }

            // Se chegou até aqui, nenhum falhou, mas nem todos terminaram, então continua rodando
            return Status.Running;
        }
    }
}
