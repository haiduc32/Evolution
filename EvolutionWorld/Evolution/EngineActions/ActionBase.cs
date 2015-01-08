using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.EngineActions
{
	public abstract class ActionBase
	{
		public abstract ActionType ActionType { get; }

		public long Tick { get; set; }
	}
}
