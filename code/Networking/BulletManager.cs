using Sandbox;
using System.Collections.Generic;

public sealed class BulletManager : Component
{
	public static BulletManager Instance { get; private set; }
	List<BulletProjectile> Bullets { get; set; }

	[Property] GameObject BulletDecal { get; set; }
	protected override void OnStart()
	{
		base.OnStart();
		Instance = this;
		Bullets = new List<BulletProjectile>();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( IsProxy )
			return;

		var tempList = Bullets;
		for ( int i = 0; i < Bullets.Count; i++ )
		{
		
			var startPos = Bullets[i].Position;
			var endPos = startPos + Bullets[i].Forward * Bullets[i].Speed;
			var tr = Scene.Trace.Ray( startPos, endPos ).Radius( 1.0f ).IgnoreGameObjectHierarchy( Bullets[i].Owner ).WithoutTags( "playercol", "ragdoll" ).UseHitboxes().UsePhysicsWorld().Run();
			if ( tr.Hit )
			{
				if ( tr.GameObject.Components.TryGet<HealthComponent>( out var health ) )
				{
					health.TakeDamage( Bullets[i].Damage );
				}

				// Broken?!?
				if ( tr.GameObject.Components.TryGet<Rigidbody>( out var rb ) )
				{
					rb.ApplyImpulseAt( tr.GameObject.Transform.Position, -Vector3.Up * 1000.0f );
				}
				//SurfaceHitEffects( tr.Surface, tr.HitPosition );
				//GameObject.Destroy();
				tempList.Remove( Bullets[i] );
				if (tr.Hitbox == null && tr.Body.BodyType == PhysicsBodyType.Static)
					BulletEffects( tr.HitPosition, tr.Normal );
				
			}
			else
			{
				var tempBullet = Bullets[i];
				tempBullet.Position = endPos;
				Bullets[i] = tempBullet;
			}
		}
		Bullets = tempList;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !Game.IsEditor )
			return;

		foreach(var bullet in Bullets)
		{
			Gizmo.Draw.LineSphere( bullet.Position, 8.0f );
		}
	}

	[Broadcast] 
	public void BulletEffects(Vector3 pos, Vector3 normal)
	{
		
		var go = BulletDecal.Clone();
		go.Transform.Position = pos + normal * 4.0f;
		go.Transform.Rotation = Rotation.LookAt( -normal );
	}

	///<summary>
	///Spawns a bullet that gets sent to the host.  Helps avoid having too many networked objects that take up perf
	///</summary>
	[Broadcast]
	public void SpawnBullet(BulletProjectile bullet)
	{
		Bullets.Add(bullet);
	}

}

public struct BulletProjectile
{
	public BulletProjectile(Vector3 pos, Vector3 forward, float speed, float damage, GameObject owner)
	{
		Position = pos;
		Forward = forward;
		Speed = speed;
		Damage = damage;
		Owner = owner;
	}

	public Vector3 Position { get; set; }
	public Vector3 Forward { get;set; }
	public float Speed { get; set; }

	public float Damage { get; set; }

	public GameObject Owner { get; set; }
}
