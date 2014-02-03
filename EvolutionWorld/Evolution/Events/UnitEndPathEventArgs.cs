using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Events
{
	public class UnitEndPathEventArgs : UnitEventArgs
	{
		public Location Location { get; set; }

		public bool PathInterrupted { get; set; }
	}
}
