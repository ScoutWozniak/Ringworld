using Sandbox;
using Editor;
using Ringworld;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[Library( "weapon_spawn" ), HammerEntity]
[Title( "Weapon Spawn" ), Category( "Weapons" ), Icon( "place" )]
public partial class WorldWeaponSpawn : Entity
{
	[Property( Title = "Weapon Data" )]
	public WeaponData weaponData { get; set; }

	[Net]
	public TimeSince lastWeaponSpawn { get; set; }

	[Net]
	public bool taken { get; set; } = false;

	public WorldWeaponSpawn()
	{

	}

	public WorldWeaponSpawn(string weaponName)
	{
		weaponData = Weapon.LoadWeaponInfo(weaponName);
	}

	public override void Spawn()
	{
		base.Spawn();
		SpawnWeapon();
	}

	public void SpawnWeapon()
	{
		var weaponSpawn = new DroppedWeapon();
		weaponSpawn.Position = Position;
		weaponSpawn.Rotation = Rotation;
		weaponSpawn.SetData( weaponData );
		weaponSpawn.spawn = this;
		if ( weaponData != null )
		{
			weaponSpawn.ammoInClip = weaponData.clipSize;
			weaponSpawn.ammoInReserve = weaponData.maxReserveAmmo;
		}
	}

	[GameEvent.Tick]
	public void Tick()
	{
		if ( Sandbox.Game.IsClient ) return;
		if (taken)
		{
			if (lastWeaponSpawn.Relative > 10.0f)
			{
				taken = false;
				SpawnWeapon();
			}
		}
	}

	public void WeaponTaken()
	{
		taken = true;
		lastWeaponSpawn = 0;
		Log.Info( "taken" );
	}
}
