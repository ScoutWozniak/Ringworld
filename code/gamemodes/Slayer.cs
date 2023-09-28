using Sandbox;
using System;
using static Ringworld.MyGame;
using System.Threading;

namespace Ringworld;


[Library( "rw_gamemode_slayer", Title = "Slayer" )]
public class Slayer : BaseGamemode
{

	[ConVar.Replicated( "gm_startingweapon" )] public static string startingWeapon { get; set; }

	public Slayer()
	{
		gameModeName = "Slayer";
		isTeamGame = ConsoleSystem.GetValue( "gm_teamgame" ).ToBool();
	}


	public override void GameStart() {
		base.GameStart();
		Sound.FromScreen( "anouncer.slayer" );
		blueScore = 0;
		redScore = 0;
	}

	public override void GameEnd() {
		base.GameEnd();
		Sound.FromScreen( "anouncer.gameover" );
	}

	public override void PlayerRespawn( Pawn pawn ) {
		base.PlayerRespawn( pawn );
		string startingWeapon = ConsoleSystem.GetValue( "gm_startingweapon" );
		Log.Info( startingWeapon );

		var weapon = Weapon.LoadWeaponInfo( startingWeapon ).CreateInstance();
		pawn.weapon1 = weapon;
		pawn.SetActiveWeapon( pawn.weapon1 );
		pawn.Health = 10;
		pawn.Shields = 100.0f;
	}

	public override void ScorePoint( Pawn pawn , int count) {
		base.ScorePoint( pawn , count );
	}

	public override void Tick(){
		base.Tick();
		foreach ( var client in Sandbox.Game.Clients )
		{
			int maxPoints = ConsoleSystem.GetValue( "gm_maxpoints" ).ToInt();
			if ( teamGame )
			{
				if (redScore >= maxPoints || blueScore >= maxPoints && CurrentState == GameStates.Live)
				{
					base.game.cancellationTokenSource.Cancel();
				}
			}
			else
			{
				// Win condition
				if ( client.GetInt( "points" ) >= maxPoints && CurrentState == GameStates.Live )
				{
					// Temp game over, will be changed later
					base.game.cancellationTokenSource.Cancel();
				}
			}
		}
	}

	public override void PlayerDeath( Pawn pawn, DamageInfo info ) {
		if (info.Attacker != null && info.Attacker != pawn)
		{
			Log.Info( pawn.Client.Name + " was killed by " + info.Attacker.Client.Name );
			ScorePoint( info.Attacker as Pawn, 1 );
		}
	}

}
