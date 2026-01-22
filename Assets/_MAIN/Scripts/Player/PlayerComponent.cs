using UnityEngine;

public abstract class PlayerComponent
{
	protected PlayerController owner;

	public virtual void Initialize(PlayerController owner)
	{
		this.owner = owner;
	}

	public virtual void OnEnable() { }
	public virtual void OnDisable() { }
	public virtual void OnUpdate() { }
	public virtual void OnFixedUpdate() { }
}
