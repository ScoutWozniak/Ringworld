using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	[GameResource("Weapon Data", "weapon", "Data for a weapon")]
	public partial class WeaponData : GameResource
	{
		public string Name { get; set; }

		public int clipSize { get; set; }

		public float reloadLength { get; set; }

		public float deployTime { get; set; }

		public float primaryFireRate { get; set; }
	}
}
