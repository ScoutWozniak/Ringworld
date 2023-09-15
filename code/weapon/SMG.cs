using Sandbox;

namespace MyGame;

public partial class SMG : Weapon
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
		var attack_hold = Input.Down( "attack1" ) ? 1.0f : 0.0f;
		(Owner as AnimatedEntity)?.SetAnimParameter( "attack_hold", attack_hold );
		ViewModelEntity?.SetAnimParameter( "attack_hold", attack_hold );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "b_attack", true );
		Pawn.PlaySound( "riflefire" );
	}

	public override void PrimaryAttack()
	{
		
		if (Game.IsServer)
			ShootEffects();
		
		if (IsAiming) 
			ShootBullet( ADSSpread, 100, 20, 10.0f );
		else
			ShootBullet( Spread, 100, 20, 10.0f );
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
