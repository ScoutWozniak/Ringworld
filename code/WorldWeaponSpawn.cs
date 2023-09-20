using Sandbox;
using Editor;
using MyGame;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[Library( "weapon_spawn" ), HammerEntity]
[Title( "Weapon Spawn" ), Category( "Weapons" ), Icon( "place" )]
public partial class WorldWeaponSpawn : AnimatedEntity, IUse
{

	[Net]
	public Weapon weaponToGive { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Model = Cloud.Model( "https://asset.party/facepunch/wooden_crate" );

		UsePhysicsCollision = true;
		PhysicsEnabled = true;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		Log.Info( "touched" );
	}

	public bool OnUse( Entity user )
	{
		// Do something when entity is used...

		Log.Info( $"{this} has been used by {user}!" );

		if (user is Pawn player)
		{
			player.AddWeapon( new SMG());
		}

		return false;
	}

	public bool IsUsable( Entity user )
	{
		if (user is Pawn player)
		{
			if ( player.ActiveWeapon.name != "SMG" )
				return true;
		}
		return false;
	}
}
