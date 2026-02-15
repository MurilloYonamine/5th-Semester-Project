using UnityEngine;

namespace FifthSemester.Player.Components {
    public interface IMovementState {
        void Enter();
        void Exit();
        void Tick();
        void FixedTick();
        void HandleMove(Vector2 input);
        void HandleSprint(bool isPressed);
        void HandleCrouch(bool isPressed);
    }
}