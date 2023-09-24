using Sandbox;
using System;
using System.Linq;

namespace MyGame;

public class BaseGamemode : BaseNetworkable
{

	public string gameModeName = "";

	public virtual MyGame game { get; set; }

	public virtual void GameStart() {}

	public virtual void GameEnd() {}

	public virtual void PlayerRespawn(Pawn pawn) {
		pawn.Transform = FindSpawnPoint();
	}

	public virtual void ScorePoint(Pawn pawn, int count) {}

	public virtual void Tick() {}

	public virtual void PlayerDeath(Pawn pawn, DamageInfo info) {}

	public virtual Transform FindSpawnPoint()
	{
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 10.0f; // raise it up
			return tx;
		}

		return Transform.Zero;
	}
}
