using Sandbox;
using System;

namespace Ringworld;

public interface ITeam
{
	public int teamID {get;set;}
	public string teamName { get;set;}

	public void SetupTeam( int id );

}
