using System;
using UnityEngine;

[Serializable]
public class PlayerLook : PlayerComponent
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 60.0f;

    private Vector2 rotation = Vector2.zero;

    public override void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        owner.PlayerControls.Player.Look.performed += OnLookPerformed;
        owner.PlayerControls.Player.Look.canceled += OnLookCanceled;
    }

    public override void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        owner.PlayerControls.Player.Look.performed -= OnLookPerformed;
        owner.PlayerControls.Player.Look.canceled -= OnLookCanceled;
    }

    public override void OnUpdate()
    {

    }
    private void OnLookPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        rotation.x += -input.y * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);

        Quaternion localRotation = Quaternion.Euler(0f, input.x * lookSpeed, 0f);
        owner.transform.rotation = owner.transform.rotation * localRotation;
    }
    private void OnLookCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // No action needed on look canceled for mouse look

    }
}
