using Sandbox;
using System;
using System.Collections.Generic;

namespace Ringworld;

public class PawnTeam : EntityComponent<Pawn>
{
	public int teamId { get; set; }

	public virtual void SetupTeam(int id)
	{
		Entity.RenderColor = Color.Red;
	}
}
