﻿
using Sandbox;
using System;
using System.Linq;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace MyGame;

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
			Game.RootPanel = new Hud();
		}
	}

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


}

