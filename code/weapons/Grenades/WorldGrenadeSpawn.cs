using Sandbox;
using Editor;
using Ringworld;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[Library( "grenade_spawn" ), HammerEntity]
[Title( "Grenade Spawn" ), Category( "Weapons" ), Icon( "place" )]
public partial class WorldGrenadeSpawn : Entity
{

	[Net]
	public TimeSince lastGrenadeSpawn { get; set; }

	[Net]
	public bool taken { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();
		SpawnGrenade();
	}

	public void SpawnGrenade()
	{
		var weaponSpawn = new DroppedGrenade();
		weaponSpawn.Position = Position;
		weaponSpawn.spawn = this;
	}

	[GameEvent.Tick]
	public void Tick()
	{
		if ( Sandbox.Game.IsClient ) return;
		if ( taken )
		{
			if ( lastGrenadeSpawn.Relative > 10.0f )
			{
				taken = false;
				SpawnGrenade();
			}
		}
	}

	public void GrenadeTaken()
	{
		taken = true;
		lastGrenadeSpawn = 0;
	}
}
