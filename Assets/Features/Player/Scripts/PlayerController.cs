using FifthSemester.Core.Events;
using FifthSemester.Player.Components;
using System;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerController : MonoBehaviour {
        [Header("Player Unity Components")]
        public Rigidbody Rigidbody { get; private set; }

        [Header("Player Components")]
        private PlayerComponent[] _playerComponents;
        [field: SerializeField] public PlayerEvents InputEvents { get; private set; }
        [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool lockCursor = true;

        private void Awake() {
            Rigidbody = GetComponent<Rigidbody>();

            _playerComponents = new PlayerComponent[] {
                InputEvents,
                PlayerMovement,
            };
            ForEachComponent(component => component.Initialize(this));
            ForEachComponent(component => component.OnAwake());
        }
        private void Start() {
            if (lockCursor) {
                Cursor.lockState = CursorLockMode.Locked;
            }

            ForEachComponent(component => component.OnStart());
        }
        private void Update() {
            ForEachComponent(component => component.OnUpdate());
        }
        private void FixedUpdate() {
            ForEachComponent(component => component.OnFixedUpdate());
        }
        private void LateUpdate() {
            ForEachComponent(component => component.OnLateUpdate());
        }
        private void OnEnable() {
            ForEachComponent(component => component.OnEnable());
        }
        private void OnDisable() {
            ForEachComponent(component => component.OnDisable());
        }
        private void ForEachComponent(Action<PlayerComponent> action) {
            foreach (var component in _playerComponents) {
                if (component == null) continue;

                action(component);
            }
        }
    }
}