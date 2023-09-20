using Sandbox;

namespace MyGame;

public partial class SMG : Weapon
{

	public override void Spawn()
	{
		LoadWeaponInfo( "rifle" );
		base.Spawn();
	}

	public override void ClientSpawn()
	{
		LoadWeaponInfo( "rifle" );
		base.ClientSpawn();
	}
}
