using Sandbox;
using System.Collections.Generic;

public sealed class WeaponInventory : Component
{
	[Property] List<GameObject> StoredWeapons { get; set; }
	[Property] int ActiveWeaponIndex { get; set; }

	GameObject ActiveWeapon { get { return StoredWeapons[ActiveWeaponIndex]; } }


	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		if (Input.Pressed("Slot1"))
		{
			UnequipCurrent();
			ActiveWeaponIndex++;
			if ( ActiveWeaponIndex > StoredWeapons.Count - 1 ) ActiveWeaponIndex = 0;
			EquipCurrent();
		}
	}

	void EquipCurrent()
	{
		ActiveWeapon.Components.Get<WeaponStateComponent>().EquipWeapon();
	}

	void UnequipCurrent()
	{
		if(StoredWeapons.Count == 0)
			return;
		ActiveWeapon.Components.Get<WeaponStateComponent>().UnequipWeapon();
	}

	public void AddWeapon(GameObject weapon)
	{
		UnequipCurrent();
		var newIndex = StoredWeapons.Count;
		StoredWeapons.Add(weapon);
		ActiveWeaponIndex = newIndex;
		var stateManager = weapon.Components.Get<WeaponStateComponent>();

		stateManager.AddWeaponToInventory();
		stateManager.EquipWeapon();
	}

	public bool CanAddWeapon()
	{
		if ( StoredWeapons.Count < 2 )
			return true;
		return false;
	}
}
