using Sandbox;

public sealed class HealthDebugTextComponent : Component
{
	[Property] HealthComponent healthComponent { get; set; }
	protected override void OnUpdate()
	{
		string displayText = "Health: " + healthComponent.Health.ToString();
		if (healthComponent.HasShields)
		{
			displayText += "\n" + "Shields: " + healthComponent.Shields;
		}
		Components.Get<TextRenderer>().Text = displayText;
		Transform.Rotation = Rotation.LookAt( Transform.Position - Scene.Camera.Transform.Position );
	}
}
