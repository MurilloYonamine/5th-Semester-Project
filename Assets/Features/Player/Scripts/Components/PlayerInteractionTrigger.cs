using FifthSemester.Framework;
using System;
using UnityEngine;

namespace FifthSemester.Player {
    public class PlayerInteractionTrigger : MonoBehaviour {
        public event Action<Collider> OnObjectEntered;
        public event Action<Collider> OnObjectExited;

        private void OnTriggerEnter(Collider other) {
            OnObjectEntered?.Invoke(other);
        }

        private void OnTriggerExit(Collider other) {
            OnObjectExited?.Invoke(other);
        }
    }
}
