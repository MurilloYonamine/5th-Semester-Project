using UnityEngine;
using System.Collections.Generic;
using INPUT;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    private PlayerComponent[] _components;
    [field: SerializeField] public PlayerMovement PlayerMovement { get; private set; }
    [field: SerializeField] public PlayerJump PlayerJump { get; private set; }
    [field: SerializeField] public PlayerLook PlayerLook { get; private set; }
    [field: SerializeField] public Transform Planet { get; private set; }

    private CharacterController _characterController;
    public PlayerControls PlayerControls { get; private set; }

    public void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        PlayerControls = new PlayerControls();

        _components = new PlayerComponent[]
        {
            PlayerLook,
            PlayerMovement,
            PlayerJump
        };

        ForEachComponent(component => component.Initialize(this));
    }
    public void Start()
    {
        // components already created and initialized in Awake
    }
    private void OnEnable()
    {
        PlayerControls.Enable();
        ForEachComponent(component => component.OnEnable());
    }
    private void OnDisable()
    {
        PlayerControls.Disable();
        ForEachComponent(component => component.OnDisable());
    }
    public void Update()
    {
        ForEachComponent(component => component.OnUpdate());
    }
    public void FixedUpdate()
    {
        ForEachComponent(component => component.OnFixedUpdate());
    }
    public void ForEachComponent(System.Action<PlayerComponent> action)
    {
        if (_components == null) return;
        foreach (var component in _components)
        {
            if (component == null) continue;
            action(component);
        }
    }
}