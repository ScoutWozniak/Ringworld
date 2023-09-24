using Sandbox;
using System;
using static MyGame.MyGame;
using System.Threading;

namespace MyGame;

public class Slayer : BaseGamemode
{

	public override void GameStart() {
		base.GameStart();
		Sound.FromScreen( "anouncer.slayer" );
	}

	public override void GameEnd() {
		base.GameEnd();
		Sound.FromScreen( "anouncer.gameover" );
	}

	public override void PlayerRespawn( Pawn pawn ) {
		base.PlayerRespawn( pawn );
		pawn.weapon1 = new Weapon();
		pawn.weapon1?.SetWeaponInfo( Weapon.LoadWeaponInfo( "rifle" ) );
		pawn.SetActiveWeapon( pawn.weapon1 );
		pawn.Health = 10;
		pawn.Shields = 100.0f;
	}

	public override void ScorePoint( Pawn pawn , int count) {
		base.ScorePoint( pawn , count );
		pawn.Client.AddInt( "points", count );
	}

	public override void Tick(){
		base.Tick();
		foreach ( var client in Game.Clients )
		{
			// Win condition
			if ( client.GetInt( "points" ) >= 25 && CurrentState == GameStates.Live )
			{
				// Temp game over, will be changed later
				game.cancellationTokenSource.Cancel();
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
