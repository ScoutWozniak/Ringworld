using Sandbox;
using Editor;
using Ringworld;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
public partial class DroppedGrenade : AnimatedEntity
{

	[Net]
	public WorldGrenadeSpawn spawn { get; set; }


	public override void Spawn()
	{
		base.Spawn();
		UsePhysicsCollision = true;
		PhysicsEnabled = true;
		EnableTouch = true;
		Model = Cloud.Model( "smartmario.simple_frag_grenade" );
		Tags.Add( "weapon" );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
	}

	public override void Touch( Entity other )
	{
		if ( Sandbox.Game.IsClient ) return;
		if ( other is Pawn player )
		{
			if ( player.currentFragGrenades < 2 )
			{
				if (spawn != null)
					spawn.GrenadeTaken();
				player.currentFragGrenades += 1;
				Delete();
			}
		}
	}

}
