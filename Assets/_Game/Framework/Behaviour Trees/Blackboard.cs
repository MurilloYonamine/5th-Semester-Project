using System.Collections.Generic;

namespace FifthSemester.Framework.BehaviourTrees {
    public class Blackboard {
        
        // O dicionário que guarda as memórias. 
        private Dictionary<string, object> _memory = new Dictionary<string, object>();

        // Salva ou atualiza um dado na memória
        public void SetData(string key, object value) {
            _memory[key] = value;
        }

        // Busca um dado genérico da memória
        public object GetData(string key) {
            if (_memory.TryGetValue(key, out object value)) {
                return value;
            }
            return null; 
        }

        // Método genérico avançado: Busca o dado já convertido para o tipo correto (ex: Vector3, Transform)
        public T GetData<T>(string key) {
            if (_memory.TryGetValue(key, out object value)) {
                return (T)value;
            }
            return default(T);
        }

        // Verifica se a IA já tem algo específico na memória
        public bool HasKey(string key) {
            return _memory.ContainsKey(key);
        }
        
        public void ClearData(string key) {
            if (_memory.ContainsKey(key)) {
                _memory.Remove(key);
            }
        }
    }
}