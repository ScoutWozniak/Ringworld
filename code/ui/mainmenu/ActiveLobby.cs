﻿using System;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Menu;
using Sandbox.UI;
namespace Ringworld;

public partial class ActiveLobby : Panel
{
	Friend Owner => Lobby.Owner;
	ILobby Lobby => Sandbox.Game.Menu.Lobby;

	int MaxPlayersSupported { get; set; } = 1;
	Package MapPackage { get; set; }

	private bool IsTeamGame { get; set; } = false;

	private int maxFrags { get; set; } = 25;

	public void LeaveLobby()
	{
		Lobby?.Leave();

		this.Navigate( "/lobby/list" );
	}

	async Task Start()
	{
		await Sandbox.Game.Menu.StartServerAsync( Sandbox.Game.Menu.Lobby.MaxMembers, $"{Sandbox.Game.Menu.Lobby.Owner.Name}'s game", Sandbox.Game.Menu.Lobby.Map );
	}

	public void StartGame()
	{
		
		_ = Game.Menu.EnterServerAsync();
	}

	async void FetchPackage()
	{
		if (Game.Menu.Lobby == null)
		MapPackage = await Package.FetchAsync( Game.Menu.Lobby?.Map ?? "facepunch.square", true );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		FetchPackage();
	}

	protected override void OnParametersSet()
	{
		MaxPlayersSupported = Sandbox.Game.Menu.Package.GetMeta<int>( "MaxPlayers", 1 );
		Lobby.SetData( "convar.gm_maxpoints", maxFrags.ToString() );
	}
}
