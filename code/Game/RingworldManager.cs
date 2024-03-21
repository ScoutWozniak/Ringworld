using Sandbox;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Channels;

public sealed class RingworldManager : Component
{
	public static RingworldManager Instance { get; private set; }
	[Property] bool CanRespawn { get; set; } = true;
	[Property] GameObject PlayerPrefab { get; set; }

	[Property] public List<GameObject> SpawnPoints { get; set; }

	[Property] PlayerStates PlayerState { get; set; }

	[Property] float RespawnTime { get; set; }

	TimeSince SinceDeath { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Instance = this;
		PlayerController.Instance = null;
		RespawnPlayer();
		
	}

	protected override void OnFixedUpdate()
	{
		if ( PlayerState == PlayerStates.DEAD && SinceDeath > RespawnTime )
			CanRespawn = true;

		if (CanRespawn)
		{
			RespawnPlayer();
		}
	}

	public void RespawnPlayer()
	{
		CanRespawn = false;
		if ( PlayerPrefab is null )
			return;

		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation().WithScale( 1 );
		Log.Info( PlayerController.Instance );
		if ( PlayerController.Instance == null )
		{
			// Spawn this object and make the client the owner
			var player = PlayerPrefab.Clone( startLocation );
			player.NetworkSpawn();
		}
		else
		{
			PlayerController.Instance.GameObject.Enabled = true;
			PlayerController.Instance.Transform.Position = startLocation.Position;

			foreach(var reset in PlayerController.Instance.Components.GetAll<IRespawnReset>(FindMode.EverythingInSelfAndDescendants))
			{
				reset.RespawnReset();
			}

			ChangePlayerEnabled( PlayerController.Instance.GameObject, true );
		}
		PlayerState = PlayerStates.ALIVE;
	}

	public void LocalPlayerDie()
	{
		PlayerState = PlayerStates.DEAD;
		Log.Info( "rippp" );
		PlayerController.Instance.GameObject.Enabled = false;
		ChangePlayerEnabled( PlayerController.Instance.GameObject, false );
		SinceDeath = 0;
	}
	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If they have spawn point set then use those
		//
		if ( SpawnPoints is not null && SpawnPoints.Count > 0 )
		{
			return Random.Shared.FromList( SpawnPoints, default ).Transform.World;
		}

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.World;
	}

	[Broadcast] public void ChangePlayerEnabled(GameObject player, bool state)
	{
		if ( !IsProxy )
			return;
		player.Enabled = state;
	}
}

public enum PlayerStates
{
	ALIVE,
	DEAD
} 
