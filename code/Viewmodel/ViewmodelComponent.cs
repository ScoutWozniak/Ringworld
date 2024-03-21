using Sandbox;
using System.Linq;

public sealed class ViewmodelComponent : Component
{
	protected override void OnUpdate()
	{
		
		if ( !IsProxy )
		{
			var Camera = ViewmodelCam.Instance;
			if ( Camera == null ) return;
			Camera.Transform.Position = Scene.Camera.Transform.Position;
			Camera.Transform.Rotation = Scene.Camera.Transform.Rotation;
			Transform.Position = Scene.Camera.Transform.Position;
			Transform.Rotation = Rotation.Lerp( Transform.Rotation, Scene.Camera.Transform.Rotation, Time.Delta * 20.0f );
		}
		else 
		{
			
			foreach(var child in GameObject.Children)
			{
				child.Enabled = false;
			}
		}
	}
}
