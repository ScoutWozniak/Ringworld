using Sandbox;
using System.Collections.Generic;
using System.Numerics;

namespace Ringworld;

public partial class PlasmaGrenade : Grenade
{

	int bounces = 1;

	public override void Spawn()
	{
		base.Spawn();
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );
		Log.Info( eventData.Other.Entity );

		if ( bounces == 0 )
		{

			if ( !timerStart )
			{
				sinceThrown = 0;
				timerStart = true;
				var newPosition = eventData.Other.Entity.Transform.ToLocal( Transform );
				SetParent( eventData.Other.Entity, true );
				Transform = newPosition;
				PhysicsEnabled = false;
			}
		}
		else bounces -= 1;
		
		if ( Sandbox.Game.IsServer)
			base.Velocity = base.Velocity / 2;
	}

	[GameEvent.Tick]
	public override void Tick()
	{
		if ( sinceThrown >= 3.0f && timerStart )
		{
			var particle = Particles.Create( "particles/explosion.vpcf" );
			particle.SetPosition( 0, Position );
			Explosion();
			if ( Sandbox.Game.IsServer ) Delete();
		}
	}

}
