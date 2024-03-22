using Sandbox;

public enum FireTypes
{
	SemiAuto,
	FullAuto
}

public sealed class WeaponBase : Component
{
	[Property] FireTypes FireType { get; set; }
	[Property] WeaponAmmoBase WeaponAmmo { get; set; }

	[Category("Visual")]
	[Property] SkinnedModelRenderer ViewModel { get; set; }

	[Category( "Visual" )]
	[Property] GameObject MuzzlePoint { get; set; }

	SoundHandle fireSoundHandle { get;set; }

	[Property] GameObject WeaponProjectile { get; set; }

	[Property] float FireRate { get; set; } = 0.1f;

	// Set it to a large value imediately so we're good to fire at the start
	TimeSince LastFire = 999.0f;

	protected override void OnUpdate()
	{
		if ( fireSoundHandle != null )
		{ 
			if ( !fireSoundHandle.IsStopped )
				fireSoundHandle.Position = MuzzlePoint.Transform.Position;
			else
				fireSoundHandle = null;
		}

		if ( IsProxy )
			return;
		 
		if (CanFire())
		{
			FireEffects();
			/*			var proj = WeaponProjectile.Clone( MuzzlePoint.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 8.0f, Scene.Camera.Transform.Rotation );
						proj.Components.Get<ProjectileComponent>().OwnerIgnore = GameObject.Root;
						proj.NetworkSpawn();*/
			Log.Info( MuzzlePoint.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 8.0f );
			BulletManager.Instance?.SpawnBullet( new( MuzzlePoint.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 8.0f, 
				Scene.Camera.Transform.Rotation.Forward, 100.0f, 10.0f, PlayerController.Instance.GameObject ) );
			LastFire = 0;
		}
	}
	 
	bool GetInputFire()
	{
		switch ( FireType ) {
			case FireTypes.SemiAuto:
				return Input.Pressed( "attack1" );
			case FireTypes.FullAuto:
				return Input.Down( "attack1" );
		}
		return false;
	}

	bool CanFire()
	{
		return GetInputFire() && LastFire > FireRate;
	}

	[Broadcast]
	public void FireEffects()
	{
		if ( IsProxy )
		{
			// Fire effects from third person
		}
		else
		{
			// Fire effects from first person
			ViewModel.Set( "b_attack", true );
		}
		if (fireSoundHandle != null && fireSoundHandle.IsValid)
		{
			fireSoundHandle.Stop(1f);
		}
		fireSoundHandle = Sound.Play( "pistolfire", MuzzlePoint.Transform.Position );
	}

	
}
