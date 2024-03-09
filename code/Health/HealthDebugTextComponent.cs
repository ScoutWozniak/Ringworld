using Sandbox;

public sealed class HealthDebugTextComponent : Component
{
	[Property] HealthComponent healthComponent { get; set; }
	protected override void OnUpdate()
	{
		Components.Get<TextRenderer>().Text = healthComponent.Health.ToString();
		Transform.Rotation = Rotation.LookAt( Transform.Position - Scene.Camera.Transform.Position );
	}
}
