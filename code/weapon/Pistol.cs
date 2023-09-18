using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon
{
	public override float PrimaryRate => 4.0f;


	public override void Spawn()
	{
		LoadWeaponInfo( "pistol" );
		Log.Info( weaponInfo );
		MaxAmmo = 10;
		base.Spawn();
		primaryDamage = 10.0f;
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "b_attack", true );
		Pawn.PlaySound( "pistol.fire" );
	}

	public override bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Pressed( "attack1" ) || deploying || reloading ) return false;

		if ( ammoInClip <= 0 ) return false;

		var rate = weaponInfo?.primaryFireRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public override void PrimaryAttack()
	{

		if ( Game.IsServer )
			ShootEffects();

		ShootBullet( 0.005f, 100, primaryDamage, 10.0f );

		ammoInClip -= 1;
	}

	public override void SecondaryAttack()
	{
		Pawn.fovZoomMult = 2.0f;
		IsAiming = true;
		if ( Game.IsClient )
			ViewModelEntity.EnableDrawing = false;
	}

	public override void SecondaryAttackRelease()
	{
		Pawn.fovZoomMult = 1.0f;
		IsAiming = false;
		if (Game.IsClient)
			ViewModelEntity.EnableDrawing = true;
	}

	[ClientRpc]
	public override void CreateViewModel()
	{
		Game.AssertClient();

		var vm = new WeaponViewModel();
		vm.Model = Cloud.Model( "facepunch.v_usp" );
		vm.Owner = Owner;
		ViewModelEntity = vm;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms_citizen_4fingers.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;
	}
}
