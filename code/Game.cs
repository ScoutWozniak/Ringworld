
using Sandbox;
using System;
using System.Linq;
using System.Threading;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Ringworld;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.GameManager
{
	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public MyGame()
	{
		if ( Game.IsClient )
		{
			Sandbox.Game.RootPanel = new Hud();
		}

		if ( Game.IsServer)
		{
			cancellationTokenSource = new CancellationTokenSource();
			_ = GameLoopAsync( cancellationTokenSource.Token);

			if ( Game.Server.MapIdent == "sugmagaming.lockout" )
			{
				Vector3[] positions = { new Vector3( 910.29f, -1800.74f, 7731.85f ), new Vector3( 912.90f, - 1841.59f, 7273.20f ), new Vector3( 534.68f, - 2025.41f, 7908.40f ) };
				var weaponSpawn = new WorldWeaponSpawn( "pistol");
				weaponSpawn.Position = positions[0];
				weaponSpawn = new WorldWeaponSpawn( "shotgun" );
				weaponSpawn.Position = positions[1];
				weaponSpawn = new WorldWeaponSpawn( "sniper" );
				weaponSpawn.Position = positions[2];
			}

		}

		base.Spawn();
		gameMode = new Slayer();
		gameMode.game = this;




		_ = new BulletManager();
	}

	public CancellationTokenSource cancellationTokenSource;

	[Net]
	public TimeSince gameStateTimer { get; set; }

	public int MaxKills = 10;

	[Net]
	public BaseGamemode gameMode { get; set; }

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Pawn();
		client.Pawn = pawn;
		pawn.Respawn();
		//pawn.DressFromClient( client );
		//pawn.team.SetupTeam(0);
		gameMode.OnPlayerJoin(pawn );
	}

	[ConCmd.Admin("kill")]
	public static void KillCmd()
	{
		if ( ConsoleSystem.Caller.Pawn is Pawn player )
			player.TakeDamage( new DamageInfo { Damage = 1000.0f } );
	}

	[ConCmd.Admin("hurt")]
	public static void HurtCmd()
	{
		if ( ConsoleSystem.Caller.Pawn is Pawn player )
			player.TakeDamage( new DamageInfo { Damage = 10.0f } );
	}

	[ConCmd.Admin( "setkill" )]
	public static void AddKill(int killNum)
	{
		ConsoleSystem.Caller.Client.SetInt( "points", killNum);
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		if ( Game.IsClient ) return;
		gameMode.Tick();
	}

	public override void Spawn()
	{
		
	}

	public static IClient GetHighestScore(IClient toIgnore)
	{
		IClient highestClient = null;
		foreach (var client in Sandbox.Game.Clients)
		{
			if ( highestClient != null)
			{
				if ( client.GetInt( "points" ) > highestClient.GetInt( "points" ) && client != toIgnore )
					highestClient = client;
			}
			else
			{
				highestClient = client;
			}
		}
		return highestClient;
	}


}

