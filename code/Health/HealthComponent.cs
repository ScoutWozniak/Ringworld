using Sandbox;
using System;

public sealed class HealthComponent : Component
{
	[Sync] public float Health { get; set; }
	[Sync] public float Shields { get; set; }

	[Category("Health")][Property] float MaxHealth { get; set; } = 100;
	[Category( "Health" )][Property] float MinHealth { get; set; }


	[Category( "Shield" )] [Property] public bool HasShields { get; set; }
	[Category( "Shield" )] [ShowIf("HasShields", true)] [Property] float MaxShields { get; set; } = 100;
	[Category( "Shield" )] [Property][ShowIf( "HasShields", true )] float MinShields { get; set; } = 0;
	[Category( "Shield" )] [Property][ShowIf( "HasShields", true )] float RechargeTime { get; set; } = 5.0f;
	[Category( "Shield" )] [Property][ShowIf( "HasShields", true )] float RechargeRate { get; set; } = 0.1f;

	[Property] bool CanKillBind { get; set; }

	[Property] GameObject DeathRagdoll { get; set; }

	TimeSince LastDamageTime;

	protected override void OnStart()
	{
		base.OnStart();
		Health = MaxHealth;
		Shields = MaxShields;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (LastDamageTime > RechargeTime && Shields < MaxShields)
		{
			Shields = Math.Min( Shields + (MaxShields * RechargeRate * Time.Delta), MaxShields );
		}

		if (CanKillBind && Input.Pressed("KillBind"))
		{
			TakeDamage( 10000.0f );
		}
	}

	[Broadcast]
	public void TakeDamage( float damage )
	{
		// Deal with visual stuff here
		if ( IsProxy )
			return;

		LastDamageTime = 0;
		Log.Info( Health );
		// Client handles damage down here
		if ( HasShields )
		{
			if ( Shields <= 0 )
			{
				Health -= damage;
				if ( Health <= 0 )
				{
					OnDeath();
				}
			}
			else
			{
				var leftOverDmg = -(Shields - damage);
				Shields = Math.Max(Shields - damage, 0);
				if (Shields <= 0)
				{
					Health -= leftOverDmg;
					if ( Health <= 0 )
					{
						OnDeath();
					}
				}
			}
		}
		else
		{
			Health -= damage;
			if ( Health <= 0 )
			{
				OnDeath();
			}
		}
		TriggerDamageEvents();
	}

	[Broadcast]
	void OnDeath()
	{
		Log.Info( GameObject.Name + " died!" );
		if ( DeathRagdoll != null )
		{
			var rag = DeathRagdoll.Clone( Transform.Position );
			if(Components.TryGet<CharacterController>(out var cc) && rag.Components.TryGet<ModelPhysics>(out var physics))
			{
				physics.PhysicsGroup.AddVelocity( cc.Velocity );
			}
		}
		TriggerDeathEvents();
	}

	void TriggerDeathEvents()
	{
		foreach(var comp in Components.GetAll<IHealthEventListener>())
		{
			comp.OnDeath();
		}
	}

	void TriggerDamageEvents()
	{
		foreach ( var comp in Components.GetAll<IHealthEventListener>() )
		{
			comp.OnDamage();
		}
	}
}

public interface IHealthEventListener
{
	public void OnDamage();

	public void OnDeath();
}
