using Sandbox;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame;

public class Bullet
{
	private float gravScale = 1.0f;
	private float damage;
	private float extraForce;
	private float liveTime = 10.0f;
	private Entity weapon;
	private Entity owner;
	private Particles bulletTracer;
	private Vector3 startPos;
	private Vector3 lastPosition;
	private TimeSince timeSinceCreated = 0;
	private Vector3 particlePosition;
	private bool ignoreOwner;

	private Vector3 velocity;
	private Vector3 position;

	public Bullet()
	{
		var currentGravity = Game.PhysicsWorld.Gravity;
		var gravityRatio = BulletManager.TargetGravity / currentGravity.Length;

		gravScale = gravityRatio;

		if ( Game.IsClient )
		{
			bulletTracer = Particles.Create( "particles/bullettracer.vpcf" );
			bulletTracer.SetPosition( 0, position );
		}

		
	}

	public static Bullet Create( Vector3 position, Vector3 direction, float speed, Entity owner, Entity weapon, float force, float damage, bool ignoreOwner = true )
	{
		var bullet = new Bullet
		{
			position = position,
			particlePosition = position,
			owner = owner,
			velocity = direction * speed + owner.Velocity * 0.7f,
			lastPosition = position,
			weapon = weapon,
			damage = damage,
			extraForce = force,
			ignoreOwner = ignoreOwner
		};


		

		return bullet;
	}
	public static Bullet CreateClientBullet( Vector3 position, Vector3 direction, float speed, Entity owner, Entity weapon, float force, Vector3 particlePosition )
	{
		Game.AssertClient();

		var bullet = new Bullet
		{
			position = position,
			particlePosition = particlePosition,
			owner = owner,
			velocity = direction * speed + owner.Velocity * 0.7f,
			lastPosition = position,
			weapon = weapon,
			damage = 0,
			extraForce = force,
			ignoreOwner = true
		};


		bullet.bulletTracer?.SetPosition( 0, bullet.particlePosition );
		return bullet;
	}



	public bool Update()
	{
		lastPosition = position;

		var dt = Time.Delta;

		//velocity += Game.PhysicsWorld.Gravity * gravScale * dt;

		var drag = BulletManager.AirDensity * velocity.Length * velocity.Length;
		drag /= 2;

		velocity -= velocity.Normal * drag * dt;

		position += velocity * dt;


		//bulletTracer?.SetPosition( 0, position );
		bulletTracer?.SetPosition( 0, velocity );

		return DoTraceCheck();
	}

	private bool DoTraceCheck()
	{
		/*
		var whizTraces = Trace.Ray( lastPosition, position )
			.UseHitboxes()
			.DynamicOnly()
			.Size( 300f )
			.WithTag( "player" )
			.Ignore( ignoreOwner ? owner : null )
			.RunAll();

		if ( whizTraces != null )
		{
			foreach ( var traceResult in whizTraces )
			{
				if ( traceResult.Entity is Player player )
					player.TriggerBulletWhizz( traceResult.EndPosition, velocity.Length, ammo.FlyBySound );
			}
		}
		*/

		var tr = Trace.Ray( lastPosition, position )
			.UseHitboxes()
			.Ignore( ignoreOwner ? owner : null )
			.Ignore( weapon )
			.Size( 3f )
			.WithAnyTags( "player", "solid", "glass" )
			.Run();

		if ( tr.Hit )
		{
			BulletHit( tr );
			return true;
		}

		return false;
	}

	private void BulletHit( TraceResult tr )
	{

		if ( Game.IsClient )
		{
			bulletTracer?.SetPosition( 1, 0 );
			bulletTracer?.Destroy();
			tr.Surface.DoBulletImpact( tr );
			return;
		}

		if ( !tr.Entity.IsValid() )
			return;

		var force = extraForce;

		var damageInfo = new DamageInfo()
			.WithPosition( tr.EndPosition )
			.WithForce( tr.Direction * force )
			.UsingTraceResult( tr )
			.WithAttacker( owner )
			.WithWeapon( weapon );

		damageInfo.Damage = damage;

		using ( Prediction.Off() )
			tr.Entity.TakeDamage( damageInfo );

		//TryPenetration(tr);
	}

	public bool ShouldRemove()
	{
		return timeSinceCreated > liveTime;
	}
}
