using Editor;
using Ringworld;
using Sandbox;
using System.Linq;
namespace Ringworld;

[EditorModel( "models/citizen/citizen.vmdl" )]
[Library( "evil_melon" ), HammerEntity]
[Title( "Evil Melon" ), Category( "NPCs" )]
public class EvilMelon : AnimatedEntity
{
	public string State;
	protected Vector3[] Path;
	protected int CurrentPathSegment;
	protected TimeSince TimeSinceGeneratedPath = 0;

	const float CHASE_DISTANCE = 1000f;
	const float MOVEMENT_SPEED = 2f;
	const float ATTACK_RANGE = 50f;

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		SetAnimParameter( "special_movement", 1);
	}

	[GameEvent.Tick]
	public void Tick( )
	{
		switch ( State )
		{
			case "idle":
				PerformStateIdle();
				break;
			case "attacking_player":
				PerformStateAttackingPlayer();
				break;
			default:
				State = "idle";
				break;
		}
	}

	protected void PerformStateIdle()
	{
		var player = GetClosestPlayer();

		if ( player != null && player.Position.Distance( Position ) <= CHASE_DISTANCE )
			State = "attacking_player";
	}

	protected void PerformStateAttackingPlayer()
	{
		var player = GetClosestPlayer();

		if ( player == null || player.Position.Distance( Position ) > CHASE_DISTANCE )
		{
			State = "idle";
			return;
		}

		if ( TimeSinceGeneratedPath >= 1 )
			GeneratePath( player );

		TraversePath();

		if ( player.Position.Distance( Position ) <= ATTACK_RANGE )
		{
			player.TakeDamage( new DamageInfo { Damage = 1f } );
		}
	}

	protected Pawn GetClosestPlayer()
	{
		return All.OfType<Pawn>()
			.OrderByDescending( x => x.Position.Distance( Position ) )
			.FirstOrDefault();
	}

	protected void GeneratePath( Pawn target )
	{
		TimeSinceGeneratedPath = 0;

		Path = NavMesh.PathBuilder( Position )
			.WithMaxClimbDistance( 16f )
			.WithMaxDropDistance( 16f )
			.WithStepHeight( 16f )
			.WithMaxDistance( 99999999 )
			.WithPartialPaths()
			.Build( target.Position )
			.Segments
			.Select( x => x.Position )
			.ToArray();

		CurrentPathSegment = 0;
	}

	protected void TraversePath()
	{
		if ( Path == null )
			return;

		var distanceToTravel = MOVEMENT_SPEED;

		while ( distanceToTravel > 0 )
		{
			var currentTarget = Path[CurrentPathSegment];
			var distanceToCurrentTarget = Position.Distance( currentTarget );

			if ( distanceToCurrentTarget > distanceToTravel )
			{
				var direction = (currentTarget - Position).Normal;
				Position += direction * distanceToTravel;
				Rotation = direction.EulerAngles.ToRotation();
				return;
			}
			else
			{
				var direction = (currentTarget - Position).Normal;
				Position += direction * distanceToCurrentTarget;
				distanceToTravel -= distanceToCurrentTarget;
				CurrentPathSegment++;
			}

			if ( CurrentPathSegment == Path.Count() )
			{
				Path = null;
				return;
			}
		}
	}
}
