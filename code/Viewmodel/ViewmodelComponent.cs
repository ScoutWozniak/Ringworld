using Sandbox;

public sealed class ViewmodelComponent : Component
{
	[Property] GameObject Camera { get; set; }
	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			Camera.Transform.Position = Scene.Camera.Transform.Position;
			Camera.Transform.Rotation = Scene.Camera.Transform.Rotation;
			Transform.Position = Scene.Camera.Transform.Position;
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, Scene.Camera.Transform.Rotation, Time.Delta * 20.0f );
		}
		else
		{
			Camera.Enabled = false;
			foreach(var child in GameObject.Children)
			{
				child.Enabled = false;
			}
		}
	}
}
