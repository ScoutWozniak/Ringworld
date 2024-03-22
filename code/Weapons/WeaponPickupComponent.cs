using Sandbox;

public sealed class WeaponPickupComponent : Component, Component.ITriggerListener
{
	[Property] GameObject Root { get; set; }
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		if ( other.GameObject == null )
			return;
		if (other.GameObject.Components.TryGet<WeaponInventory>(out var inv, FindMode.InDescendants))
		{
			Log.Info( "test" );
			if ( inv.CanAddWeapon() )
				inv.AddWeapon( Root );
		}
	}
}
