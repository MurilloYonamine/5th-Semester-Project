// Autor: Murillo Gomes Yonamine
// Data: 14/02/2026

using FifthSemester.Player.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FifthSemester.Player {
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerJump))]
    [RequireComponent(typeof(PlayerCamera))]
    [RequireComponent(typeof(PlayerInteraction))]
    public class PlayerController : MonoBehaviour {
        [Header("Player Unity Components")]
        public Rigidbody Rigidbody { get; private set; }

        [Header("Player Components")]
        public PlayerEvents InputEvents { get; private set; }
        public PlayerMovement PlayerMovement { get; private set; }
        public PlayerJump PlayerJump { get; private set; }
        public PlayerCamera PlayerCamera { get; private set; }
        public PlayerInteraction PlayerInteraction { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool lockCursor = true;
        public bool IsGrounded => PlayerJump.IsGrounded;

        private void Awake() {
            Rigidbody = GetComponent<Rigidbody>();
            PlayerMovement = GetComponent<PlayerMovement>();
            PlayerJump = GetComponent<PlayerJump>();
            PlayerCamera = GetComponent<PlayerCamera>();
            PlayerInteraction = GetComponent<PlayerInteraction>();
            InputEvents.OnAwake();
        }
        private void Start() {
            if (lockCursor) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        private void OnEnable() {
            InputEvents?.OnEnable();
        }
        private void OnDisable() {
            InputEvents?.OnDisable();
        }
    }
}