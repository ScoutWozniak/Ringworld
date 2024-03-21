using Sandbox;
using Sandbox.Citizen;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : Component, IRespawnReset
{
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	public Vector3 WishVelocity { get; private set; }

	[Property] public GameObject Body { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }

	[Sync]
	public Angles EyeAngles { get; set; }

	[Sync]
	public bool IsRunning { get; set; }

	[Sync] bool IsDucking { get; set; }

	TimeSince SinceLastGrounded { get; set; }
	bool LastGroundState { get; set; }

	[Property] float MaxCyoteTime { get; set; } = 1.0f;

	public static PlayerController Instance { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( IsProxy )
			return;

		var cam = Scene.Camera;
		if ( cam is not null )
		{
			var ee = cam.Transform.Rotation.Angles();
			ee.roll = 0;
			EyeAngles = ee;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Tags.Add( GameObject.Name );
		var cc = Components.Get<CharacterController>();
		cc.IgnoreLayers.Add(GameObject.Name);
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			Body.Components.Get<SkinnedModelRenderer>().RenderType = ModelRenderer.ShadowRenderType.On;
		else
			Body.Components.Get<SkinnedModelRenderer>().RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

		// Eye input
		if ( !IsProxy )
		{
			//Body.Enabled = false;
			var ee = EyeAngles;
			ee += Input.AnalogLook * 0.5f;
			ee.roll = 0;
			ee.pitch = Math.Clamp( ee.pitch, -89.0f, 89.0f );
			EyeAngles = ee;
		

			var cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

			var lookDir = EyeAngles.ToRotation();

			cam.Transform.Position = Transform.Position + Transform.Rotation.Up * GetDuckHeight();
			cam.Transform.Rotation = lookDir;

			IsRunning = Input.Down( "Run" );
			IsDucking = Input.Down( "Duck" );
		}

		var cc = GameObject.Components.Get<CharacterController>();
		if ( cc is null ) return;

		float rotateDifference = 0;

		// rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 10.0f );

		}


		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithVelocity( cc.Velocity );
			AnimationHelper.WithWishVelocity( WishVelocity );
			AnimationHelper.IsGrounded = cc.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 1, 1.0f );
			AnimationHelper.MoveStyle = IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
			AnimationHelper.DuckLevel = IsDucking ? 0.75f : 0.0f;
		}
	}

	[Broadcast]
	public void OnJump()
	{
		AnimationHelper?.TriggerJump();
	}

	float fJumps;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		Instance = this;

		BuildWishVelocity();

		var cc = GameObject.Components.Get<CharacterController>();

		cc.Height = GetDuckHeight() + 8.0f;

		if ( (cc.IsOnGround || SinceLastGrounded < MaxCyoteTime) && Input.Pressed( "Jump" ) && fJumps == 0 )
		{
			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.5f;
			//if ( Duck.IsActive )
			//	flMul *= 0.8f;

			//cc.Velocity += ( Vector3.Up * flMul * flGroundFactor );
			//cc.GroundObject = null;
			cc.Velocity = cc.Velocity.WithZ(flMul * flGroundFactor);
			cc.IsOnGround = false;

			OnJump();

			fJumps += 1.0f;

		}

		if ( cc.IsOnGround )
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			cc.Accelerate( WishVelocity );
			cc.ApplyFriction( 4.0f );
		}
		else
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			cc.Accelerate( WishVelocity.ClampLength( 50 ) );
			cc.ApplyFriction( 0.1f );
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity -= Gravity * Time.Delta * 0.5f;
			if ( LastGroundState == true )
				SinceLastGrounded = 0;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
			fJumps = 0.0f;
		}
		LastGroundState = cc.IsOnGround;
	}

	public void BuildWishVelocity()
	{
		var rot = EyeAngles.ToRotation();

		WishVelocity = rot * Input.AnalogMove;
		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		WishVelocity *= GetMoveSpeed();
	}

	float GetDuckHeight()
	{
		if ( IsDucking )
			return 32.0f;
		else
			return 64.0f;
	}

	float GetMoveSpeed()
	{
		if ( IsDucking ) return 120.0f;
		if ( IsRunning ) return 200.0f;
		return 285.0f;
	}

	public void RespawnReset()
	{
		Log.Info( "test" );
		var cc = Components.Get<CharacterController>();
		cc.Velocity = Vector3.Zero;
	}
}
