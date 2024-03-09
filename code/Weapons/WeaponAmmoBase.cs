using Sandbox;

public class WeaponAmmoBase : Component
{
	[Property][Category( "Ammo" )] public int MaxClip { get; set; } = 16;
	[Property]
	[Category( "Ammo" )] public int AmmoPerFire { get; set; } = 1;
	public int CurrentClip { get; set; }

	[Property] bool OverrideAmmo { get; set; }
	[ShowIf( "OverrideAmmo", true)]
	[Property] int OverrideStartingAmmo { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if ( !OverrideAmmo )
			CurrentClip = MaxClip;
		else
			CurrentClip = OverrideStartingAmmo;
	}

	public virtual void OnFire()
	{
		CurrentClip -= AmmoPerFire;
	}
}
