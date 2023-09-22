using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame;

public partial class BulletManager
{
	public const float InchesPerMeter = 39.3701f;
	public const float AirDensity = 2.007e-5f; //In kilograms per inch cubed.
	public const float TargetGravity = 9.8f * InchesPerMeter;

	public static BulletManager Instance { get; set; }

	private HashSet<Bullet> bullets = new();

	public BulletManager()
	{
		Instance = this;
		Event.Register( this );
	}

	~BulletManager()
	{
		Event.Unregister( this );
	}

	[GameEvent.Tick.Server]
	private void ServerUpdate()
	{
		bullets.RemoveWhere( x => x.Update() || x.ShouldRemove() );
	}

	[GameEvent.Client.Frame]
	private void FrameUpdate()
	{
		bullets.RemoveWhere( x => x.Update() || x.ShouldRemove() );
	}

	public void AddBullet( Bullet bullet )
	{
		bullets.Add( bullet );
	}

	[ClientRpc]
	public static void ReplicateBullet( Vector3 position, Vector3 direction, float speed, Entity owner, Entity weapon, float force, float damage )
	{
		var bullet = Bullet.Create( position, direction, speed, owner, weapon, force, damage );
		Instance.AddBullet( bullet );
	}
}
