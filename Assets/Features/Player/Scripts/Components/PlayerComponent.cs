// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using UnityEngine;

namespace FifthSemester.Player.Components {
    public class PlayerComponent {
        protected PlayerController _player;

        public void Initialize(PlayerController player) {
            _player = player;
        }
        public virtual void OnAwake() { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}