using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Events
{
	public class UnitEndMoveEventArgs : UnitEventArgs
	{
		public Location LastLocation { get; set; }

		public Location NewLocation { get; set; }
	}
}
