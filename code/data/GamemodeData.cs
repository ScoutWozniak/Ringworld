using Sandbox;
using System.Collections.Generic;
using Sandbox.UI;

namespace Ringworld;

[GameResource( "Gamemode", "gamemode", "Data for a gamemode" )]
public partial class GamemodeData : GameResource
{
	public string modeName { get; set; }

	public string engineClassName { get; set; }

	public List<ConvarDefinition> convarNames { get; set; }


	public GamemodeData CreateInstance()
	{
		if ( string.IsNullOrEmpty( engineClassName ) )
			return null;

		var type = TypeLibrary.GetType<GamemodeData>( engineClassName ).TargetType;
		if ( type == null )
			return null;

		var mode = TypeLibrary.Create<GamemodeData>( engineClassName );
		return mode;
	}
}

public struct ConvarDefinition
{
	public string conVarName { get;set; }
	public ConVarTypes conVarType { get; set; }
	public string readableName { get; set; }

	public int maxVal { get; set; }
	public int minVal { get; set; }

	public List<string> options { get; set; }
}

public enum ConVarTypes
{
	cInt = 0,
	cBool = 1,
	cString = 2,
}
