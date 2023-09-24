namespace Sandbox
{
	[GameResource("Weapon Data", "weapon", "Data for a weapon")]
	public partial class WeaponData : GameResource
	{
		public string weaponName { get; set; }

		public int clipSize { get; set; }


		public int maxReserveAmmo { get; set; }

		public float reloadLength { get; set; }

		public int weaponDamage { get; set; }

		public float deployTime { get; set; }

		public float primaryFireRate { get; set; }

		public bool canZoom { get; set; }
		public float zoomMult { get; set; }

		[ResourceType( "vmdl" )]
		public string ViewModel { get; set; }

		[ResourceType( "vmdl" )]
		public string WorldModel { get; set; }

		public float spread { get; set; }

		public enum FireType
		{
			FullAuto,
			SemiAuto
		}

		public FireType fireMode { get; set; }

		[ResourceType("sound")]
		public string fireSound { get; set; }

		public int numberOfBullets { get; set; }

		public testCategory test { get; set; }

	}
}

public struct testCategory
{
	public int num1 { get; set; }
	public int num2 { get; set; }
}
