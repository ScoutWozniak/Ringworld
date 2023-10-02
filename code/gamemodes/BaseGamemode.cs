using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ringworld;

public partial class BaseGamemode : BaseNetworkable
{
	
	[ConVar.Replicated( "gm_teamgame" )] public static bool teamGame { get; set; }
	[ConVar.Replicated( "gm_maxpoints" )] public static int maxPoints { get; set; }

	public string gameModeName = "";

	public bool isTeamGame = false;

	public List<Pawn> blueTeam = new List<Pawn>();
	public List<Pawn> redTeam = new List<Pawn>();
	public List<Pawn> noTeam = new List<Pawn>();

	[Net]
	public int blueScore { get; set; } = 0;
	[Net]
	public int redScore { get; set; } = 0;

	public virtual MyGame game { get; set; }

	public virtual void GameStart() {}

	public virtual void GameEnd() {}

	public virtual void OnPlayerJoin(Pawn pawn) {
		if (isTeamGame)
		{
			if (blueTeam.Count() > redTeam.Count())
			{
				pawn.SetupTeam( ((int)Teams.RED) );
				redTeam.Add( pawn );
			}
			else
			{
				pawn.SetupTeam( ((int)Teams.BLUE) );
				blueTeam.Add( pawn );
			}
		}
		else
		{
			pawn.SetupTeam((int)Teams.FFA);
			noTeam.Add( pawn );
		}
	}

	public virtual void PlayerRespawn(Pawn pawn) {
		pawn.Transform = FindSpawnPoint();
	}

	public virtual void ScorePoint(Pawn pawn, int count) {
		if ( isTeamGame )
		{
			if ( pawn.teamID == (int)Teams.RED ) redScore += count;
			if ( pawn.teamID == (int)Teams.BLUE ) blueScore += count;
		}
		else pawn.Client.AddInt( "points", count );
	}

	public virtual void Tick() {}

	public virtual void PlayerDeath(Pawn pawn, DamageInfo info) { }

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

	public enum Teams
	{
		FFA = 0,
		BLUE = 1,
		RED = 2,
	}
}
