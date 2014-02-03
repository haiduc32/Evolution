using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
	public class ZombieNpc : UnitBase
	{
		public override int Range
		{
			get { return 2; }
		}

		public override bool IsNpc
		{
			get { return true; }
		}

		public override int MovementSpeed
		{
			get { return 50; }
		}

		/// <summary>
		/// Engine event. Called when a new unit comes in range.
		/// </summary>
		public override void UnitInRange(UnitBase unit)
		{
			//if (AttackedUnit == null && (!unit.IsNpc || unit is VillagerNpc))
			//{
			//	if (MoveToAttack(unit)) AttackedUnit = unit;
			//}
		}
	}
}
