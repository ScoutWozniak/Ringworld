using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Effects;

namespace Ringworld;

[SceneCamera.AutomaticRenderHook]
public partial class MyPostProcessEffect : ScreenEffects
{


	float vignetteState = 0f;
	float vignetteTarget = 0f;

	public override void OnStage( SceneCamera target, Stage renderStage )
	{

		if ( renderStage == Stage.AfterUI )
		{
			if ( Sandbox.Game.LocalPawn is Pawn pawn && pawn.fovZoomMult > 1.0f )
				vignetteTarget = 0.5f;
			else
				vignetteTarget = 0f;


			vignetteState = vignetteTarget;
			Vignette.Intensity = vignetteState;

			Pixelation = 0.0f;

			RenderEffect( target );
		}
	}
}
