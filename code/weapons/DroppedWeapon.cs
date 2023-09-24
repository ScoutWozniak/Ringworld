using Sandbox;
using Editor;
using MyGame;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
public partial class DroppedWeapon : AnimatedEntity, IUse
{

	[Net]
	public WeaponData weaponSpawn { get; set; }

	[Net]
	public int droppedReserveAmmo { get; set; }

	[Net]
	public WorldWeaponSpawn spawn { get; set; }

	[Net]
	public int ammoInReserve { get; set; }
	public int ammoInClip { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		UsePhysicsCollision = true;
		PhysicsEnabled = true;
		SetData( weaponSpawn );
		EnableTouch = true;
		Scale = 1.5f;
	}

	public void SetData( WeaponData weapon )
	{
		weaponSpawn = weapon;
		Tags.Add( "weapon" );
		SetModel( weaponSpawn?.WorldModel );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
	}

	public override void Touch( Entity other )
	{
		if (Game.IsClient) return;
		if ( other is Pawn player )
		{
			var weaponDupe = player.CheckDuplicateWeapon( weaponSpawn );
			if ( weaponDupe != null )
			{
				
				if ( GiveWeaponAmmo( weaponDupe ) )
				{
					if ( spawn != null )
						spawn.WeaponTaken();
					Delete();
				}
			}
		}
	}

	public bool OnUse( Entity user )
	{

		if ( user is Pawn player )
		{
			if ( player.CheckDuplicateWeapon( weaponSpawn ) != null ) return false;
			GivePlayerWeapon( player );
			if ( spawn != null )
				spawn.WeaponTaken();
			Delete();
			return true;
		}

		return false;
	}

	void GivePlayerWeapon(Pawn player)
	{
		Weapon newWeapon = new Weapon();
		newWeapon.SetWeaponInfo( weaponSpawn );
		newWeapon.reserveAmmo = ammoInReserve;
		newWeapon.ammoInClip = ammoInClip;
		player.AddWeapon( newWeapon );
	}

	bool GiveWeaponAmmo(Weapon weapon)
	{
		if ( weapon.reserveAmmo < weaponSpawn.maxReserveAmmo )
		{
			weapon.reserveAmmo += ammoInReserve;
			weapon.reserveAmmo = weapon.reserveAmmo.Clamp(0,weaponSpawn.maxReserveAmmo);
			return true;
		}
		return false;
	}

	public bool IsUsable( Entity user )
	{
		if ( user is Pawn player )
		{
			return true;
		}
		return false;
	}
}
