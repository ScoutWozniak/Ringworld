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
			ActiveWeapon.Enabled = false;
			ActiveWeaponIndex++;
			if ( ActiveWeaponIndex > StoredWeapons.Count - 1 ) ActiveWeaponIndex = 0;
			ActiveWeapon.Enabled = true;
		}
	}

	void SwitchWeapons()
	{
		
	}
}
