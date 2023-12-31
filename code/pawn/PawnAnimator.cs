﻿using Sandbox;
using System;

namespace Ringworld;

public class PawnAnimator : EntityComponent<Pawn>, ISingletonComponent
{
	public void Simulate()
	{
		var helper = new CitizenAnimationHelper( Entity );
		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		//helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();

		helper.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
		helper.Handedness = CitizenAnimationHelper.Hand.Both;

		helper.DuckLevel = Entity.Controller.ducking ? 0.75f : 0;

		if ( Entity.Controller.HasEvent( "jump" ) )
		{
			helper.TriggerJump();
		}
	}
}
