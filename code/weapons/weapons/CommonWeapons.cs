using Sandbox;

namespace Ringworld;

[Library( "rw_weapon_smg", Title = "SMG" )]
public class SMG : Weapon
{
	public override void PrimaryAttack()
	{
		base.PrimaryAttack();
	}
}

[Library( "rw_weapon_pistol", Title = "Pistol" )]
public class Pistol : Weapon {}

[Library( "rw_weapon_shotgun", Title = "Shotgun" )]
public class Shotgun : Weapon {}
