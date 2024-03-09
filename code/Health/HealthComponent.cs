using Sandbox;
using System;

public sealed class HealthComponent : Component
{
	[Sync] public float Health { get; set; }

	[Property] float MaxHealth { get; set; } = 100;
	[Property] float MinHealth { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Health = MaxHealth;
	}

	[Broadcast]
	public void TakeDamage(float damage)
	{
		// Deal with visual stuff here
		if ( IsProxy )
			return;

		// Client handles damage down here
		Health = MathF.Max( Health - damage, 0f );
	}
}
