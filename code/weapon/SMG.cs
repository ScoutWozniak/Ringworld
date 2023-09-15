using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon
{
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 10.0f;

	float Spread => 0.3f;
	float ADSSpread => 0.1f;

	[Net]
	public bool IsAiming { set; get; } = false;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void Simulate( IClient player )
	{
		base.Simulate( player );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "b_attack", true );
	}

	public override void PrimaryAttack()
	{
		ShootEffects();
		Pawn.PlaySound( "rust_pistol.shoot" );
		if (IsAiming) 
			ShootBullet( ADSSpread, 100, 20, 1 );
		else
			ShootBullet( Spread, 100, 20, 1 );
	}

	public override void SecondaryAttack()
	{
		base.SecondaryAttack();
		ViewModelEntity?.SetAnimParameter( "ironsights", 2 );
		IsAiming = true;
	}

	public override void SecondaryAttackRelease()
	{
		base.SecondaryAttackRelease();
		ViewModelEntity?.SetAnimParameter( "ironsights", 0 );
		IsAiming = false;
	}

	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
