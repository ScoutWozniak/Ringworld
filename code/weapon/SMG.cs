using Sandbox;

namespace MyGame;

public partial class SMG : Weapon
{
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

	public override float PrimaryRate => 10.0f;

	float Spread => 0.1f;


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
		Pawn.PlaySound( "smg.fire" );
	}

	public override void PrimaryAttack()
	{
		
		if (Game.IsServer)
			ShootEffects();
		

		ShootBullet( Spread, 100, 5, 10.0f );
		ammoInClip -= 1;
	}

	public override void SecondaryAttack()
	{
		Pawn.fovZoomMult = 1.5f;
		IsAiming = true;
		if ( Game.IsClient )
			ViewModelEntity.EnableDrawing = false;
	}

	public override void SecondaryAttackRelease()
	{
		Pawn.fovZoomMult = 1.0f;
		IsAiming = false;
		if ( Game.IsClient )
			ViewModelEntity.EnableDrawing = true;
	}

	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
