using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerMovement : PlayerComponent
{
    [SerializeField] private Vector3 _movementDirection;
    [SerializeField, Range(0f, 20f)] private float _moveSpeed = 5f;

    public override void OnEnable()
    {
        owner.PlayerControls.Player.Move.performed += OnMovePerformed;
        owner.PlayerControls.Player.Move.canceled += OnMoveCanceled;
    }
    public override void OnDisable()
    {
        owner.PlayerControls.Player.Move.performed -= OnMovePerformed;
        owner.PlayerControls.Player.Move.canceled -= OnMoveCanceled;
    }
    public override void OnUpdate()
    {
        CharacterController controller = owner.GetComponent<CharacterController>();

        Vector3 move = _movementDirection * _moveSpeed * Time.deltaTime;
        if (controller != null)
        {
            Vector3 worldMove = owner.transform.TransformDirection(move);
            controller.Move(worldMove);
        }
    }
    public override void OnFixedUpdate()
    {

    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _movementDirection = new Vector3(input.x, 0, input.y);
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _movementDirection = Vector3.zero;
    }

}
