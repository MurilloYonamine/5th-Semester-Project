using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    [RequireComponent(typeof(Collider))]
    public class RoomTrigger : MonoBehaviour {
        [Tooltip("Optional id to identify the room. Used by PlayerEnteredRoomEvent.")]
        [SerializeField] private string _roomId = "";

        private void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player")) return;

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new PlayerEnteredRoomEvent(_roomId, other.transform));
        }
    }
}
