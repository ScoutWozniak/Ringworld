using Sandbox;
using System.Collections.Generic;
using System.Numerics;

namespace MyGame;

public partial class Grenade : AnimatedEntity
{
	[Net]
	public TimeSince sinceThrown { get; set; }

	public bool timerStart = false;

	const int grenadeRange = 125;

	public override void Spawn()
	{
		base.Spawn();
		Model = Cloud.Model( "smartmario.simple_frag_grenade" );
		UsePhysicsCollision = true;
		PhysicsEnabled = true;
		//DeleteAsync( 10.0f );
		Tags.Add( "grenade" );
		
		sinceThrown = 0;
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );
	}

	[GameEvent.Tick]
	public virtual void Tick()
	{
		if (sinceThrown >= 3.0f && timerStart)
		{
			var particle = Particles.Create( "particles/explosion.vpcf" );
			particle.SetPosition( 0, Position );
			Explosion();
			if (Game.IsServer) Delete();
		}
	}

	public virtual void Explosion()
	{
		/*
		var explosion = new ExplosionEntity();
		explosion.Position = Position;
		explosion.Damage = 100;
		explosion.Explode( this );
		*/
		foreach ( Entity ent in Entity.FindInSphere( Position, grenadeRange ) )
		{
			ent.TakeDamage( new DamageInfo()
			.WithDamage( 100.0f ) );
		}
		Sound.FromWorld( "frag.explode", Position );
		//DebugOverlay.Sphere( Position, grenadeRange, Color.White, 10.0f );
	}
}
