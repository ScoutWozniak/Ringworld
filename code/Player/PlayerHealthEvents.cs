using Sandbox;

public sealed class PlayerHealthEvents : Component, IHealthEventListener
{
	public void OnDamage() {}

	public void OnDeath()
	{
		if ( IsProxy )
			return;

		RingworldManager.Instance.LocalPlayerDie();
	}

	protected override void OnUpdate()
	{

	}
}
