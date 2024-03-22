using Sandbox;

public sealed class WeaponStateComponent : Component
{
	[Property] bool IsInInv { get; set; }

	[Property] GameObject HeldObject { get; set; }

	[Property] GameObject DroppedObject { get; set; }

	[Property] bool AddOnStart { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		DroppedObject.Enabled = true;
		HeldObject.Enabled = false;
	}

	public void EquipWeapon()
	{
		HeldObject.Enabled = true;
	}

	public void UnequipWeapon()
	{
		HeldObject.Enabled = false;
	}

	public void DropWeapon()
	{

	}

	public void AddWeaponToInventory()
	{
		DroppedObject.Enabled = false;
	}
}
