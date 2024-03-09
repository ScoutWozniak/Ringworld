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

		if (GetInputFire())
		{
			FireEffects();
			var startPos = Scene.Camera.Transform.Position;
			var endPos = startPos + Scene.Camera.Transform.Rotation.Forward * 1000.0f;
			var tr = Scene.Trace.Ray( startPos, endPos ).IgnoreGameObjectHierarchy(GameObject.Root).UseHitboxes().UsePhysicsWorld().Run();
			if (tr.Hit)
			{
				if (tr.GameObject.Components.TryGet<HealthComponent>(out var health))
				{
					health.TakeDamage( 10.0f );
				}
			}
		}
	}

	bool GetInputFire()
	{
		switch ( FireType ) {
			case FireTypes.SemiAuto:
				return Input.Pressed( "attack1" );
			case FireTypes.FullAuto:
				return Input.Pressed( "down" );
		}
		return false;
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
		fireSoundHandle = Sound.Play( "pistolfire", MuzzlePoint.Transform.Position );
	}
}
