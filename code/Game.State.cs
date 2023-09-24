using Sandbox;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace MyGame;

partial class MyGame
{
	public static GameStates CurrentState => (Current as MyGame)?.GameState ?? GameStates.Warmup;

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Warmup;
	[Net]
	public string NextMap { get; set; } = "facepunch.datacore";

	public void GameModeRespawn(Pawn pawn)
	{
		if (pawn != null || gameMode != null)
			gameMode.PlayerRespawn(pawn);
	}

	private async Task WaitStateTimer(CancellationToken token, bool ignorecancel = false)
	{
		while ( StateTimer > 0 )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
			if ( token.IsCancellationRequested && !ignorecancel )
			{
				return;
			}
		}

		// extra second for fun
		await Task.DelayRealtimeSeconds( 1.0f );
	}

	private async Task GameLoopAsync( CancellationToken token )
	{

		GameState = GameStates.Warmup;
		Log.Info( GameState );
		StateTimer = 10;
		await WaitStateTimer(token);

		GameState = GameStates.Live;
		Log.Info( GameState );
		gameMode.GameStart();
		StateTimer = 10 * 60;
		FreshStart();
		await WaitStateTimer( token );

		GameState = GameStates.GameEnd;
		Log.Info( GameState );
		StateTimer = 10.0f;
		gameMode.GameEnd();
		await WaitStateTimer( token, true );

		Game.ChangeLevel( Game.Server.MapIdent );
	}

	private bool HasEnoughPlayers()
	{
		if ( All.OfType<Pawn>().Count() < 2 )
			return false;

		return true;
	}

	private void FreshStart()
	{
		foreach ( var cl in Game.Clients )
		{
			cl.SetInt( "points", 0 );
			cl.SetInt( "deaths", 0 );
		}

		All.OfType<Pawn>().ToList().ForEach( x =>
		{
			x.Respawn();
		} );

		foreach (var weapon in All.OfType<DroppedWeapon>())
		{
			weapon.Delete();
		}
		foreach (var spawner in All.OfType<WorldWeaponSpawn>().ToArray())
		{
			spawner.taken = false;
			spawner.SpawnWeapon();
		}

		foreach ( var spawner in All.OfType<Grenade>().ToArray() )
		{
			spawner.Delete();
		}
	}

	public enum GameStates
	{
		Warmup,
		Live,
		GameEnd,
		MapVote
	}
}
