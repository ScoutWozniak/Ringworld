using Sandbox;
using System.Collections.Generic;
using System.Numerics;

namespace MyGame;

public partial class Weapon : AnimatedEntity
{
	/// <summary>
	/// The View Model's entity, only accessible clientside.
	/// </summary>
	public WeaponViewModel ViewModelEntity { get; protected set; }

	/// <summary>
	/// An accessor to grab our Pawn.
	/// </summary>
	public Pawn Pawn => Owner as Pawn;

	/// <summary>
	/// This'll decide which entity to fire effects from. If we're in first person, the View Model, otherwise, this.
	/// </summary>
	public AnimatedEntity EffectEntity => Camera.FirstPersonViewer == Owner ? ViewModelEntity : this;

	public string ViewModelPath;
	public virtual string ModelPath => null;

	/// <summary>
	/// How often you can shoot this gun.
	/// </summary>
	public virtual float PrimaryRate => 5.0f;
	public virtual float SecondaryRate => 1.0f;

	/// <summary>
	/// How long since we last shot this gun.
	/// </summary>
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }

	[Net]
	public bool IsAiming { set; get; } = false;


	public float primaryDamage = 1.0f;

	[Net, Predicted] public TimeSince deployTime { get; set; }

	[Net]
	public bool deploying { get; set; }

	public float deployDelay = 1.0f;

	[Net,Predicted]
	public int ammoInClip { get; set; }

	public int MaxAmmo;

	[Net, Predicted]
	public TimeSince reloadTime { get; set; } = 0;
	[Net]
	public bool reloading { get; set; } = false;

	public float reloadLength = 1.0f;

	[Net]
	public WeaponData weaponInfo { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;

		Model = Cloud.Model( "facepunch.w_mp5" );

		ammoInClip = weaponInfo.clipSize;
	}

	public void LoadWeaponInfo( string weaponName )
	{
		if ( ResourceLibrary.TryGet<WeaponData>( "data/weapons/" + weaponName + ".weapon", out var data ) )
			weaponInfo = data;
		else if ( ResourceLibrary.TryGet<WeaponData>( "/data/weapons/" + weaponName + ".weapon", out var data2 ) )
			weaponInfo = data2;

	}

	/// <summary>
	/// Called when <see cref="Pawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
	/// </summary>
	/// <param name="pawn"></param>
	public void OnEquip( Pawn pawn )
	{
		Owner = pawn;
		SetParent( pawn, true );
		EnableDrawing = true;
		if ( Game.IsServer )
			CreateViewModel( To.Single( Owner ) );

		deploying = true;
		deployTime = 0;
	}

	/// <summary>
	/// Called when the weapon is either removed from the player, or holstered.
	/// </summary>
	public void OnHolster()
	{
		EnableDrawing = false;
		Delete();
		if ( Game.IsServer )
			DestroyViewModel( To.Single( Owner ) );
	}

	/// <summary>
	/// Called from <see cref="Pawn.Simulate(IClient)"/>.
	/// </summary>
	/// <param name="player"></param>
	public override void Simulate( IClient player )
	{
		Animate();

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				TimeSincePrimaryAttack = 0;
				PrimaryAttack();
			}
		}

		if ( Input.Down("attack2") ) {
			SecondaryAttack();
		}
		else if (Input.Released("attack2"))
		{
			SecondaryAttackRelease();
		}

		if (deploying )
		{
			if ( weaponInfo?.deployTime <  deployTime)
				deploying = false;
		}

		if ( Input.Pressed( "reload" ))
		{
			Reload();
		}

		if ( reloading )
			if ( reloadTime > weaponInfo?.reloadLength )
				reloading = false;

	}

	/// <summary>
	/// Called every <see cref="Simulate(IClient)"/> to see if we can shoot our gun.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) || deploying || reloading) return false;

		if ( ammoInClip <= 0 ) return false;

		var rate = weaponInfo?.primaryFireRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Owner.IsValid() || (!Input.Down( "attack2" ) || !Input.Released( "attack2" )) ) return false;

		return true;
	}

	/// <summary>
	/// Called when your gun shoots.
	/// </summary>
	public virtual void PrimaryAttack()
	{
	}

	public virtual void SecondaryAttack()
	{
	}

	public virtual void SecondaryAttackRelease()
	{
	}

	public virtual void Reload()
	{
		reloadTime = 0;
		reloading = true;
		ViewModelEntity?.SetAnimParameter( "b_reload", true );
		if (weaponInfo != null)
			ammoInClip = weaponInfo.clipSize;
	}

	/// <summary>
	/// Useful for setting anim parameters based off the current weapon.
	/// </summary>
	protected virtual void Animate()
	{
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocheting or something.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach (var tr in TraceBullet( pos, pos + forward * 1000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
	
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Game.SetRandomSeed( Time.Tick );

		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}

	[ClientRpc]
	public virtual void CreateViewModel()
	{
		Game.AssertClient();

		var vm = new WeaponViewModel();
		vm.Model = Cloud.Model("facepunch.v_mp5");
		vm.Owner = Owner;
		ViewModelEntity = vm;

		var arms = new AnimatedEntity( "models/first_person/first_person_arms_citizen_4fingers.vmdl" );
		arms.SetParent( ViewModelEntity, true );
		arms.EnableViewmodelRendering = true;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		if ( ViewModelEntity.IsValid() )
		{
			ViewModelEntity.Delete();
		}
	}
}
