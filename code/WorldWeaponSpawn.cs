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

	[Property(Title = "Weapon Data")]
	public WeaponData weaponSpawn { get;set; }

	public override void Spawn()
	{
		base.Spawn();
		Log.Info( weaponSpawn?.WorldModel );
		SetModel( weaponSpawn?.WorldModel);

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
			Weapon newWeapon = new Weapon();
			newWeapon.weaponInfo = weaponSpawn;
			player.AddWeapon( newWeapon);
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
