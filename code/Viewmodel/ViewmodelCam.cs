using Sandbox;

public sealed class ViewmodelCam : Component
{
	public static GameObject Instance { get; set; }
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !IsProxy )
			Instance = GameObject;
		else
		{
			Instance = null;
			GameObject.Enabled = false;
		}
	}
}
