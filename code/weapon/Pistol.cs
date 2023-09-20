using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon
{
	public override float PrimaryRate => 4.0f;


	public override void Spawn()
	{
		LoadWeaponInfo( "pistol" );
		MaxAmmo = 10;
		base.Spawn();
		primaryDamage = 10.0f;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		LoadWeaponInfo( "pistol" );
	}
}
