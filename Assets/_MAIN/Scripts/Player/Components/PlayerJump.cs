using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlayerJump : PlayerComponent
{
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _verticalVelocity = 0f;

    public override void OnEnable()
    {
        owner.PlayerControls.Player.Jump.performed += OnJumpPerformed;
        owner.PlayerControls.Player.Jump.canceled += OnJumpCanceled;
    }
    public override void OnDisable()
    {
        owner.PlayerControls.Player.Jump.performed -= OnJumpPerformed;
        owner.PlayerControls.Player.Jump.canceled -= OnJumpCanceled;
    }
    public override void OnUpdate()
    {
        
    }
    public override void OnFixedUpdate()
    {
        
    }
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        
    }
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
    }
}
