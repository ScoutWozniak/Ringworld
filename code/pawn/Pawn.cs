using Sandbox;
using System.ComponentModel;
using System.Linq;
using System;
using System.Numerics;

namespace Ringworld;

public partial class Pawn : AnimatedEntity, ITeam
{
	[Net, Predicted]
	public Weapon ActiveWeapon { get; set; }

	[Net]
	public Weapon weapon1 { get; set; }
	[Net]
	public Weapon weapon2 { get; set; }

	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	public float fov = 90.0f;
	public float fovZoomMult = 1.0f;
	float curFov = 90.0f;

	private DamageInfo lastDamage;

	[Net, Predicted]
	public TimeSince lastHurtTime {get; set; }

	[Net, Predicted]
	public float Shields { get; set; } = 100.0f;

	[Net]
	float nextRespawn { set; get; }

	[Net]
	public int currentFragGrenades { get; set; } = 2;

	[Net]
	public int teamValue { get; set; } = 0;

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( true )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }
	[BindComponent] public PawnTeam team { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, new Vector3( -10, -10, 0 ), new Vector3( 10, 10, 72 ) );
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		if ( weapon != null )
		{
			ActiveWeapon?.OnHolster();
			ActiveWeapon = weapon;
			ActiveWeapon.OnEquip( this );
		}
	}

	public void Respawn()
	{
		Components.RemoveAll();
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();

		weapon1?.Delete();
		weapon2?.Delete();

		// We move the majority of respawn code to the gamemode so we can customise more
		var game = MyGame.Current as MyGame;
		
		game.gameMode.PlayerRespawn( this );


		UsePhysicsCollision = true;
		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHitboxes = true;

		Tags.Add( "player" );

		LifeState = LifeState.Alive;

		//GivePropPusher();

		Input.ClearActions();

		currentFragGrenades = 2;
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		if ( LifeState != LifeState.Dead )
		{
			Controller?.Simulate( cl );
			Animator?.Simulate();
			ActiveWeapon?.Simulate( cl );

			if ( Input.Pressed( "slot1" ) )
			{
				if ( ActiveWeapon == weapon1 )
					SetActiveWeapon( weapon2 );
				else
					SetActiveWeapon( weapon1 );
			}

			if ( Sandbox.Game.IsClient)
				Input.GetBindingForButton( "grenade" );
			if ( Input.Pressed( "grenade" ) && currentFragGrenades > 0 )
			{
				if ( Sandbox.Game.IsServer )
				{
					ThrowGrenade();
					currentFragGrenades--;
				}

			}

			if ( lastHurtTime > 10.0f && Shields != 100.0f )
			{
				Shields = MathX.Lerp( Shields, 100.0f, 0.05f );
			}

			TickPlayerUse();

			
		}
		else
		{
			if ( Time.Now > nextRespawn)
			{
				Respawn();
			}
		}
		EyeLocalPosition = Vector3.Up * (Controller.ducking ? 52.0f : 64.0f * Scale);
	}

	public override void BuildInput()
	{
		base.BuildInput();
		InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	bool IsThirdPerson { get; set; } = false;

	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		curFov = MathX.Lerp( curFov, fov / fovZoomMult, 0.05f );

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( curFov );

		if ( Input.Pressed( "view" ) )
		{
			//IsThirdPerson = !IsThirdPerson;
		}

		if ( IsThirdPerson )
		{
			Vector3 targetPos;
			var pos = Position + Vector3.Up * 64;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 80.0f * Scale;
			targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();
			
			Camera.FirstPersonViewer = null;
			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}

	}



	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( (info.Attacker as Pawn) != null && (info.Attacker as Pawn).teamID == teamID && teamID != 0 && !(info.Attacker == this)) return;

		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( Shields <= 0 )
		{
			if ( isHeadshot )
			{
				info.Damage *= 10.0f;
			}
			Health -= info.Damage;
			
			if ( Health <= 0 )
			{
				OnKilledInfo( info );
			}

		}
		else
		{
			var leftOverDamage = -(Shields - info.Damage);
			Shields -= info.Damage;
			if ( Shields <= 0 )
			{
				OnShieldBreak();
				if ( isHeadshot )
				{
					leftOverDamage *= 10.0f;
				}
				info.Damage = leftOverDamage;
				Health -= info.Damage;
				if ( Health <= 0 )
				{
					OnKilledInfo( info );
				}
			}
		}
		lastHurtTime = 0;
	}

	public void OnShieldBreak()
	{
		var particles = Particles.Create( Cloud.ParticleSystem( "garry.spark_burst" ).Name.ToString());
		particles?.SetPosition( 0, Position );
		particles?.Destroy();
	}

	public void OnKilledInfo(DamageInfo info)
	{
		//base.OnKilled();

		if ( Sandbox.Game.IsServer)
			BecomeRagdollOnClient( lastDamage.Force, lastDamage.BoneIndex );

		EnableAllCollisions = false;
		EnableDrawing = false;

		nextRespawn = Time.Now + 5.0f;
		LifeState = LifeState.Dead;

		ActiveWeapon?.OnDeath();

		if ( weapon1 != null ) LaunchWeapon( weapon1 );
		if ( weapon2 != null ) LaunchWeapon( weapon2 );

		for ( int i = 0; i < currentFragGrenades; i++ )
		{
			LaunchGrenade();
		}
		currentFragGrenades = 0;

		var game = MyGame.Current as MyGame;
		game.gameMode.PlayerDeath( this, info );
	}

	public void GivePropPusher()
	{
		Pawn_PropPusher propPusher = new Pawn_PropPusher();
		propPusher.Position = Position;
		propPusher.Parent = this;
	}

	public void AddWeapon( Weapon weapon )
	{

		if ( weapon1 == null || weapon2 == null )
		{
			if ( weapon1 == null )
			{
				weapon1 = weapon;
			}
			else if ( weapon2 == null )
			{
				weapon2 = weapon;
				
			}
			SetActiveWeapon( weapon );
		}
		else
		{
			if (ActiveWeapon == weapon1)
			{
				LaunchWeapon( weapon1 );
				weapon1 = weapon;
			}
			else
			{
				LaunchWeapon( weapon2 );
				weapon2 = weapon;
			}
			SetActiveWeapon( weapon );
		}
	}

	void LaunchWeapon(Weapon weapon)
	{
		DroppedWeapon oldSpawn = new DroppedWeapon();
		oldSpawn.SetData( weapon.weaponInfo );
		oldSpawn.Position = AimRay.Position;
		oldSpawn.Velocity = AimRay.Forward * 10.0f;
		oldSpawn.ammoInClip = weapon.ammoInClip;
		oldSpawn.ammoInReserve = weapon.reserveAmmo;
		oldSpawn.DeleteAsync( 30.0f );
		weapon?.Delete();
	}

	void LaunchGrenade()
	{
		DroppedGrenade droppedGrenade = new DroppedGrenade();
		droppedGrenade.Position = AimRay.Position + AimRay.Forward * 10.0f;
		droppedGrenade.DeleteAsync( 30.0f );
	}

	void ThrowGrenade()
	{
		var grenade = new FragGrenade();
		grenade.Position = AimRay.Position;
		grenade.Velocity = AimRay.Forward * 750.0f;
		grenade.Velocity += Vector3.Up * 100.0f;
		grenade.Owner = this;
	}

	public Weapon CheckDuplicateWeapon(WeaponData weapon)
	{
		if ( (weapon1 != null && weapon1.weaponInfo.weaponName == weapon.weaponName) ) return weapon1;
		if ( (weapon2 != null && weapon2.weaponInfo.weaponName == weapon.weaponName) ) return weapon2;
		return null;
	}

	[Net]
	public int teamID { get; set; }
	public string teamName { get; set; }

	public void SetupTeam( int id ) {
		Log.Info( "test" );
		teamID = id;
		if (id == 1)
		{
			RenderColor = Color.Blue;
		}
		else if (id == 2)
		{
			RenderColor = Color.Red;
		}
	}
}

public class Pawn_PropPusher : AnimatedEntity
{
	public override void Spawn()
	{
		

		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = false;
		EnableTraceAndQueries = false;

		Tags.Add( "player" );
	}
}
