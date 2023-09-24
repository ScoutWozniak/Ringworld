using Sandbox;
using System.Collections.Generic;
using System.Numerics;

namespace MyGame;

public partial class FragGrenade : Grenade
{

	public override void Spawn()
	{
		base.Spawn();
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		base.OnPhysicsCollision( eventData );
		if ( !timerStart )
		{
			sinceThrown = 0;
			timerStart = true;
		}
	}

}
