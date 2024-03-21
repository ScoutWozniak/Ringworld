using Sandbox;
using System.Linq;

public sealed class ProjectileComponent : Component
{
	[Property] float Radius { get; set; } = 8.0f;
	[Property] float Speed { get; set; } = 10.0f;
	[Property] float Damage { get; set; }

	public GameObject OwnerIgnore { get; set; }
	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		var startPos = Transform.Position;
		var endPos = startPos + Transform.Rotation.Forward * Speed;
		var tr = Scene.Trace.Ray( startPos, endPos ).Radius(Radius).IgnoreGameObjectHierarchy(OwnerIgnore).WithoutTags("playercol", "ragdoll").UseHitboxes().UsePhysicsWorld().Run(); 
		if (tr.Hit)
		{
			if (tr.GameObject.Components.TryGet<HealthComponent>(out var health))
			{
				health.TakeDamage( Damage );
			}

			// Broken?!?
			if(tr.GameObject.Components.TryGet<Rigidbody>(out var rb))
			{
				rb.ApplyImpulseAt( tr.GameObject.Transform.Position, -Vector3.Up * 1000.0f );
			}

			SurfaceHitEffects( tr.Surface, tr.HitPosition );
			GameObject.Destroy();
		}

		Transform.Position = endPos;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		Gizmo.Draw.Arrow( Vector3.Zero, Vector3.Forward * 10.0f );
	}

	[Broadcast]
	public void SurfaceHitEffects( Surface surface, Vector3 hitPos )
	{
		Sound.Play( surface.Sounds.Bullet, hitPos );
	}
}
